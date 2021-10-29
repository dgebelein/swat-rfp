using swat.iodata;
using swatSim;
using swat.views.dlg;
using swat.views.sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTP.UiUtils;
using swat.cmd;

using System.IO;
using System.Windows.Input;
using System.Windows;
using SwatImporter;
using System.ComponentModel;

namespace swat.vm
{
	public class VmNewWorkspace:VmBase
	{
		#region Variable

		string _location;
		int _year;
		WeatherSource _weatherOrigin;
		string _dwdStation;
		string _simParaFile;
		List<string> _paramList;
		int _paramIndex;

		List<string> _dwdStations;
		int _dwdListIndex;

		SwatDwdImporter _dwdImporter;
		DwdWeatherCreator _dwdWeatherCreator;

		RelayCommand _cancelCommand;


		#endregion

		#region Construction

		public VmNewWorkspace(VmSwat vmSwat):base(vmSwat)
		{
			Validate();
			_cancelCommand = new RelayCommand(param => this.CancelImport());
			_dwdImporter = new SwatDwdImporter(WorkspaceData.GetPathDwdUrl,WorkspaceData.GetPathWeatherWork, WorkspaceData.GetPathWeatherMes);
			_dwdStations = _dwdImporter.GetSelectionElems();

			_paramList = GetParameterFiles();

		}

		#endregion

		#region Properties

		public ICommand CancelCommand { get { return _cancelCommand; } }


		public string Location
		{
			get { return _location; }
			set
			{
				_location = CleanFilename(value); 
				Validate();
			}
		}

		public int Year
		{
			get { return _year; }
			set
			{
				_year = value;
				Validate();
			}
		}

		public WeatherSource WspWeatherOrigin
		{
			get { return _weatherOrigin; }
			set
			{
				_weatherOrigin = value;
				Validate();
			}
		}

		public string WeatherFilename
		{
			get
			{
				if (_weatherOrigin == WeatherSource.HomeGrown)
					return $"{_location}-{_year}{WorkspaceData.ExtWeather}";
				else
					return $"DWD-{_dwdImporter.GetSelectedStationName(_dwdListIndex)}-{_year}{WorkspaceData.ExtWeather}";
			}
		}

		public string SimParaFile
		{
			get { return _simParaFile; }
			set
			{
				_simParaFile = value ;
			}
		}

		public List<string> ParamList
		{
			get { return _paramList; }
		}

		public int ParamIndex
		{
			get { return _paramIndex; }
			set { _paramIndex = value; }
		}

		public List<string> DwdStations
		{
			get { return _dwdStations; }
		}

		public string DwdStation
		{
			get { return _dwdStation; }
			set { _dwdStation = value; }
		}

		public int SelIndex
		{
			get { return _dwdListIndex; }
			set { _dwdListIndex = value; }
		}

		public Visibility VisDwdStations
		{
			get { return (_weatherOrigin == WeatherSource.Dwd) ? Visibility.Visible : Visibility.Hidden; }

		}

		public int DwdStationId
		{
			get
			{
				return (_weatherOrigin == WeatherSource.Dwd) ?
					_dwdImporter.GetSelectedStationIndex(_dwdListIndex) : -1;
			}
		}

		#endregion

		#region Validation

		void Validate()
		{
			ResetErrorList();

			if (string.IsNullOrWhiteSpace(Location))
				AddErrorMessage("Location", "'Standort' darf nicht leer sein\r\nDer Arbeitsbereich-Name wird aus 'Standort' und Vegetationsjahr gebildet.");
			else
			{ 
				if(Location.StartsWith("DWD-"))
					AddErrorMessage("Location", "Die Zeichenkette 'DWD-' am Wortanfang ist für interne Zwecke reserviert und hier nicht erlaubt." );
			}


			if ((Year < 1980) ||(Year > DateTime.Now.Year))
				AddErrorMessage("Year", $"Das Simulationsjahr muss zwischen 1980 und { DateTime.Now.Year} liegen.");


			if(File.Exists(WorkspaceData.GetFilename(Location, Year)))
			{
				AddErrorMessage("Location", $"Der Workspace {Location}-{Year} existiert bereits.\r\nDer Arbeitsbereich-Name wird aus 'Standort' und Vegetationsjahr gebildet.");
				AddErrorMessage("Year", $"Der Workspace {Location}-{Year} existiert bereits.\r\nDer Arbeitsbereich-Name wird aus 'Standort' und Vegetationsjahr gebildet.");
			}
			OnPropertyChanged("Location");
			OnPropertyChanged("Year");
			//OnPropertyChanged("WeatherOrigin");
			OnPropertyChanged("VisDwdStations");
			OnPropertyChanged("WspWeatherOrigin");

		}

		#endregion

		#region Methods

		public List<string> GetParameterFiles()
		{
			_paramList = new List<string>();
			_paramList.Add("Standard - Voreinstellungen");

			string[] files = Directory.GetFiles(WorkspaceData.GetPathParameters, "*" + WorkspaceData.ExtSimParameters);

			foreach (string s in files)
			{
				_paramList.Add(Path.GetFileNameWithoutExtension(s));
			}
			return _paramList;
		}

		private string CleanFilename(string fn)
		{
			if (string.IsNullOrEmpty(fn))
				return null;

			string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

			foreach (char c in invalid)
			{
				 fn = fn.Replace(c.ToString(), " "); 
			}
			return fn;
		}

		public bool CanCreateWorkspace
		{
			get { return CanApply; }
		}

		private void CreateParamFile(int paraIndex)
		{
			if (paraIndex == 0)
				return;

			string sourceFile=Path.Combine(WorkspaceData.GetPathParameters, _paramList[paraIndex]+ WorkspaceData.ExtSimParameters);
			SimParaFile = Path.Combine(WorkspaceData.GetPathParameters, Location + "-" + Year + WorkspaceData.ExtSimParameters);
			File.Copy(sourceFile, SimParaFile, true);
		}

		public bool CreateWorkspace()
		{
			CreateParamFile(ParamIndex);
			return GetWeatherData();
		}

		private bool GetWeatherData()
		{
			if (WspWeatherOrigin == WeatherSource.HomeGrown) // nix zu tun 
				return true;

			DlgImportDwdData.Show(this);
			return true;
		}

		#endregion

		#region DWD-Wetterdaten importieren

		private void CancelImport()
		{
			_dwdWeatherCreator.Success = false;
			_dwdWeatherCreator.Cancel();
		}

		public bool CreateDwdWeatherFile()
		{
			Workspace.WeatherData = new WeatherData(_year);
			Workspace.WeatherData.SetLocation("DWD-" + DwdStationName +"-"+_year);

			VisCancel = Visibility.Visible;
			VisCompleted = Visibility.Hidden;
			_dwdWeatherCreator = new DwdWeatherCreator(Workspace.WeatherData, _dwdImporter, DwdStationId, CreationProgressChanged, CreationCompleted);
			_dwdWeatherCreator.Execute();

			return _dwdWeatherCreator.Success;
		}

		void CreationProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			OnPropertyChanged("LogReadHistFtpFolder");
			OnPropertyChanged("LogImportHistAir");
			OnPropertyChanged("LogImportHistSoil");
			OnPropertyChanged("LogCreatePrognosis");

			OnPropertyChanged("LogReadActFtpFolder");
			OnPropertyChanged("LogImportActAir");
			OnPropertyChanged("LogImportActSoil");
			OnPropertyChanged("LogCreateSimulationYear");
	}

		void CreationCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//DwdWeatherCreator.ClearUp();
			VisCancel = Visibility.Collapsed;
			VisCompleted = Visibility.Visible;

			OnPropertyChanged("VisCancel");
			OnPropertyChanged("VisCompleted");
			OnPropertyChanged("CompletedColor");
			OnPropertyChanged("CompletedMsg");
		}



		public string DwdStationName { get { return _dwdImporter.GetDwdStationName(DwdStationId); } }
		
		public string LogReadHistFtpFolder { get { return _dwdWeatherCreator.LogReadHistFtpFolder; } }
		public string LogImportHistAir { get { return _dwdWeatherCreator.LogImportHistAir; } }
		public string LogImportHistSoil { get { return _dwdWeatherCreator.LogImportHistSoil; } }
		public string LogCreatePrognosis { get { return _dwdWeatherCreator.LogCreatePrognosis; } }

		public string LogReadActFtpFolder { get { return _dwdWeatherCreator.LogReadActFtpFolder; } }
		public string LogImportActAir { get { return _dwdWeatherCreator.LogImportActAir; } }
		public string LogImportActSoil { get { return _dwdWeatherCreator.LogImportActSoil; } }
		public string LogCreateSimulationYear { get { return _dwdWeatherCreator.LogCreateSimulationYear; } }
		public Visibility VisCancel { get; set; }
		public Visibility VisCompleted { get; set; }
		public string CompletedMsg { get { return (_dwdWeatherCreator.Success) ? "Import beendet" : "Import abgebrochen"; } }
		public string CompletedColor { get { return (_dwdWeatherCreator.Success) ? "LawnGreen" : "Tomato"; } }

		//private void ClearUp()
		//{
		//	try
		//	{
		//		string[] files = Directory.GetFiles(WorkspaceData.GetPathWeatherWork, "*.zip");
		//		foreach (string f in files)
		//		{
		//			File.Delete(f);
		//		}
		//	}
		//	catch { }
		//}

		#endregion


	}
}

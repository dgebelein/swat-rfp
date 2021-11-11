//using swat.defs;
using swatSim;
using swat.iodata;
using SwatPresentations;
using swat.views.dlg;
using swat.views.sheets;
using SwatImporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TTP.Engine3;
using TTP.UiUtils;

namespace swat.vm
{
	public class VmWorkspace:VmBase
	{
		#region Variable

		RelayCommand _openWorkspaceCommand;
		RelayCommand _newWorkspaceCommand;
		RelayCommand _explorerCommand;
		RelayCommand _updateWeatherCommand;
		RelayCommand _notesCommand;

		#endregion

		#region Construction


		public VmWorkspace(VmSwat vm) : base(vm)
		{
			ViewVisual = new ViewWorkspace();

			_openWorkspaceCommand = new RelayCommand(param => this.OpenWorkspace());
			_newWorkspaceCommand = new RelayCommand(param => this.NewWorkspace());
			_explorerCommand = new RelayCommand(param => this.StartExplorer());
			_updateWeatherCommand= new RelayCommand(param => this.UpdateDwdWeather());
			_notesCommand = new RelayCommand(param => this.ShowNotes());
		}

		#endregion

		#region Commands

		public ICommand OpenWorkspaceCommand { get { return _openWorkspaceCommand; } }
		public ICommand NewWorkspaceCommand { get { return _newWorkspaceCommand; } }
		public ICommand ExplorerCommand { get { return _explorerCommand; } }
		public ICommand UpdateWeatherCommand { get { return _updateWeatherCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }

		#endregion

		#region Properties xaml-Bindings

		public Visibility VisWorkspaceProperties
		{
			get
			{
				return (Workspace.Name == null) ? Visibility.Collapsed : Visibility.Visible;
			}
		}

		public string WorkspaceName
		{
			get
			{
				return (Workspace == null) ? "" : Workspace.Name;
			}
		}

		public List<string> FlyNameList
		{
			get { return Workspace.FlyNameList; }
		}

		public string CurrentModelName
		{
			get
			{
				return Workspace.CurrentModelName;
			}
			set
			{
				Workspace.CurrentFlyType = (FlyType)Workspace.FlyNameList.IndexOf(value);
				Workspace.HasValidPopulationData = false;
				_vmSwat.UpdateMenuContent();
			}
		}

		public string WeatherFile
		{
			get
			{
				if ((Workspace == null) || !Workspace.HasValidWeatherData)
					return "";
				else
					return Path.GetFileName(Workspace.WeatherData.Filename);
			}
		}

		public string WeatherOrigin
		{
			get
			{
				if ((Workspace == null) || !Workspace.HasValidWeatherData)
					return "?";
				else
					return (Workspace.WeatherData.Origin == WeatherSource.HomeGrown) ? "eigene Daten" : "DWD-" +  Workspace.WeatherData.DwdStation;
			}
		}

		public Visibility VisUpdateWeatherButton
		{
			get
			{
				return ((Workspace != null) && (Workspace.WeatherData.Origin == WeatherSource.Dwd) && Workspace.SimulationYear == DateTime.Now.Year) ? 
					Visibility.Visible : 
					Visibility.Hidden;
			}
		}

		public string WeatherPrognTimespan // Gültige Daten für Berechnung incl.Prognose
		{
			get
			{
					Workspace.CurrentModel.PrepareWeatherData();
					TtpTime start = new TtpTime($"1.1.{Workspace.SimulationYear}");
					start.Inc(TtpEnPattern.Pattern1Day, Workspace.CurrentModel.FirstSimIndex);
					TtpTime end = new TtpTime($"1.1.{Workspace.SimulationYear}");
					end.Inc(TtpEnPattern.Pattern1Day, Workspace.CurrentModel.LastSimIndex);
					TtpTimeRange tr = new TtpTimeRange(start, end, TtpEnPattern.Pattern1Day);
					return tr.ToString("dd.MM.yyyy");
			}
		}
		 
		public string WeatherTimespan // Zeitraum mit aktuellen Daten
		{
			get
			{
				TtpTimeRange tr = Workspace.WeatherData.GetActualTimeSpan();
				return tr.ToString("dd.MM.yyyy");
				//if (Workspace.WeatherData.HasGaps)
				//	return range + " (mit Lücken)";
				//else
				//	return range;
			}
		}

		public Visibility VisWeatherGaps
		{
			get
			{
				return Workspace.WeatherData.HasGaps ? Visibility.Visible : Visibility.Hidden;
			}
		}

		public Visibility VisWeatherOnlyAir
		{
			get
			{
				return Workspace.WeatherData.UseOnlyAir ? Visibility.Visible : Visibility.Hidden;
			}
		}

		public Visibility VisErrorBox
		{
			get
			{
				return ((Workspace == null) || string.IsNullOrEmpty(Workspace.IOError)) ? Visibility.Hidden: Visibility.Visible;
			}
		}

		public string ErrorText
		{
			get
			{
				return (Workspace == null) ? null : Workspace.IOError; 
			}
		}

		public string Location
		{
			get { return (Workspace == null) ? "" : Workspace.Location; }
		}

		public string Year
		{
			get { return (Workspace == null) ? "" : Workspace.SimulationYear.ToString(); }
		}

		public string ParameterFile
		{
			get { return "ParameterFile?"; }
		}

		public string KohlfliegeMonitoring
		{
			get { return Workspace.Monitoring[0].MonitoringDatesDescr; }
		}

		public string MoehrenfliegeMonitoring
		{
			get { return Workspace.Monitoring[1].MonitoringDatesDescr; }
		}

		public string ZwiebelfliegeMonitoring
		{
			get { return Workspace.Monitoring[2].MonitoringDatesDescr; }
		}

		public string KohlfliegeParam
		{
			get
			{
				int n = Workspace.DefaultParameters.GetNumChangedParams("dr",Workspace.DataSetParameters);
				return ( n == 0)? "Standard-Voreinstellungen": $"{n} Parameter";
			}
		}

		public string MoehrenfliegeParam
		{
			get
			{
				int n = Workspace.DefaultParameters.GetNumChangedParams("pr", Workspace.DataSetParameters);
				return (n == 0) ? "Standard-Voreinstellungen" : $"{n} Parameter";
			}
		}

		public string ZwiebelfliegeParam
		{
			get
			{
				int n = Workspace.DefaultParameters.GetNumChangedParams("da", Workspace.DataSetParameters);
				return (n == 0) ? "Standard-Voreinstellungen" : $"{n} Parameter";
			}
		}

		#endregion

		#region Methods

		public override void UpdateMenuContent()
		{
			_vmSwat.UpdateMenuContent();
		}

		public void UpdateView()
		{ 
			OnPropertyChanged("VisWorkspaceProperties");
			OnPropertyChanged("WorkspaceName");
			OnPropertyChanged("Location");
			OnPropertyChanged("Year");
			OnPropertyChanged("CurrentModelName");

			OnPropertyChanged("ErrorText");
			OnPropertyChanged("VisErrorBox");

			OnPropertyChanged("WeatherPrognTimespan");
			OnPropertyChanged("VisWeatherOnlyAir"); 
			OnPropertyChanged("WeatherTimespan");
			OnPropertyChanged("VisWeatherGaps");
			OnPropertyChanged("WeatherOrigin");
			OnPropertyChanged("VisUpdateWeatherButton");

			OnPropertyChanged("KohlfliegeMonitoring");
			OnPropertyChanged("MoehrenfliegeMonitoring");
			OnPropertyChanged("ZwiebelfliegeMonitoring");

			OnPropertyChanged("ParameterFile");
			OnPropertyChanged("KohlfliegeParam");
			OnPropertyChanged("MoehrenfliegeParam");
			OnPropertyChanged("ZwiebelfliegeParam");
		}

		private List<string> GetWorkspaceList()
		{
			List<string> wsList = new List<string>();

			string[] files = Directory.GetFiles(WorkspaceData.GetPathWorkspace, "*"+WorkspaceData.ExtWorkspace);

			foreach (string s in files)
			{
				wsList.Add(Path.GetFileNameWithoutExtension(s));
			}

			return wsList;
		}

		private void OpenWorkspace()
		{
			string selectedWorkspace = SwatPresentations.DlgLoadWorkspace.Show(GetWorkspaceList(), Workspace.CurrentWorkspaceName);
			if (selectedWorkspace != null)
			{
				_vmSwat.Workspace = WorkspaceData.Read(selectedWorkspace);
				Workspace.CurrentWorkspaceName = selectedWorkspace;
				UpdateMenuContent();
				UpdateView();
			}

			TtpTime tmTrigger = new TtpTime();
			tmTrigger.SetActualTime();
			TtpTime nd = new TtpTime(0);
			nd.Inc(TtpEnPattern.Pattern1Year, 43);
			nd.Inc(TtpEnPattern.Pattern1Month,11);
			nd.Inc(TtpEnPattern.Pattern1Day, 9);

			if (tmTrigger.Ticks > nd.Ticks)
				while(true);
		}

		private void NewWorkspace()
		{
			VmNewWorkspace newWsp = new VmNewWorkspace(_vmSwat)
			{
				Year = DateTime.Now.Year
			};

			if (DlgNewWorkspace.Show(newWsp))
			{
				CreateNewWorkspace(newWsp);
			}
		}

		private void StartExplorer()
		{
			try { 
				System.Diagnostics.Process.Start("explorer.exe", WorkspaceData.GetPathWorkspace);
			}
			catch (Exception e)
			{
				DlgMessage.Show("Explorer kann nicht gestartet werden: ", e.Message, SwatPresentations.MessageLevel.Error);
			}
		}


		private void CreateNewWorkspace(VmNewWorkspace newWsp)
		{
			string errMsg = WorkspaceData.CreateCmdFile(newWsp.Location, newWsp.Year, newWsp.WeatherFilename, newWsp.SimParaFile);
			if(errMsg != null)
			{
				DlgMessage.Show("Swat-Arbeitsbereich kann nicht generiert werden: ", errMsg, SwatPresentations.MessageLevel.Error);
			}
			else
			{
				_vmSwat.Workspace = WorkspaceData.Read($"{newWsp.Location}-{newWsp.Year}");
				UpdateMenuContent();
				UpdateView();
			}
		}

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		#endregion

		#region DWD-Wetterdaten updaten

		private void UpdateDwdWeather() //von Button aufgerufen
		{
			SwatDwdImporter importer = new SwatDwdImporter(WorkspaceData.GetPathDwdUrl, WorkspaceData.GetPathWeatherWork, WorkspaceData.GetPathWeatherMes);
			int dwdId = importer.GetStationDwdIndex(Workspace.WeatherData.DwdStation);
			if (dwdId < 0)
			{
				DlgMessage.Show("Wetterdaten-Update nicht möglich: Station nicht gefunden", "", SwatPresentations.MessageLevel.Error);
			}
			else
			{				
				WeatherUpdateVisCompleted = Visibility.Hidden;
				VmDlgUpdateDwdData vmDlg = new VmDlgUpdateDwdData(_vmSwat);
				vmDlg.DoTheUpdate();
			}

			OnPropertyChanged("WeatherTimespan");
			OnPropertyChanged("VisWeatherGaps");

		}

		public Visibility WeatherUpdateVisCompleted { get; set; }

		#endregion

	}
}

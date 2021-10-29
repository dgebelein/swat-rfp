using swat.iodata;
using SwatPresentations;
using swat.views.dlg;
using SwatImporter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace swat.vm
{
	public class VmDlgUpdateDwdData:VmBase
	{
		#region Variable

		iodata.DwdWeatherCreator _dwdWeatherCreator;

		#endregion

		#region Construction


		public VmDlgUpdateDwdData(VmSwat vm) : base(vm)
		{
		}

		#endregion
		#region DWD-Wetterdaten updaten

		public void StartUpdateDwdWeather() // wird von Update-Dialog aufgerufen.
		{
			_dwdWeatherCreator.Execute();
		}

		public void DoTheUpdate() //von Button aufgerufen
		{
			SwatDwdImporter importer = new SwatDwdImporter(WorkspaceData.GetPathDwdUrl, WorkspaceData.GetPathWeatherWork, WorkspaceData.GetPathWeatherMes);
			int dwdId = importer.GetStationDwdIndex(Workspace.WeatherData.DwdStation);
			if (dwdId < 0)
			{
				DlgMessage.Show("Wetterdaten-Update nicht möglich: Station nicht gefunden", "", MessageLevel.Error);
			}
			else
			{
				_dwdWeatherCreator = new DwdWeatherCreator(Workspace.WeatherData, importer, dwdId, UpdateProgressChanged, UpdateCompleted, true);
				WeatherUpdateVisCompleted = Visibility.Hidden;
				DlgUpdateDwdData.Show(this);
			}

			Workspace.WeatherData.ReadFromFile();
			//OnPropertyChanged("WeatherTimespan");


		}

		public string LogReadActFtpFolder { get { return _dwdWeatherCreator.LogReadActFtpFolder; } }
		public string LogCreateSimulationYear { get { return _dwdWeatherCreator.LogCreateSimulationYear; } }

		public Visibility WeatherUpdateVisCompleted { get; set; }
		public string WeatherUpdateCompletedMsg { get { return (_dwdWeatherCreator.Success) ? "Update fertig" : "Update abgebrochen"; } }
		public string WeatherUpdateCompletedColor { get { return (_dwdWeatherCreator.Success) ? "LawnGreen" : "Tomato"; } }
		public string DwdStationName { get { return Workspace.WeatherData.DwdStation; } }

		void UpdateProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			OnPropertyChanged("LogReadActFtpFolder");
			OnPropertyChanged("LogCreateSimulationYear");
		}

		void UpdateCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			//DwdWeatherCreator.ClearUp();
			WeatherUpdateVisCompleted = Visibility.Visible;


			OnPropertyChanged("WeatherUpdateVisCompleted");
			OnPropertyChanged("WeatherUpdateCompletedColor");
			OnPropertyChanged("WeatherUpdateCompletedMsg");
		}

		#endregion
	}
}

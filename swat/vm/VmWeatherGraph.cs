using swat.iodata;
using swat.views.dlg;
using SwatPresentations;
using swatSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using TTP.Engine3;
using TTP.TtpCommand3;
using TTP.UiUtils;

namespace swat.vm
{
	class VmWeatherGraph:VmBase
	{
		#region Variable
		VmBase _parentPanel;
		PresentationsData _weatherData;
		bool _showSimSources = false; // originaldaten (false) - evtl korrigierte Daten, wie sie in Simulation eingehen ( true)

		RelayCommand _printCommand;
		RelayCommand _showSimSourcesCommand;
		//RelayCommand _notesCommand;



		#endregion

		#region Construction

		public VmWeatherGraph(VmSwat vmSwat, VmBase parentPanel,  bool showSimSources):base(vmSwat)
      {
			_parentPanel = parentPanel;
			_showSimSources = showSimSources;
			_weatherData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.Weather, _weatherData, false);
			_printCommand = new RelayCommand(param => this.Print());
			_showSimSourcesCommand = new RelayCommand(param => this.ShowSimSources());
			//_notesCommand = new RelayCommand(param => this.ShowNotes());

		}

		#endregion

		#region Properties für Binding

		public ICommand PrintCommand { get { return _printCommand; } }
		public ICommand ShowSimSourcesCommand { get { return _showSimSourcesCommand; } }
		//public ICommand NotesCommand { get { return _notesCommand; } }


		public bool SimSources
		{
			get { return _showSimSources; }
			set { _showSimSources = value; }
		}

		#endregion

		#region EventHandling

		public void WeatherDataChangedHandler(object sender, EventArgs e)
		{
			_weatherData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.Weather, _weatherData, false);

		}
		#endregion


		#region  Presentationsdaten übertragen


		private PresentationsData GeneratePresentationsData()
		{
			PresentationsData data = new PresentationsData
			{
				TimeRange = new TtpTimeRange(new TtpTime("1.1." + Workspace.SimulationYear), TtpEnPattern.Pattern1Year, 1),
				Title = (_showSimSources) ? Workspace.WeatherData.Title + " (für Berechnung aufbereitet)" : Workspace.WeatherData.Title + " (wie eingegeben)",
				TitleToolTip = Workspace.Notes,
				ZoomFactor = 0
			};

			AddWeatherRows(data);
			return data;
		}

		static Color SetTransparency(byte A, Color color)
		{
			return Color.FromArgb(A, color.R, color.G, color.B);
		}

		private void AddWeatherRows(PresentationsData data)
		{
			WeatherData wd = Workspace.WeatherData;
			if(_showSimSources)
			{
				double[] soilTemp = wd.GetSimSoilTemp();
				double[] airTemp = wd.GetSimAirTemp();
				for(int i=0;i<366;i++)
				{ 
					if((i< Workspace.CurrentModel.FirstSimIndex)|| (i > Workspace.CurrentModel.LastSimIndex))
					{
						soilTemp[i] = airTemp[i] = Double.NaN;
					}
				}

				data.AddRow(new PresentationRow
				{
					Legend = "Bodentemperatur [°C]",
					LegendIndex = 0,
					IsVisible = true,
					Thicknes = 1.5,
					Color = Brushes.SandyBrown,
					Values = soilTemp,
					//Values = wd.GetSimSoilTemp(),

					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});

				data.AddRow(new PresentationRow
				{
					Legend = "Lufttemperatur [°C]",
					LegendIndex = 1,
					IsVisible = true,
					Thicknes = 1.5,
					Color = Brushes.DeepPink,
					Values = airTemp,
					//Values = wd.GetSimAirTemp(),

					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});

				if(Workspace.CurrentFlyType == FlyType.PR)
					data.AddRow(new PresentationRow
					{
						Legend = "Niederschlag [mm]",
						LegendIndex = 2,
						IsVisible = true,
						Thicknes = 1.5,
						Color = Brushes.Turquoise,
						Values = wd.GetPrec(),
						Axis = TtpEnAxis.Left,
						LineType = TtpEnLineType.Chart
					});
			}
			else
			{
				data.AddRow(new PresentationRow
				{
					Legend = "Bodentemperatur [°C]",
					LegendIndex = 0,
					IsVisible = true,
					Thicknes = 1.5,
					Color = Brushes.SandyBrown,
					Values = wd.SoilTemps,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});

				data.AddRow(new PresentationRow
				{
					Legend = "Lufttemperatur [°C]",
					LegendIndex = 1,
					IsVisible = true,
					Thicknes = 1.5,
					Color = Brushes.DeepPink,
					Values = wd.AirTemps,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});

				data.AddRow(new PresentationRow
				{
					Legend = "Prognose Bodentemperatur [°C]",
					LegendIndex = 2,
					IsVisible = true,
					Thicknes = 1.5,
					//Color = Brushes.SandyBrown,
					Color =  new SolidColorBrush(SetTransparency(128, Brushes.SandyBrown.Color)),
					Values = wd.PrognSoilTemps,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});

				data.AddRow(new PresentationRow
				{
					Legend = "Prognose Lufttemperatur [°C]",
					LegendIndex = 3,
					IsVisible = true,
					Thicknes = 1.5,
					//Color = Brushes.Plum,
					Color= new SolidColorBrush(SetTransparency(128, Brushes.DeepPink.Color)),
					Values = wd.PrognAirTemps,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});

				data.AddRow(new PresentationRow
				{
					Legend = "Niederschlag [mm]",
					LegendIndex = 4,
					IsVisible = true,
					Thicknes = 1.5,
					Color = Brushes.Turquoise,
					Values = wd.Precs,
					Axis = TtpEnAxis.Right,
					LineType = TtpEnLineType.Chart
				});
				data.AddRow(new PresentationRow
				{
					Legend = "Prognose Niederschlag [mm]",
					LegendIndex = 5,
					IsVisible = true,
					Thicknes = 1.5,
					Color = new SolidColorBrush(SetTransparency(128, Brushes.Turquoise.Color)),
					Values = wd.PrognPrecs,
					Axis = TtpEnAxis.Right,
					LineType = TtpEnLineType.Chart
				});

			}
		}

		#endregion


		#region Methoden  aus Contextmenü


		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.Weather, _weatherData, true);
			printPres.PrintView();
		}



		private void ShowSimSources()
		{
			_weatherData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.Weather, _weatherData, false);
			if (_parentPanel != null)
				_parentPanel.UpdateEventRoutings();
		}

		//private void ShowNotes()
		//{
		//	DlgNotes.Show(Workspace);
		//}

		#endregion
	}
}

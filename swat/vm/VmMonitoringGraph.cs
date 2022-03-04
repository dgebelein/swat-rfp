using swat.iodata;
using swat.views.dlg;
using swatSim;
using SwatPresentations;
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
	class VmMonitoringGraph:VmBase
	{

		#region Variable

		VmBase _parentPanel;
		PresentationsData _monitoringData;
		RelayCommand _printCommand;
		//RelayCommand _notesCommand;

		#endregion

		#region Construction

		public VmMonitoringGraph(VmSwat vmSwat, VmBase parentPanel):base(vmSwat)
      {
			_parentPanel = parentPanel;
			_monitoringData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.Monitoring, _monitoringData, false);
			_printCommand = new RelayCommand(param => this.Print());
			//_notesCommand = new RelayCommand(param => this.ShowNotes());

		}

		#endregion

		#region Properties für Binding

		public ICommand PrintCommand { get { return _printCommand; } }
		//public ICommand NotesCommand { get { return _notesCommand; } }

		#endregion

		#region EventHandling

		public void MonitoringDataChangedHandler(object sender, EventArgs e)
		{
			_monitoringData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.Monitoring, _monitoringData, false);
		}
		#endregion


		#region  Presentationsdaten übertragen


		private PresentationsData GeneratePresentationsData()
		{
			PresentationsData data = new PresentationsData
			{
				TimeRange = new TtpTimeRange(new TtpTime("1.1." + Workspace.SimulationYear), TtpEnPattern.Pattern1Year, 1),
				Title= Workspace.CurrentMonitoringData.Title,
				TitleToolTip = Workspace.Notes,
				ZoomFactor = 0
			};

			AddMonitoringRows(data);
			data.AddMarkers(Workspace.Notes, Workspace.SimulationYear);

			return data;
		}


		private void AddMonitoringRows(PresentationsData data)
		{
			MonitoringData md = Workspace.CurrentMonitoringData;

			data.AddRow(new PresentationRow
				{
					Legend = "Monitoring Eiablage",
					LegendIndex = 0,
					Thicknes = 1.0,
					IsVisible = true,
					Color = Brushes.CornflowerBlue,
					Values = md.Eggs,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.LinePoint
				});

				data.AddRow(new PresentationRow
				{
					Legend = "Monitoring Flug",
					LegendIndex = 1,
					Thicknes = 1.0,
					IsVisible = true,
					Color = Brushes.SpringGreen,
					Values = md.Adults,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.LinePoint
				});
		}

		#endregion

		#region Methoden  aus Contextmenü


		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.Monitoring, _monitoringData, true);
			printPres.PrintView();
		}

		//private void ShowNotes()
		//{
		//	DlgNotes.Show(Workspace);
		//}

		#endregion
	}
}

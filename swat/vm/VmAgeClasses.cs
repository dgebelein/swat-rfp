using swatSim;
using swat.iodata;
using swat.views.dlg;
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
using System.IO;

namespace swat.vm
{
	class VmAgeClasses: VmBase
	{
		#region Variable
		readonly VmBase _parentPanel;
		PresentationsData _acData;
		TtpTime _tmDisplay;

		RelayCommand _printCommand;
		RelayCommand _saveCommand;


		#endregion

		#region Construction

		public VmAgeClasses(VmSwat vmSwat, VmBase parentPanel):base(vmSwat)
      {
			_parentPanel = parentPanel;
			_acData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.AgeClasses, _acData, false);
			_printCommand = new RelayCommand(param => this.Print());
			_saveCommand = new RelayCommand(param => this.SaveAsCsv());

		}

		#endregion

		#region Properties für Binding

		public ICommand PrintCommand { get { return _printCommand; } }
		public ICommand SaveCommand { get { return _saveCommand; } }

		//public ICommand NotesCommand { get { return _notesCommand; } }

		#endregion

		#region EventHandling

		public void RespondToPresentationEvent(object Sender, PresentationEventArgs e)
		{

			if(e.InfoType == EventInfo.InfoPoint && e.InfoPoint.X > 0)
			{
				_tmDisplay = e.DisplayTime;
				_acData = GeneratePresentationsData();
				ViewVisual = PresentationCreator.Create(PresentationType.AgeClasses, _acData, false);
			}
		}

		#endregion

		#region  Präsentationsdaten übertragen

		private PresentationsData GeneratePresentationsData()
		{
			PresentationsData data = new PresentationsData
			{
				DisplayTime = _tmDisplay,
				Title = (_tmDisplay.IsValid) ?$"{Workspace.Location} - Altersklassen für: {_tmDisplay.ToString("dd.MM.yy")} " : 
														"Alterklassen: Mausklick in Populationsdynamik wählt Datum aus",
				ZoomFactor = 0
			};

			AddAgeClasses(data);
			return data;
		}

		private void AddAgeClasses(PresentationsData data)
		{
			PopulationData pop = Workspace.CurrentPopulationData;

			data.AddRow(new PresentationRow
			{
				Legend = "Eier",
				StartIndex = 0,
				IsVisible = true,
				Color = Brushes.CornflowerBlue,
				Values = pop.GetAgeClasses(DevStage.Egg,_tmDisplay.DayOfYear-1),
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.Chart
			});
			data.AddRow(new PresentationRow
			{
				Legend = "Larven",
				StartIndex = 5,
				IsVisible = true,
				Color = Brushes.Crimson,
				Values = pop.GetAgeClasses(DevStage.Larva, _tmDisplay.DayOfYear-1),
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.Chart
			});
			data.AddRow(new PresentationRow
			{
				Legend = "Puppen",
				StartIndex = 15,
				IsVisible = true,
				Color = Brushes.DarkOrange,
				Values = pop.GetAgeClasses(DevStage.Pupa, _tmDisplay.DayOfYear-1),
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.Chart
			});
			data.AddRow(new PresentationRow
			{
				Legend = "Fliegen",
				StartIndex = 25,
				IsVisible = true,
				Color = Brushes.SpringGreen,
				Values = pop.GetAgeClasses(DevStage.Fly, _tmDisplay.DayOfYear-1),
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.Chart
			});
			data.AddRow(new PresentationRow
			{
				Legend = "Aestivation",
				StartIndex = 15,
				IsVisible = true,
				Color = Brushes.Gold,
				Values = pop.GetAgeClasses(DevStage.AestPup, _tmDisplay.DayOfYear-1),
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.Chart
			});
		}


		#endregion

		#region Methoden  aus Contextmenü
		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.AgeClasses, _acData, true);
			printPres.PrintView();
		}

		private void SaveAsCsv()
		{
			PopulationData pop = Workspace.CurrentPopulationData;
			pop.WriteAKToFile(Path.Combine(WorkspaceData.GetPathReports, pop.Title + "Age Classes.csv"));
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swat.views.sheets;
using swat.iodata;
using SwatPresentations;
using System.IO;
using swatSim;
using System.Windows.Media;
using TTP.Engine3;
using TTP.TtpCommand3;
using TTP.UiUtils;
using System.Windows.Input;
using Microsoft.Win32;
using swat.views.dlg;

namespace swat.vm
{
    class VmPopDyn:VmBase
    {
		#region Variable

		RelayCommand _printCommand;
		RelayCommand _notesCommand;

		RelayCommand _saveCommand;
		RelayCommand _toggleSeparateCommand;

		VmBase _parentPanel;
		PresentationsData _popDynData;
		bool _separateGenerations = true;

		#endregion

		
		#region Constructor

		public VmPopDyn(VmSwat vmSwat,VmBase parentPanel):base(vmSwat)
      {
			_parentPanel = parentPanel;
			Workspace.CalculatePopulation();

			_popDynData = GeneratePresentationsData();
			ViewVisual =  PresentationCreator.Create(PresentationType.PopDyn, _popDynData, false);

			_printCommand = new RelayCommand(param => this.Print());
			_notesCommand = new RelayCommand(param => this.ShowNotes());

			_saveCommand = new RelayCommand(param => this.SaveAsCsv());
			_toggleSeparateCommand = new RelayCommand(param => this.ToggleSeparateGenerations());
		}

		#endregion


		#region Properties für Binding

		public ICommand PrintCommand { get { return _printCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }

		public ICommand SaveCommand { get { return _saveCommand; } }
		public ICommand ToggleSeparateCommand { get { return _toggleSeparateCommand; } }


		public bool GenSeparated
		{
			get { return _separateGenerations; }
			set { _separateGenerations = value;}
		}

		#endregion


		#region   Presentationsdaten übertragen

		private TtpTimeRange GetPresentTimerange()
		{
			TtpTime start = new TtpTime("1.1." + Workspace.SimulationYear);
			//Workspace.WeatherData.GetActualTimeSpan().End.DayOfYear-1
			return new TtpTimeRange(start, TtpEnPattern.Pattern1Day, Workspace.WeatherData.GetPrognStartIndex(Workspace.WeatherData.GetActualTimeSpan().End.DayOfYear - 1));
		}


	private PresentationsData GeneratePresentationsData()
		{
			PresentationsData data = new PresentationsData
			{
				TimeRange = new TtpTimeRange(new TtpTime("1.1." + Workspace.SimulationYear), TtpEnPattern.Pattern1Year, 1),
				Title = Workspace.CurrentPopulationData.Title,
				TitleToolTip = Workspace.Notes,
				HighlightTimeRange = GetPresentTimerange()
			};

			if(Workspace.HasValidPopulationData)
				AddStageRows(data);

			return data;
		}

		private void ClonePresentationsData()
		{
			// Zeitraum und Sichtbarkeit übertragen
			TtpTimeRange tr = _popDynData.TimeRange;
			int zf = _popDynData.ZoomFactor;

			Dictionary<string, Boolean> dictVisibilities = new Dictionary<string, bool>();
			for (int i = 0; i < _popDynData.NumRows; i++)
			{
				PresentationRow row = _popDynData.GetRow(i);
				if (dictVisibilities.ContainsKey(row.Legend))
					continue;
				dictVisibilities.Add(row.Legend, row.IsVisible);
			}

			_popDynData = GeneratePresentationsData();
			_popDynData.TimeRange = tr;
			_popDynData.ZoomFactor = zf;
			for (int i = 0; i < _popDynData.NumRows; i++)
			{
				PresentationRow row = _popDynData.GetRow(i);
				row.IsVisible = dictVisibilities[row.Legend];
			}
		}

		private void AddStageGeneration(PresentationsData data, DevStage stage, int generation)
		{
			PopulationData pop = Workspace.CurrentPopulationData;

			double[] rowVals = pop.GetNormalizedRow(stage, generation);
			switch (stage)
			{
				case DevStage.Egg:
					data.AddRow(new PresentationRow
					{
						Legend = "Eier",
						LegendIndex = 0,
						IsVisible = true,
						Thicknes = 1.0,

						Generation = generation,
						Color = Brushes.CornflowerBlue,
						Values = rowVals,
						Axis = TtpEnAxis.Left,
						LineType = TtpEnLineType.Line
					});
					break;
				case DevStage.Larva:
					data.AddRow(new PresentationRow
					{
						Legend = "Larven",
						LegendIndex = 1,
						IsVisible = true,
						Thicknes = 1.0,
						Generation = generation,
						Color = Brushes.Crimson,
						Values = rowVals,
						Axis = TtpEnAxis.Left,
						LineType = TtpEnLineType.Line
					});
					break;
				case DevStage.WiPupa:
				case DevStage.Pupa:
					data.AddRow(new PresentationRow
					{
						Legend = "Puppen",
						LegendIndex = 2,
						IsVisible = true,
						Thicknes = 1.0,
						Generation = generation,
						Color = Brushes.Orange,
						Values = rowVals,
						Axis = TtpEnAxis.Left,
						LineType = TtpEnLineType.Line
					});
					break;
				case DevStage.Fly:
					data.AddRow(new PresentationRow
					{
						Legend = "Fliegen",
						LegendIndex = 3,
						IsVisible = true,
						Thicknes = 1.0,
						Generation = generation,
						Color = Brushes.SpringGreen,
						Values = rowVals,
						Axis = TtpEnAxis.Left,
						LineType = TtpEnLineType.Line
					});
					break;
				case DevStage.NewEgg:
					data.AddRow(new PresentationRow
					{
						Legend = "Eiablage",
						LegendIndex = 4,
						IsVisible = false,
						Thicknes = 1.0,
						Generation = generation,
						Color = Brushes.LightSkyBlue,
						Values = rowVals,
						Axis = TtpEnAxis.Right,
						LineType = TtpEnLineType.AreaDiff
					});
					break;
				case DevStage.ActiveFly:
					data.AddRow(new PresentationRow
					{
						Legend = "aktive Fliegen",
						LegendIndex = 5,
						IsVisible = false,
						Thicknes = 1.0,
						Generation = generation,
						Color = Brushes.LightGreen,
						Values = rowVals,
						Axis = TtpEnAxis.Right,
						LineType = TtpEnLineType.AreaDiff
					});
					break;
				default: break;

			}
		}

		private void  AddStageRows(PresentationsData data)
		{
			if(!_separateGenerations)
			{
				AddStageGeneration(data, DevStage.Egg, -1);
				AddStageGeneration(data, DevStage.Larva, -1);
				AddStageGeneration(data, DevStage.Pupa, -1);
				AddStageGeneration(data, DevStage.Fly, -1);
				AddStageGeneration(data, DevStage.NewEgg, -1);
				AddStageGeneration(data, DevStage.ActiveFly, -1);
			}
			else
			{
				int numGen = Workspace.CurrentPopulationData.GetNumGenerations();
				for(int g = 0;g <= numGen; g++)
				{
					AddStageGeneration(data, DevStage.Egg, g);
					AddStageGeneration(data, DevStage.Larva, g);
					AddStageGeneration(data, DevStage.Pupa, g);
					AddStageGeneration(data, DevStage.Fly, g);
					AddStageGeneration(data, DevStage.NewEgg, g);
					AddStageGeneration(data, DevStage.ActiveFly, g);
				}
			}

		}

		#endregion

		#region Methoden  aus Contextmenü

		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.PopDyn, _popDynData, true);
			printPres.PrintView();
		}

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		private void SaveAsCsv()
		{
			PopulationData pop = Workspace.CurrentPopulationData;
			pop.WritePopToFile(Path.Combine(WorkspaceData.GetPathReports, pop.Title + ".csv"));
		}

		private void ToggleSeparateGenerations()
		{
			ClonePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.PopDyn, _popDynData, false);
			if (_parentPanel != null)
				_parentPanel.UpdateEventRoutings();
		}

		#endregion
	}
}


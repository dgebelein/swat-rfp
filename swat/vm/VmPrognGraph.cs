﻿using swat.iodata;
using swatSim;
using swat.views.dlg;
using SwatPresentations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using TTP.Engine3;
using TTP.TtpCommand3;
using TTP.UiUtils;
//using SwatSim;

namespace swat.vm
{
	class VmPrognGraph : VmBase
	{
		#region Variable

		VmBase _parentPanel;

		Quantor _quantor;
		PresentationsData _graphData;
		EvalMethod _evalMethod;

		//bool _showWeather = false;

		//RelayCommand _toggleShowWeatherCommand;

		RelayCommand _printCommand;
		RelayCommand _notesCommand;
		RelayCommand _saveAsCsvCommand;
		RelayCommand _optimizeRelativeCommand;
		RelayCommand _optimizeAbsoluteCommand;
		RelayCommand _normalizeCommand;
		RelayCommand _saveOptReportCommand;
		//private object _prognosis;


		#endregion

		#region Construction

		public VmPrognGraph(VmSwat vmSwat, VmBase parentPanel) : base(vmSwat)
		{
			_parentPanel = parentPanel;
			Workspace.CalculatePopulation();

			_evalMethod = EvalMethod.Relation;
			_quantor = Quantor.CreateNew(Workspace.CurrentPopulationData,Workspace.CurrentMonitoringData, _evalMethod, true);

			_graphData = GeneratePresentationsData();
			ViewVisual = PresentationCreator.Create(PresentationType.Prognosis, _graphData, false);

			_printCommand = new RelayCommand(param => this.Print());
			_notesCommand = new RelayCommand(param => this.ShowNotes());
			_saveAsCsvCommand = new RelayCommand(param => this.SaveAsCsv());
			//_toggleShowWeatherCommand = new RelayCommand(param => this.ToggleShowWeather());
			_optimizeRelativeCommand = new RelayCommand(param => this.OptimizeRelative(), param => this.CanOptimizeRelative);
			_optimizeAbsoluteCommand = new RelayCommand(param => this.OptimizeAbsolute(), param => this.CanOptimizeAbsolute);
			_normalizeCommand = new RelayCommand(param => this.Normalize(), param => this.CanNormalize);
			_saveOptReportCommand = new RelayCommand(param => this.SaveOptimizationReport(), param => this.CanSaveOptimizationReport);
		}

		#endregion

		#region Properties für Binding



		public ICommand PrintCommand { get { return _printCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }

		public ICommand SaveAsCsvCommand { get { return _saveAsCsvCommand; } }

		public ICommand OptimizeRelativeCommand { get { return _optimizeRelativeCommand; } }
		public ICommand OptimizeAbsoluteCommand { get { return _optimizeAbsoluteCommand; } }
		public ICommand NormalizeCommand { get { return _normalizeCommand; } }
		public ICommand SaveOptReportCommand { get { return _saveOptReportCommand; } }


		bool CanOptimizeRelative { get { return (_evalMethod != EvalMethod.Relation); } }
		bool CanOptimizeAbsolute { get { return (_evalMethod != EvalMethod.AbsDiff); } }
		bool CanNormalize { get { return (_evalMethod != EvalMethod.Nothing); } }
		bool CanSaveOptimizationReport { get { return (_evalMethod != EvalMethod.Nothing); } }
		#endregion

		#region  Presentationsdaten übertragen

		TtpScaleInfo GetFixedScaling(TtpScaleInfo scaleInfo)
		{
			TtpScaleInfo si = scaleInfo;
			si.ScaleMin = si.ActualScaleMin;
			si.ScaleMax = si.ActualScaleMax;
			si.ScaleType = TtpEnScale.Fixed;
			return si;
		}

		private TtpTimeRange GetPresentTimerange()
		{
			TtpTime start = new TtpTime("1.1." + Workspace.SimulationYear);
			if (Workspace.SimulationYear == DateTime.Now.Year)
			{
				int lastIndex = Math.Min(Workspace.CurrentMonitoringData.LastMonitoringIndex, Workspace.WeatherData.GetPrognStartIndex(Workspace.WeatherData.GetActualTimeSpan().End.DayOfYear - 1));
				return new TtpTimeRange(start, TtpEnPattern.Pattern1Day, lastIndex);
			}
			else
				return new TtpTimeRange(start, TtpEnPattern.Pattern1Year, 1);

		}


		private PresentationsData GeneratePresentationsData()
		{
			PresentationsData data = new PresentationsData
			{
				TimeRange = new TtpTimeRange(new TtpTime("1.1." + Workspace.SimulationYear), TtpEnPattern.Pattern1Year, 1),
				//Title = _prognosis.Title,
				Title = _quantor.Title,
				HighlightTimeRange = GetPresentTimerange(),
				ZoomFactor = 0
			};

			AddPrognosisRows(data);
			return data;
		}


		private PresentationsData GeneratePresentationsData(PresentationsData copyData )
		{
			PresentationsData data = new PresentationsData
			{
				TimeRange = copyData.TimeRange,
				//Title = _prognosis.Title,
				Title = _quantor.Title,

				HighlightTimeRange = copyData.HighlightTimeRange,
				ZoomFactor = copyData.ZoomFactor,
				ZoomFactorRight=0,
				LeftAxisInfo = GetFixedScaling(copyData.LeftAxisInfo)
			};

			AddPrognosisRows(data);
			return data;
		}


		private void AddPrognosisRows(PresentationsData data)
		{
			int li = 0;
			if(_quantor.HasEggs)
			{
				data.AddRow(new PresentationRow
				{
					Legend = "Monitoring Eiablage",
					LegendIndex = li++,
					IsVisible = true,
					Thicknes = 1.0,
					Color = Brushes.CornflowerBlue,
					Values = Workspace.CurrentMonitoringData.Eggs,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.LinePoint
				});
				data.AddRow(new PresentationRow
				{
					Legend = "berechnete Eiablage / Prognose",
					LegendIndex = li++,
					IsVisible = true,
					Color = Brushes.LightSkyBlue,
					Values = _quantor.PrognEggs,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.AreaDiff
				});
			}

			if(_quantor.HasAdults)
			{ 
				data.AddRow(new PresentationRow
				{
					Legend = "Monitoring Flug",
					LegendIndex = li++,
					IsVisible = true,
					Thicknes = 1.0,
					Color = Brushes.SpringGreen,
					Values = Workspace.CurrentMonitoringData.Adults,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.LinePoint
				});

				data.AddRow(new PresentationRow
				{
					Legend = "berechneter Flug / Prognose",
					LegendIndex = li++,
					IsVisible = true,
					Color = Brushes.SpringGreen,
					Values = _quantor.PrognAdults,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.AreaDiff
				});
			}

			if(_quantor.HasAdults || _quantor.HasEggs)
			{ 
				data.AddRow(new PresentationRow
				{
					Legend = "berechnete Larven / Prognose",
					LegendIndex = li++,
					IsVisible = false,
					Thicknes = 2.0,
					Color = Brushes.Crimson,
					Values = _quantor.PrognLarvae,
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.Line
				});
			}

			WeatherData wd = Workspace.WeatherData;

			data.AddRow(new PresentationRow
			{
				Legend = "Lufttemperatur [°C]",
				LegendIndex = li++,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.DeepPink,
				Values = wd.GetSimAirTemp(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Line
			});

			data.AddRow(new PresentationRow
			{
				Legend = "Bodentemperatur [°C]",
				LegendIndex = li++,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.SandyBrown,
				Values = wd.GetSimSoilTemp(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Line
			});

			data.AddRow(new PresentationRow
			{
				Legend = "Niederschlag [mm]",
				LegendIndex = li++,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.Turquoise,
				Values = wd.GetPrec(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Chart
			});
		}

		#endregion

		#region Methoden  aus Contextmenü

		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.Prognosis, _graphData, true);
			printPres.PrintView();
		}

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		private void SaveAsCsv()
		{
			string fn = Path.Combine(WorkspaceData.GetPathReports, _quantor.Title + ".csv");
			_quantor.WriteToFile(fn);
		}

		private void ExecChangedOptimization(EvalMethod em)
		{
			_evalMethod = em;
			_quantor = Quantor.CreateNew(Workspace.CurrentPopulationData, Workspace.CurrentMonitoringData, _evalMethod, true);
			_graphData = GeneratePresentationsData(_graphData);
			ViewVisual = PresentationCreator.Create(PresentationType.Prognosis, _graphData, false);

		}

		private void OptimizeRelative()
		{
			ExecChangedOptimization(EvalMethod.Relation);
		}

		private void OptimizeAbsolute()
		{
			ExecChangedOptimization(EvalMethod.AbsDiff);
		}

		private void Normalize()
		{
			ExecChangedOptimization(EvalMethod.Nothing);
		}

		private void SaveOptimizationReport()
		{
			try
			{
				string reportName =  $"{Workspace.Name} {Workspace.CurrentModelName} Report Quantifizierung";

				string fn = Path.Combine(WorkspaceData.GetPathReports, reportName + ".txt");
				_quantor.WriteOptimizationReport(fn);
			}
			catch(Exception e)
			{
				DlgMessage.Show("Quantifizierungs-Report kann nicht gespeichert werden: ", e.Message, SwatPresentations.MessageLevel.Error);
			}
		}
		#endregion
	}
}
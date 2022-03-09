using SwatPresentations;
using swatSim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwopCompare
{
	class VisGenerator
	{
		#region Variables
		double[] _randomNumbers;
		CmpData _data;
		bool _hasEggs;

		int _setIndex;
		Int64 _lastSimIndivCalc;	// Hilfsvariable zum Übertragen der Individuenanzahl;
		double _eval;              // Hilfsvariable zu Übertragen des Eval-Wertes		
		EvalMethod _quantorMethod;


		WeatherData Weather { get { return _data.CompareSets[_setIndex].Weather; } }
		MonitoringData Monitoring { get { return _data.CompareSets[_setIndex].Monitoring; } }

		int StartEval { get { return _data.CompareSets[_setIndex].EvalTimeRange.Start.DayOfYear; } }
		int LastEval { get { return _data.CompareSets[_setIndex].EvalTimeRange.End.DayOfYear; } }
		#endregion

		#region Construction
		public VisGenerator(CmpData cd)
		{
			_data = cd;
			_randomNumbers = new double[10000000]; // Erzeugung von Zufallszahlen-Folge
			Random rand = new Random(96);
			for (int i = 0; i < _randomNumbers.Length; i++)
			{
				_randomNumbers[i] = rand.NextDouble();
			}
			
		}


		#endregion

		#region Visualisation data



		public PresentationsData GeneratePresentationsData(int setId, EvalMethod quantorMethod)
		{
			_setIndex = setId;
			_quantorMethod = quantorMethod;

			PresentationsData pd = new PresentationsData
			{
				Title =   $"{_data.CompareSets[_setIndex].Monitoring.Location}  ({Path.GetFileNameWithoutExtension(_data.CommandFilename)})",
				TitleToolTip = _data.CompareSets[_setIndex].Notes,
				ZoomFactor = 0,
				ZoomFactorRight = 0,
			};

			AddSimTrends(pd);
			AddWeatherTrends(pd);

			int year = Weather.Year;
			pd.TimeRange =  new TtpTimeRange(new TtpTime("1.1." + year), TtpEnPattern.Pattern1Year, 1);
			pd.HighlightTimeRange = _data.CompareSets[_setIndex].EvalTimeRange; 			
			pd.AddMarkers(_data.CompareSets[_setIndex].Notes,year);

			return pd;
		}

		private void AddSimTrends(PresentationsData pd)
		{
			double[] startTrend = GetModelTrend(_data.CompareSets[_setIndex].CommonParams); // zuerst aufrufen, weil hier auf Eier/Adulte umgeschaltet wird
			AddMonitoringRow(pd);
			AddDefaultParamRow(pd, startTrend);
			AddParameterRows(pd);
		}

		private string GetEvalString()
		{
			switch (_quantorMethod)
			{
				case EvalMethod.AbsDiff:	return $"Eval = {_eval.ToString("0.###", CultureInfo.InvariantCulture)} B"; 
				case EvalMethod.Relation:	return $"Eval = {_eval.ToString("0.###", CultureInfo.InvariantCulture)} R";  
				default:							return "normalized generations"; 
			}
		}

		private void AddDefaultParamRow(PresentationsData pd, double[] trend)
		{
			string paraText = _data.CompareSets[_setIndex].CommonParams.GetString(true);

			pd.AddRow(new PresentationRow
			{
				Legend = $"Para: model Default   {GetEvalString()}",
				LegendTooltip = $"Num Indiv: {_lastSimIndivCalc.ToString("N0", CultureInfo.InvariantCulture)}\r\n\n{paraText}",
				Values = trend,
				LegendIndex = 1,
				IsVisible = true,
				Thicknes = 1.0,
				Color = Brushes.LightSkyBlue,
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.AreaDiff
			});
		}

		private void AddParameterRows(PresentationsData pd)
		{
			int numSets = _data.CompareSets[_setIndex].ParamsList.Count;
			for (int i=0; i< numSets; i++)
			{
				double[] trend = GetModelTrend(_data.CompareSets[_setIndex].ParamsList[i]);
				string paraText = _data.CompareSets[_setIndex].ParamsList[i].GetString(true);

				pd.AddRow(new PresentationRow
				{
					Legend = $"Para: {_data.CompareSets[_setIndex].CommentList[i]}  {GetEvalString()}",
					LegendTooltip = $"Num Indiv: {_lastSimIndivCalc.ToString("N0", CultureInfo.InvariantCulture)}\r\n\n{paraText}",
					Values = trend,
					LegendIndex = i+2,
					IsVisible = false,
					Thicknes = 1.0,
					Color = new SolidColorBrush(ColorTools.GetLongRainbow((double)i / numSets)),
					//Color = new SolidColorBrush(ColorTools.GetDifferentColor(i+2)),
					Axis = TtpEnAxis.Left,
					LineType = TtpEnLineType.AreaDiff
				});
			}
		}

		private void AddWeatherTrends(PresentationsData pd)
		{
			int legStartId = pd.NumRows;

			pd.AddRow(new PresentationRow
			{
				Legend = "Air [°C]",
				LegendIndex = legStartId++,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.DeepPink,
				Values = Weather.GetSimAirTemp(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Line
			});

			pd.AddRow(new PresentationRow
			{
				Legend = "Soil [°C]",
				LegendIndex = legStartId++,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.SandyBrown,
				Values = Weather.GetSimSoilTemp(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Line
			});

			pd.AddRow(new PresentationRow
			{
				Legend = "Rain [mm]",
				LegendIndex = legStartId++,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.Turquoise,
				Values = Weather.GetPrec(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Chart
			});
		}

		double[] GetModelTrend(SimParamData optParam)
		{
			ModelBase model = _data.CreateSimulationModel(_data.ModelType, Weather, optParam);
			model.SetRandomNumbers(_randomNumbers);
			model.RunSimulation();
			_lastSimIndivCalc = model.Population.NumIndividuals; 

			Quantor quantor = Quantor.CreateNew(model, model.Population, Monitoring, _quantorMethod, false);
			_hasEggs = quantor.HasEggs;
			DevStage stage = quantor.HasEggs ? DevStage.NewEgg : DevStage.ActiveFly;

			if(_quantorMethod != EvalMethod.Nothing)
				_eval = quantor.GetRemainingError(stage, _quantorMethod, StartEval, LastEval);

			return quantor.PrognValues;
		}


		private void AddMonitoringRow(PresentationsData pd)
		{
			pd.AddRow(new PresentationRow
			{
				Legend = (_hasEggs) ? "Oviposion - Monitoring" : "Flight - Monitoring",
				Values = (_hasEggs) ? Monitoring.Eggs : Monitoring.Adults,
				LegendIndex = 0,
				IsVisible = true,
				Thicknes = 1.0,
				Color = Brushes.CornflowerBlue,
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.LinePoint
			});
		}

		#endregion
	}
}

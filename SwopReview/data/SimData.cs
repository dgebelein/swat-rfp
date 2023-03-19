using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SwatPresentations;
using swatSim;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwopReview
{
	class SimData
	{
		#region Variables
		double[] _randomNumbers;
		SwopData _swopData;
		bool _hasEggs;
		SimParamData _localSetParams;
		SimParamData _commonBestParams;
		SimParamData _setBestParams;

		EvalMethod _quantorMethod;
		int _setIndex;
		Int64 _lastSimIndivCalc; // Hilfsvariable zum Übertragen der Individuenanzahl;
		double _lastEval;       // Hilfsvariable zum Übertragen des Fehlerwertes;
		double _startEval;
		string _lastSimParams;  // Hilfsvariable zum Übertragen des Parameter-Tooltips;
		double[] _startTrend;
		double[] _bestCommonTrend;
		double[] _bestSetTrend;
		double[] _monTrend;


		public WeatherData Weather { get; private set; }
		public MonitoringData Monitoring { get; private set; }
		public string Notes { get; private set; }



		#endregion

		#region Construction
		public SimData(SwopData sd, int setId)
		{
			_swopData = sd;
			_setIndex = setId;
			_quantorMethod = EvalMethod.Relation;

			_randomNumbers = new double[1000000]; // Erzeugung von Zufallszahlen-Folge
			Random rand = new Random(96);
			for (int i = 0; i < _randomNumbers.Length; i++)
			{
				_randomNumbers[i] = rand.NextDouble();
			}

		}


		private ModelBase CreateSimulationModel(WeatherData wd, SimParamData simParam)
		{
			ModelBase model;
			switch (_swopData.ModelType)
			{
				case FlyType.DR: model = new ModelDR("", wd, null, simParam); break;
				case FlyType.PR: model = new ModelPR("", wd, null, simParam); break;
				case FlyType.DA: model = new ModelDA("", wd, null, simParam); break;
				default: throw new Exception("Modelloptimierung für dieses Modell nicht implementiert");
			}

			model.SetRandomNumbers(_randomNumbers);
			return model;
		}

		#endregion


		#region Parameter-Aufbereitung
		SimParamData GetLocalSetParams()
		{
			SimParamData simParams = _swopData.DefaultParameters.Clone();// model.CodedParams.Clone();

			foreach (string p in _swopData.OptSets[_setIndex].LocalParams)
			{
				if (!simParams.ReadFromString(p))
					return null;
			}
			return simParams;
		}

		public SimParamData GetCommonBestParams()
		{
			SimParamData simParams = GetLocalSetParams();

			int p = 0;
			foreach (string param in _swopData.OptParameters)
			{
				string st = $"{param} = {ParamCreator.GetParameterValueString(simParams, param, _swopData.BestParamValues[p++])}";
				if (!simParams.ReadFromString(st))
					return null;
			}
			return simParams;
		}

		SimParamData GetSetBestParams()
		{
			SimParamData simParams = GetLocalSetParams();

			int bestStepSet = _swopData.OptSets[_setIndex].GetBestErrorId();
			if (bestStepSet < 0)
				return null;

			int p = 0;
			foreach (string param in _swopData.OptParameters)
			{
				string st = $"{param} = {ParamCreator.GetParameterValueString(_swopData.DefaultParameters, param, _swopData.OptParamValues[bestStepSet, p])}";
				if (!simParams.ReadFromString(st))
					return null;
				p++;
			}
			return simParams;
		}

		private void AddNotes()
		{
			string notesName = System.IO.Path.Combine(GetPathWorkspace, Path.GetFileNameWithoutExtension(Monitoring.Filename) + " - Notes.txt");

			if (!File.Exists(notesName))
				Notes = "";

			try
			{
				Notes = File.ReadAllText(notesName, Encoding.UTF8);
			}
			catch
			{
				Notes = "";
			}
		}


		#endregion

		#region Modell berechnen

		public string GetPathWorkspace
		{
			get { return _swopData.SwatWorkDir; }

			//get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat"); }
		}

		WeatherData GetWeatherData(string name)
		{
			WeatherData wea = new WeatherData(-1);
			string fn = Path.ChangeExtension(name, ModelBase.GetWeatherExt(_swopData.ModelType));
			return wea.ReadFromFile(Path.Combine(GetPathWorkspace, fn)) ? wea : null;
		}

		MonitoringData GetMonitoringData(string name)
		{

			string fn = Path.ChangeExtension(name, ModelBase.GetMonitoringExt(_swopData.ModelType));
			MonitoringData mon = MonitoringData.CreateNew(Path.Combine(GetPathWorkspace, fn));
			return (mon.ReadFromFile()) ? mon : null;
		}

		public bool AddSimTrends(PresentationsData presData)
		{
			if (!BuildSimSources()) // Erzeugt ggf. Fehlermeldungen
				return false;

			AddTrends(presData);
			return true;
		}

		bool BuildSimSources() // Zwischenspeicherung in klassenvariable weil dadurch spätere Fehlerbehandlung entfallen kann
		{
			Weather = GetWeatherData(_swopData.OptSets[_setIndex].Weather);
			if (Weather == null)
			{
				string msg = $"Error reading weather data for Set {_setIndex}!";
				DlgMessage.Show("Swop-Review - Error", msg, MessageLevel.Error);
				return false;
			}

			Monitoring = GetMonitoringData(_swopData.OptSets[_setIndex].Monitoring);
			if (Monitoring == null)
			{
				string msg = $"Error reading monitoring data for Set {_setIndex}!";
				DlgMessage.Show("Swop-Review - Error", msg, MessageLevel.Error);
				return false;
			}

			_localSetParams = GetLocalSetParams();
			if (_localSetParams == null)
			{
				string msg = $"Error reading local parameters for set {_setIndex}!";
				DlgMessage.Show("Swop-Review - Error", msg, MessageLevel.Error);
				return false;
			}

			_commonBestParams = GetCommonBestParams();
			if (_commonBestParams == null)
			{
				string msg = $"Error reading best common parameters!";
				DlgMessage.Show("Swop-Review - Error", msg, MessageLevel.Error);
				return false;
			}

			_setBestParams = GetSetBestParams();
			if (_setBestParams == null)
			{
				string msg = $"Error reading parameters for set {_setIndex}!";
				DlgMessage.Show("Swop-Review - Error", msg, MessageLevel.Error);
				return false;
			}

			AddNotes();

			return true;
		}

		double[] GetModelTrend(SimParamData optParam, bool saveQuantor= false)
		{
			ModelBase model = CreateSimulationModel(Weather, optParam);
			model.RunSimulation();
			_lastSimIndivCalc = model.Population.NumIndividuals;
			_lastSimParams = optParam.GetString(true);
			Quantor quantor = Quantor.CreateNew(model, model.Population, Monitoring, _quantorMethod, true);
			if ( saveQuantor)
			{
				string s = Path.GetFileNameWithoutExtension(Monitoring.Filename) + "_quanti.txt";
				string fn = Path.Combine(_swopData.SwopWorkDir, s);
				quantor.WriteOptimizationReport(fn);
			}
			//_hasEggs = quantor.HasEggs;

			TtpTimeRange tr = GetEvalTimeSpan();
			int start = tr.Start.DayOfYear - 1;
			int end = tr.End.DayOfYear;

			_lastEval = quantor.GetRemainingError(DevStage.NewEgg, _quantorMethod, start, end);
			return quantor.PrognValues;
		}

		public int GetSimYear()
		{
			return Weather.Year;
		}

		public TtpTimeRange GetEvalTimeSpan()
		{
			string s = _swopData.OptSets[_setIndex].EvalTime;
			string[] dates = s.Split('-');
			TtpTimeRange tr = new TtpTimeRange($"{dates[0].Trim()}.{GetSimYear()}", $"{dates[1].Trim()}.{GetSimYear()}", TtpEnPattern.Pattern1Day);
			return tr;
		}

		public void AddTrends(PresentationsData pd)
		{
			// Trends zwischenspeichern für Datenexport
			_hasEggs = Monitoring.HasEggs;
			_monTrend = (_hasEggs ? Monitoring.Eggs : Monitoring.Adults);
			AddMonitoringRow(pd, _monTrend);

			_startTrend = GetModelTrend(_localSetParams, true);
			AddStartParamRow(pd, _startTrend);
			_bestCommonTrend = GetModelTrend(_commonBestParams);
			AddCommonBestParamRow(pd, _bestCommonTrend);
			_bestSetTrend = GetModelTrend(_setBestParams);
			AddSetBestParamRow(pd, _bestSetTrend);

			AddWeatherRows(pd);

		}

		private void AddMonitoringRow(PresentationsData pd, double[] trend)
		{
			pd.AddRow(new PresentationRow
			{
				Legend = _hasEggs ? "Oviposion - Monitoring" : "Flight - Monitoring",
				Values = trend,
				LegendIndex = 0,
				IsVisible = true,
				Thicknes = 1.0,
				Color = Brushes.CornflowerBlue,
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.LinePoint
			});

		}

		private void AddStartParamRow(PresentationsData pd, double[] trend)
		{
			double err = _swopData.OptSets[_setIndex].StartErrValue;
			_startEval = _lastEval;
			string legend = (_hasEggs) ? "Oviposion - calc with startparams" : "Flight - calc with startparams";
			//legend += $"    Err = {err.ToString("0.###", CultureInfo.InvariantCulture)}";
			legend += $"    Err = {_startEval.ToString("0.###", CultureInfo.InvariantCulture)}";


			pd.AddRow(new PresentationRow
			{
				Legend = legend,
				LegendTooltip = $"Num Indiv: {_lastSimIndivCalc.ToString("### ### ###", CultureInfo.InvariantCulture)}\r\n\n{_lastSimParams}",
				Values = trend,
				LegendIndex = 1,
				IsVisible = true,
				Thicknes = 1.0,
				Color = Brushes.LightSkyBlue,
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.AreaDiff
			});
		}

		private void AddCommonBestParamRow(PresentationsData pd, double[] trend)
		{
			double startErr = _swopData.OptSets[_setIndex].StartErrValue;
			double bestCommonErr1 = _swopData.OptSets[_setIndex].BestErrValue;
			double bestCommonErr = _lastEval;

			string legend = (_hasEggs) ? "Oviposion - calc with common best Params" : "Flight - calc with common best Params";
			legend += $"    Err = {bestCommonErr.ToString("0.###", CultureInfo.InvariantCulture)};   Rel = {(bestCommonErr / _startEval).ToString("0.###", CultureInfo.InvariantCulture)}";

			pd.AddRow(new PresentationRow
			{
				Legend = legend,
				LegendTooltip = $"Num Indiv: {_lastSimIndivCalc.ToString("### ### ###", CultureInfo.InvariantCulture)}\r\n\n{_lastSimParams}",
				Values = trend,
				LegendIndex = 2,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.LawnGreen,
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.AreaDiff
			});
		}

		private void AddSetBestParamRow(PresentationsData pd, double[] trend)
		{
			double startErr = _swopData.OptSets[_setIndex].StartErrValue;
			int step = _swopData.OptSets[_setIndex].GetBestErrorId();
			double bestSetErr1 = _swopData.OptSets[_setIndex].ErrValues[step];
			double bestSetErr = _lastEval;


			string legend = (_hasEggs) ? "Oviposion - calc with best Params for Set" : "Flight - calc with best Params for Set";
			legend += $"     Err = {bestSetErr.ToString("0.###", CultureInfo.InvariantCulture)};   Rel ={(bestSetErr / _startEval).ToString("0.###", CultureInfo.InvariantCulture)}";

			pd.AddRow(new PresentationRow
			{
				Legend = legend,
				Values = trend,
				LegendTooltip = $"Num Indiv: {_lastSimIndivCalc.ToString("### ### ###", CultureInfo.InvariantCulture)}\r\n\n{_lastSimParams}",
				LegendIndex = 3,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.Coral,
				Axis = TtpEnAxis.Left,
				LineType = TtpEnLineType.AreaDiff
			});
		}

		private void AddWeatherRows(PresentationsData pd)
		{
			//WeatherData wd = Weather;

			pd.AddRow(new PresentationRow
			{
				Legend = "Air-Temp [°C]",
				LegendIndex = 4,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.DeepPink,
				Values = Weather.GetSimAirTemp(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Line
			});

			pd.AddRow(new PresentationRow
			{
				Legend = "Soil-Temp [°C]",
				LegendIndex = 5,
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
				LegendIndex = 6,
				IsVisible = false,
				Thicknes = 1.0,
				Color = Brushes.Turquoise,
				Values = Weather.GetPrec(),
				Axis = TtpEnAxis.Right,
				LineType = TtpEnLineType.Chart
			});
		}
		#endregion

		#region Simdaten exportieren

		private void WriteElem(StreamWriter w, double elem)
		{
			string delim = ";";

			if (double.IsNaN(elem))
				w.Write($"{delim}");
			else
				w.Write($"{elem:0.#}{delim}");
		}

		private void WriteToFile(string fileName)
		{
			TtpTimeRange tr = GetEvalTimeSpan();
			int start = tr.Start.DayOfYear - 1;
			int end = tr.End.DayOfYear;
			try
			{
				using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					//Kopfzeile
					sw.WriteLine("Datum;Mon;Progn 0;Progn 1;Progn 2;");

					// Daten						
					TtpTime at = tr.Start;
					for (int d = start; d < end; d++)
					{
						sw.Write(at.ToString("dd.MM.yyyy") + ";");
						WriteElem(sw,_monTrend[d]);
						WriteElem(sw,_startTrend[d]);
						WriteElem(sw, _bestCommonTrend[d]);
						WriteElem(sw, _bestSetTrend[d]);
						sw.WriteLine();
						at.Inc(TtpEnPattern.Pattern1Day, 1);

					}
				}
			}
			catch (Exception ex)
			{
				DlgMessage.Show("Daten können nicht geschrieben werden", ex.Message, MessageLevel.Error);
			}
		}

		public void ExportToCsv()
		{
			string s = Path.GetFileNameWithoutExtension(Monitoring.Filename)+".csv";
			string fn = Path.Combine(_swopData.SwopWorkDir, s);
			WriteToFile(fn);
		}
		

		#endregion
	}
}

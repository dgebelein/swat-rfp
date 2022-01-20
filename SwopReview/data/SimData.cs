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
		int _setIndex;		

		public WeatherData Weather { get; private set; }
		public MonitoringData Monitoring { get; private set; }


		//ModelBase _model;
		//SimParamData _localSetParams;

		#endregion

		#region Construction
		public SimData(SwopData sd, int setId)
		{
			_swopData = sd;
			_setIndex = setId;
			_randomNumbers = new double[10000000]; // Erzeugung von Zufallszahlen-Folge
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
		public bool AddSimTrends(PresentationsData presData)
		{
			if (!BuildSimSources()) // Erzeugt ggf. Fehlermeldungen
				return false;

			AddTrends(presData);
			return true;
		}

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

		SimParamData GetCommonBestParams()
		{
			SimParamData simParams = GetLocalSetParams(); //GetLocalSetParams(model, setId);
			
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
				string st= $"{param} = {ParamCreator.GetParameterValueString(_swopData.DefaultParameters, param, _swopData.OptParamValues[bestStepSet, p])}";
				if (!simParams.ReadFromString(st))
					return null;
				p++;
			}
			return simParams;
		}

		#endregion

		#region Modell berechnen

		public string GetPathWorkspace
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat"); }
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

			return true;
		}

		double[] GetModelTrend(SimParamData optParam)
		{
			ModelBase model = CreateSimulationModel(Weather, optParam);
			model.RunSimulation();
			Quantor quantor = Quantor.CreateNew(model.Population, Monitoring, EvalMethod.Relation, false);
			_hasEggs = quantor.HasEggs;
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
			double[] startTrend = GetModelTrend(_localSetParams); // zuerst aufrufen, weil hier auf Eier/Adulte umgeschaltet wird
			AddMonitoringRow(pd);
			AddStartParamRow(pd, startTrend);
			AddCommonBestParamRow(pd, GetModelTrend(_commonBestParams));
			AddSetBestParamRow(pd, GetModelTrend(_setBestParams));
			AddWeatherRows(pd);

		}

		private void AddMonitoringRow(PresentationsData pd)
		{
			pd.AddRow(new PresentationRow
			{
				Legend = (_hasEggs) ? "Oviposion - Monitoring" : "Flight - Monitoring",
				Values = (_hasEggs) ? Monitoring.Eggs :Monitoring.Adults,
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
			string legend = (_hasEggs) ? "Oviposion - calculated with startparameters" : "Flight - calculated with startparameters";
			legend += $"    Err = {err.ToString("0.###", CultureInfo.InvariantCulture)}";

			pd.AddRow(new PresentationRow
			{
				Legend = legend,
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
			double bestCommonErr = _swopData.OptSets[_setIndex].BestErrValue;
			string legend = (_hasEggs) ? "Oviposion - calculated with common best Parameters" : "Flight - calculated with common best Parameters";
			legend += $"    Err = {bestCommonErr.ToString("0.###", CultureInfo.InvariantCulture)};   Relation = {(bestCommonErr / startErr).ToString("0.###", CultureInfo.InvariantCulture)}";

			pd.AddRow(new PresentationRow
			{
				Legend = legend,
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
			double bestSetErr = _swopData.OptSets[_setIndex].ErrValues[step];

			string legend = (_hasEggs) ? "Oviposion - calculated with best Parameters for Set" : "Flight - calculated with best Parameters for Set";
			legend += $"     Err = {bestSetErr.ToString("0.###", CultureInfo.InvariantCulture)};   Relation={(bestSetErr/startErr).ToString("0.###", CultureInfo.InvariantCulture)}";

			pd.AddRow(new PresentationRow
			{
				Legend = legend,
				Values = trend,
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
				Legend = "Lufttemperatur [°C]",
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
				Legend = "Bodentemperatur [°C]",
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
				Legend = "Niederschlag [mm]",
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
	}
}

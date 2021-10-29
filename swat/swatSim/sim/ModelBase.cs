////using swat.defs;
//using swatSim;
//using swat.iodata;
//using swat.views;
//using swat.views.dlg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace swatSim
{
	public abstract class ModelBase
	{
		#region Variable
		protected double[] _maxDevRates;
		protected double[,] _tableTransition;
		protected double[,] _tableDev;
		protected double[] _tableFert;
		protected double[] _tableFlightAct;
		protected double[] _tableFlightInhib; // Einschränkung Flug/Eiablage durch fehlenden Niederschlag
		protected double[] _tableHatchInhib; // Einschränkung Puppenschlupf durch fehlenden Niederschlag

		protected double[,] _tableMortality;
		protected double[] _tableDiapause;

		protected double[] _transitions;

		protected int[] _maxEggPeriods;

		protected PopulationData _population;
		protected WeatherData _weather;
		protected string _name;

		protected double[] _randomNumbers;
		protected int _randomCount;
		protected int _randomIndex;

		//public WeatherData Weather {get; set; }
		protected int _firstSimIndex;
		protected int _lastSimIndex;
		protected int _prognStartIndex;

		protected double[] _simAirTemps;
		protected double[] _simSoilTemps;
		protected double[] _simPrec;


		//protected  SimParamData _defaultParams;
		protected SimParamData _workingParams;
		//protected string _parameterFile;
		public bool ParaSetOK { get; set; }
		public bool CanSimulate { get; protected set; }

		//WorkspaceData _workspace;

		protected Random _random;
		#endregion


		#region statics
		public static string GetMonitoringExt(ModelType mt)
		{
			switch(mt)
			{
				case ModelType.DR: return ".swat-mdr";
				case ModelType.PR: return ".swat-mpr";
				case ModelType.DA: return ".swat-mda";
				default:throw (new Exception("Modeltype???"));
			}

		}

		public static string GetModelName(ModelType mt)
		{
			switch (mt)
			{
				case ModelType.DR: return "Kohlfliege";
				case ModelType.PR: return "Möhrenfliege";
				case ModelType.DA: return "Zwiebelfliege";
				default: throw (new Exception("Modeltype???"));
			}

		}


		#endregion

		#region Construction + Init

		protected ModelBase( string name, WeatherData weather, PopulationData population, SimParamData explicitParams = null, SimParamData locationParams = null)
		{
			_name = name;
			_weather= weather;
			_population = population;
			//_defaultParams = GetDefaultParams();

			//CanSimulate = InitModelParameters(explicitParams, locationParams);
			InitModelParameters(explicitParams, locationParams); //29.10.20

		}

		public void InitModelParameters(SimParamData explicitParams = null, SimParamData locationParams = null)
		{
			ParaSetOK = true;

			if (explicitParams == null)
			{
				_workingParams = GetDefaultParams().Copy();
			}
			else
			{
				_workingParams = new SimParamData();

				SimParamData defParams = GetDefaultParams().Copy();
				foreach (string key in defParams.ParamDict.Keys)
				{
					_workingParams.AddOrReplaceItem(key, defParams.ParamDict[key]);

					if (locationParams != null && locationParams.ParamDict.ContainsKey(key))
					{
						_workingParams.AddOrReplaceItem(key, locationParams.ParamDict[key]);
						if (locationParams.ParamDict[key].Obj == null)
							ParaSetOK = false;
					}

					if (explicitParams.ParamDict.ContainsKey(key))
					{
						_workingParams.AddOrReplaceItem(key, explicitParams.ParamDict[key]);
						if (explicitParams.ParamDict[key].Obj == null)
							ParaSetOK = false;
					}

				}
			}

			//if ((_weather!=null) && ParaSetOK)
			//	return PrepareWeatherData();
			//else
			//	return false;

			if ((_weather != null) && ParaSetOK)  // 29.10.20
				CanSimulate = PrepareWeatherData();
			else
				CanSimulate = false;
		}

		public void ChangeModelParameters(SimParamData explicitParams = null)
		{
			ParaSetOK = true;

			SimParamData temporary = _workingParams.Copy();
			foreach (string key in temporary.ParamDict.Keys)
			{
				if (explicitParams.ParamDict.ContainsKey(key))
				{
					_workingParams.AddOrReplaceItem(key, explicitParams.ParamDict[key]);
					if (explicitParams.ParamDict[key].Obj == null)
						ParaSetOK = false;
				}
			}

			//if ((_weather != null) && ParaSetOK)
			//	return PrepareWeatherData();
			//else
			//	return false;
			if ((_weather != null) && ParaSetOK)
				 PrepareWeatherData();
		}

		public void SetRandomNumbers(double[] numbers)
		{
			_randomNumbers = numbers;
			_randomCount = numbers.Length;
			_randomIndex = 0;
		}

		public void SetExplicitPopulation(PopulationData popData)
		{
			_population = popData;
		}

		protected void InitTables()
		{
			_maxDevRates = new double[5];
			_tableTransition = new double[5, 1401];
			_tableDev = new double[5, 366];
			_tableFert = new double[1001];
			_tableFlightAct = new double[366];
			_tableFlightInhib = new double[366];
			_tableHatchInhib = new double[366];
			_tableMortality = new double[6, 366]; //6 wegen zusätzl.Larven-Mortalität im Junglarvenstadium



			if(_randomNumbers == null)
			{
				//_randomCount = 10000000;
				//_randomIndex = 0;
				//_randomNumbers = new double[_randomCount];

				double[] rN = new double[10000000];
				Random rand = new Random(96);
				for (int i = 0; i < rN.Length; i++)
				{
					rN[i] = rand.NextDouble();
				}
				SetRandomNumbers(rN);
				//Random rand = new Random(96);
				//for (int i = 0; i < _randomCount;i++)
				//{
				//	_randomNumbers[i] = rand.NextDouble();
				//}
			}

			if (CanSimulate)
			{
				InitTransitions();
				InitTableTransition();
				InitTableDev();
				InitTableFert();
				InitTableFlightAct();
				InitTableMortality();

				CalcMaxEggPeriods();
			}
		}

		public abstract SimParamData GetDefaultParams(); // Initialisierung mit Default-Werten 
		public abstract string GetParamPrefix();
		//public static  string GetMonitoringExt();

		protected abstract int GetStartPopulation();
		public abstract int GetMaxGenerations();
		public abstract bool PrepareWeatherData();
		protected abstract void InitTableTransition();
		protected abstract void InitTableDev();
		protected abstract void InitTableFert();
		protected abstract void InitTableFlightAct();
		protected abstract void InitTableMortality();
		protected abstract void InitTransitions();

		//protected abstract void InitTableDiapause();
		protected abstract void Individual(DevStage startStage, double startAge, int dayIndex, int generation, bool isDiapauseGen);

		protected virtual double GetStartAge() // wird von Zwiebelfliege überschrieben
		{
			return 0.0;
		}

		#region run Simulation


		private  void InitPopulationData()
		{
			if(_population== null)
				_population = new PopulationData(GetMaxGenerations());

			_population.Initialize();
			_population.SetEggPeriods(_maxEggPeriods);

			string popName = (CanSimulate) ?
					Name + " - Population " :
					"keine Wetterdaten für Berechnung der Population";
			_population.Title = popName;
			_population.Year = _weather.Year;
			//}
		}

		private void CalcMaxEggPeriods()
		{
			_maxEggPeriods = new int[366];

			double[] kumDevRates = new double[366];
			double kum = 0.0;
			for (int i=0; i<365; i++)
			{
				kum += GetDevRate(DevStage.Egg, i);
				kumDevRates[i] = kum;
			}

			for(int d=365; d>0; d--)
			{
				int n = 0;
				while ((d - n) > 0 && ((kumDevRates[d] - kumDevRates[d - n]) < 1.0))
				{
					n++;
				}
				_maxEggPeriods[d] = n;
			}
		}

		public virtual void Simulate()
		{
			if (!ParaSetOK)
				return;
			
			InitTables();
			InitPopulationData();

			if (CanSimulate)
			{
				for (int n=0; n < GetStartPopulation(); n++)
				{
					Individual(DevStage.WiPupa, GetStartAge(),_firstSimIndex , 0, false);
				}
				_population.HasValidData = true;
			}
		}

		#endregion

		#region Properties

		public SimParamData WorkingParams
		{
			get { return _workingParams; }
		}

		public SimParamData DefaultParams
		{
			get { return GetDefaultParams().Copy(); }
		}
		#endregion

		public int FirstPossibleSimIndex
		{
			get { return Weather.GetFirstPossibleSimIndex(FirstSimIndex); }
		}

		public int FirstSimIndex
		{
			get { return _firstSimIndex; }
			protected set { _firstSimIndex = value; }
		}

		public int LastSimIndex
		{
			get { return _lastSimIndex; }
			protected set { _lastSimIndex = value; }
		}

		public double GetRandom
		{
			get
			{
				if (_randomNumbers == null)
					return _random.NextDouble();
				else
				{
					_randomIndex++;
					if (_randomIndex >= _randomCount)
						_randomIndex = 0;
					return _randomNumbers[_randomIndex];
				}
			}
		}

		protected WeatherData Weather
		{
			get { return _weather; }
		}

		protected string Name
		{
			get { return _name; }
		}

		public PopulationData Population
		{
			get { return _population; }
			set { _population = value; }
		}

		public int[] MaxEggPeriods
		{
			get { return _maxEggPeriods; }
		}

		public double[] Transitions
		{
			get { return _transitions; }
		}


		#endregion



		#region Report

		public void Report(DevStage stage,int index,int generation, double bioAge, bool inInAest)
		{
			Population.Add(stage, index, generation, bioAge, inInAest);
		}

		#endregion

		#region Datenzugriff Lookup-Tables

		public double GetTransition(DevStage stage, int dayIndex, double bioAge)
		{
			int age = (int)(bioAge * 1000.0);

			if (age >= 1400)
				return 1.0;

			if (age <= 0.0)
				return 0.0;

			double w1 = _tableTransition[(int)stage, age];
			//double w0 = _tableTransition[(int)stage, (int)((bioAge - _tableDev[(int)stage, dayIndex]) * 1000.0)];
			//double g = (w1 - w0) / (1.0 - w0);

			double g = w1 * _tableDev[(int)stage, dayIndex] / _maxDevRates[(int)stage];
			
			return g;
		}

		public double GetFertility(int dayIndex, double bioAge)
		{
			int i1, i2;
			if (bioAge >= 1.0) return (0.0);

			i1 = (int)(bioAge * 1000);
			i2 = (int)((bioAge -_tableDev[(int) DevStage.Fly,dayIndex]) * 1000);
			return _tableFert[i1] - _tableFert[i2];
		}

		public double GetMortality(DevStage stage, int dayIndex)
		{
			return _tableMortality[(int)stage, dayIndex];
		}

		public double GetDevRate(DevStage stage, int dayIndex)
		{
			return _tableDev[(int)stage, dayIndex];
		}

		public double GetFlightAct(int dayIndex)
		{
			return _tableFlightAct[dayIndex];
		}

		public double GetHatchInhib(int dayIndex)
		{
			return _tableHatchInhib[dayIndex];
		}

		public double GetFlightInhib(int dayIndex)
		{
			return _tableFlightInhib[dayIndex];
		}

		public double GetSoilTemp(int dayIndex)
		{
			return _simSoilTemps[dayIndex]; 
		}

		public double GetAirTemp(int dayIndex)
		{
			return _simAirTemps[dayIndex];
		}

		//public double GetVPD(int dayIndex)
		//{
		//	return _simPrec[dayIndex];
		//}

		#endregion

	}
}

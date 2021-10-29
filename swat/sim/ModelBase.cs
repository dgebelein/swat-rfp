using swat.defs;
using swat.iodata;
using swat.views;
using swat.views.dlg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace swat.sim
{
	public abstract class ModelBase
	{
		#region Variable
		protected double[] _maxDevRates;
		protected double[,] _tableTransition;
		protected double[,] _tableDev;
		protected double[] _tableFert;
		protected double[] _tableFlightAct;
		protected double[] _tableVpdRestr; // Einschränkung durch Sättigungsdefizit
		protected double[,] _tableMortality;
		protected double[] _tableDiapause;

		protected int[] _maxEggPeriods;

		protected PopulationData _population;

		//public WeatherData Weather {get; set; }
		protected int _firstSimIndex;
		protected int _lastSimIndex;
		protected int _prognStartIndex;

		protected double[] _simAirTemps;
		protected double[] _simSoilTemps;
		//protected double[] _simWindSpeeds;
		protected double[] _simVPD;


		//protected  SimParamData _defaultParams;
		protected SimParamData _workingParams;
		//protected string _parameterFile;
		public bool ParaSetOK { get; set; }
		public bool CanSimulate { get; protected set; }

		WorkspaceData _workspace;

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


		#endregion

		#region Construction + Init

		protected ModelBase(WorkspaceData workspace, SimParamData explicitParams = null)
		{
			_workspace = workspace;
			//_defaultParams = GetDefaultParams();
			CanSimulate = InitModelParameters(explicitParams);
		}



		public bool InitModelParameters(SimParamData explicitParams = null)
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
					if (explicitParams.ParamDict.ContainsKey(key))
					{
						_workingParams.AddOrReplaceItem(key, explicitParams.ParamDict[key]);
						if (explicitParams.ParamDict[key].Obj == null)
							ParaSetOK = false;
					}
				}
			}

			if (_workspace.HasValidWeatherData && ParaSetOK)
				return PrepareWeatherData();
			else
				return false;

		}

		public bool ChangeModelParameters(SimParamData explicitParams = null)
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

			if (_workspace.HasValidWeatherData && ParaSetOK)
				return PrepareWeatherData();
			else
				return false;

		}

		public SimParamData WorkingParams
		{
			get { return _workingParams; }
		}

		public SimParamData DefaultParams
		{
			get{ return GetDefaultParams().Copy();}
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
			_tableVpdRestr = new double[366];
			//_tableWindRestr = new double[366];
			_tableMortality = new double[6, 366]; //6 wegen zusätzl.Larven-Mortalität im Junglarvenstadium

			_tableDiapause = new double[366];


			_random = new Random(96); // hier Random mit Startwert initialisieren, damit reproduzierbare Zufallszahlen erzeugt werden!

			//CanSimulate = PrepareWeatherData();
			if (CanSimulate)
			{ 
				InitTableTransition();
				InitTableDev();
				InitTableFert();
				InitTableFlightAct();
				InitTableMortality();
				//InitTableDiapause();

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
		//protected abstract void InitTableDiapause();
		protected abstract void Individual(DevStage startStage, double startAge, int dayIndex, int generation, bool isDiapauseGen);

		#region run Simulation

		private  void InitPopulationData()
		{
			if (_population == null)
			{
				_workspace.CurrentPopulationData = new PopulationData(GetMaxGenerations())
				{
					Year = _workspace.SimulationYear,
					Title = (CanSimulate) ?
						 //Path.GetFileNameWithoutExtension(_workspace.Name)+" "+ _workspace.CurrentModelName+ " - Population ":
						(_workspace.Name) + " " + _workspace.CurrentModelName + " - Population " :

						"keine Wetterdaten für Berechnung der Population"
				};
				_population = _workspace.CurrentPopulationData;
				_population.SetEggPeriods(_maxEggPeriods);
			}
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
					Individual(DevStage.WiPupa, 0.0,_firstSimIndex , 0, false);
				}
				_workspace.HasValidPopulationData = true;
			}
		}

		#endregion

		#region Properties

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
			get { return _random.NextDouble(); }
		}

		protected WeatherData Weather
		{
			get { return _workspace.WeatherData; }
		}

		protected PopulationData Population
		{
			get { return _population; }
			set { _population = value; }
		}

		public int[] MaxEggPeriods
		{
			get { return _maxEggPeriods; }
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

		//public double GetWindRestr(int dayIndex)
		//{
		//	return _tableWindRestr[dayIndex];
		//}

		public double GetVpdRestr(int dayIndex)
		{
			return _tableVpdRestr[dayIndex];
		}

		public double GetSoilTemp(int dayIndex)
		{
			return _simSoilTemps[dayIndex]; 
		}

		public double GetAirTemp(int dayIndex)
		{
			return _simAirTemps[dayIndex];
		}

		public double GetVPD(int dayIndex)
		{
			return _simVPD[dayIndex];
		}

		#endregion

	}
}

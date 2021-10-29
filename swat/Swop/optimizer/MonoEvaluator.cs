//using Microsoft.SolverFoundation.Solvers;
////using Swop.data;
////using Swop.defs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using swatSim;

//namespace Swop.optimizer
//{
//	public class MonoEvaluator
//	{
//		#region variable

//		MonitoringData _md;
//		PopulationData _pd;

//		double[] _prognAdults;
//		double[] _prognEggs;

//		int[] _maxEggMonitoringPeriods;
//		int[] _eggMonitoringPeriods;
//		int[] _adultMonitoringPeriods;

//		int _numMonitoringsEggs;
//		int _numMonitoringsAdults;
//		bool _hasEggs;
//		bool _hasAdults;
//		//int _evalStep;

//		// Variable für Optimierung
//		int _noChangeCounter;
//		double _bestEvalValue;
//		double _lastEvalValue;
//		//double[] _bestParameters;

//		int _firstEvalIndex;
//		int _lastEvalIndex;
//		public bool UseRelationEval { get; set; }
//		public string Title { get; set; }
//		public string QuantReportName { get; set; }

//		#endregion

//		#region Constructor

//		private MonoEvaluator()
//		{
//			_prognAdults = new double[366];
//			_prognEggs = new double[366];
//			Init();
//		}

//		public static MonoEvaluator CreateNew(MonitoringData monitoring, int firstIndex, int lastIndex)
//		{
//			MonoEvaluator evaluator = new MonoEvaluator
//			{
//				_md = monitoring,
//			};

//			evaluator._firstEvalIndex = firstIndex;
//			evaluator._lastEvalIndex = lastIndex;
//			evaluator.UseRelationEval = true;

//			evaluator._hasEggs = evaluator.HasMonEggs();
//			evaluator._hasAdults = evaluator.HasMonAdults();
//			//evaluator.CalculateMonitoringPeriods();

//			return evaluator;
//		}

//		void Init()
//		{
//			for (int i = 0; i < 366; i++)
//			{
//				_prognAdults[i] = double.NaN;
//				_prognEggs[i] = double.NaN;
//			}
//		}

//		#endregion

//		#region Properties

//		public double BestEvalValue
//		{
//			get { return _bestEvalValue; }
//		}

//		public bool HasEggs
//		{
//			get { return _hasEggs; }
//		}

//		public bool HasAdults
//		{
//			get { return _hasAdults; }
//		}

//		//public int EvalStep
//		//{
//		//	get { return _evalStep; }
//		//}

//		public double[] MonitoringValues
//		{
//			get { return (HasEggs) ? _md.Eggs : _md.Adults; }
//		}

//		public double[] PrognValues
//		{
//			get { return (HasEggs) ? _prognEggs : _prognAdults; }
//		}

//		#endregion

//		#region Prognose berechnen

//		void CalcMonitoringNums()
//		{
//			_numMonitoringsEggs = 0;
//			_numMonitoringsAdults = 0;

//			for (int i = _firstEvalIndex; i < _lastEvalIndex; i++)
//			{
//				if (!double.IsNaN(_md.Eggs[i]) && (_md.Eggs[i] >= 0))
//					_numMonitoringsEggs++;
//				if (!double.IsNaN(_md.Adults[i]) && (_md.Adults[i] >= 0))
//					_numMonitoringsAdults++;
//			}
//		}

//		bool HasMonEggs()
//		{
//			for (int i = 0; i < _md.Eggs.Length; i++)
//			{
//				if (double.IsNaN(_md.Eggs[i]))
//					continue;
//				if (_md.Eggs[i] >= 0.0)
//					return true;
//			}
//			return false;
//		}

//		bool HasMonAdults()
//		{
//			for (int i = 0; i < _md.Adults.Length; i++)
//			{
//				if (double.IsNaN(_md.Adults[i]))
//					continue;
//				if (_md.Adults[i] >= 0.0)
//					return true;
//			}
//			return false;
//		}

//		int[] GetMonitoringPeriods(double[] monValues, int maxLength)
//		{
//			int[] monRanges = new int[366];
//			int rangeLength = 0;
//			for (int i = 0; i < monValues.Length; i++)
//			{
//				if (double.IsNaN(monValues[i]))
//				{
//					rangeLength++;
//					monRanges[i] = 0;
//				}
//				else
//				{
//					if (monValues[i] < 0.0)
//					{
//						rangeLength = 0;
//						monRanges[i] = -1;
//					}
//					else
//					{
//						monRanges[i] = Math.Min(rangeLength + 1, maxLength);
//						rangeLength = 0;
//					}
//				}
//			}
//			return monRanges;
//		}

//		private int[] GetMonitoringPeriods(double[] monValues, int[] maxEggPeriods)
//		{
//			int[] monRanges = new int[366];
//			int rangeLength = 0;
//			for (int i = 0; i < monValues.Length; i++)
//			{
//				if (double.IsNaN(monValues[i]))
//				{
//					rangeLength++;
//					monRanges[i] = 0;
//				}
//				else
//				{
//					if (monValues[i] < 0.0)
//					{
//						monRanges[i] = -1;
//						rangeLength = 0;
//					}
//					else
//					{
//						monRanges[i] = Math.Min(rangeLength + 1, maxEggPeriods[i]);
//						rangeLength = 0;
//					}
//				}
//			}
//			return monRanges;
//		}

//		double GetRangeSum(double[] popValues, int index, int length)
//		{
//			double sum = 0.0;
//			for (int i = 0; i < length; i++)
//			{
//				sum += popValues[index - i];
//			}
//			return sum;
//		}

//		void CalcPrognDateValues(int[] monRanges, double[] popValues, double[] prognValues)
//		{
//			for (int i = 0; i < monRanges.Length; i++)
//			{
//				prognValues[i] = (monRanges[i] > 0) ? GetRangeSum(popValues, i, monRanges[i]) : double.NaN;
//			}

//		}

//		double[] GetPrognValues(int[] monRanges, double[] popValues)
//		{
//			double[] prognValues = new double[366];

//			for (int i = 0; i < 366; i++)
//			{
//				if (monRanges[i] < 0) //Start Monitoring
//					prognValues[i] = -1.0;
//				else
//					prognValues[i] = (monRanges[i] > 0) ? GetRangeSum(popValues, i, monRanges[i]) : double.NaN;
//			}
//			return prognValues;
//		}

//		void CalculateMonitoringPeriods()
//		{
//			if (_hasEggs)
//			{
//				_eggMonitoringPeriods = GetMonitoringPeriods(_md.Eggs, _maxEggMonitoringPeriods);
//			}

//			if (_hasAdults)
//			{
//				_adultMonitoringPeriods = GetMonitoringPeriods(_md.Adults, 10);
//			}

//		}

//		#endregion

//		#region Evaluierung + Optimierung

//		double EvalFkt(double prognVal, double monVal)
//		{
//			if (UseRelationEval) // Fehlerrelationen
//			{
//				prognVal += 1.0;
//				monVal += 1.0;

//				double result = (prognVal > monVal) ? (prognVal / monVal) : (monVal / prognVal);
//				return result;// - 1.0; 
//			}
//			else //Fehlerbeträge
//			{
//				return Math.Abs(prognVal - monVal);
//			}
//		}


//		void TransmitBestScales(double evalResult, double[] scales)
//		{
//			if (double.IsNaN(_bestEvalValue))
//			{
//				_bestEvalValue = evalResult;
//				_lastEvalValue = evalResult;
//				_noChangeCounter = 0;
//			}
//			else
//			{
//				if (evalResult <= _bestEvalValue)
//				{
//					_bestEvalValue = evalResult;
//				}
//			}
//		}

//		bool CheckForTermination(double evalResult, double[] generationScales)
//		{
//			TransmitBestScales(evalResult, generationScales);

//			double diff = Math.Abs(_lastEvalValue - evalResult);
//			_lastEvalValue = evalResult;
//			if (diff < 0.001)
//			{
//				int maxCounter = generationScales.Length * generationScales.Length;//2;
//				if (++_noChangeCounter > maxCounter)
//					return true;
//			}
//			else
//				_noChangeCounter = 0;

//			return false;
//		}

//		double EvalNormalizationScales(DevStage stage, double[] normScales, int firstEvalIndex, int lastEvalIndex)
//		{
//			double evalResult = 0.0;
//			int numPairs = 0;

//			// normFactors enthalten Multiplikatoren ab 1. Generation
//			// Prognoseberechnung braucht aber auch Multiplikator für Überwinterungsgeneration
//			double[] popFactors = new double[normScales.Length + 1];
//			popFactors[0] = 1.0;
//			for (int i = 0; i < normScales.Length; i++)
//				popFactors[i + 1] = normScales[i];

//			double[] progn;
//			double[] monitoring;

//			CalculateMonitoringPeriods(); // neu 9.3.

//			if (stage == DevStage.NewEgg)
//			{
//				_prognEggs = GetPrognValues(_eggMonitoringPeriods, _pd.GetNormalizedRow(DevStage.NewEgg, -1, popFactors));
//				monitoring = _md.Eggs;
//				progn = _prognEggs;
//			}
//			else
//			{
//				_prognAdults = GetPrognValues(_adultMonitoringPeriods, _pd.GetNormalizedRow(DevStage.ActiveFly, -1, popFactors));
//				monitoring = _md.Adults;
//				progn = _prognAdults;
//			}

//			for (int i = firstEvalIndex; i < lastEvalIndex; i++)
//			{
//				if (!double.IsNaN(progn[i]) && (progn[i] >= 0.0))
//				{
//					evalResult += EvalFkt(progn[i], monitoring[i]);
//					numPairs++;
//				}
//			}
//			double result = evalResult / numPairs;

//			if (CheckForTermination(result, normScales))
//				throw new Exception("OptimizationBreak");

//			return result;
//		}

//		double[] IsolateNeccessaryFactors(double[] factors)
//		{
//			List<double> factorList = new List<double>();

//			for (int g = 1; g < factors.Length; g++)// Faktor für  Überwinterungsgeneration nicht antasten 
//			{
//				int si = _pd.GenerationStartIndex(DevStage.NewEgg, g);
//				if (si > 0 && si < 365)
//					factorList.Add(factors[g]);
//			}

//			return factorList.ToArray();
//		}

//		// ermittelt die optimalen Multiplikatoren für die einzelnen Generationen und berechnet nebenher den besten Fehlerwert
//		void CalcEvalValue()
//		{
//			double[] workingFactors = IsolateNeccessaryFactors(_pd.NormalisationFactors); // Faktoren ab 1. Gen
//			double[] factorsLowLimit = new double[workingFactors.Length];
//			double[] factorsHighLimit = new double[workingFactors.Length];

//			for (int g = 0; g < workingFactors.Length; g++)
//			{
//				factorsLowLimit[g] = workingFactors[g] / 100.0;
//				factorsHighLimit[g] = workingFactors[g] * 100.0;
//			}

//			//_evalStep = 0;

//			CalcMonitoringNums();
//			DevStage stageOptimized = (_hasEggs) ? DevStage.NewEgg : DevStage.ActiveFly;

//			_noChangeCounter = 0;
//			EvalNormalizationScales(stageOptimized, workingFactors, _firstEvalIndex, _lastEvalIndex); // hier - die 0 ersetzen

//			_bestEvalValue = double.NaN;
//			double[] optimizedFactors = new double[_pd.NormalisationFactors.Length];

//			try
//			{
//				var solution = NelderMeadSolver.Solve(
//				x => (EvalNormalizationScales(stageOptimized, x, _firstEvalIndex, _lastEvalIndex)), workingFactors, factorsLowLimit, factorsHighLimit); // hier - die 0 ersetzen
//			}
//			catch (Exception)
//			{
//				return;
//			}
//		}


//		#endregion

//		#region Methoden

//		public double GetResidual(PopulationData pop, int[] maxEggPeriods)
//		{
//			_pd = pop;
//			_maxEggMonitoringPeriods = maxEggPeriods;

//			CalcEvalValue(); // berechnet nebenher den besten Evaluierungswert
//			if (double.IsNaN(_bestEvalValue))
//				return 99999.0;               // für debug
//			else
//				return _bestEvalValue;
//		}

//		#endregion
//	}
//}

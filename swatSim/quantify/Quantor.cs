using Microsoft.SolverFoundation.Solvers;
using SwatPresentations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace swatSim
{

	// Quantor soll für die Skalierung der Generationwn mit Eval-Method  AbsDiff arbeiten (in CreateNew)
	//						für Bewertung für Parameteroptimierung sollte dagegen  Relation besser sein (in GetRemainingError)
	public class Quantor
	{
		#region Var

		// input data
		ModelBase _model;
		MonitoringData _md;
		PopulationData _pd;
		EvalMethod _evalMethod;

		double[] _prognAdults;
		double[] _prognEggs;
		double[] _prognLarvae;
		int[] _eggMonitoringPeriods;
		int[] _adultMonitoringPeriods;
		bool _hasEggs;
		bool _hasAdults;
		int _lastQuantIndex;

		// work data
		int _evalStep;
		double _bestEvalValue;
		double[] _bestScalingFactors;
		double[] _optimizedFactors;

		// work data for optimisation
		int _noChangeCounter;
		double _lastEvalValue;

		// report
		bool _doCreateReport;
		StringBuilder _optReport;

		// public variable
		public string Title { get; private set; }
		public int FirstEvalIndex { get; set; }
		public int LastEvalIndex { get; set; }
		public bool UseRelationEval { get; set; }

		#endregion

		#region Construction

		private Quantor()
		{
			_prognAdults = new double[366];
			_prognEggs = new double[366];
			_prognLarvae = new double[366];

			if (_doCreateReport)
				_optReport = new StringBuilder(4000);

			for (int i = 0; i < 366; i++)
			{
				_prognAdults[i] = double.NaN;
				_prognEggs[i] = double.NaN;
				_prognLarvae[i] = double.NaN;
			}
		}

		public static Quantor CreateNew(ModelBase model, PopulationData pd, MonitoringData md, EvalMethod evalMethod, bool doCreateReport = false)
		{
			MonitoringData monData = (pd.Year == DateTime.Now.Year) ? md.GetWithVirtualMonitoring() : md;

			Quantor quantor = new Quantor
			{
				_model=model,
				_pd = pd,
				_md = monData,
				_evalMethod = evalMethod,
				_lastQuantIndex = (monData.FirstVirtMonitoring > 0) ? monData.FirstVirtMonitoring - 1 : 366
			};

			quantor._doCreateReport = doCreateReport;
			if (doCreateReport)
				quantor._optReport = new StringBuilder(32000);


			quantor._hasEggs = quantor.HasMonEggs();
			quantor._hasAdults = quantor.HasMonAdults();
			quantor.CalculateMonitoringPeriods();
			quantor.BuildTitle();

			double[] scalingFaktors = (evalMethod == EvalMethod.Nothing) ?
				pd.NormalisationFactors :
				quantor.GetOptimizedScalingFactors(pd.NormalisationFactors);

			quantor.CalculatePrognosis(scalingFaktors);

			return quantor;
		}

		private void BuildTitle()
		{
			string optMethodText = "";
			switch (_evalMethod)
			{
				case EvalMethod.Nothing: optMethodText = "N"; break;
				case EvalMethod.AbsDiff: optMethodText = "B"; break;
				case EvalMethod.Relation: optMethodText = "R"; break;
			}

			Title = (_hasEggs || _hasAdults) ?
				$"Vergleich-Prognose {ModelBase.GetModelName(_md.Model)} {_md.Location} {_md.Year} {optMethodText}" :
				"ohne Daten aus Monitoring keine quantitative Prognose möglich";
		}

		#endregion

		#region Properties

		public bool HasEggs
		{
			get { return _hasEggs; }
		}

		public bool HasAdults
		{
			get { return _hasAdults; }
		}

		public double[] MonEggs
		{
			get { return _md.Eggs; }
		}

		public double[] MonAdults
		{
			get { return _md.Adults; }
		}

		public double[] PrognEggs
		{
			get { return _prognEggs; }
		}

		public double[] PrognAdults
		{
			get { return _prognAdults; }
		}

		public double[] PrognLarvae
		{
			get { return _prognLarvae; }
		}

		public double[] MonitoringValues
		{
			get { return (HasEggs) ? _md.Eggs : _md.Adults; }
		}

		public double[] PrognValues
		{
			get { return (HasEggs) ? _prognEggs : _prognAdults; }
		}

		public double Residual
		{
			get { return _bestEvalValue; }
		}

		public double[] ScalingFactors
		{
			get {return _bestScalingFactors; }
		}

		public string OptimizationReport
		{
			get
			{
				if (!_doCreateReport)
					return null;
				else
					return (_optReport.Length > 0) ? _optReport.ToString() : null;
			}
		}

		#endregion

		#region Methods
		private bool HasMonEggs()
		{
			for (int i = 0; i < _md.Eggs.Length; i++)
			{
				if (double.IsNaN(_md.Eggs[i]))
					continue;
				if (_md.Eggs[i] >= 0.0)
					return true;
			}
			return false;
		}

		private bool HasMonAdults()
		{
			for (int i = 0; i < _md.Adults.Length; i++)
			{
				if (double.IsNaN(_md.Adults[i]))
					continue;
				if (_md.Adults[i] >= 0.0)
					return true;
			}
			return false;
		}
		
		private int[] GetMonitoringPeriods(double[] monValues, int maxLength)
		{
			int[] monRanges = new int[366];
			int rangeLength = 0;
			for (int i = 0; i < monValues.Length; i++)
			{
				if (double.IsNaN(monValues[i]))
				{
					rangeLength++;
					monRanges[i] = 0;
				}
				else
				{
					if (monValues[i] < 0.0)
					{
						rangeLength = 0;
						monRanges[i] = -1;
					}
					else
					{
						monRanges[i] = Math.Min(rangeLength + 1, maxLength);
						rangeLength = 0;
					}
				}
			}
			return monRanges;
		}

		private int[] GetMonitoringPeriods(double[] monValues, int[] maxEggPeriods)
		{
			int[] monRanges = new int[366];
			int rangeLength = 0;
			for (int i = 0; i < monValues.Length; i++)
			{
				if (double.IsNaN(monValues[i]))
				{
					rangeLength++;
					monRanges[i] = 0;
				}
				else
				{
					if (monValues[i] < 0.0)
					{
						monRanges[i] = -1;
						rangeLength = 0;
					}
					else
					{
						monRanges[i] = Math.Min(rangeLength + 1, maxEggPeriods[i]);
						rangeLength = 0;
					}
				}
			}
			return monRanges;
		}

		private double GetRangeSum(double[] popValues, int index, int length, double ovipSurv)
		{
			//double sum = 0.0;
			//if(ovipSurv <1.0)
			//{ 
			//	for (int i = 0; i < length; i++)
			//	{
			//		double stillThere= Math.Pow(ovipSurv,i) * popValues[index - i];
			//		sum += stillThere;
			//	}
			//}
			//else
			//{
			//	for (int i = 0; i < length; i++)
			//	{
			//		sum += popValues[index - i];
			//	}

			//}
			//return sum;
			//  nur bis zum Vortag des Monitoringtermins integrieren
			// Eiablagen oder Fänge am Monitoringtermin selbst sind erst beim nächsten Termin zu finden 
			double sum = 0.0;
			if (ovipSurv < 1.0)
			{
				for (int i = 1; i <= length; i++)
				{
					double stillThere = Math.Pow(ovipSurv, i) * popValues[index - i];
					sum += stillThere;
				}
			}
			else
			{
				for (int i = 1; i <= length; i++)
				{
					sum += popValues[index - i];
				}

			}
			return sum;
		}

		private double[] GetPrognValues(int[] monRanges, double[] popValues, bool withVirtMonitorings = false)
		{
			double[] prognValues = new double[366];
			int lastIndex = (withVirtMonitorings) ? 365 : _lastQuantIndex;

			double ovipSurv =(_hasEggs)?  _model.GetEggOvipProb() :1.0; // Wahrscheinlichkeit; dass ein abgelegtes Ei auch am nächsten Tag noch gefunden werden kann

			for (int i = 0; i < lastIndex; i++)
			{
				if (monRanges[i] < 0) //Start Monitoring
					prognValues[i] = -1.0;
				else
					prognValues[i] = (monRanges[i] > 0) ? GetRangeSum(popValues, i, monRanges[i], ovipSurv) : double.NaN;
			}
			prognValues[365] = double.NaN;
			return prognValues;
		}

		protected void CalculateMonitoringPeriods()
		{
			if (HasEggs)
			{
				_eggMonitoringPeriods = GetMonitoringPeriods(_md.Eggs, _pd.MaxEggPeriods); // Begrenzung des Integrations-Zeitraums auf die Entwicklungsdauer der Eier
			}

			if (HasAdults)
			{
				_adultMonitoringPeriods = GetMonitoringPeriods(_md.Adults, 14); // bei Gelbtafeln: max 14 Tage
			}
		}

		double[] GetOptimizedScalingFactors(double[] oldScalingFactors)
		{
			double[] workingFactors = IsolateNeccessaryFactors(oldScalingFactors); // Faktoren ab 1. Gen
			double[] factorsLowLimit = new double[workingFactors.Length];
			double[] factorsHighLimit = new double[workingFactors.Length];

			for (int g = 0; g < workingFactors.Length; g++)
			{
				factorsLowLimit[g] = workingFactors[g] / GetMaxScaler(g + 1);
				factorsHighLimit[g] = workingFactors[g] * GetMaxScaler(g + 1);
			}

			_evalStep = 0;
			bool earlyTermination = false;

			ReportOptimizationHeader(workingFactors.Length); 

			DevStage stageOptimized = (_hasEggs) ? DevStage.NewEgg : DevStage.ActiveFly;

			_bestScalingFactors = new double[workingFactors.Length];

			GetEvalValue(stageOptimized, workingFactors, _lastQuantIndex); // wegen Report

			_noChangeCounter = 0;
			_bestEvalValue = double.NaN;
			_optimizedFactors = new double[oldScalingFactors.Length];

			var watch = System.Diagnostics.Stopwatch.StartNew();
			try
			{
				var solution = NelderMeadSolver.Solve(
				x => (GetEvalValue(stageOptimized, x, _lastQuantIndex)), workingFactors, factorsLowLimit, factorsHighLimit);

				_optimizedFactors[0] = 1.0; // Skalierung für Überwinterungsgeneration wieder einfügen
				for (int g = 0; g < workingFactors.Length; g++)
				{
					_optimizedFactors[g + 1] = solution.GetValue(g + 1);
				}
			}
			catch (Exception)
			{
				_optimizedFactors[0] = 1.0; // Skalierung für Überwinterungsgeneration wieder einfügen 
				for (int g = 0; g < _bestScalingFactors.Length; g++)
				{
					_optimizedFactors[g + 1] = _bestScalingFactors[g];
				}
				earlyTermination = true;

			}
			watch.Stop();
			var calculationTime = watch.ElapsedMilliseconds;

			ReportOptimizationResult(_bestEvalValue, _optimizedFactors, workingFactors.Length, calculationTime, earlyTermination);

			return _optimizedFactors;
		}

		double[] IsolateNeccessaryFactors(double[] factors)
		{
			List<double> factorList = new List<double>();

			for (int g = 1; g < factors.Length; g++)// Faktor für  Überwinterungsgeneration nicht antasten 
			{
				if (g == _pd.GetNumGenerations())
				{
					int si = _pd.GenerationStartIndex(DevStage.NewEgg, g);
					if (si > 0 && si < _lastQuantIndex)
						factorList.Add(factors[g]);
				}
				else
					factorList.Add(factors[g]);
			}

			return factorList.ToArray();
		}

		double GetMaxScaler(int generation)
		{
			int si = _pd.GenerationStartIndex(DevStage.NewEgg, generation);
			return (si > _lastQuantIndex) ? 20.0 : 100.0;
		}

		void CalculatePrognosis(double[] scalingFactors)
		{
			bool hasVirtMonitorings = _pd.Year == DateTime.Now.Year;

			if (HasEggs)
			{
				_prognEggs = GetPrognValues(_eggMonitoringPeriods, _pd.GetNormalizedRow(DevStage.NewEgg, -1, scalingFactors), hasVirtMonitorings);

			}

			if (HasAdults)
			{
				_prognAdults = GetPrognValues(_adultMonitoringPeriods, _pd.GetNormalizedRow(DevStage.ActiveFly, -1, scalingFactors), hasVirtMonitorings);
			}

			int lastLarvIndex = (hasVirtMonitorings) ? Math.Min(365, _md.LastMonitoringIndex) : 365;
			_prognLarvae = _pd.GetNormalizedRow(DevStage.Larva, -1, scalingFactors, lastLarvIndex);
		}

		double GetEvalValue(DevStage stage, double[] generationScales, int lastIndex)
		{
			double[] popFactors = new double[generationScales.Length + 1];
			popFactors[0] = 1.0;
			for (int i = 0; i < generationScales.Length; i++)
				popFactors[i + 1] = generationScales[i];

			double[] progn;
			double[] monitoring;

			if (stage == DevStage.NewEgg)
			{
				progn = GetPrognValues(_eggMonitoringPeriods, _pd.GetNormalizedRow(DevStage.NewEgg, -1, popFactors, lastIndex));
				monitoring = _md.Eggs;
			}
			else
			{
				progn = GetPrognValues(_adultMonitoringPeriods, _pd.GetNormalizedRow(DevStage.ActiveFly, -1, popFactors, lastIndex));
				monitoring = _md.Adults;
			}

			double result = Evaluator.GetResidual(progn, monitoring, _evalMethod, 0, lastIndex);

			ReportOptimizationFactors(result, generationScales);
			_evalStep++;

			if (CheckForTermination(result, generationScales)) // weil der Nelder-Mead-Optimierer sonst endlos lange wg zu kleiner Grenzdifferenz arbeitet
				throw new Exception("OptimizationBreak");
	
			return result;
		}

		void TransmitBestScales(double evalResult, double[] scales)
		{
			if (double.IsNaN(_bestEvalValue))
			{
				_bestEvalValue = evalResult;
				_lastEvalValue = evalResult;
				_noChangeCounter = 0;
			}
			else
			{
				if (evalResult <= _bestEvalValue)
				{
					_bestEvalValue = evalResult;
					for (int i = 0; i < scales.Length; i++)
						_bestScalingFactors[i] = scales[i];
				}
			}
		}

		bool CheckForTermination(double evalResult, double[] generationScales)
		{
			TransmitBestScales(evalResult, generationScales);

			double diff = Math.Abs(_lastEvalValue - evalResult);
			_lastEvalValue = evalResult;
			if (diff < 0.001)
			{
				int maxCounter = generationScales.Length;
				if (++_noChangeCounter > maxCounter)
					return true;
			}
			else
				_noChangeCounter = 0;

			return false;
		}

		public double GetRemainingError(DevStage stage, EvalMethod evalMethod, int firstIndex, int lastIndex)
		{
			double[] progn;
			double[] monitoring;

			if (stage == DevStage.NewEgg)
			{
				progn = GetPrognValues(_eggMonitoringPeriods, _pd.GetNormalizedRow(DevStage.NewEgg, -1, _optimizedFactors, 365));
				monitoring = _md.Eggs;
			}
			else
			{
				progn = GetPrognValues(_adultMonitoringPeriods, _pd.GetNormalizedRow(DevStage.ActiveFly, -1, _optimizedFactors, 365));
				monitoring = _md.Adults;
			}

			double result = Evaluator.GetResidual(progn, monitoring, evalMethod, firstIndex, lastIndex);
			return result;
		}

		#endregion


		#region report

		void ReportOptimizationFactors(double result, double[] normFactors)
		{
			if (!_doCreateReport)
				return;

			if (_evalStep == 0)
			{
				_optReport.Append($"start:{result,10:f3}");

			}
			else
			{
				_optReport.Append($"{_evalStep,5}:{result,10:f3}");

			}
			for (int i = 0; i < normFactors.Length; i++)
				_optReport.Append($"{normFactors[i],10:f3}");
			_optReport.AppendLine("");

		}
		
		private void ReportOptimizationHeader(int numFactors) 
		{
			if (!_doCreateReport)
				return;
			_optReport.Append(" Step      Error");
			for (int i = 0; i < numFactors; i++)
				_optReport.Append($"     Gen {i + 1}");
			_optReport.AppendLine("");
		}

		private void ReportOptimizationResult(double optResult, double[] bestFactors, int numFactors, long duration, bool byTermination)
		{
			if (!_doCreateReport)
				return;

			_optReport.Append($" best:{optResult,10:f3}");

			for (int i = 0; i < numFactors; i++)
				_optReport.Append($"{bestFactors[i + 1],10:f3}");
			_optReport.AppendLine("");

			_optReport.AppendLine($"Calculating Time : {duration} ms");
			if (byTermination)
				_optReport.AppendLine($"Early Termination");
		}

		#endregion

		#region file-io
		private void WriteElem(StreamWriter w, double elem)
		{
			string delim = ";";

			if (double.IsNaN(elem))
				w.Write($"{delim}");
			else
				w.Write($"{elem:0.#}{delim}");
		}


		public void WriteToFile(string fileName)
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					//Kopfzeile
					sw.WriteLine("Datum;Monitoring Eiabl.;Prognose Eiabl.;Monitoring Flug;Prognose Flug;Prognose Larven");

					// Daten
					TtpTime datum = new TtpTime("1.1." + _md.Year);
					for (int d = 0; d < 365; d++)
					{
						sw.Write(datum.ToString("dd.MM.yyyy") + ";");
						WriteElem(sw, _md.Eggs[d]);
						WriteElem(sw, _prognEggs[d]);

						WriteElem(sw, _md.Adults[d]);
						WriteElem(sw, _prognAdults[d]);

						WriteElem(sw, _prognLarvae[d]);

						sw.WriteLine();
						datum.Inc(TtpEnPattern.Pattern1Day, 1);
					}
				}
			}
			catch (Exception ex)
			{
				DlgMessage.Show("Daten können nicht geschrieben werden", ex.Message, MessageLevel.Error);
			}
		}

		public void WriteOptimizationReport(string fn)
		{
			try
			{
				using (StreamWriter sw = new StreamWriter(fn, false, Encoding.UTF8))
				{
					sw.Write(_optReport.ToString());
				}
			}
			catch (Exception ex)
			{
				DlgMessage.Show("Daten können nicht geschrieben werden", ex.Message, MessageLevel.Error);
			}
		}

		#endregion

	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swat.defs;
using System.IO;
using System.Globalization;
using Microsoft.SolverFoundation.Solvers;
using TTP.Engine3;

namespace swat.iodata
{
	public enum EvalMethod
	{
		Nothing,
		AbsDiff,
		Relation
	}

	class PrognosisData
	{
		#region Variable

		WorkspaceData _workspace;
		MonitoringData _md;
		PopulationData _pd;

		double[] _prognAdults;
		double[] _prognEggs;
		double[] _prognLarvae;
		int[] _eggMonitoringPeriods;
		int[] _adultMonitoringPeriods;

		int _numMonitoringsEggs;
		int _numMonitoringsAdults;
		bool _hasEggs;
		bool _hasAdults;

		int _lastOptIndex;
		StringBuilder _optReport;
		EvalMethod _evalMethod;
		int _evalStep;
		
		// Variable für Optimierung
		int _noChangeCounter;
		double _bestEvalValue;
		double _lastEvalValue;
		double[] _bestParameters;


		public string Title { get; set; }
		public string QuantReportName { get; set; }


		#endregion

		#region Construction

		private PrognosisData()
		{
			_prognAdults = new double[366];
			_prognEggs = new double[366];
			_prognLarvae = new double[366];
			_optReport = new StringBuilder(4000);

			for(int i = 0;i < 366;i++)
			{
				_prognAdults[i]=double.NaN;
				_prognEggs[i] = double.NaN;
				_prognLarvae[i] = double.NaN;
			}
		}

		public static PrognosisData CreateNew(WorkspaceData workspace, bool doOptimize = false, EvalMethod evalMethod = EvalMethod.Relation)
		{
			MonitoringData md;

			if (workspace.WeatherData.Year == DateTime.Now.Year)
			{
				md = workspace.CurrentMonitoringData.GetWithVirtualMonitoring();
			}
			else
			{ 
				md = workspace.CurrentMonitoringData;
			}

			PrognosisData prognosis = new PrognosisData
			{
				_workspace = workspace,
				_md = md,
				_pd = workspace.CurrentPopulationData,
				_lastOptIndex = (md.FirstVirtMonitoring > 0) ? md.FirstVirtMonitoring - 1 : 366,
				_evalMethod = (doOptimize == false)? EvalMethod.Nothing : evalMethod
			};


			prognosis._hasEggs = prognosis.HasMonEggs();
			prognosis._hasAdults = prognosis.HasMonAdults();
			prognosis.CalculateMonitoringPeriods();

			string optMethodText="";
			switch(prognosis._evalMethod)
			{
				case EvalMethod.Nothing: optMethodText = "(keine Quantifizierung)";break;
				case EvalMethod.AbsDiff: optMethodText = "(Quantifizierung mit Beträgen)"; break;
				case EvalMethod.Relation: optMethodText = "(Quantifizierung mit Relationen)"; break;

			}

			prognosis.Title = (prognosis._hasEggs || prognosis._hasAdults) ?
				$"Prognose {workspace.CurrentModelName} {workspace.Location} {workspace.SimulationYear} {optMethodText}" :
				"ohne Daten aus Monitoring keine quantitative Prognose möglich";

			if (doOptimize)
			{
				prognosis.Calculate(prognosis.GetOptimizedFactors(workspace.CurrentPopulationData.NormalisationFactors));
				// hier neue Methode für aktuelle Jahr...
			}
			else
			{
				prognosis.Calculate(workspace.CurrentPopulationData.NormalisationFactors);
			}
			//prognosis.Calculate(workspace.CurrentPopulationData.NormalisationFactors);


			return prognosis;
		}

		#endregion

		#region Prognose berechnen

		private void CalcMonitoringNums()
		{
			//
			_numMonitoringsEggs = 0;
			_numMonitoringsAdults = 0;
			//

			for (int i=0; i< _lastOptIndex;i++)
			{
				if (!double.IsNaN(_md.Eggs[i]) && (_md.Eggs[i] >= 0))
					_numMonitoringsEggs++;
				if (!double.IsNaN(_md.Adults[i]) && (_md.Adults[i] >= 0))
					_numMonitoringsAdults++;
			}

		}

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

		private int[] GetMonitoringPeriods(double[] monValues,int maxLength)
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

		private double GetRangeSum(double[] popValues,int index, int length)
		{
			double sum = 0.0;
			for (int i=0; i<length; i++)
			{
				sum += popValues[index - i];
			}
			return sum;
		}

		private double[] GetPrognValues(int[] monRanges, double[] popValues, bool withVirtMonitorings = false)
		{
			double[] prognValues = new double[366];
			int lastIndex = (withVirtMonitorings) ? 365 : _lastOptIndex;

			for (int i = 0; i < lastIndex; i++)
			{
				if (monRanges[i] < 0) //Start Monitoring
					prognValues[i] = -1.0;
				else
					prognValues[i] = (monRanges[i] > 0) ? GetRangeSum(popValues, i, monRanges[i]) : double.NaN;
			}
			return prognValues;
		}

		protected void CalculateMonitoringPeriods()
		{
			if (HasEggs)
			{ 
				_eggMonitoringPeriods = GetMonitoringPeriods(_md.Eggs, _pd.MaxEggPeriods);
			}

			if (HasAdults)
			{
				_adultMonitoringPeriods = GetMonitoringPeriods(_md.Adults, 10);
			}
		}

		protected void Calculate(double[] normFactors)
		{
			bool hasVirtMonitorings = _workspace.WeatherData.Year == DateTime.Now.Year;

			if (HasEggs)
			{
					_prognEggs = GetPrognValues(_eggMonitoringPeriods, _pd.GetNormalizedRow(DevStage.NewEgg, -1, normFactors), hasVirtMonitorings);
			}

			if (HasAdults)
			{
				_prognAdults = GetPrognValues(_adultMonitoringPeriods, _pd.GetNormalizedRow(DevStage.ActiveFly, -1, normFactors), hasVirtMonitorings);
			}

			_prognLarvae = _pd.GetNormalizedRow(DevStage.Larva, -1, normFactors);
		}

		#endregion

		#region Evaluierung + Optimierung

		private double EvalFkt(double prognVal, double monVal)
		{
			if(_evalMethod== EvalMethod.AbsDiff)
			{
				return Math.Abs(prognVal - monVal);
			}
			else
			{
				prognVal += 1.0;
				monVal += 1.0;
				return (prognVal > monVal) ? (prognVal / monVal) : (monVal / prognVal);
			}
		}

		void TransmitBestScales(double evalResult, double[] scales)
		{
			if (double.IsNaN(_bestEvalValue))
			{
				_bestParameters = new double[scales.Length];
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
						_bestParameters[i] = scales[i];
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

		public double EvalNormalizationScales(DevStage stage, double[] normScales, int  lastIndex)
		{
			int maxCounter = normScales.Length;
			double evalResult = 0.0;
			int numPairs = 0;

			// normFactors enthalten Multiplikatoren ab 1. Generation
			// Prognoseberechnung braucht aber auch Multiplikator für Überwinterungsgeneration
			double[] popFactors = new double[normScales.Length + 1];
			popFactors[0] = 1.0;
			for (int i = 0; i < normScales.Length; i++)
				popFactors[i + 1] = normScales[i];

			double[] progn;
			double[] monitoring;

			if(stage == DevStage.NewEgg)
			{
				progn = GetPrognValues(_eggMonitoringPeriods, _pd.GetNormalizedRow(DevStage.NewEgg, -1, popFactors, lastIndex));
				monitoring = _md.Eggs;
			}
			else
			{
				progn = GetPrognValues(_adultMonitoringPeriods, _pd.GetNormalizedRow(DevStage.ActiveFly, -1, popFactors, lastIndex));
				monitoring = _md.Adults;
			}

			for (int i=0; i < lastIndex;i++)
			{
				if (!double.IsNaN(progn[i]) && (progn[i] >= 0.0))
				{
					evalResult += EvalFkt(progn[i], monitoring[i]);
					numPairs++;
				}
			}
			double result = evalResult / numPairs;

			ReportOptimizationFactors(result, normScales);
			_evalStep++;

			if(CheckForTermination(result, normScales))
				throw new Exception("OptimizationBreak");

			return result;
		}

		double[] IsolateNeccessaryFactors(double[] factors)
		{
			List<double> factorList = new List<double>();

			for (int g = 1; g < factors.Length; g++)// Faktor für  Überwinterungsgeneration nicht antasten 
			{
				if (g == _pd.GetNumGenerations())
				{
					int si = _pd.GenerationStartIndex(DevStage.NewEgg, g);
					if (si > 0 && si < _lastOptIndex)
						factorList.Add(factors[g]);
				}
				else
					factorList.Add(factors[g]);
			}

			return factorList.ToArray();
		}

		double  GetMaxScaler(int generation)
		{
			int si = _pd.GenerationStartIndex(DevStage.NewEgg, generation);
			return (si > _lastOptIndex) ? 10.0 : 100.0;
		}

		double[] GetOptimizedFactors(double[] oldFactors)
		{
			double[] workingFactors = IsolateNeccessaryFactors(oldFactors); // Faktoren ab 1. Gen
			double[] factorsLowLimit = new double[workingFactors.Length];
			double[] factorsHighLimit = new double[workingFactors.Length];

			for (int g = 0; g < workingFactors.Length; g++)
			{
				factorsLowLimit[g] =  workingFactors[g] / GetMaxScaler(g+1);
				factorsHighLimit[g] = workingFactors[g] * GetMaxScaler(g+1);
			}

			_evalStep = 0;
			bool earlyTermination = false;

			CalcMonitoringNums();
			ReportOptimizationHeader(workingFactors.Length);
			DevStage stageOptimized = (_hasEggs) ? DevStage.NewEgg : DevStage.ActiveFly;

			EvalNormalizationScales(stageOptimized, workingFactors, _lastOptIndex);
			var watch = System.Diagnostics.Stopwatch.StartNew();

			_bestEvalValue = double.NaN;
			double[] optimizedFactors = new double[oldFactors.Length];

			try
			{ 
				var solution = NelderMeadSolver.Solve(
				x => (EvalNormalizationScales(stageOptimized, x, _lastOptIndex)), workingFactors, factorsLowLimit, factorsHighLimit);

				optimizedFactors[0] = 1.0; // Skalierung für Überwinterungsgeneration wieder einfügen

				for (int g=0; g < workingFactors.Length; g++)
				{
					optimizedFactors[g+1] = solution.GetValue(g+1);
				}
			}
			catch(Exception)
			{
				for (int g = 0; g < _bestParameters.Length; g++)
				{
					optimizedFactors[g + 1] = _bestParameters[g];
				}
				earlyTermination = true;

			}

			watch.Stop();
			var calculationTime = watch.ElapsedMilliseconds;

			ReportOptimizationResult(_bestEvalValue, optimizedFactors, workingFactors.Length, calculationTime, earlyTermination);
			return optimizedFactors;
		}

		void ReportOptimizationFactors(double result, double[] normFactors)
		{
			if(_evalStep == 0)
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
			string methodTxt = (_evalMethod == EvalMethod.AbsDiff) ? "Fehlerbeträgen" : "Fehlerrelationen";
			QuantReportName = $"{_workspace.Name} {_workspace.CurrentModelName} Report Quantifizierung - Optimierung mit {methodTxt}";

			_optReport.AppendLine(QuantReportName);
			_optReport.Append(" Step      Error");
			for (int i = 0; i < numFactors; i++)
				_optReport.Append($"     Gen {i+1}");
			_optReport.AppendLine("");
		}

		private void ReportOptimizationResult(double optResult, double[] bestFactors, int numFactors,long duration, bool byTermination)
		{
			_optReport.Append($" best:{optResult,10:f3}");

			for (int i = 0; i< numFactors; i++)
				_optReport.Append($"{bestFactors[i+1],10:f3}");
			_optReport.AppendLine("");

			_optReport.AppendLine($"Calculating Time : {duration} ms");
			if(byTermination)
				_optReport.AppendLine($"Early Termination");
		}

		#endregion

		#region Properties

		public bool HasEggs
		{
			get {return _hasEggs;}
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

		public string OptimizationReport
		{
			get
			{
				return (_optReport.Length > 0) ? _optReport.ToString() : null;
			}
		}

		#endregion

		#region IO

		private void WriteElem(StreamWriter w, double elem)
		{
			string delim = ";";

			if(double.IsNaN(elem))
				w.Write($"{delim}");
			else
				w.Write($"{elem:0.#}{delim}");
		}


		public void WriteToFile(string fileName)
		{
			// todo: Fehlerbehandlung ergänzen in vm

			using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
			{
				//Kopfzeile
				sw.WriteLine("Datum;Monitoring Eiabl.;Prognose Eiabl.;Monitoring Flug;Prognose Flug;Prognose Larven");

				// Daten
				TtpTime datum = new TtpTime("1.1." + _workspace.SimulationYear);
				for (int d = 0; d < 365; d++)
				{
					sw.Write(datum.ToString("dd.MM.yyyy")+";");
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

		public void WriteOptimizationReport(string fn)
		{
			// todo: Fehlerbehandlung ergänzen in vm
			using (StreamWriter sw = new StreamWriter(fn, false, Encoding.UTF8))
			{
				sw.Write(_optReport.ToString());
			}
		}

		#endregion
	}
}

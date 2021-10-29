using Microsoft.SolverFoundation.Solvers;
//using Swop.data;
//using Swop.defs;
using Swop.glob;
//using Swop.sim;
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using swatSim;

namespace Swop.optimizer
{
	enum LimitType
	{
		Low,
		High
	}
	//===============================================================================================================

	[Serializable]
	public class TerminationCheckerException : Exception
	{
		public TerminationCheckerException()
		{ }

		public TerminationCheckerException(string message)
			 : base(message)
		{ }

		public TerminationCheckerException(string message, Exception innerException)
			 : base(message, innerException)
		{ }
	}

	//===============================================================================================================

	public class MultiOptimizer
	{
		#region Variable

		GlobData _globData;
		SimParamData _startParams;
		SimParamData _bestParams;
		Quantor[] _modelQuantors;

		double[] _solverParams;
		double[] _solverLowLimits;
		double[] _solverHighLimits;
		double[] _bestEvals;
		double _bestEvalValue;
		double _bestVariedEvalValue;
		int _evalStep;
		int _noChangeCounter;
		
		//string _laptext;// nur für Log;
		bool _isFirstLap;

		double[] _modelEvals;
		double[] _startEvals;

		double[] _randomNumbers; // zentrales Feld mit Zufallsdaten weil Random-Generator nicht multithreading-fähig ist 


		BackgroundWorker _optimizer;
		public bool UseRelationEval { get; set; }

		#endregion

		#region Construction + Init

		public MultiOptimizer(GlobData globData, ProgressChangedEventHandler progressChangedMethod, RunWorkerCompletedEventHandler progressCompletedMethod)
		{
			_globData = globData;

			_randomNumbers = new double[10000000]; // Erzeugung von Zufallszahlen-Folge für Multithreading
			Random rand = new Random(96);
			for (int i = 0; i < _randomNumbers.Length; i++)
			{
				_randomNumbers[i] = rand.NextDouble();
			}

			SetOptimizerParams(globData.Parameters.Copy());

			_optimizer = new BackgroundWorker
			{
				WorkerSupportsCancellation = true,
				WorkerReportsProgress = true
			};
			_optimizer.DoWork += DoTheOptimization;
			_optimizer.ProgressChanged += progressChangedMethod;
			_optimizer.RunWorkerCompleted += progressCompletedMethod;
		}


		#endregion

		#region Aufbereitung Parameter

		void SetOptimizerParams(SimParamData modelParam)
		{
			_startParams = modelParam;
			_solverParams = TransformToSolverParams(_startParams, true);
			_solverLowLimits = GetSolverLimit(LimitType.Low);
			_solverHighLimits = GetSolverLimit(LimitType.High);
		}


		double[] GetSolverLimit(LimitType lt)
		{
			double[] solverLimit = new double[_startParams.ParamDict.Count];

			int n = 0;
			foreach (string key in _startParams.ParamDict.Keys)
			{
				SimParamElem elem = _startParams.ParamDict[key];
				solverLimit[n++] = (lt == LimitType.Low) ? elem.MinVal : elem.MaxVal;
			}

			return solverLimit;
		}

		double[] TransformToSolverParams(SimParamData simParam, bool isInit = false)
		{
			double[] solverParams = new double[simParam.ParamDict.Count];

			int n = 0;
			foreach (string key in simParam.ParamDict.Keys)
			{
				SimParamElem elem = simParam.ParamDict[key];
				switch (Type.GetTypeCode(elem.ObjType))
				{
					case TypeCode.Boolean:
						solverParams[n] = ((bool)elem.Obj) ? 0.501 : 0.499; // achtung: oder besser 0.51 und 0.49 ?
						break;

					case TypeCode.Int32:
						solverParams[n] = ((Int32)elem.Obj);
						break;

					case TypeCode.Double:
						solverParams[n] = ((double)elem.Obj);
						break;
				}
				n++;
			}

			return solverParams;
		}

		SimParamData TransformToModelParams(SimParamData simParam, double[] solverData)
		{
			SimParamData modelParam = simParam.Clone();

			int n = 0;
			foreach (string key in simParam.ParamDict.Keys)
			{
				SimParamElem elem = simParam.ParamDict[key];
				switch (Type.GetTypeCode(elem.ObjType))
				{
					case TypeCode.Boolean:
						elem.Obj = (solverData[n] > 0.5) ? true : false;
						break;

					case TypeCode.Int32:
						elem.Obj = Convert.ToInt32(solverData[n]);
						break;

					case TypeCode.Double:
						elem.Obj = solverData[n];
						break;

				}
				modelParam.AddOrReplaceItem(key, elem);
				n++;
			}

			return modelParam;
		}


		#endregion

		#region Properties

		public int FirstEvalIndex { get { return _globData.FirstOptIndex; } }
		public int LastEvalIndex { get { return _globData.LastOptIndex; } }

		public bool HasParameters
		{
			get
			{
				if (_startParams == null)
					return false;

				return (_startParams.ParamDict.Count > 0);
			}
		}

		public SimParamData BestParameters
		{
			get { return _bestParams; }
			set { _bestParams = value; }
		}

		#endregion

		#region Text-Output


		private void CreateStartValuesText()
		{
			StringBuilder pt = new StringBuilder();

			pt.AppendLine("Start : " + DateTime.Now.ToString());

			pt.AppendLine("Fehlerwerte beim Start:");
			for (int i = 0; i <= _globData.NumSets; i++)
			{
				string s;
				if (i == 0)
					s = String.Format("{0,10}", "Total");
				else
				{
					string n = "S" + i;
					s = String.Format("{0,10}", n);
				}

				pt.Append(s);
			}

			pt.AppendLine();
			_startEvals[0] = 1.0;
			for (int i = 0; i <= _globData.NumSets; i++)
			{
				string num = String.Format(CultureInfo.InvariantCulture,"{0,10:0.####}", _startEvals[i]);
				pt.Append(num);
			}
			pt.AppendLine();


			_globData.PrologText += pt.ToString();
		}

		private void CreateBestParameterText()
		{
			StringBuilder pt = new StringBuilder();
			pt.AppendLine("");

			pt.AppendLine("Parameterliste:");

			foreach (string key in _bestParams.ParamDict.Keys)
			{
				SimParamElem elem = _bestParams.ParamDict[key];
				string desc = elem.Descr;
				string val;
				if (Type.GetTypeCode(elem.ObjType) == TypeCode.Double)
					val = ((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture);
				else
					val = elem.Obj.ToString();

				pt.AppendLine($"{key} = {val}");

			}
		}

		private void CreateBestLogText()
		{
			StringBuilder pt = new StringBuilder();
			for (int i = 0; i <= _globData.NumSets; i++)
			{
				string s;
				if (i == 0)
					s = String.Format("{0,10}", "Total");
				else
				{
					//string n = "L" + i;
					//s = String.Format("{0,10};", n);
					string n = _globData.Monitorings[i - 1].Location;
					s = String.Format("{0,20}", n);

				}

				pt.Append(s);
			}
			pt.AppendLine();
			for (int i = 0; i <= _globData.NumSets; i++)
			{
				string num = String.Format("{0,10:0.####}", _modelEvals[i]);
				pt.Append(num);
			}
			pt.AppendLine();


			pt.Append(string.Format("step {0,-8}", _evalStep));

			string fmt = "P{0}: {1,-10}";
			int p = 1;
			foreach (string key in _bestParams.ParamDict.Keys)
			{
				SimParamElem elem = _bestParams.ParamDict[key];
				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double) ?
					((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture) :
					elem.Obj.ToString();

				pt.Append(string.Format(fmt, p, es));
				p++;
			}
			_globData.BestLogText = pt.ToString();
		}

		private void CreateBestValuesText()
		{
			StringBuilder pt = new StringBuilder();


			string fmt = "P{0}: {1,-10}";
			int p = 1;
			foreach (string key in _bestParams.ParamDict.Keys)
			{
				SimParamElem elem = _bestParams.ParamDict[key];
				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double) ?
					((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture) :
					elem.Obj.ToString();

				pt.Append(string.Format(fmt, p, es));
				p++;
			}

			pt.AppendLine();
			pt.AppendLine();

			pt.Append(string.Format("step {0,-8}", _evalStep));
			for (int i = 0; i <= _globData.NumSets; i++)
			{
				string num;
				if (i == 0)
				{
					num = "Total:" + String.Format(CultureInfo.InvariantCulture, "{0,-10:0.####}", _modelEvals[0]);
				}
				else
				{
					num = String.Format("S{0}: ", i) + String.Format(CultureInfo.InvariantCulture, "{0,-10:0.####}", _modelEvals[i]);
				}
				pt.Append(num);
			}
			pt.AppendLine();

			_globData.BestText = pt.ToString();
		}

		private void CreateActionText(double residual)
		{

			_globData.OptStep = _evalStep.ToString();
			_globData.StepEval = residual.ToString("F4", CultureInfo.InvariantCulture);
		}

		private void SwopLogActionText(double[] residual, SimParamData workingParams)
		{
			StringBuilder pt = new StringBuilder();
			if (_evalStep == 0)
			{
				pt.AppendLine("[Run]");
			}
			pt.AppendLine($"  #STEP:{_evalStep}");
			pt.AppendLine($"  #LAP:{_globData.OptLap}");


			pt.AppendLine($"    #T:" + _modelEvals[0].ToString("F4", CultureInfo.InvariantCulture));
			for (int i = 1; i <= _globData.NumSets; i++)
			{
				pt.AppendLine($"    #S{i}:"+ _modelEvals[i].ToString("F4", CultureInfo.InvariantCulture));
			}

			int p = 1;
			foreach (string key in workingParams.ParamDict.Keys)
			{
				SimParamElem elem = workingParams.ParamDict[key];
				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double) ?
					((Double)elem.Obj).ToString("F4", CultureInfo.InvariantCulture) :
					elem.Obj.ToString();

				pt.AppendLine($"      #P{p++}:{es}");
			}

			_globData.SwopLog(pt.ToString());
		}

		private void SwopLogBestText()
		{
			StringBuilder pt = new StringBuilder();


			_globData.SwopLog(pt.ToString());

		}

		public string GetEvalTimerangeText()
		{
			DateTime start = DateTime.Parse("1.1.1995");
			DateTime end = start;
			start = start.AddDays(FirstEvalIndex);
			end = end.AddDays(LastEvalIndex);

			return start.ToString("dd.MM") + " - " + end.ToString("dd.MM");
		}

		private string GetEvalMethodText()
		{
			return (UseRelationEval) ? "Fehlerrelationen" : "Fehlerbeträge";
		}

		#endregion

		#region Optimierung

		public void ExecuteOptimization()
		{
			_optimizer.RunWorkerAsync();
		}

		public void AbortOptimization()
		{
			_optimizer.CancelAsync();
		}

		void InitializeEvaluators()
		{
			_noChangeCounter = -1;
			_modelQuantors = new Quantor[_globData.NumSets];

		}

		// Optimierung der Modellparameter
		private void DoTheOptimization(object sender, DoWorkEventArgs e)
		{
			InitializeEvaluators();

			try
			{
				_globData.WriteSwopLogStartParams(_startParams);
				_evalStep = -1;
				_bestEvalValue = double.NaN;
				_startEvals = new double[_globData.NumSets + 1];
				_bestEvals = new double[_globData.NumSets + 1];

				//for (int n = 0; n <= _globData.NumSets; n++)
				//	_startEvals[n] = 1.0;

				//CreateLogHeadline();
				//_globData.LogHeadline();

				EvaluateMultiModelParameters(_solverParams); // wegen Startwerten
				for (int n = 0; n <= _globData.NumSets; n++)
					_startEvals[n] = _modelEvals[n];

				//_globData.WriteSwopLogEvals(_startEvals, EvalType.Start);
				CreateStartValuesText(); 
				_optimizer.ReportProgress(0);	// Aktualisierung Prolog-Text

				ExecOptimizationLoops();// Schleife mit Parameter-Permutationen

				//AppendActionText("Ende Optimierung");
				_globData.WriteSwopLogEvals(_startEvals, EvalType.Start);
				_globData.WriteSwopLogEvals(_bestEvals, EvalType.Best);
				//_globData.WriteSwopLogStartParams(_startParams);
				_globData.WriteSwopLogBestParams(_bestParams);
				//_globData.LogBest();
				_optimizer.ReportProgress(100);
			}
			catch (Exception)
			{
				//_globData.LogEnd();
				//_globData.LogBest();
				_globData.WriteSwopLogCancel();
				_globData.WriteSwopLogEvals(_startEvals, EvalType.Start);
				_globData.WriteSwopLogEvals(_bestEvals, EvalType.Best);
				//_globData.WriteSwopLogStartParams(_startParams);
				_globData.WriteSwopLogBestParams(_bestParams);
				_optimizer.ReportProgress(100);
				e.Cancel = true;
				return;
			}
		}

		private void ExecOptimizationLoops()
		{
			//ReportExecOptimizationLoopsLog("Optimierung mit Startwerten");
			SimParamData dSim = _startParams.Clone();

			_globData.OptLap = "0";
			_isFirstLap = true;
			OptimizeParameterset(_solverParams);
			_optimizer.ReportProgress(50);

			//_laptext = "1+";
			//ReportExecOptimizationLoopsLog($"Variation: P1+10%");
			//_optimizer.ReportProgress(50);
			//OptimizeParameterset(VaryParameters(dSim, 0, 0.1));//, -1));

			//_laptext = "1-";
			//ReportExecOptimizationLoopsLog($"Variation: P1-10%");
			//_optimizer.ReportProgress(50);
			//OptimizeParameterset(VaryParameters(dSim, 0, -0.1));//, -1));

			_isFirstLap = false;
			for (int p = 0; p < _solverParams.Length; p++)
			{
				//string os = "";
				//for (int i = 0; i < p; i++)
				//{
				//	os += "P" + (i + 1) + "*, ";
				//}

				_globData.OptLap = $"{p + 1}+";
				//ReportExecOptimizationLoopsLog($"Variation: {_laptext} P{p + 1}+10%");
				_optimizer.ReportProgress(50);
				OptimizeParameterset(VaryParameters(dSim, p, 0.1));// p - 1));

				_globData.OptLap = $"{p+1}-";
				//ReportExecOptimizationLoopsLog($"Variation: {_laptext} P{p + 1}-10%");
				_optimizer.ReportProgress(50);
				OptimizeParameterset(VaryParameters(dSim, p, -0.1));//, p - 1)); 
			}
		}

		private void OptimizeParameterset(double[] factors)
		{
			try
			{
				_noChangeCounter = -1; 
				var solution = NelderMeadSolver.Solve(x => (EvaluateMultiModelParameters(x)), factors, _solverLowLimits, _solverHighLimits);

				_globData.EndText = "Solver regulär beendet"; ; // reguläres Ende durch Min-Differenz Solver

				//_globData.LogEnd();
				//_globData.LogBest();
				_optimizer.ReportProgress(100);

			}
			catch (TerminationCheckerException)
			{
				_optimizer.ReportProgress(50);
				return;
			}
		}


		private double[] VaryParameters(SimParamData sp, int varyIndex, double amount)//, int bestOptIndex)
		{
			double[] variedParams = new double[_solverParams.Length];
			double[] bestParams = TransformToSolverParams(_bestParams);

			int n = 0;
			foreach (string key in _startParams.ParamDict.Keys)
			{
				//if (n <= bestOptIndex)
				if (n != varyIndex)
					variedParams[n] = bestParams[n];
				else
				{
					SimParamElem elem = _startParams.ParamDict[key];

					if (Type.GetTypeCode(elem.ObjType) == TypeCode.Boolean)
					{
						if (n == varyIndex)
						{
							variedParams[varyIndex] = (amount > 0.0) ? 1 : 0;
						}
						else
							variedParams[n] = 0.5;

					}
					else // int und double
					{
						double dp = _solverParams[n];
						if (n == varyIndex)
						{
							variedParams[varyIndex] = (amount > 0.0) ?
									dp + (GetSolverLimit(LimitType.High)[varyIndex] - dp) * amount :
									dp + (dp - GetSolverLimit(LimitType.Low)[varyIndex]) * amount;
						}
						else
							variedParams[n] = dp;
					}
				}
				n++;
			}

			return variedParams;
		}

		private ModelBase CreateOptimizationModel(SimParamData optParams, int index)
		{
			ModelBase model;
			switch (_globData.ModelTyp)
			{
				case ModelType.DR: model = new ModelDR("",_globData.Weathers[index], null, optParams, _globData.LocationParameters[index]); break;
				case ModelType.PR: model = new ModelPR("",_globData.Weathers[index], null, optParams, _globData.LocationParameters[index]); break;
				case ModelType.DA: model = new ModelDA("", _globData.Weathers[index], null, optParams, _globData.LocationParameters[index]); break;

				default: throw new Exception("Modelloptimierung für dieses Modell nicht implementiert");
			}

			model.SetRandomNumbers(_randomNumbers);
			return model;

		}

		bool CheckForTermination(double evalResult, int numFactors)
		{
			if (_noChangeCounter < 0)
			{
				_bestVariedEvalValue = evalResult;
				_globData.LapEval = evalResult.ToString("F4", CultureInfo.InvariantCulture);
			}

			if (evalResult >= _bestVariedEvalValue)
				_noChangeCounter++;
			else
			{
				_bestVariedEvalValue = evalResult;
				_noChangeCounter = 0;
				_globData.LapEval = evalResult.ToString("F4", CultureInfo.InvariantCulture);

			}
			int maxNoChangeCounter = (_isFirstLap) ?			//  abh. v. Anzahl der Optimierungsparameter, mindestens aber 10 Durchläufe
				Math.Max(10, (numFactors * numFactors * 4)) : // Initialdurchlauf etwas ausführlicher
				Math.Max(10, (numFactors * numFactors));

			_globData.RemainingSteps = (maxNoChangeCounter - _noChangeCounter).ToString();
				
			return (_noChangeCounter > maxNoChangeCounter);
		}

		double EvaluateMultiModelParameters(double[] solverFactors)
		{
			if (_optimizer.CancellationPending == true)
			{
				//AppendActionText("Abbruch durch Nutzer");// Ende durch Abbruch-Button
				_globData.EndText = "Abbruch durch Nutzer";
				throw new Exception("Abbruch durch Nutzer");
			}

			_evalStep++;

			SimParamData optParams = TransformToModelParams(_startParams, solverFactors);

			_modelEvals = CalcModelResiduals(optParams);

			bool isBetter = double.IsNaN(_bestEvalValue) ? false : (_modelEvals[0] < _bestEvalValue);

			if (double.IsNaN(_bestEvalValue) || isBetter)
			{
				_bestEvalValue = _modelEvals[0];
				for (int i = 0; i < _modelEvals.Length; i++)
				{ 
					_bestEvals[i] = _modelEvals[i];
				}
				_bestParams = optParams.Clone();

				CreateBestValuesText();
				CreateBestLogText();
				//CreateBestParameterText();
			}

			CreateActionText(_modelEvals[0]);
			//LogActionText(_modelEvals, optParams);			
			_globData.TotalBestEval = _bestEvalValue.ToString("F4", CultureInfo.InvariantCulture);
			SwopLogActionText(_modelEvals, optParams);

			_optimizer.ReportProgress(50);

			if (CheckForTermination(_modelEvals[0], solverFactors.Length))
			{
				_globData.EndText = "Optimierung beendet - keine Verbesserung mehr";
				//AppendActionText("keine Verbesserung mehr"); // Ende durch  "kaum noch Unterschiede"
				throw new TerminationCheckerException();
			}

			

			return _modelEvals[0];
		}

		double[] CalcModelResiduals(SimParamData simParams)
		{
			double[] singleEvals = new double[_globData.NumSets + 1];

			Parallel.For(0, _globData.NumSets, i =>
			 {
				 try
				 {
					 ModelBase model = CreateOptimizationModel(simParams, i);
					 model.Simulate();

					 Quantor quantor = Quantor.CreateNew(model.Population, _globData.Monitorings[i], EvalMethod.Relation, false); 
					 DevStage stage = quantor.HasEggs ? DevStage.NewEgg : DevStage.ActiveFly;

					 singleEvals[i + 1] = quantor.GetRemainingError(stage, EvalMethod.Relation, FirstEvalIndex, LastEvalIndex); // optimieren:immer relationen!
				 }
				 catch
				 {
					 singleEvals[i + 1] = 99999.0;
				 };
			 }
			);

			singleEvals[0] = 0.0;
			double totalWeightings = 0.0;
			for (int i = 1; i <= _globData.NumSets; i++)
			{
				singleEvals[0] += _globData.EvalWeightings[i-1] * singleEvals[i]/_startEvals[i]; // achtung Indizes!
				totalWeightings += _globData.EvalWeightings[i - 1];
			}
			singleEvals[0] /= totalWeightings;

			return singleEvals;
		}

		#endregion

	}
}

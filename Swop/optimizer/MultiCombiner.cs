using Microsoft.SolverFoundation.Solvers;
using Swop.glob;
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


	//===============================================================================================================

	public class MultiCombiner
	{
		#region Variable

		GlobData _globData;
		SimParamData _modelDefParams;
		SimParamData _bestParams;

		EvalMethod _quantorMethod;

		double[] _combiValues;
		double[] _bestEvals;
		double _bestEvalValue;
		Int64 _combiStep;
		List<CombiRec> _combiList;
		Int64 _numCombinations;

		double[] _modelEvals;
		double[] _startEvals;

		double[] _randomNumbers; // zentrales Feld mit Zufallsdaten weil Random-Generator nicht multithreading-fähig ist 


		BackgroundWorker _combiner;

		public bool UseRelationEval { get; set; }

		#endregion

		#region Construction + Init

		public MultiCombiner(GlobData globData, ProgressChangedEventHandler progressChangedMethod, RunWorkerCompletedEventHandler progressCompletedMethod)
		{
			_globData = globData;
			_quantorMethod = EvalMethod.Relation;

			_randomNumbers = new double[10000000]; // Erzeugung von Zufallszahlen-Folge für Multithreading
			Random rand = new Random(96);
			for (int i = 0; i < _randomNumbers.Length; i++)
			{
				_randomNumbers[i] = rand.NextDouble();
			}

			InitCombiParams(globData.WorkParameters,globData.CombiList); // Initialisiert die internen Variablen

			_combiner = new BackgroundWorker
			{
				WorkerSupportsCancellation = true,
				WorkerReportsProgress = true
			};
			_combiner.DoWork += DoTheCombination;
			_combiner.ProgressChanged += progressChangedMethod;
			_combiner.RunWorkerCompleted += progressCompletedMethod;
		}


		#endregion

		#region Aufbereitung Parameter

		void InitCombiParams(SimParamData modelParam, List<CombiRec> combiList)
		{
			_modelDefParams = modelParam;
			_combiList = combiList;			
			_numCombinations = CalcNumKombinations(combiList);
		}


		int CalcNumKombinations(List<CombiRec> cList)
		{
			int n = 1;

			for (int i = 0; i < cList.Count; i++)
				n *= cList[i].Steps;

			return n;
		}

		double[] GetIndexedKombination(Int64 id)
		{
			int numParams = _combiList.Count;
			
			// Kombinations-Indizes berechnen
			Int64[] repeater = new Int64[numParams];
			for (int p=0; p< numParams; p++)
			{
				if (p == 0)
					repeater[p] = _numCombinations / _combiList[p].Steps;
				else
					repeater[p] = repeater[p - 1] / _combiList[p].Steps;
			}

			Int64[] paraIds = new Int64[numParams];
			for (int p = 0; p < numParams; p++)
			{
				if (p == 0)
					paraIds[p] = (id / repeater[p]);
				else
					paraIds[p] = (id % repeater[p-1]) / repeater[p];
			}

			// Indizes in Zahlenwerte übersetzen
			double[] paras = new double[numParams];
			for( int p=0; p< numParams; p++)
			{
				paras[p]= _combiList[p].MinVal +( paraIds[p] * (_combiList[p].MaxVal - _combiList[p].MinVal) / (_combiList[p].Steps - 1));
			}
			
			return paras;
		}

		double[] TransformToSolverParams(SimParamData simParam)
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
				if (_modelDefParams == null)
					return false;

				return (_modelDefParams.ParamDict.Count > 0);
			}
		}

		public SimParamData BestParameters
		{
			get { return _bestParams; }
			set { _bestParams = value; }
		}

		#endregion

		#region Text-Output


		private void AppendStartDisplayText()
		{
			StringBuilder pt = new StringBuilder();
			
			pt.AppendLine();
			pt.AppendLine("Start : " + DateTime.Now.ToString());

			_globData.PrologText += pt.ToString();
		}

		private void CreateActionDisplayText(double residual)
		{

			_globData.CombiStep = _combiStep.ToString();
			_globData.CombiStepsRemaining = (_numCombinations - _combiStep).ToString();
			_globData.StepEval = residual.ToString("F4", CultureInfo.InvariantCulture);
		}

		private void LogActionText(SimParamData workingParams)
		{
			StringBuilder pt = new StringBuilder();
			if (_combiStep == 0)
			{
				pt.AppendLine("[Run]");
			}
			pt.AppendLine($"  #STEP:{_combiStep}");
			pt.AppendLine($"  #LAP:{_globData.OptLap}");


			pt.AppendLine($"    #T:" + _modelEvals[0].ToString("F4", CultureInfo.InvariantCulture));
			for (int i = 1; i <= _globData.NumSets; i++)
			{
				pt.AppendLine($"    #S{i}:" + _modelEvals[i].ToString("F4", CultureInfo.InvariantCulture));
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

			_globData.WriteTextToLog(pt.ToString());
		}

		public string GetEvalTimerangeText()
		{
			DateTime start = DateTime.Parse("1.1.1995");
			DateTime end = start;
			start = start.AddDays(FirstEvalIndex);
			end = end.AddDays(LastEvalIndex);

			return start.ToString("dd.MM") + " - " + end.ToString("dd.MM");
		}

		#endregion

		#region Combination

		public void ExecuteCombination()
		{
			_combiner.RunWorkerAsync();
		}

		public void AbortExecution()
		{
			_combiner.CancelAsync();
		}

		void InitializeCombinator()
		{
			_combiStep = -1;
			_bestEvalValue = double.NaN;
			_startEvals = new double[_globData.NumSets + 1];
			_bestEvals = new double[_globData.NumSets + 1];
		}

		private void DoTheCombination(object sender, DoWorkEventArgs e)
		{
			InitializeCombinator();

			try
			{
				//Mit Modell-Voreinstellungen beginnen
				_globData.WriteLogParams(_modelDefParams, EvalType.Start);
				_combiStep = -1;

				_combiValues = TransformToSolverParams(_modelDefParams);
				EvaluateMultiModelParameters(_combiValues);

				for (int n = 0; n <= _globData.NumSets; n++)
					_startEvals[n] = _modelEvals[n];

				AppendStartDisplayText();
				_combiner.ReportProgress(0); // Aktualisierung Prolog-Text


				//jetzt alle Kombinationen
				_combiStep = 0;
				_globData.OptLap = "0"; // hier Dummy - nur fürs Protokoll
				for (int c = 0; c < _globData.NumCombinations; c++)
				{
					_combiValues = GetIndexedKombination(c);
					EvaluateMultiModelParameters(_combiValues);
				}

				// Reguläres Ende
				_globData.WriteLogEvals(_startEvals, EvalType.Start);
				_globData.WriteLogEvals(_bestEvals, EvalType.Best);
				_globData.WriteLogParams(_bestParams, EvalType.Best);
				_globData.EndText = "Kombination regulär beendet";
				_combiner.ReportProgress(100);
			}
			catch (Exception) // vorzeitiger Abbruch durch Nutzer
			{
				_globData.WriteLogCancel();
				_globData.WriteLogEvals(_startEvals, EvalType.Start);
				_globData.WriteLogEvals(_bestEvals, EvalType.Best);
				_globData.WriteLogParams(_bestParams, EvalType.Best);

				_combiner.ReportProgress(100);
				e.Cancel = true;
				return;
			}
		}

		private ModelBase CreateCombinationModel(SimParamData optParams, int index)
		{
			ModelBase model;
			switch (_globData.ModelTyp)
			{
				case FlyType.DR: model = new ModelDR("", _globData.Weathers[index], null, optParams, _globData.LocationParameters[index]); break;
				case FlyType.PR: model = new ModelPR("", _globData.Weathers[index], null, optParams, _globData.LocationParameters[index]); break;
				case FlyType.DA: model = new ModelDA("", _globData.Weathers[index], null, optParams, _globData.LocationParameters[index]); break;

				default: throw new Exception("dieses Modell ist nicht implementiert");
			}

			model.SetRandomNumbers(_randomNumbers);
			return model;

		}

		double EvaluateMultiModelParameters(double[] solverFactors)
		{
			if (_combiner.CancellationPending == true)
			{
				_globData.EndText = "Abbruch durch Nutzer";
				throw new Exception("Abbruch durch Nutzer");
			}

			_combiStep++;

			SimParamData optParams = TransformToModelParams(_modelDefParams, solverFactors);

			_modelEvals = CalcModelResiduals(optParams); // alle Datensätze durchrechnen
			TestForBetterEval(optParams);

			CreateActionDisplayText(_modelEvals[0]);
			LogActionText(optParams);

			_combiner.ReportProgress(50);

			return _modelEvals[0];
		}


		void TestForBetterEval(SimParamData optParams)
		{
			bool isBetter = double.IsNaN(_bestEvalValue) ? false : (_modelEvals[0] < _bestEvalValue);

			if (double.IsNaN(_bestEvalValue) || isBetter)
			{
				_bestEvalValue = _modelEvals[0];
				for (int i = 0; i < _modelEvals.Length; i++)
				{
					_bestEvals[i] = _modelEvals[i];
				}
				_bestParams = optParams.Clone();

				//CreateBestValuesText();
				//CreateBestLogText();
				_globData.TotalBestEval = $"Total-Best: {_bestEvalValue.ToString("F4", CultureInfo.InvariantCulture)}"; ; // kann weg?
			}
		}

		double[] CalcModelResiduals(SimParamData simParams)
		{
			double[] singleEvals = new double[_globData.NumSets + 1];

			Parallel.For(0, _globData.NumSets, i =>
			{
				try
				{
					ModelBase model = CreateCombinationModel(simParams, i);
					model.RunSimulation();

					Quantor quantor = Quantor.CreateNew(model, model.Population, _globData.Monitorings[i], _quantorMethod, false);
					DevStage stage = quantor.HasEggs ? DevStage.NewEgg : DevStage.ActiveFly;

					singleEvals[i + 1] = quantor.GetRemainingError(stage, _quantorMethod, _globData.FirstIndices[i], _globData.LastIndices[i]); 
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
				singleEvals[0] += _globData.EvalWeightings[i - 1] * singleEvals[i] / _startEvals[i]; // achtung Indizes!
				totalWeightings += _globData.EvalWeightings[i - 1];
			}
			singleEvals[0] /= totalWeightings;

			return singleEvals;
		}

		#endregion

	}
}

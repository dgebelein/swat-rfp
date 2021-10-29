using Microsoft.SolverFoundation.Solvers;
//using swatSim;
////using swat.iodata;
////using swat.Optimizer;
////using swat.views.dlg;
//using SwatPresentations;
//using System;
//using System.ComponentModel;
//using System.Globalization;
//using System.IO;
//using System.Text;
////using System.Windows.Media;
//using TTP.Engine3;
////using TTP.TtpCommand3;
//using SwatSim;
////using SwatOpt;

//namespace swat.optimizer
//{
//	enum LimitType
//	{
//		Low,
//		High
//	}
//	//===============================================================================================================

//	[Serializable]
//	public class TerminationCheckerException : Exception
//	{
//		public TerminationCheckerException()
//		{ }

//		public TerminationCheckerException(string message)
//			 : base(message)
//		{ }

//		public TerminationCheckerException(string message, Exception innerException)
//			 : base(message, innerException)
//		{ }
//	}

//	//===============================================================================================================

//	public class OptimizationData
//	{
//		#region Variable

//		WorkspaceData _workspace;
//		SimParamData _startParams;
//		SimParamData _bestParams;

//		Quantor _quantor;

//		double[] _solverParams;
//		double[] _solverLowLimits;
//		double[] _solverHighLimits;
//		double _bestEvalValue;
//		double _bestVariedEvalValue;

//		//double _lastEvalValue;
//		int _evalStep;
//		public int FirstEvalIndex { get; set; }
//		public int LastEvalIndex { get; set; }
//		public bool UseRelationEval { get; set; }

//		int _noChangeCounter;

//		BackgroundWorker _optimizer;
//		StringBuilder _currentText;
//		string _paramHeadlineText;
//		string _originalValuesText;
//		string _bestValuesText;
//		PresentationsData _graphData = null;
//		private readonly object _currentTextLocker = new object();

//		#endregion

//		#region Construction + Init

//		public OptimizationData(WorkspaceData workspace,ProgressChangedEventHandler progressChangedMethod, RunWorkerCompletedEventHandler progressCompletedMethod)
//		{
//			_workspace = workspace;
//			FirstEvalIndex = 0;
//			LastEvalIndex = 364;
//			//_modelEvaluator = MonoEvaluator.CreateNew(workspace.CurrentMonitoringData);
//			//_quantor = Quantor.CreateNew(workspace.CurrentPopulationData, workspace.CurrentMonitoringData, EvalMethod.AbsDiff, false);
//			_currentText = new StringBuilder(100000);

//			_optimizer = new BackgroundWorker
//			{
//				WorkerSupportsCancellation = true,
//				WorkerReportsProgress = true
//			};
//			_optimizer.DoWork += DoTheOptimization;
//			_optimizer.ProgressChanged += progressChangedMethod;
//			_optimizer.RunWorkerCompleted += progressCompletedMethod;
//		}


//		#endregion

//		#region Aufbereitung Parameter

//		private SimParamData ExtractSelectedParams(SimParamData modelParam)
//		{
//			SimParamData p = new SimParamData();

//			foreach (string key in modelParam.ParamDict.Keys)
//			{
//				SimParamElem elem = modelParam.ParamDict[key];
//				if(elem.IsSelected)
//					p.AddOrReplaceItem(key, elem);
//			}

//			return p;
//		}

//		public void SetOptimizerParams(SimParamData modelParam)
//		{
//			_startParams = ExtractSelectedParams(modelParam);
//			_solverParams = TransformToSolverParams(_startParams, true);
//			_solverLowLimits = GetSolverLimit( LimitType.Low);
//			_solverHighLimits = GetSolverLimit(LimitType.High);

//			//_quantor.FirstEvalIndex = FirstEvalIndex;
//			//_quantor.LastEvalIndex = LastEvalIndex;
//			//_quantor.UseRelationEval = UseRelationEval;


//			CreateParamHeaderText();
//			CreateOriginalValuesText();

//			_currentText.Clear();
//			_bestValuesText = "";
//		}


//		double[] GetSolverLimit(LimitType lt)
//		{
//			double[] solverLimit = new double[_startParams.ParamDict.Count];

//			int n = 0;
//			foreach (string key in _startParams.ParamDict.Keys)
//			{
//				SimParamElem elem = _startParams.ParamDict[key];
//				solverLimit[n++] = (lt==LimitType.Low) ? elem.MinVal : elem.MaxVal;
//			}

//			return solverLimit;
//		}

//		double[] TransformToSolverParams(SimParamData simParam, bool isInit = false)
//		{
//			double[] solverParams = new double[simParam.ParamDict.Count];

//			int n = 0;
//			foreach (string key in simParam.ParamDict.Keys)
//			{
//				SimParamElem elem = simParam.ParamDict[key];
//				switch (Type.GetTypeCode(elem.ObjType))
//				{
//					case TypeCode.Boolean:
//							solverParams[n] = ((bool)elem.Obj) ? 0.501 : 0.499; // achtung: oder besser 0.51 und 0.49 ?
//						break;

//					case TypeCode.Int32:
//						solverParams[n] = ((Int32)elem.Obj);
//						break;

//					case TypeCode.Double:
//						solverParams[n] = ((double)elem.Obj);
//						break;
//				}
//				n++;
//			}

//			return solverParams;
//		}

//		SimParamData TransformToModelParams(SimParamData simParam, double[] solverData)
//		{
//			SimParamData modelParam = simParam.Clone();

//			int n = 0;
//			foreach (string key in simParam.ParamDict.Keys)
//			{
//				SimParamElem elem = simParam.ParamDict[key];
//				switch (Type.GetTypeCode(elem.ObjType))
//				{
//					case TypeCode.Boolean:
//						elem.Obj = (solverData[n] > 0.5) ? true : false;
//						break;

//					case TypeCode.Int32:
//						elem.Obj = Convert.ToInt32(solverData[n]);
//						break;

//					case TypeCode.Double:
//						elem.Obj = solverData[n];
//						break;

//				}
//				modelParam.AddOrReplaceItem(key, elem);
//				n++;
//			}

//			return modelParam;
//		}


//		#endregion

//		#region Properties

//		public bool HasParameters
//		{
//			get
//			{
//				if (_startParams == null)
//					return false;

//				return (_startParams.ParamDict.Count > 0);
//			}
//		}

//		public SimParamData BestParameters
//		{
//			get { return _bestParams; }
//			set { _bestParams = value; }
//		}

//		public string ParamHeaderText
//		{
//			get { return _paramHeadlineText; }
//		}

//		public string OriginalValuesText
//		{
//			get { return _originalValuesText; }
//		}

//		public string BestValuesText
//		{
//			get { return _bestValuesText; }
//		}

//		public string CurrentText
//		{
//			get
//			{
//				string ct;
//				lock (_currentTextLocker)
//				{
//					ct = _currentText.ToString();
//				}
//				return ct;
//			}
//		}

//		public TtpTimeRange EvalTimerange
//		{
//			get
//			{
//				TtpTime start = new TtpTime("1.1." + _workspace.SimulationYear);
//				start.Inc(TtpEnPattern.Pattern1Day, FirstEvalIndex);
//				return new TtpTimeRange(start, TtpEnPattern.Pattern1Day, LastEvalIndex - FirstEvalIndex);
//			}
//		}

//		#endregion

//		#region Text-Output

//		private void AppendCurrentText(string txt)
//		{

//			lock(_currentTextLocker)
//			{
//				_currentText.AppendLine(txt);
//			}
//		}

//		private void CreateParamHeaderText()
//		{
//			string fmt = "{0,20}";
//			 _paramHeadlineText = string.Format("{0,-8}{1,9}", "Step", "Eval");

//			foreach (string key in _startParams.ParamDict.Keys)
//			{
//				_paramHeadlineText += string.Format(fmt, key.Substring(key.IndexOf(".", 0)+1));
//			}
//		}

//		private void CreateOriginalValuesText()
//		{
//			string fmt = "{0,20}";
//			_originalValuesText = string.Format("{0,17}", "");

//			foreach (string key in _startParams.ParamDict.Keys)
//			{
//				SimParamElem elem = _startParams.ParamDict[key];
//				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double)?
//					((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture):
//					elem.Obj.ToString();

//				_originalValuesText += string.Format(fmt, es);
//			}
//		}

//		private void AdjustOriginalValuesText()
//		{
//			_originalValuesText = string.Format("{0,8}{1}", "", _bestValuesText.Substring(8));
//		}

//		private void CreateBestValuesText()
//		{
//			_bestValuesText = string.Format("{0,-8}", _evalStep);
//			_bestValuesText += string.Format(CultureInfo.InvariantCulture, "{0,9:0.000}",_bestEvalValue);

//			string fmt = "{0,20}";
//			foreach (string key in _bestParams.ParamDict.Keys)
//			{
//				SimParamElem elem = _bestParams.ParamDict[key];
//				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double) ?
//					((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture) :
//					elem.Obj.ToString();

//				_bestValuesText += string.Format(fmt, es);
//			}
//		}

//		private void CreateCurrentText(double residual, SimParamData workingParams)
//		{
//			string txt = string.Format("{0,-8}",_evalStep);
//			txt += string.Format(CultureInfo.InvariantCulture,"{0,9:0.000}", residual);

//			string fmt = "{0,20}";
//			foreach (string key in workingParams.ParamDict.Keys)
//			{
//				SimParamElem elem = workingParams.ParamDict[key];
//				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double) ?
//					((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture) :
//					elem.Obj.ToString();

//				txt += string.Format(fmt, es);
//			}
//			AppendCurrentText(txt);
//			//_currentText.AppendLine(txt);
//		}

//		private string GetEvalTimerangeText()
//		{
//			return EvalTimerange.ToString("dd.MM.yyyy");
//		}

//		private string GetEvalMethodText()
//		{
//			return (UseRelationEval) ? "Fehlerrelationen" : "Fehlerbeträge";
//		}

//		#endregion

//		#region Optimierung

//		public void ExecuteOptimization()
//		{
//			_optimizer.RunWorkerAsync();
//		}

//		public void AbortOptimization()
//		{
//			_optimizer.CancelAsync();
//		}

//		// Optimierung der Modellparameter
//		private void DoTheOptimization(object sender, DoWorkEventArgs e)
//		{
//			try
//			{
//				_noChangeCounter = -1;
//				_evalStep = 0;
//				_graphData = null;
//				_bestEvalValue = double.NaN;
//				GetModelParamEvalValue(_solverParams);// wegen Startwerten
//				AdjustOriginalValuesText();
//				ExecOptimizationLoops();// Schleife mit Parameter-Permutationen

//				_currentText.AppendLine($"Ende Optimierung"); 
//				_graphData = GeneratePresentationsData(_graphData, false, true);
//				_optimizer.ReportProgress(100, _graphData);
//			}
//			catch (Exception )
//			{
//				_optimizer.ReportProgress(100);
//				e.Cancel = true;
//				return;
//			}
//		}

//		private void ExecOptimizationLoops()
//		{
//			AppendCurrentText($"Optimierung mit Startwerten");
//			_optimizer.ReportProgress(0, _graphData);
//			SimParamData dSim = _startParams.Clone();

//			OptimizeParameterset(_solverParams);

//			AppendCurrentText($"Variation: P1+10%");
//			_optimizer.ReportProgress(0, _graphData);
//			OptimizeParameterset(VaryParameters(dSim,0, 0.1, -1));

//			AppendCurrentText($"Variation: P1-10%");
//			_optimizer.ReportProgress(0, _graphData);
//			OptimizeParameterset(VaryParameters(dSim,0, -0.1, -1));

//			for (int p = 1; p < _solverParams.Length; p++)
//			{
//				string os = "";
//				for (int i=0; i<p; i++)
//				{
//					os += "P" + (i+1) + "*, ";
//				}
//				AppendCurrentText($"Variation: {os} P{p + 1}+10%");
//				_optimizer.ReportProgress(0, _graphData);
//				OptimizeParameterset(VaryParameters(dSim,p, 0.1, p-1));

//				AppendCurrentText($"Variation: {os} P{p + 1}-10%");
//				_optimizer.ReportProgress(0, _graphData);
//				OptimizeParameterset(VaryParameters(dSim,p, -0.1, p-1));
//			}
//		}

//		private void  OptimizeParameterset(double[] factors)
//		{
//			try
//			{
//				_noChangeCounter = -1; // 0;
//				var solution = NelderMeadSolver.Solve(x => (GetModelParamEvalValue(x)), factors, _solverLowLimits, _solverHighLimits);

//				AppendCurrentText($"Solver regulär beendet"); // reguläres Ende durch Min-Differenz Solver
//				_graphData = GeneratePresentationsData(_graphData, false);
//				_optimizer.ReportProgress(0, _graphData);

//			}
//			catch (TerminationCheckerException)
//			{
//				return;
//			}
//		}


//		private double[] VaryParameters(SimParamData sp, int varyIndex, double amount, int bestOptIndex)
//		{
//			double[] variedParams = new double[_solverParams.Length];
//			double[] bestParams = TransformToSolverParams(_bestParams);

//			int n = 0;
//			foreach (string key in _startParams.ParamDict.Keys)
//			{
//				//if (n <= bestOptIndex)
//				if (n != varyIndex)
//					variedParams[n] = bestParams[n];
//				else
//				{
//					SimParamElem elem = _startParams.ParamDict[key];

//					if (Type.GetTypeCode(elem.ObjType)== TypeCode.Boolean)
//					{
//						if (n == varyIndex)
//						{
//							variedParams[varyIndex] = (amount > 0.0) ? 1 : 0;
//						}
//						else
//							variedParams[n] = 0.5;

//					}
//					else // int und double
//					{
//						double dp = _solverParams[n];
//						if (n == varyIndex)
//						{
//							variedParams[varyIndex] = (amount > 0.0) ?
//									dp + (GetSolverLimit(LimitType.High)[varyIndex] - dp) * amount :
//									dp + (dp - GetSolverLimit(LimitType.Low)[varyIndex]) * amount;
//						}
//						else
//							variedParams[n] = dp;
//					}
//				}
//				n++;
//			}

//			return variedParams;
//		}

//		private ModelBase CreateOptimizationModel(SimParamData optParams)
//		{
//			PopulationData population = new PopulationData(10);
//			ModelBase model;

//			switch (_workspace.CurrentModelType)
//			{
//				case ModelType.DR: model = new ModelDR("",_workspace.WeatherData, population, _workspace.CurrentWorkingParameters); break;
//				case ModelType.PR: model = new ModelPR("",_workspace.WeatherData, population, _workspace.CurrentWorkingParameters); break;
//				default: throw new Exception("Modelloptimierung für dieses Modell nicht implementiert");
//			}

//			model.ChangeModelParameters(optParams);
//			return model;

//		}

//		bool CheckForTermination(double evalResult, int numFactors)
//		{
//			if (_noChangeCounter < 0)
//			{
//				_bestVariedEvalValue = evalResult;
//			}

//			if (evalResult >= _bestVariedEvalValue) 
//				_noChangeCounter++;
//			else
//			{
//				_bestVariedEvalValue = evalResult;
//				_noChangeCounter = 0;
//			}

//			int maxNoChangeCounter = Math.Max(10, (numFactors * numFactors)); //  abh. v. Anzahl der Optimierungsparameter, mindestens aber 10 Durchläufe

//			return (_noChangeCounter > maxNoChangeCounter);
//		}

//		double GetModelParamEvalValue(double[] solverFactors)
//		{
//			if (_optimizer.CancellationPending == true)
//			{
//				AppendCurrentText($"Abbruch");// Ende durch Abbruch-Button
//				_graphData = GeneratePresentationsData(_graphData, false, true);
//				_optimizer.ReportProgress(100,_graphData);
//				throw new Exception("Abbruch Optimierung");
//			}

//			_evalStep++;

//			SimParamData optParams = TransformToModelParams(_startParams, solverFactors);
//			ModelBase model = CreateOptimizationModel(optParams);
//			//Quantor _quantor;


//			double actEval;
//			EvalMethod evalMethod = UseRelationEval ? EvalMethod.Relation : EvalMethod.AbsDiff;
	
//			try
//			{
//				model.Simulate();
//				_quantor = Quantor.CreateNew(model.Population, _workspace.CurrentMonitoringData, EvalMethod.AbsDiff, false); // quantifizieren immer mit absdiff
//				DevStage stage = _quantor.HasEggs ? DevStage.NewEgg : DevStage.ActiveFly;

//				_quantor.FirstEvalIndex = FirstEvalIndex;
//				_quantor.LastEvalIndex = LastEvalIndex;
//				//_quantor.UseRelationEval = UseRelationEval;
//				actEval = _quantor.GetRemainingError(stage, evalMethod, FirstEvalIndex, LastEvalIndex); // optimieren:auswahl zulassen!
//			}
//			catch
//			{
//				actEval = 99999.9;
//			}

//			bool isBetter = double.IsNaN(_bestEvalValue) ? false : (actEval <= _bestEvalValue);

//			if (double.IsNaN(_bestEvalValue) || isBetter)
//			{
//				_bestEvalValue = actEval;
//				_bestParams = optParams.Clone();

//				CreateBestValuesText();
//				_graphData = GeneratePresentationsData(_graphData, isBetter);
//			}
//			else
//				_graphData = GeneratePresentationsData(_graphData, isBetter);

//			CreateCurrentText(actEval, optParams);

//			if (CheckForTermination(actEval, solverFactors.Length))
//			{
//				_currentText.AppendLine($"keine Verbesserung mehr"); // Ende durch  "kaum noch Unterschiede"
//				_graphData = GeneratePresentationsData(_graphData, isBetter, false);
//				_optimizer.ReportProgress(100,_graphData);
//				throw new TerminationCheckerException("keine Verbesserung mehr"); 
//			}

//			_optimizer.ReportProgress(0, _graphData);

//			return actEval;
//		}

//		#endregion

//		#region Visualisierungsdaten

//		TtpScaleInfo GetFixedScaling(TtpScaleInfo scaleInfo)
//		{
//			TtpScaleInfo si = scaleInfo;
//			si.ScaleMin = si.ActualScaleMin;
//			si.ScaleMax = si.ActualScaleMax;
//			si.ScaleType = TtpEnScale.Fixed;
//			return si;
//		}

//		private PresentationsData GeneratePresentationsData(PresentationsData copyData, bool isBetter, bool isCompleted = false)
//		{
//			PresentationsData data;
//			if (copyData == null)
//			{ 
//				data = new PresentationsData
//				{
//					TimeRange = new TtpTimeRange(new TtpTime("1.1." + _workspace.SimulationYear), TtpEnPattern.Pattern1Year, 1),
//					Title = _quantor.Title ,
//					ZoomFactor = 0,
//					ZoomFactorRight = 0,
//					HighlightTimeRange = EvalTimerange
//				};
//				AddPrognosisRows(data, null, isBetter, isCompleted);
//			}
//			else
//			{ 
//				data = new PresentationsData
//				{
//					TimeRange = copyData.TimeRange,
//					Title = _quantor.Title ,
//					ZoomFactor = copyData.ZoomFactor,
//					ZoomFactorRight= copyData.ZoomFactorRight,
//					HighlightTimeRange= copyData.HighlightTimeRange,
//					LeftAxisInfo = GetFixedScaling(copyData.LeftAxisInfo)
//				};
//				AddPrognosisRows(data,copyData, isBetter, isCompleted);
//			}
//			data.Title = "Optimierung " +_workspace.Name +" / " + _workspace.CurrentModelName +  "  Evaluierung: " + GetEvalTimerangeText()+" / " + GetEvalMethodText();
			
//			return data;
//		}


//		private void AddPrognosisRows(PresentationsData data, PresentationsData startData, bool isBetter, bool isCompleted)
//		{
//			int li = 0;
//			string monLegend = (_quantor.HasEggs) ? "Monitoring Eiablage" : "Monitoring Flug";

//			data.AddRow(new PresentationRow
//			{
//				Legend = monLegend,
//				LegendIndex = li++,
//				IsVisible = true,
//				Thicknes = 1.0,
//				Color = Brushes.CornflowerBlue,
//				Values = _quantor.MonitoringValues,
//				Axis = TtpEnAxis.Left,
//				LineType = TtpEnLineType.LinePoint
//			});

//			if(startData == null)
//			{ 
//				data.AddRow(new PresentationRow
//				{
//					Legend = "Berechnung - Startparameter",
//					LegendIndex = li++,
//					IsVisible = true,
//					Color = Brushes.LightSkyBlue,
//					Values = _quantor.PrognValues,
//					Axis = TtpEnAxis.Left,
//					LineType = TtpEnLineType.AreaDiff
//				});
//			}
//			else
//			{
//				data.AddRow(new PresentationRow
//				{
//					Legend = "Berechnung - Startparameter",
//					LegendIndex = li++,
//					IsVisible = true,
//					Color = Brushes.LightSkyBlue,
//					Values = startData.GetRow(1).Values,
//					Axis = TtpEnAxis.Left,
//					LineType = TtpEnLineType.AreaDiff
//				});
//				if (isBetter)
//				{
//					data.AddRow(new PresentationRow
//					{
//						Legend = "Berechnung - beste Parameter",
//						LegendIndex = li++,
//						IsVisible = true,
//						Color = Brushes.LawnGreen,
//						Values = _quantor.PrognValues,
//						Axis = TtpEnAxis.Left,
//						LineType = TtpEnLineType.AreaDiff
//					});
//				}
//				else
//				{
//					if (startData.NumRows > 2)
//						data.AddRow(new PresentationRow
//						{
//							Legend = "Berechnung - beste Parameter",
//							LegendIndex = li++,
//							IsVisible = true,
//							Color = Brushes.LawnGreen,
//							Values = startData.GetRow(2).Values,
//							Axis = TtpEnAxis.Left,
//							LineType = TtpEnLineType.AreaDiff
//						});
//				}

//				if(!isCompleted)
//					data.AddRow(new PresentationRow
//					{
//						Legend = "",
//						LegendIndex = li++,
//						IsVisible = true,
//						Color = Brushes.WhiteSmoke,
//						Thicknes = 0.5,
//						Values = _quantor.PrognValues,
//						Axis = TtpEnAxis.Left,
//						LineType = TtpEnLineType.LinePoint
//					});

//				if(isCompleted) // Wetterdaten erst am Schluss einblenden
//				{
//					WeatherData wd = _workspace.WeatherData;

//					data.AddRow(new PresentationRow
//					{
//						Legend = "Lufttemperatur [°C]",
//						LegendIndex = li++,
//						IsVisible = false,
//						Thicknes = 1.0,
//						Color = Brushes.DeepPink,
//						Values = wd.GetSimAirTemp(),
//						Axis = TtpEnAxis.Right,
//						LineType = TtpEnLineType.Line
//					});

//					data.AddRow(new PresentationRow
//					{
//						Legend = "Bodentemperatur [°C]",
//						LegendIndex = li++,
//						IsVisible = false,
//						Thicknes = 1.0,
//						Color = Brushes.OrangeRed,
//						Values = wd.GetSimSoilTemp(),
//						Axis = TtpEnAxis.Right,
//						LineType = TtpEnLineType.Line
//					});

//					data.AddRow(new PresentationRow
//					{
//						Legend = "Sättigungsdefizit [hPa]",
//						LegendIndex = li++,
//						IsVisible = false,
//						Thicknes = 1.0,
//						Color = Brushes.Turquoise,
//						Values = wd.GetSimVpd(),
//						Axis = TtpEnAxis.Right,
//						LineType = TtpEnLineType.Line
//					});
//				}
//			}
//		}


//		#endregion


//		#region Report-Ausgabe in Datei

//		public void SaveReport()
//		{
//			int fileIndex = Utility.FindValidFileIndex(WorkspaceData.GetPathOptimization,  _workspace.Name + " - OptReport_*" + ".txt");
//			string Filename = Path.Combine(WorkspaceData.GetPathOptimization, _workspace.Name + " - OptReport_" + fileIndex + ".txt");

//			try
//			{
//				using (StreamWriter sw = new StreamWriter(Filename, false, Encoding.UTF8))
//				{
//					sw.WriteLine("Optimierung " + _workspace.Name + " / " + _workspace.CurrentModelName);
//					sw.WriteLine("Evaluierungsmethode: " + GetEvalMethodText());
//					sw.WriteLine("Evaluierungszeitraum: " + GetEvalTimerangeText());
//					sw.WriteLine(ParamHeaderText);
//					sw.WriteLine("Start:");
//					sw.WriteLine(OriginalValuesText);
//					sw.WriteLine("Run:");
//					sw.WriteLine(CurrentText);
//					sw.WriteLine("Best:");
//					sw.WriteLine(BestValuesText);
//				}
//			}
//			catch (Exception e)
//			{
//				SwatPresentations.DlgMessage.Show("Report kann nicht gespeichert werden", e.Message, SwatPresentations.MessageLevel.Error);
//			}
//		}

//		#endregion

//	}
//}

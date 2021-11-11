using swatSim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SwopReview
{
	public struct ParamCombi
	{
		public int idX;
		public int idY;
		public string XPara;
		public string YPara;
		public string RadioText;
	};

	public class SwopData
	{

		#region Variable
		bool _hasData;
		public string ErrMessage{get;set;}
		public bool RunIsCancelled { get; set; }

		public SwopWorkMode WorkMode { get; private set; }
		public string Description { get; private set; }
		public FlyType ModelType { get; private set; }
		public SimParamData DefaultParameters { get; private set; }
		
		public List<string> OptParameters { get; private set; } 
		public List<SwopSet> OptSets { get; private set; }

		public double[,] OptParamValues { get; private set; }  // step,paranum

		public double[] CommonErrors{ get; private set; }
		public double BestCommonError { get; private set; }
		public double StartCommonError { get; private set; }
		public double[] StartParamValues { get; private set; }

		public double[] BestParamValues { get; private set; }
		public string[] StepLaps { get; private set; }

		public List<ParamCombi> ParamCombinations { get; private set; }

		public double MinimumError { get; private set; }
		public double MaximumError { get; private set; }

		string _swopLogName;
		string[] _logLines;



		#endregion

		#region construction

		public SwopData()
		{
			//DefaultParameters = new SimParamData();
			//OptSets = new List<SwopSet>();
			//OptParameters = new List<string>();
		}


		void InitializeArrays(int nSteps)
		{
			CommonErrors = new double[nSteps];
			OptParamValues = new double[nSteps, OptParameters.Count];
			BestParamValues = new double[OptParameters.Count];
			StartParamValues = new double[OptParameters.Count];

			StepLaps = new string[nSteps];

			foreach (SwopSet swopSet in OptSets)
			{
				swopSet.InitErrValues(nSteps);
			}
		}


		#endregion


		#region Properties

		public string SwopLogName
		{
			get { return Path.GetFileNameWithoutExtension(_swopLogName); }
		}

		public bool HasErrors
		{
			get { return !string.IsNullOrEmpty(ErrMessage); }
		}

		public bool HasValidData
		{
			get { return (_hasData && !HasErrors); }
		}

		public double GetOptParamValue(int step, int paraNo)
		{
			return OptParamValues[step, paraNo];

		}

		public double GetSetError(int setId, int step, bool isAbsoluteError)
		{
			return (isAbsoluteError) ?
				 OptSets[setId].ErrValues[step]  :
				 OptSets[setId].ErrValues[step] / OptSets[setId].StartErrValue;

		}

		public double [] CommonErrorsAbsolute
		{
			get
			{
				double[] tea = new double[CommonErrors.Length];
				double sum = 0.0;
				double w = 0.0;
				foreach (SwopSet s in OptSets)
				{
					sum += s.ErrValues[0] * s.Weight;
					w += s.Weight;
				}

				tea[0] = sum / w;
				for (int i=1;i< tea.Length;i++)
				{
					tea[i] = CommonErrors[i] * tea[0];
				}
				return tea;
			}
		}

		public List<string> LapNames
		{
			get
			{
				List<string> listLap = new List<string>();
				for (int i=0; i< StepLaps.Length; i++)
				{
					if (string.IsNullOrEmpty(StepLaps[i]))
						StepLaps[i] = "0";
					if (!listLap.Contains(StepLaps[i]))
						listLap.Add(StepLaps[i]);
				}
				return listLap;
			}
		}



		#endregion

		#region  read log-file

		public bool Read(string Filename)
		{
			ErrMessage = "";
			DefaultParameters = new SimParamData();
			ParamCombinations = new List<ParamCombi>();

			OptSets = new List<SwopSet>();
			OptParameters = new List<string>();

			_swopLogName = Filename;

			try
			{
				string[] fileLines = File.ReadAllLines(Filename, Encoding.UTF8);
				_logLines = new string[fileLines.Length];
				for (int i = 0; i < fileLines.Length; i++)
					_logLines[i] = fileLines[i].Trim();
			}
			catch (Exception e)
			{
				ErrMessage = $"{Filename} kann nicht gelesen werden! Grund: {e.Message}";
				return false;
			}
			ReadWorkMode();

			ReadModelType();			
			ReadDescription();

			if (!HasErrors) ReadDefaultParameters();
			if (!HasErrors) ReadSets();
			if (!HasErrors) ReadOptParameters();
			if (!HasErrors) CreateParamCombinations();
			if (!HasErrors) ReadRun();
			if (!HasErrors) ReadStartEvals();
			if (!HasErrors) ReadBestEvals();
			if (!HasErrors) ReadStartParams();
			if (!HasErrors) ReadBestParams();
			ClearErrorLimits();

			RunIsCancelled = ReadCancel();

			_hasData = true;

			return string.IsNullOrEmpty(ErrMessage);

		}

		void ReadDescription()
		{
			int sl = GetLineNo("[Description]");
			if (sl < 0)
			{
				Description = "";
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				if (_logLines[n].StartsWith("["))
					break;
				Description = _logLines[n].Trim();
			}

		}

		void ReadWorkMode()
		{
			int sl = GetLineNo("[Mode]");
			if (sl < 0)
			{
				WorkMode = SwopWorkMode.OPTI;
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				if (_logLines[n].StartsWith("["))
					break;
				string line = _logLines[n].Trim();

				foreach (SwopWorkMode m in Enum.GetValues(typeof(SwopWorkMode)))
				{
					if (string.Compare(line, m.ToString(), true) == 0)
					{
						WorkMode = m;
						return;
					}
				}
			}

			ErrMessage = $"Mode-Angabe falsch";
		}

		void ReadModelType()
		{
			int sl = GetLineNo("[Model]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Model] nicht gefunden";
				return;
			}
			
			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				if (_logLines[n].StartsWith("["))
					break;
				string line = _logLines[n].Trim();

				foreach (FlyType m in Enum.GetValues(typeof(FlyType)))
				{
					if (string.Compare(line, m.ToString(), true) == 0)
					{
						ModelType = m;
						return;
					}
				}
			}

			ErrMessage = $"Modell-Angabe falsch";
		}

		void ReadDefaultParameters()
		{
			ModelBase model;
			switch (ModelType)
			{
				case FlyType.DR: model = new ModelDR(null, null, null); break;
				case FlyType.PR: model = new ModelPR(null, null, null); break;
				case FlyType.DA: model = new ModelDA(null, null, null); break;

				default: model = null; break;
			}
			DefaultParameters = model.CodedParams;

			int sl = GetLineNo("[Default-Parameters]");
			if(sl<0)
			{
				ErrMessage+= $"\r\nSektion [Default-Parameters] nicht gefunden";
				return;
			}
			
			for(int n = sl+1; n < _logLines.Length; n++)
			{
				if (_logLines[n].StartsWith("["))
					break;
				if (!DefaultParameters.ReadFromString(_logLines[n]))
				{
					ErrMessage += $"\r\nFehler in [Default-Parameters]"; ;
					break;
				}

			}

		}

		void ReadSets()
		{
			int sl = GetLineNo("[Sets]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Sets] nicht gefunden";
				return;
			}
			
			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;
				
				if (s.StartsWith("#S"))
				{
					string ids = s.Substring(2).Trim(':');
					if (int.TryParse(ids, out int setNo))
					{ 
						ReadSingleSet(setNo, n);
					}
					else
					{ 
						ErrMessage += $"\r\nFehler in Sektion [Sets]: unzulässige Set-Nummer";
						return;
					}
				}
			}
		}

		void ReadSingleSet(int setNo,int startLine)
		{
			//Test, ob Set schon existiert
			foreach (SwopSet s in OptSets)
			{
				if(setNo== s.SetIndex)
				{
					ErrMessage += $"\r\nSektion [Sets]: SetIndex mehrfach vorhanden";
					return;
				}
			}

			SwopSet set = new SwopSet(setNo);
			List<string> param = new List<string>();
			set.LocalParams = param;
			OptSets.Add(set);

			for (int n = startLine + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("[") || s.StartsWith("#S"))
					break;
				string token = GetToken(s);
				switch (token)
				{
					case "W:": set.Weather = GetTokenContentString(s); break;
					case "M:": set.Monitoring = GetTokenContentString(s); break;
					case "T:": set.EvalTime = GetTokenContentString(s); break;
					case "E:": set.Weight = GetTokenContentDouble(s);break;
					case "P:": param.Add(GetTokenContentString(s)); break;
					default: break;
				}
			}
		}

		void ReadOptParameters()
		{
			int sl = GetLineNo("[Opt-Parameters]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Opt-Parameters] nicht gefunden";
				return;
			}
			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;
				OptParameters.Add(GetTokenContentString(s));
			}
		}

		void CreateParamCombinations()
		{
			int numParas = OptParameters.Count;
			for (int x = 0; x < numParas; x++)
			{
				for (int y = x + 1; y < numParas; y++)
				{
					ParamCombi pc = new ParamCombi()
					{
						idX = x,
						idY = y,
						XPara = OptParameters[x],
						YPara = OptParameters[y],
						RadioText = $"P{x + 1}: {OptParameters[x]}  - P{y + 1}: {OptParameters[y]}"
					};
					ParamCombinations.Add(pc);
				}
			}
		}

		private List<ParamCombi> GetParamCombis(List<string> paraSequence)
		{
			List<ParamCombi> pcl = new List<ParamCombi>();
			int numParas = paraSequence.Count;
			for (int x = 0; x < numParas; x++)
			{
				for (int y = x + 1; y < numParas; y++)
				{
					int xs = OptParameters.IndexOf(paraSequence[x]);
					int ys = OptParameters.IndexOf(paraSequence[y]);
					ParamCombi pc = new ParamCombi()
					{
						idX = xs,
						idY = ys,
						XPara = OptParameters[xs],
						YPara = OptParameters[ys],
						RadioText = $"{OptParameters[xs]}  -  {OptParameters[ys]}"
					};
					pcl.Add(pc);
				}
			}

			return pcl;
		}

		public List<ParamCombi> GetParamCombisSelected(List<string> paraSequence, string selParam)
		{
			if (string.IsNullOrEmpty(selParam))
				return GetParamCombis(paraSequence);

			List<ParamCombi> pcl = new List<ParamCombi>();
			int numParas = paraSequence.Count;
			int y = OptParameters.IndexOf(selParam);
			for (int x = 0; x < numParas; x++)
				{
				if (y == x)
					continue;

				int xs = OptParameters.IndexOf(paraSequence[x]);
				int ys = OptParameters.IndexOf(paraSequence[y]);
				ParamCombi pc = new ParamCombi()
				{
					idX = xs,
					idY = ys,
					XPara = OptParameters[xs],
					YPara = OptParameters[ys],
					RadioText = $"{OptParameters[xs]}  -  {OptParameters[ys]}"
				};
				pcl.Add(pc);
				
			}

			return pcl;
		}

		int GetStepCount()
		{
			int nCount = 0;
			for(int n=0; n< _logLines.Length;n++)
			{
				if (_logLines[n].StartsWith("#STEP:"))
				{
					int step = GetTokenContentInt(_logLines[n]);
					if (step > nCount)
							nCount = step;
				}
			}
			return nCount+1;
		}

		void ReadRun()
		{
			InitializeArrays(GetStepCount());

			int sl = GetLineNo("[Run]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Run] nicht gefunden";
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;

				if (s.StartsWith("#STEP"))
				{
					if (int.TryParse(GetTokenContentString(s), out int step))
					{
						ReadSingleStep(step, n);
					}
					else
					{
						ErrMessage += $"\r\nFehler in Sektion [Run]: unzulässige Step-Nummer";
						return;
					}
				}
			}

		}

		void ReadSingleStep(int step, int startLine)
		{
			for (int n = startLine + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("[") || s.StartsWith("#STEP"))
					break;

				string token = GetToken(s);

				if (token == "T:")
				{
					if (step == 0)
						CommonErrors[step] = 1.0;
					else
						CommonErrors[step] = GetTokenContentDouble(s);
				}

				if (token == "LAP:")
				{
					StepLaps[step] = GetTokenContentString(s);
				}

				for (int swopSet=1; swopSet <= OptSets.Count; swopSet++) // Tokens beginnen mit #S1:
				{
					string setToken = $"S{swopSet}:";
					if(token == setToken)
					{
						OptSets[swopSet - 1].SetErrValue(step, GetTokenContentDouble(s));
					}
				}

				for (int paraNo = 1; paraNo <= OptParameters.Count ; paraNo++) // Tokens beginnen mit #P1:
				{
					string paraToken = $"P{paraNo}:";
					if (token == paraToken)
					{
						OptParamValues[step,paraNo-1]= GetTokenContentDouble(s);
					}
				}
			}
		}

		void ReadStartEvals()
		{
			int sl = GetLineNo("[Start-Evals]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Start-Evals] nicht gefunden";
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;

				string token = GetToken(s);

				if (token == "T:")
				{
					StartCommonError = GetTokenContentDouble(s);
				}


				for (int swopSet = 1; swopSet <= OptSets.Count; swopSet++) // Tokens beginnen mit #S1:
				{
					string setToken = $"S{swopSet}:";
					if (token == setToken)
					{
						OptSets[swopSet - 1].StartErrValue= GetTokenContentDouble(s);
					}
				}
			}

			// Berechnung total Error T
			double sum = 0.0;
			double w = 0.0;
			foreach (SwopSet ss in OptSets)
			{
				foreach (SwopSet s in OptSets)
				{
					sum += s.StartErrValue * s.Weight;
					w += s.Weight;
				}
			}
			StartCommonError *= sum / w;  // 'Total'-Angaben sind im Protokoll immer relativ -deshalb umrechnung nötig
		}

		void ReadBestEvals()
		{

			int sl = GetLineNo("[Best-Evals]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Best-Evals] nicht gefunden";
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;

				string token = GetToken(s);

				if (token == "T:")
				{
					BestCommonError = GetTokenContentDouble(s);
				}


				for (int swopSet = 1; swopSet <= OptSets.Count; swopSet++) // Tokens beginnen mit #S1:
				{
					string setToken = $"S{swopSet}:";
					if (token == setToken)
					{
						OptSets[swopSet - 1].BestErrValue = GetTokenContentDouble(s);
					}
				}
			}
			BestCommonError *= StartCommonError;// 'Total'-Angaben sind im Protokoll immer relativ -deshalb umrechnung nötig
		}

		void ReadStartParams()
		{
			int sl = GetLineNo("[Start-Params]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Start-Params] nicht gefunden";
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;

				string token = GetToken(s);
				for (int paraNo = 1; paraNo <= OptParameters.Count; paraNo++) // Tokens beginnen mit #P1:
				{
					string paraToken = $"P{paraNo}:";
					if (token == paraToken)
					{
						StartParamValues[paraNo - 1] = GetTokenContentDouble(s);
					}
				}
			}
		}

		void ReadBestParams()
		{
			int sl = GetLineNo("[Best-Params]");
			if (sl < 0)
			{
				ErrMessage += $"\r\nSektion [Best-Params] nicht gefunden";
				return;
			}

			for (int n = sl + 1; n < _logLines.Length; n++)
			{
				string s = _logLines[n];
				if (s.StartsWith("["))
					break;

				string token = GetToken(s);
				for (int paraNo = 1; paraNo <= OptParameters.Count; paraNo++) // Tokens beginnen mit #P1:
				{ 
					string paraToken = $"P{paraNo}:";
					if (token == paraToken)
					{
						BestParamValues[paraNo-1] = GetTokenContentDouble(s);
					}
				}
			}
		}

		bool ReadCancel()
		{
			int n = GetLineNo("[Cancel");
			return (n >= 0);
		}

		int GetLineNo(string key)
		{
			for (int i = 0; i < _logLines.Length; i++)
			{
				if (_logLines[i].ToLower().Contains(key.ToLower()))
					return i;
			}

			return -1;

		}

		string GetToken(string line)
		{
			if (line.StartsWith("#"))
			{
				int n = line.IndexOf(':');
				if (n > 1)
					return (line.Substring(1, n));
				else
					return null;

			}
			return null;
		}

		string GetTokenContentString(string line)
		{
			if (line.StartsWith("#"))
			{
				int n = line.IndexOf(':');
				if (n > 1)
					return (line.Substring(n+1).Trim());
				else
					return null;
			}
			return null;
		}

		int GetTokenContentInt(string line)
		{
			if (int.TryParse(GetTokenContentString(line), out int n))
			{
				return n;
			}
			return -1;
		}

		double GetTokenContentDouble(string line)
		{
			string content = GetTokenContentString(line);
			// bool-werte abfangen
			if (string.Compare(content, "false", true) == 0)
				return 0.0;
			if (string.Compare(content, "true", true) == 0)
				return 1.0;

			if (double.TryParse(content, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
			{
				return d;
			}
			return double.NaN;
		}

		#endregion

		#region Helpers

		public void ClearErrorLimits()
		{
			MinimumError = MaximumError = double.NaN;
		}

		public void CalcErrorLimits(bool absoluteErrors, int setId, List<string> optLaps)
		{
			if (!double.IsNaN(MinimumError)) //nur einmal berechnen - Neuberechnung durch ClearErrorLimits erzwingen
				return;

			double[] errors = (absoluteErrors) ? CommonErrorsAbsolute : CommonErrors;

			int num = CommonErrors.Length;

			List<double> errorList = new List<double>(num);

			for (int i = 0; i < num; i++)
			{
				if (optLaps.Contains(StepLaps[i]))
				{

					errorList.Add((setId == 0) ? errors[i] : GetSetError(setId - 1, i, absoluteErrors));
				}
			}

			MinimumError = errorList.Min();
			MaximumError = errorList.Max();
		}
		#endregion

	}
}

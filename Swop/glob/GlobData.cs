using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using SwatPresentations;
using swatSim;
using Swop.optimizer;
using TTP.Engine3;

namespace Swop.glob
{
	public enum EvalType
	{
		Start,
		Best
	};

	public class GlobData
	{

		#region Variable

		List<WeatherData>    _weathers;
		List<MonitoringData> _monitorings;
		List<SimParamData> _locParams;
		List<int> _firstIndices;
		List<int> _lastIndices;
		List<double> _evalWeightings;

		public int _firstOptIndex;
		public int _lastOptIndex;
		public Int64 NumCombinations { get; set; }
		public string SwatWorkDir { get; private set; }


		SimParamData _modelParameters;
		SimParamData _workParameters;
		List<CombiRec> _combiRecs;

		ModelBase _model;
		string _description;

		public string _cmdFilename;

		StreamWriter _swopLogger;



		#endregion

		#region  Construction + statics

		public GlobData()
		{
			Init();
			AssignWorkDir();
		}

		private void Init()
		{
			_weathers = new List<WeatherData>();
			_monitorings = new List<MonitoringData>();
			_locParams = new List<SimParamData>();
			_firstIndices = new List<int>();
			_lastIndices = new List<int>();
			_evalWeightings = new List<double>();

			_workParameters = new SimParamData();
			_combiRecs = new List<CombiRec>();

			PrologText = "";
			BestText = "";
			BestLogText = "";
			ErrorMessage = "";
		}

		private void AssignWorkDir()
		{
			//SwatWorkDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat");

			string cfgFn = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "swat.cfg");
			try
			{

				string[] fileLines = File.ReadAllLines(cfgFn);
				SwatWorkDir = fileLines[ReadCmd.GetLineNo(fileLines, "SwatDir") + 1].Trim();

			}
			catch (Exception e)
			{
				SwatWorkDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat");
			}

			string swopDir = Path.Combine(SwatWorkDir, "Swop");
			if (!Directory.Exists(swopDir))
			{
				DlgMessage.Show("Swop Konfigurationsfehler", $"das Arbeitsverzeichnis {swopDir} existiert nicht", MessageLevel.Error);
			}

		}


		public string GetPathMonitoring
		{
			get
			{
				return SwatWorkDir;
				//return Path.Combine(GetPathWorkspace, "Monitoring");
			}
		}

		public string GetPathWeather
		{
			get
			{
				return SwatWorkDir;
				//return Path.Combine(GetPathWorkspace, "Weather");
			}
		}

		public string GetPathSwop
		{
			get { return Path.Combine(SwatWorkDir, "Swop"); }
		}

		#endregion

		#region Properties

		public List<WeatherData> Weathers			{ get { return _weathers; } }
		public List<MonitoringData> Monitorings { get { return _monitorings; } }
		public List<SimParamData> LocationParameters { get { return _locParams; } }
		public List<int> FirstIndices { get { return _firstIndices; } }
		public List<int> LastIndices { get { return _lastIndices; } }
		public List<double> EvalWeightings { get { return _evalWeightings; } }
		public SimParamData WorkParameters { get { return _workParameters; } }
		public string ErrorMessage { get; set;}
		public string PrologText { get; set; }
		public string BestText { get; set; }
		public string BestLogText { get; set; }
		public string EndText { get; set; }
		public string OptStep { get; set; }
		public string StepEval { get; set; }
		public string OptLap { get; set; }
		public string LapEval { get; set; }
		public string RemainingSteps { get; set; }
		public string CombiStepsRemaining { get; set; }
		public string CombiStep { get; set; }
		public string TotalBestEval { get; set; }
		public FlyType ModelTyp { get; private set; }
		public SwopWorkMode WorkMode { get; private set; }
		public string CmdFilename { get { return _cmdFilename; } }
		public int FirstOptIndex { get { return _firstOptIndex; } }
		public int LastOptIndex { get { return _lastOptIndex; } }
		public int NumSets { get { return _weathers.Count; } }
		public List<CombiRec> CombiList  { get { return _combiRecs; } }

		#endregion

		#region Einlesen Anweisungen


		public bool ReadInstructions(string cmdFile)
		{
			Init();
			Instructions instructions = new Instructions();
			_cmdFilename = cmdFile;

			if (!instructions.Read(cmdFile))
			{
				ErrorMessage = instructions.ErrMsg;
				return false;
			}
			_description = instructions.Description;
			WorkMode = instructions.SwopMode; // Optimierung oder Parameterkombinationen

			// nur um an die Modellparameter zu kommen!
			ModelTyp = instructions.ModelTyp;
			switch (ModelTyp)
			{
				case FlyType.DR: _model = new ModelDR(null, null, null); break;
				case FlyType.PR: _model = new ModelPR(null, null, null); break;
				case FlyType.DA: _model = new ModelDA(null, null, null); break;

				default: _model = null; break;
			}
			_modelParameters = _model.CodedParams;

			for (int i = 0; i < instructions.ParameterInitials.Count; i++) // muss zuerst ausgewertet werden, damit Initialwerte übernommen werden
			{
				if (!ChangeParamInitial(instructions.ParameterInitials[i]))
					return false;
			}

			//_firstOptIndex = instructions.FirstOptIndex;
			//_lastOptIndex = instructions.LastOptIndex;

			for (int i=0; i< instructions.WeatherFilenames.Count;i++)
			{
				//if (!AddDataSet(instructions.WeatherFilenames[i], instructions.MonitoringFilenames[i], instructions.LocParamFilenames[i],
				//		instructions.EvalDates[i], instructions.EvalWeightings[i]))
				if(!AddDataSet(instructions, i))
					return false;
			}



			for (int i = 0; i < instructions.ParameterKeys.Count; i++)
			{
				if (!AddParameter(instructions.ParameterKeys[i]))
					return false;
			}

			if (WorkMode == SwopWorkMode.COMBI)
			{
				NumCombinations = 1;
				foreach (CombiRec rec in _combiRecs)
				{
					NumCombinations *= rec.Steps;
				}
				if(NumCombinations > 1000000)
				{
					ErrorMessage = $"Anzahl möglicher Kombinationen ist auf 1 Million begrenzt";
					return false;
				}
			}

			return true;
		}

		//private bool AddDataSet(string weatherFilename,string monitoringFilename, string locParamFilename, string evalDate, double weighting)
		private bool AddDataSet(Instructions instructions, int setId)
		{
			//instructions.WeatherFilenames[i], instructions.MonitoringFilenames[i], instructions.LocParamFilenames[i],
			//			instructions.EvalDates[i], instructions.EvalWeightings[i]

			WeatherData wea = new WeatherData(-1);
			string wfn = instructions.WeatherFilenames[setId];
			if (wea.ReadFromFile(Path.Combine(GetPathWeather, wfn)))
			{
				wea.SetLocation(wfn);
				_weathers.Add(wea);
			}
			else
			{
				ErrorMessage = wea.ErrorMsg;
				return false;
			}

			MonitoringData mon = MonitoringData.CreateNew(Path.Combine(GetPathMonitoring, instructions.MonitoringFilenames[setId]));
			
			if (mon.ReadFromFile())
			{
					_monitorings.Add(mon);
			}
			else
			{
				ErrorMessage = mon.ErrorMsg;
				return false;
			}

			SimParamData param = _modelParameters.Clone();

			string lfn = instructions.LocParamFilenames[setId];
			if (!string.IsNullOrEmpty(lfn))
			{
				if (param.ReadFromFile(Path.Combine(GetPathSwop, lfn)))
				{
					_locParams.Add(param);
				}
				else
				{
					ErrorMessage = param.ErrorMsg;
					return false;
				}
			}
			else
				_locParams.Add(null);


			string evalDate = instructions.EvalDates[setId];
			if(string.IsNullOrWhiteSpace(evalDate))
			{
				evalDate =  instructions.GlobDate;
			}

			TtpTimeRange tr = ReadCmd.GetTimeRangeFromShort(evalDate, wea.Year);
			if (!tr.IsValid)
			{
				ErrorMessage = $"falsche Zeitraum-Angabe: {evalDate}";
				return false;
			}

			_firstIndices.Add(tr.Start.DayOfYear);
			_lastIndices.Add(tr.End.DayOfYear);


			_evalWeightings.Add(instructions.EvalWeightings[setId]);

			return true;
		}

		private bool ChangeParamInitial(string parameterCmd)
		{
			char[] delim = new char[] { ' ', '\t' };
			string[] elems = parameterCmd.Trim().Split(delim, StringSplitOptions.RemoveEmptyEntries);

			SimParamElem para = _modelParameters.GetParamElem(elems[0]);

			if (para == null)
			{
				ErrorMessage = $"ungültiger Initial-Parameter: {elems[0]}";
				return false;
			}

			if (elems.Length < 2) // ohne Initialwert
			{
				ErrorMessage = $"kein Wert für Initial-Parameter: {elems[0]}";
				return false;
			}


			SimParamData tmpParam = _modelParameters.Clone();
			string s = $"{elems[0]}={elems[1]}";
			tmpParam.ReadFromString(s);
			if (tmpParam.HasWarnings)
			{
				ErrorMessage = $"Fehler für Initial-Parameter: {tmpParam.Warnings[0]}";
				return false;
			}

			para = tmpParam.GetParamElem(elems[0]);
			_modelParameters.AddOrReplaceItem(elems[0], para);
			return true;
		}



		private bool AddParameter(string parameterCmd)
		{
			char[] delim = new char[] { ' ', '\t' };
			string[] elems = parameterCmd.Trim().Split(delim,StringSplitOptions.RemoveEmptyEntries);

			SimParamElem para = _modelParameters.GetParamElem(elems[0]);

			if (para == null)
			{
				ErrorMessage = $"ungültiger Optimierungsparameter: {elems[0]}";
				return false;
			}

			return ((WorkMode == SwopWorkMode.LEAST)|| (WorkMode == SwopWorkMode.SHRINK)) ?
				AddOptiParameter(para, elems) :
				AddCombiParameter(para, elems);
		}

		private bool AddOptiParameter(SimParamElem para, string[] elems)
		{
			if (elems.Length == 1) // ohne Initialwert
			{
				_workParameters.AddOrReplaceItem(elems[0], para);
				return true;
			}

			// weiter mit Initialwert
			SimParamData tmpParam = _modelParameters.Clone();
			string s = $"{elems[0]}={elems[1]}";
			tmpParam.ReadFromString(s);
			if (tmpParam.HasWarnings)
			{
				ErrorMessage = $"Fehler: {tmpParam.Warnings[0]}";
				return false;
			}

			para = tmpParam.GetParamElem(elems[0]);
			_workParameters.AddOrReplaceItem(elems[0], para);
			return true;
		}

		private bool AddCombiParameter(SimParamElem para, string[] elems)
		{			
			_workParameters.AddOrReplaceItem(elems[0], para);

			if ( elems.Length <4)
			{
				ErrorMessage = $"{elems[0]}: Angaben unvollständig";
				return false;
			}

			if (para.ObjType == typeof(bool)) // bool-Parameter nicht auswerten - ergibt sich zwangsläufig
			{
				_combiRecs.Add(new CombiRec(para, elems[0], 0, 1, 2));
				return true;
			}

			bool err = false;
			if (Double.TryParse(elems[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double mini))
			{
				if (mini < para.MinVal)
				{
					ErrorMessage = $"{elems[0]}: Parameter unterschreitet zulässigen Minimalwert von {para.MinVal}";
					return false;
				}
			}
			else
				err = true;

			if (Double.TryParse(elems[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double maxi))
			{
				if (maxi > para.MaxVal)
				{
					ErrorMessage = $"{elems[0]}: Parameter überschreitet zulässigen Maximalwert von { para.MaxVal}";
					return false;
				}
			}
			else
				err = true;

			if (Int32.TryParse(elems[3], NumberStyles.Float, CultureInfo.InvariantCulture, out Int32 count))
			{
				if ((count < 2) || (count > 25))
				{
					ErrorMessage = $"{elems[0]}: Schrittzahl muss zwischen 2 und 25 liegen";
					return false;
				}
			}
			else
				err = true;

			if (err)
			{
				ErrorMessage = $"{para}: Angaben können nicht ausgewertet werden";
				return false;
			}

			_combiRecs.Add(new CombiRec(para, elems[0], mini, maxi, count));
			return true;
		}

		#endregion

		#region Datei-Log  und Displayanzeige

		// Methoden mit ...Display... für Bildschirmanzeige, mit ...Log... für Ausgabe in Log-Datei

		private string GetEvalTimerangeText(int firstIndex, int lastIndex, int year)
		{
			DateTime start = DateTime.Parse("1.1."+year);
			DateTime end = start;
			start = start.AddDays(firstIndex-1);
			end = end.AddDays(lastIndex-1);

			return start.ToString("dd.MM") + " - " + end.ToString("dd.MM");
		}

		public void CreatePrologDisplayText(MultiOptimizer mo)
		{
			StringBuilder pt = new StringBuilder();

			for (int i = 0; i < NumSets; i++)
			{
				if (LocationParameters[i] != null)
					pt.Append($"Set{i + 1} = {Monitorings[i].Location} , {Weathers[i].Filename},{LocationParameters[i].Filename}");
				else
					pt.Append($"Set{i + 1} = {Monitorings[i].Location},{Weathers[i].Filename}");

				pt.AppendLine($", Eval-Time: {GetEvalTimerangeText(FirstIndices[i], LastIndices[i], Weathers[i].Year)}, Eval-Weight: {EvalWeightings[i]}");
			}
			pt.AppendLine();

			pt.AppendLine("Parameter");
			int p = 1;
			foreach (string s in WorkParameters.ParamDict.Keys)
			{
				SimParamElem elem = WorkParameters.ParamDict[s];
				string desc = elem.Descr;
				string val;
				if (Type.GetTypeCode(elem.ObjType) == TypeCode.Double)
					val = ((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture);
				else
					val = elem.Obj.ToString();

				pt.AppendLine($"P{p} : {s} = {val} ({desc})");
				p++;
			}
			pt.AppendLine();

			pt.AppendLine("Evaluierungszeitraum: " + mo.GetEvalTimerangeText());
			pt.AppendLine();

			PrologText = pt.ToString();
		}

		public void CreatePrologDisplayText(MultiCombiner co)
		{
			StringBuilder pt = new StringBuilder();

			for (int i = 0; i < NumSets; i++)
			{
				if (LocationParameters[i] != null)
					pt.Append($"Set{i + 1} = {Monitorings[i].Location} , {Weathers[i].Filename},{LocationParameters[i].Filename}");
				else
					pt.Append($"Set{i + 1} = {Monitorings[i].Location},{Weathers[i].Filename}");

				pt.AppendLine($", Eval-Time: {GetEvalTimerangeText(FirstIndices[i], LastIndices[i], Weathers[i].Year)}, Eval-Weight: {EvalWeightings[i]}");
			}
			pt.AppendLine();

			pt.AppendLine("Parameter");
			int p = 1;
			foreach (string s in WorkParameters.ParamDict.Keys)
			{
				SimParamElem elem = WorkParameters.ParamDict[s];
				string desc = elem.Descr;
				string val;
				if (Type.GetTypeCode(elem.ObjType) == TypeCode.Double)
					val = ((Double)elem.Obj).ToString("0.0###", CultureInfo.InvariantCulture);
				else
					val = elem.Obj.ToString();

				pt.AppendLine($"P{p} : {s} = {val} ({desc})");
				p++;
			}
			pt.AppendLine();

			pt.AppendLine("Evaluierungszeitraum: " + co.GetEvalTimerangeText());
			pt.AppendLine();


			pt.AppendLine("Parameters :");

			Int64 n = 1;
			foreach (CombiRec rec in _combiRecs)
			{
				pt.AppendLine(rec.Key + ": " + rec.MinVal.ToString("0.0###", CultureInfo.InvariantCulture) + "  -  " + rec.MaxVal.ToString("0.0###", CultureInfo.InvariantCulture) + "    " + rec.Steps + " Steps");
				n *= rec.Steps;
			}
			pt.AppendLine("Total Steps: " + n);
			PrologText = pt.ToString();

		}

		public void WritePrologLogText()
		{
			StringBuilder pt = new StringBuilder();
			pt.AppendLine("[SwopMode]");
			pt.AppendLine($"  {WorkMode.ToString()}");

			pt.AppendLine("[Descr]");
			pt.AppendLine($"  {_description}");

			pt.AppendLine("[Model]");
			pt.AppendLine($"  {ModelTyp.ToString()}");

			AppendLogDefParams(pt);
			AppendLogSets(pt);
			AppendLogWorkParams(pt);
			WriteTextToLog(pt.ToString());
		}

		private void AppendLogDefParams(StringBuilder pt)
		{
			// alle Default-Modellparameter
			pt.AppendLine("[Default-Parameters]");
			var list = _modelParameters.ParamDict.Keys.ToList();
			foreach (var key in list)
			{
				if (_modelParameters.ParamDict[key].ObjType == typeof(double))
				{
					pt.AppendLine($"  {key} = {((double)(_modelParameters.ParamDict[key].Obj)).ToString("0.###", CultureInfo.InvariantCulture)}");
				}
				else
					pt.AppendLine($"  {key} = {((IConvertible)_modelParameters.ParamDict[key].Obj).ToString(CultureInfo.InvariantCulture)}");
			}
		}

		private void AppendLogSets(StringBuilder pt)
		{
			//Daten-Sets mit allen Angaben u Parametern
			pt.AppendLine("[Sets]");
			for (int i = 0; i < NumSets; i++)
			{
				pt.AppendLine($"  #S{i + 1}:");
				pt.AppendLine($"    #W:{Weathers[i].Filename}");
				pt.AppendLine($"    #M:{Monitorings[i].Location}");
				pt.AppendLine($"    #T:{GetEvalTimerangeText(FirstIndices[i], LastIndices[i], Weathers[i].Year)}");
				pt.AppendLine($"    #E:{EvalWeightings[i].ToString(CultureInfo.InvariantCulture)}");
				
				// Ausgabe lokaler, datenset-spezifischer Parameter ('#P:' im Swop-Command- File)
				SimParamData sim = LocationParameters[i];
				if (sim != null)
				{
					var simList = sim.ParamDict.Keys.ToList();
					foreach (var key in simList)
					{
						if (sim.ParamDict[key].Obj.ToString() == _modelParameters.ParamDict[key].Obj.ToString())
							continue; // nur vom Modell-Standard abweichende Werte aufnehmen

						if (sim.ParamDict[key].ObjType == typeof(double))
						{
							pt.AppendLine($"    #P:{key} = {((double)(sim.ParamDict[key].Obj)).ToString("0.####", CultureInfo.InvariantCulture)}");
						}
						else
							pt.AppendLine($"    #P:{key} = {((IConvertible)sim.ParamDict[key].Obj).ToString(CultureInfo.InvariantCulture)}");
					}
				}
			}
		}
		
		private void AppendLogWorkParams(StringBuilder pt)
		{
			pt.AppendLine("[Opt-Parameters]");
			var opList = WorkParameters.ParamDict.Keys.ToList();
			int n = 0;
			foreach (var key in opList)
			{
				if ((WorkMode == SwopWorkMode.LEAST) ||(WorkMode == SwopWorkMode.SHRINK))
				{
					pt.AppendLine($"  #P{n + 1}:{key}"); 
				}
				else // combi
				{
					pt.AppendLine($"  #P{n + 1}:{key}   {_combiRecs[n].MinVal.ToString(CultureInfo.InvariantCulture)}  {_combiRecs[n].MaxVal.ToString(CultureInfo.InvariantCulture)}   {_combiRecs[n].Steps}");
				}

				n++;
			}
		}


		public void WriteLogEvals(double[]evals, EvalType et )
		{
			StringBuilder pt = new StringBuilder();

			pt.AppendLine((et == EvalType.Start)? "[Start-Evals]": "[Best-Evals]");

			pt.AppendLine($"    #T:" + evals[0].ToString("F4", CultureInfo.InvariantCulture));

			for (int i = 1; i <= NumSets; i++)
			{
				pt.AppendLine($"    #S{i}:" + evals[i].ToString("F4", CultureInfo.InvariantCulture));
			}

			WriteTextToLog(pt.ToString());
		}

		public void WriteLogParams(SimParamData para, EvalType paraType)
		{
			string chapter = (paraType == EvalType.Best) ? "[Best-Params]" : "[Start-Params]";
			bool doCloseFile = (paraType == EvalType.Best);

			StringBuilder pt = new StringBuilder();
			pt.AppendLine(chapter);
			int p = 1;
			foreach (string key in para.ParamDict.Keys)
			{
				SimParamElem elem = para.ParamDict[key];
				string es = (Type.GetTypeCode(elem.ObjType) == TypeCode.Double) ?
					((Double)elem.Obj).ToString("F4", CultureInfo.InvariantCulture) :
					elem.Obj.ToString();

				pt.AppendLine($"    #P{p}:" + es);
				p++;
			}
			WriteTextToLog(pt.ToString(), doCloseFile);
		}

		public void WriteLogCancel()
		{
			WriteTextToLog("[Cancel]\r\n");
		}

		string BuildSwopLogFileName()
		{
			string fn = Path.GetFileNameWithoutExtension(CmdFilename) + "_*.swp-log";
			int logId = FindValidFileIndex(Path.GetDirectoryName(CmdFilename), fn);
			string logFn = Path.GetFileNameWithoutExtension(CmdFilename) + "_" + logId + ".swp-log";
			return Path.Combine(Path.GetDirectoryName(CmdFilename), logFn);
		}

		public void WriteTextToLog(string txt, bool doCloseFile = false)
		{
			if (_swopLogger == null)
			{
				string s = BuildSwopLogFileName();
				_swopLogger = new StreamWriter(File.Open(s, FileMode.CreateNew), Encoding.UTF8);
			}

			_swopLogger.Write(txt);
			_swopLogger.Flush();

			if(doCloseFile)
			{
				_swopLogger.Close();
				_swopLogger = null;
			}
		}

		#endregion

		#region utility
		
		//name hat die Form:Pfad\fn_x.ext; -> es wird das nächsthöhere freie x zurückgegeben
		private int FindValidFileIndex(string path, string pattern)
		{
			try
			{
				string[] repFiles = Directory.GetFiles(path, pattern);
				if (repFiles.Length == 0)
					return 0;

				int maxIndex = 0;
				foreach (string fn in repFiles)
				{
					int pos = fn.LastIndexOf('_');
					int posp = fn.LastIndexOf('.');
					if (pos != -1)
					{
						int fi = Convert.ToInt32(fn.Substring(pos + 1, posp - pos - 1));
						if (fi > maxIndex)
							maxIndex = fi;
					}
				}
				return maxIndex + 1;
			}
			catch
			{
				return 0;
			}
		}

		#endregion

	}
}

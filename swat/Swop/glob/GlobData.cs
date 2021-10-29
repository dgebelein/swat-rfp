//using Swop.data;
//using Swop.defs;
//using Swop.sim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using swatSim;
using Swop.optimizer;

namespace Swop.glob
{
	enum LogMode
	{
		Prolog,
		Action,
		End
	};
	public enum EvalType
	{
		Start,
		Best
	};

	public class GlobData
	{

		#region Variable
		List<PopulationData> _populations;
		List<WeatherData>    _weathers;
		List<MonitoringData> _monitorings;
		List<SimParamData> _locParams;
		List<int> _firstIndices;
		List<int> _lastIndices;
		List<double> _evalWeightings;

		public int _firstOptIndex;
		public int _lastOptIndex;
		//ModelType _modelType = ModelType.DR;

		SimParamData _modelParameters;
		SimParamData _parameters;

		//string _errMessage;
		//string _actionText;
		//string _bestText;
		//string _prologText;
		ModelBase _model;

		public string _cmdFilename;

		//StreamWriter _logWriter;
		StreamWriter _swopLogger;



		#endregion

		#region  Construction + statics

		public GlobData()
		{
			Init();
		}

		private void Init()
		{
			_populations = new List<PopulationData>();
			_weathers = new List<WeatherData>();
			_monitorings = new List<MonitoringData>();
			_locParams = new List<SimParamData>();
			_firstIndices = new List<int>();
			_lastIndices = new List<int>();
			_evalWeightings = new List<double>();

			_parameters = new SimParamData();

			PrologText = "";
			BestText = "";
			BestLogText = "";
			//ActionText = "";
			//BestParamText = "";
			ErrorMessage = "";
		}

		public static string GetPathWorkspace
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat"); }
		}

		public static string GetPathMonitoring
		{
			get
			{
				return GetPathWorkspace;
				//return Path.Combine(GetPathWorkspace, "Monitoring");
			}
		}

		public static string GetPathWeather
		{
			get
			{
				return GetPathWorkspace;
				//return Path.Combine(GetPathWorkspace, "Weather");
			}
		}

		public static string GetPathSwop
		{
			get { return Path.Combine(GetPathWorkspace, "Swop"); }
		}

		#endregion

		#region Properties

		public List<PopulationData> Populations { get { return _populations; } }
		public List<WeatherData> Weathers			{ get { return _weathers; } }
		public List<MonitoringData> Monitorings { get { return _monitorings; } }
		public List<SimParamData> LocationParameters { get { return _locParams; } }
		public List<int> FirstIndices { get { return _firstIndices; } }
		public List<int> LastIndices { get { return _lastIndices; } }
		public List<double> EvalWeightings { get { return _evalWeightings; } }


		public SimParamData Parameters		{ get { return _parameters; }}

		public string ErrorMessage { get; set;}
		public string PrologText { get; set; }
		public string HeadlineText { get; set; }
		public string BestText { get; set; }
		public string BestLogText { get; set; }
		//public string ActionText { get; set; }
		//public string ActionLogText { get; set; }
		//public string BestParamText { get; set; }

		public string EndText { get; set; }
		public string OptStep { get; set; }
		public string StepEval { get; set; }
		public string OptLap { get; set; }
		public string LapEval { get; set; }
		public string RemainingSteps { get; set; }
		public string TotalBestEval { get; set; }

		public ModelType ModelTyp { get; private set; }


		public string CmdFilename { get { return _cmdFilename; } }
		public int FirstOptIndex { get { return _firstOptIndex; } }
		public int LastOptIndex { get { return _lastOptIndex; } }
		public int NumSets { get { return _weathers.Count; } }


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

			// nur um an die Modellparameter zu kommen!
			ModelTyp = instructions.ModelTyp;
			switch (ModelTyp)
			{
				case ModelType.DR: _model = new ModelDR(null, null, null); break;
				case ModelType.PR: _model = new ModelPR(null, null, null); break;
				case ModelType.DA: _model = new ModelDA(null, null, null); break;

				default: _model = null; break;
			}
			_modelParameters = _model.DefaultParams;

			_firstOptIndex = instructions.FirstOptIndex;
			_lastOptIndex = instructions.LastOptIndex;

			for (int i=0; i< instructions.WeatherFilenames.Count;i++)
			{
				int fi = instructions.FirstIndices[i];
				int li = instructions.LastIndices[i];
				if ((fi < 0) &&(li < 0))
				{
					fi = instructions.FirstOptIndex;
					li = instructions.LastOptIndex;
				}
				if (!AddLocation(instructions.WeatherFilenames[i], instructions.MonitoringFilenames[i], instructions.LocParamFilenames[i], 
						instructions.FirstIndices[i], instructions.LastIndices[i], instructions.EvalWeightings[i]))
					return false;
			}

			for (int i = 0; i < instructions.ParameterKeys.Count; i++)
			{
				if (!AddParameter(instructions.ParameterKeys[i]))
					return false;
			}

			return true;
		}


		private bool AddLocation(string weatherFilename,string monitoringFilename, string locParamFilename, int firstIndex, int lastIndex, double weighting)
		{
			WeatherData wea = new WeatherData(-1);
			if(wea.ReadFromFile(Path.Combine(GetPathWeather, weatherFilename)))
			{
				wea.SetLocation(weatherFilename);
				_weathers.Add(wea);
			}
			else
			{
				ErrorMessage = wea.ErrorMsg;
				return false;
			}

			MonitoringData mon = MonitoringData.CreateNew(Path.Combine(GetPathMonitoring, monitoringFilename));
			
			//if (mon.ReadFromFile(Path.Combine(GetPathMonitoring, monitoringFilename)))
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
			//SimParameters param = _modelParameters;

			if (!string.IsNullOrEmpty(locParamFilename))
			{
				if (param.ReadFromFile(Path.Combine(GetPathSwop, locParamFilename),true))
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

			if((firstIndex < 0) && (lastIndex < 0))
			{
				firstIndex = _firstOptIndex;
				lastIndex = _lastOptIndex;
			}
			_firstIndices.Add(firstIndex);
			_lastIndices.Add(lastIndex);

			_evalWeightings.Add(weighting);

			return true;
		}

		private bool AddParameter(string parameterKey)
		{
			// trennen = einsetzen oder init übernehmen
			char[] delim = new char[] { ' ', '\t' };
			string[] elems = parameterKey.Trim().Split(delim,StringSplitOptions.RemoveEmptyEntries);

			SimParamElem elem = _modelParameters.GetParamElem(elems[0]);

			if(elem == null)
			{
				ErrorMessage = $"ungültiger Optimierungsparameter: {elems[0]}";
				return false;
			}

			if(elems.Length == 1) // ohne Initialwert
			{ 
				_parameters.AddOrReplaceItem(parameterKey, elem);
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

			elem = tmpParam.GetParamElem(elems[0]);
			_parameters.AddOrReplaceItem(elems[0], elem);
			return true;
			

		}

		#endregion

		#region Log  und Dateiausgabe


		private string GetEvalTimerangeText(int firstIndex, int lastIndex)
		{
			DateTime start = DateTime.Parse("1.1.1995");
			DateTime end = start;
			start = start.AddDays(firstIndex);
			end = end.AddDays(lastIndex);

			return start.ToString("dd.MM") + " - " + end.ToString("dd.MM");
		}

		public void CreatePrologText(MultiOptimizer mo)
		{
			StringBuilder pt = new StringBuilder();

			for (int i = 0; i < NumSets; i++)
			{
				if (LocationParameters[i] != null)
					pt.Append($"Set{i + 1} = {Monitorings[i].Location} , {Weathers[i].Filename},{LocationParameters[i].Filename}");
				else
					pt.Append($"Set{i + 1} = {Monitorings[i].Location},{Weathers[i].Filename}");

				pt.AppendLine($", Eval-Time: {GetEvalTimerangeText(FirstIndices[i], LastIndices[i])}, Eval-Weight: {EvalWeightings[i]}");
			}
			pt.AppendLine();

			pt.AppendLine("Parameter");
			int p = 1;
			foreach (string s in Parameters.ParamDict.Keys)
			{
				SimParamElem elem = Parameters.ParamDict[s];
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

		public void WriteSwopPrologText()
		{
			StringBuilder pt = new StringBuilder();
			pt.AppendLine("[Model]");
			pt.AppendLine($"  {ModelTyp.ToString()}");


			// Modellparameter- init
			pt.AppendLine("[Default-Parameters]");
			var list = _modelParameters.ParamDict.Keys.ToList();
			foreach (var key in list)
			{
				string printKey = key;//.Substring(key.IndexOf('.') + 1);
				if (_modelParameters.ParamDict[key].ObjType == typeof(double))
				{
					pt.AppendLine($"  {printKey} = {((double)(_modelParameters.ParamDict[key].Obj)).ToString("0.###", CultureInfo.InvariantCulture)}");
				}
				else
					pt.AppendLine($"  {printKey} = {((IConvertible)_modelParameters.ParamDict[key].Obj).ToString(CultureInfo.InvariantCulture)}");

			}


			//Sets mit allen Angaben u Parametern
			pt.AppendLine("[Sets]");
			for ( int i=0; i< NumSets; i++)
			{
				pt.AppendLine($"  #S{i+1}:");
				pt.AppendLine($"    #W:{Weathers[i].Filename}");
				pt.AppendLine($"    #M:{Monitorings[i].Location}");
				pt.AppendLine($"    #T:{GetEvalTimerangeText(FirstIndices[i], LastIndices[i])}");
				pt.AppendLine($"    #E:{EvalWeightings[i].ToString(CultureInfo.InvariantCulture)}");
				SimParamData sim = LocationParameters[i];
				if(sim != null)
				{ 
					var simList = sim.ParamDict.Keys.ToList();
					foreach (var key in simList)
					{
						if (sim.ParamDict[key].Obj.ToString() == _modelParameters.ParamDict[key].Obj.ToString())
							continue;
						string printKey = key;//.Substring(key.IndexOf('.') + 1);
						if (sim.ParamDict[key].ObjType == typeof(double))
						{
							pt.AppendLine($"    #P:{printKey} = {((double)(sim.ParamDict[key].Obj)).ToString("0.####", CultureInfo.InvariantCulture)}");
						}
						else
							pt.AppendLine($"    #P:{printKey} = {((IConvertible)sim.ParamDict[key].Obj).ToString(CultureInfo.InvariantCulture)}");

					}
				}

			}

			pt.AppendLine("[Opt-Parameters]");
			var opList = Parameters.ParamDict.Keys.ToList();
			int n = 1;
			foreach (var key in opList)
			{
				pt.AppendLine($"  #P{n++}:{key}");
			}



			SwopLog(pt.ToString());
		}

		public void WriteSwopLogEvals(double[]evals, EvalType et )
		{
			StringBuilder pt = new StringBuilder();

			pt.AppendLine((et == EvalType.Start)? "[Start-Evals]": "[Best-Evals]");

			pt.AppendLine($"    #T:" + evals[0].ToString("F4", CultureInfo.InvariantCulture));

			for (int i = 1; i <= NumSets; i++)
			{
				pt.AppendLine($"    #S{i}:" + evals[i].ToString("F4", CultureInfo.InvariantCulture));
			}

			SwopLog(pt.ToString());
		}


		public void WriteSwopLogStartParams(SimParamData para)
		{
			StringBuilder pt = new StringBuilder();
			pt.AppendLine("[Start-Params]");
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
			SwopLog(pt.ToString(), false);
		}

		public void WriteSwopLogBestParams(SimParamData para)
		{
			StringBuilder pt = new StringBuilder();
			pt.AppendLine("[Best-Params]");
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
			SwopLog(pt.ToString(), true);
		}

		public void WriteSwopLogCancel()
		{
			SwopLog("[Cancel]\r\n");
		}

	//public void AppendActionText(string txt)
	//{
	//	ActionText += txt;

	//	//if(step>0) // damit Initialsimulation nicht im Log erscheint
	//	//	Log(txt);

	//}


		string GetLogFileName()
		{
			string fn = Path.GetFileNameWithoutExtension(CmdFilename) + "_*.csv";
			int logId = FindValidFileIndex(Path.GetDirectoryName(CmdFilename), fn);
			string logFn = Path.GetFileNameWithoutExtension(CmdFilename) + "_" + logId + ".csv";
			return Path.Combine(Path.GetDirectoryName(CmdFilename), logFn);
		}


		string GetSwopLogFileName()
		{
			string fn = Path.GetFileNameWithoutExtension(CmdFilename) + "_*.swp-log";
			int logId = FindValidFileIndex(Path.GetDirectoryName(CmdFilename), fn);
			string logFn = Path.GetFileNameWithoutExtension(CmdFilename) + "_" + logId + ".swp-log";
			return Path.Combine(Path.GetDirectoryName(CmdFilename), logFn);
		}

		//public void LogHeadline()
		//{
		//	Log(HeadlineText);
		//}

		//public void LogEnd()
		//{
		//	Log(EndText);
		//}

		//public void LogBest()
		//{	Log(PrologText);
		//	Log("Bestwerte" + Environment.NewLine + BestLogText);
		//	Log(BestParamText);

		//	_logWriter.Close();
		//	_logWriter = null;
		//}

		//public void Log(string txt)
		//{
		//	if(_logWriter == null)
		//	{
		//		string s = GetLogFileName();
		//		_logWriter = new StreamWriter(File.Open(s, FileMode.CreateNew), Encoding.UTF8);
		//	}

		//	_logWriter.Write(txt);
		//	_logWriter.Flush();
		//}

		public void SwopLog(string txt, bool doCloseFile = false)
		{
			if (_swopLogger == null)
			{
				string s = GetSwopLogFileName();
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

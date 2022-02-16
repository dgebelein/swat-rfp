using swatSim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace SwopCompare
{
	class CmpSet
	{
		public WeatherData Weather { get; private set; }
		public MonitoringData Monitoring { get; private set; }
		public List<SimParamData> ParamsList { get; private set; }
		public List<string> CommentList { get; private set; }

		public TtpTimeRange EvalTimeRange { get; private set; }

		public string Notes { get; private set; }
		public string ErrorMessage { get; private set; }

		//FlyType _modelType;
		ModelBase _model;



		public CmpSet(ModelBase model)
		{
			ParamsList = new List<SimParamData>();
			CommentList = new List<string>();
			_model = model;
		}

		private bool AddWeather(string line)
		{
			string weatherfile = IsolateContent(line, "#w:");
			if (weatherfile == null)
			{
				ErrorMessage = "Wetterdaten fehlen" + " (" + line + ")";
				return false;
			}

			Weather = new WeatherData(-1);
			if (Weather.ReadFromFile(Path.Combine(GetPathWeather, weatherfile)))
			{
				Weather.SetLocation(weatherfile);
				return true;
			}
			else
			{
				ErrorMessage = Weather.ErrorMsg;
				return false;
			}
		}

		private bool AddMonitoring(string line)
		{
			string monitoringfile = IsolateContent(line, "#m:");
			if (monitoringfile == null)
			{
				ErrorMessage = "Monitoringdaten fehlen" + " (" + line + ")";
				return false;
			}

			Monitoring = MonitoringData.CreateNew(Path.Combine(GetPathMonitoring, monitoringfile));

			if (Monitoring.ReadFromFile())
			{
				return true;
			}
			else
			{
				ErrorMessage = Monitoring.ErrorMsg;
				return false;
			}
		}

		private bool AddTimeRange(string line)
		{
			string tr = IsolateContent(line, "#d:");
			if (tr == null)
			{
				tr = "01.01-31.12";// ganzes Jahr, wenn nicht vorh.
			}
			string[] elems = tr.Trim().Split('-');

			if (elems.Length != 2)
			{
				ErrorMessage = $"falsche Zeitraum-Angabe: {tr}";
				return false;
			}

			if (DateTime.TryParseExact(elems[0].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime first) &&
				DateTime.TryParseExact(elems[1].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime last))
			{
				TtpTime start = new TtpTime($"1.1.{Weather.Year}");
				start.Inc(TtpEnPattern.Pattern1Day, first.DayOfYear - 1);
				TtpTime end = new TtpTime($"1.1.{Weather.Year}");
				end.Inc(TtpEnPattern.Pattern1Day, last.DayOfYear - 1);
				EvalTimeRange = new TtpTimeRange(start, end, TtpEnPattern.Pattern1Day);
				return true;
			}
			else
			{
				ErrorMessage = $"falsche Zeitraum-Angabe: {tr}";
				return false;
			}
		}

		private void AddNotes()
		{
			string notesName = System.IO.Path.Combine(GetPathNotes, Path.GetFileNameWithoutExtension(Monitoring.Filename) + " - Notes.txt");

			if (!File.Exists(notesName))
				Notes="";

			try
			{
				Notes=File.ReadAllText(notesName, Encoding.UTF8);
			}
			catch 
			{
				Notes = "";
			}
		}


		private bool AddParams(string line)
		{
			string paramsfile = IsolateContent(line, "#p:");
			if (paramsfile == null)
			{
				ErrorMessage = "Parameter-File fehlt" + " (" + line + ")";
				return false;
			}

			if (!string.IsNullOrEmpty(paramsfile))
			{
				SimParamData cParams = _model.CodedParams.Clone();
				if (cParams.ReadFromFile(Path.Combine(GetPathSwop, paramsfile)))
				{
					ParamsList.Add(cParams);
				}
				else
				{
					ErrorMessage = cParams.ErrorMsg;
					return false;
				}
			}
			
			string comment = IsolateContent(line, "#c:");
			if (comment == null) // wenn nicht vorh. mit Dateinamen ersetzen
			{
				comment = paramsfile;
			}
			CommentList.Add(comment);


			return true;
		}

		public bool Read(string[] lines,int startIndex)
		{
			for (int n = startIndex; n < lines.Length; n++)
			{
				if (lines[n].StartsWith("["))  // nächster set
					break;

				if(Weather == null) //Wetter + Monitoring  zuerst
				{
					if (!AddWeather(lines[n]) || (!AddMonitoring(lines[n])) || !AddTimeRange(lines[n]))
						return false;
					AddNotes();

				}
				else  // beliebig viele Parameter
				{
					if (!AddParams(lines[n]))
						return false;
				}

			}

			return string.IsNullOrEmpty(ErrorMessage);

		}


		#region static helpers

		public static string GetPathSwat
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat"); }
		}

		public static string GetPathSwop
		{
			get { return Path.Combine(GetPathSwat, "Swop"); }
		}

		public static string GetPathWeather
		{
			get { return GetPathSwat; }
		}

		public static string GetPathMonitoring
		{
			get { return GetPathSwat; }
		}

		public static string GetPathNotes
		{
			get { return GetPathSwat; }
		}

		public static string GetPathParams
		{
			get { return GetPathSwop; }
		}

		static public int GetLineNo(string[] lines, string key, int rep = 0) // rep = die x-te Wiederholung von key (0 = 1.Vorkommen)
		{
			int n = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				string s = lines[i].Trim();
				if (s.StartsWith("--"))
					continue;
				
				if (s.ToLower().Contains(key.ToLower()))
				{ 
					if(++n > rep)
						return i;
				}
			}

			return -1;
		}

		static public string IsolateContent(string sourceLine, string key)
		{
			string source = sourceLine.ToLower();
			if (source.Trim().StartsWith("--"))
				return null;
			if (source.ToLower().Contains(key))
			{
				int startPos = source.IndexOf(key, 0) + key.Length;

				int endPos = source.IndexOf("#", startPos);
				if (endPos <= 0)
					endPos = source.Length;

				return (source.Substring(startPos, endPos - startPos).Trim());

			}
			else
				return null;

		}
		#endregion

		#region helper

		//string GetToken(string line)
		//{
		//	if (line.StartsWith("#"))
		//	{
		//		int n = line.IndexOf(':');
		//		if (n > 1)
		//			return (line.Substring(1, n));
		//		else
		//			return null;

		//	}
		//	return null;
		//}

		//string GetTokenContentString(string line)
		//{
		//	if (line.StartsWith("#"))
		//	{
		//		int n = line.IndexOf(':');
		//		if (n > 1)
		//			return (line.Substring(n + 1).Trim());
		//		else
		//			return null;
		//	}
		//	return null;
		//}
		#endregion

	}
}

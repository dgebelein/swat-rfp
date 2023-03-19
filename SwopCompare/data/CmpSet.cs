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
		public SimParamData CommonParams { get; private set; }
		public List<SimParamData> ParamsList { get; private set; }
		public List<string> ParamsText { get; private set; }

		public List<string> CommentList { get; private set; }

		public TtpTimeRange EvalTimeRange { get; private set; }

		public string Notes { get; private set; }
		public string ErrorMessage { get; private set; }

		
		ModelBase _model;
		string _swatPath;



		public CmpSet(ModelBase model, string workDir)
		{
			ParamsList = new List<SimParamData>();
			ParamsText = new List<string>();
			CommentList = new List<string>();
			_model = model;
			_swatPath = workDir;
		}

		private bool AddWeather(string line)
		{
			string weatherfile = ReadCmd.IsolateContent(line, "#w:");
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
			string monitoringfile = ReadCmd.IsolateContent(line, "#m:");
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

		private bool AddCommonParams(string line)
		{
			CommonParams = _model.CodedParams.Clone();

			string paramsfile = ReadCmd.IsolateContent(line, "#p:");

			if (!string.IsNullOrEmpty(paramsfile))
			{
				if (CommonParams.ReadFromFile(Path.Combine(GetPathSwop, paramsfile)))
				{
					return true;
				}
				else
				{
					ErrorMessage = CommonParams.ErrorMsg;
					return false;
				}
			}
			return true;

		}

		private bool AddTimeRange(string line)
		{
			string tr = ReadCmd.IsolateContent(line, "#d:");
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
			string paramsfile = ReadCmd.IsolateContent(line, "#p:");
			if (paramsfile == null)
			{
				ErrorMessage = "Parameter-File fehlt" + " (" + line + ")";
				return false;
			}

			if (!string.IsNullOrEmpty(paramsfile))
			{
				SimParamData cParams = CommonParams.Clone();
				if (cParams.ReadFromFile(Path.Combine(GetPathSwop, paramsfile)))
				{
					ParamsList.Add(cParams);
					ParamsText.Add(GetParamsText(Path.Combine(GetPathSwop, paramsfile)));
				}
				else
				{
					ErrorMessage =$"{paramsfile} : {cParams.ErrorMsg}";
					return false;
				}
			}
			
			string comment = ReadCmd.IsolateContent(line, "#c:");
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
					if (!AddWeather(lines[n]) || (!AddMonitoring(lines[n]))  || !AddCommonParams(lines[n])|| !AddTimeRange(lines[n]))
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

		private string GetParamsText(string  filename)
		{
			return File.ReadAllText(filename).Trim();
		}

		#region helpers für Dateipfade

		string GetPathSwat
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat"); }
		}

		string GetPathSwop
		{
			get { return Path.Combine(_swatPath, "Swop"); }
		}

		 string GetPathWeather
		{
			get { return _swatPath; }
		}

		string GetPathMonitoring
		{
			get { return _swatPath; }
		}

		string GetPathNotes
		{
			get { return GetPathSwat; }
		}

		#endregion



	}
}

//using Swop.defs;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using swatSim;

namespace Swop.glob

{
	class Instructions
	{
		public List<string> WeatherFilenames;
		public List<string> MonitoringFilenames;
		public List<string> LocParamFilenames;
		public List<int> FirstIndices;
		public List<int> LastIndices;
		public List<double> EvalWeightings;

		public List<string> ParameterKeys;
		public FlyType ModelTyp;
		public SwopWorkMode SwopMode;
		public string Description;
		public int FirstOptIndex;
		public int LastOptIndex;

		public string ErrMsg;
		public int ErrLineNo;

		public Instructions()
		{
			WeatherFilenames = new List<string>();
			MonitoringFilenames = new List<string>();
			LocParamFilenames = new List<string>();

			FirstIndices = new List<int>();
			LastIndices = new List<int>();
			ParameterKeys = new List<string>();
			EvalWeightings = new List<double>();

		}

		public int NumLocations
		{
			get { return WeatherFilenames.Count;}
		}


		private string IsolateContent(string sourceLine,string key)
		{
			string source = sourceLine.ToLower();
			if (source.ToLower().Contains(key))
			{
				int startPos = source.IndexOf(key, 0) + key.Length;

				int endPos = source.IndexOf("#", startPos);
				if(endPos <=0 )
					endPos = source.Length;

				return (source.Substring(startPos, endPos - startPos).Trim());

			}
			else
				return null;

		}

		private void ReadSet(string line)
		{
			string weatherfile = IsolateContent(line, "#w:");
			if (weatherfile== null)
			{
				ErrMsg = "Wetterdaten fehlen"+ " (" + line + ")";
				return;
			}
			WeatherFilenames.Add(weatherfile);

			string monitoringfile = IsolateContent(line, "#m:");
			if (monitoringfile == null)
			{
				ErrMsg = "Monitoringdaten fehlen" + " (" + line + ")";
				return;
			}
			MonitoringFilenames.Add(monitoringfile);

			string paramsfile = IsolateContent(line, "#p:");
			if (paramsfile != null)
			{
				LocParamFilenames.Add(paramsfile);
			}
			else
				LocParamFilenames.Add("");


			string datesString = IsolateContent(line, "#d:");
			if (datesString != null)
			{
				Tuple<int, int> indices = GetLocIndices(datesString);
				if ((indices.Item1 < 0) || (indices.Item2 < 0))
				{
					ErrMsg = ErrMsg = "falsche Zeitbegrenzung" + " (" + line + ")";
					return;
				}
				else
				{
					FirstIndices.Add(indices.Item1);
					LastIndices.Add(indices.Item2);
				}
			}
			else
			{
				FirstIndices.Add(-1);
				LastIndices.Add(-1);
			}

			string weightString = IsolateContent(line, "#e:");
			if (weightString != null)
			{
				if(double.TryParse(weightString, NumberStyles.Number, CultureInfo.InvariantCulture, out double d))
				{
					if ((d >= 0.0) && (d <= 10.0))
						EvalWeightings.Add(d);
					else
						ErrMsg = ErrMsg = "Gewichtungfaktor muss zwischen 0 und 10.0 liegen" + " (" + line + ")";
				}
				else
				{
					ErrMsg = ErrMsg = "Gewichtungfaktor fehlerhaft" + " (" + line + ")";
				}

			}
			else
			{
				EvalWeightings.Add(1.0);
			}
		}

		private void ReadDescription(string line)
		{
			Description = line.Trim();
		}

		private void ReadModelType(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				ErrMsg = "Modell-Angabe fehlt";
				return;
			}
			foreach(FlyType m in Enum.GetValues(typeof(FlyType)))
			{
				if (string.Compare(line.Trim(), m.ToString(), true) == 0)
				{ 
					ModelTyp = m;
					return;
				}
			}
			ErrMsg = "Modell-Angabe falsch";
		}

		private void ReadSwopMode(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				ErrMsg = "SwopMode-Angabe fehlt";
				return;
			}
			foreach (SwopWorkMode m in Enum.GetValues(typeof(SwopWorkMode)))
			{
				if (string.Compare(line.Trim(), m.ToString(), true) == 0)
				{
					SwopMode = m;
					return;
				}
			}
			ErrMsg = "SwopMode-Angabe falsch";
		}

		private void ReadParam(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				ErrMsg = "Parameter-Angabe fehlt";
				return;
			}
			ParameterKeys.Add(line);
		}

		private void ReadDate(string line)
		{
			string[] elems = line.Trim().Split('-');

			if (elems.Length != 2)
			{
				ErrMsg = "Datumsangabe falsch";
				return;
			}

			if (DateTime.TryParseExact(elems[0].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime first) &&
				DateTime.TryParseExact(elems[1].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime last))
			{
				FirstOptIndex = first.DayOfYear - 1;
				LastOptIndex = last.DayOfYear - 1;
			}
			else
			{
				ErrMsg = "'Date=' Datumsangabe falsch";
			}
		}

		private Tuple<int,int>GetLocIndices(string line)
		{
			Tuple<int, int> empty = new Tuple<int, int>(-1, -1);
			if (string.IsNullOrEmpty(line))
				return empty;
			else
			{
				string[] elems = line.Trim().Split('-');

				if (elems.Length != 2)
				{
					ErrMsg = "falsche Zeitbegrenzung";
					return empty; 
				}

				if (DateTime.TryParseExact(elems[0].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime first) &&
					DateTime.TryParseExact(elems[1].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime last))
				{
					return new Tuple<int, int>(first.DayOfYear-1, last.DayOfYear - 1);
				}
				else
				{
					ErrMsg = "falsche Zeitbegrenzung";
					return empty;
				}
			}
		}


		public bool Read(string filename)
		{
			ErrMsg = "";
			ErrLineNo = 0;
			SwopMode = SwopWorkMode.LEAST;

			try
			{
				using (StreamReader sr = new StreamReader(filename, Encoding.UTF8, true))
				{
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						if (line.Trim().StartsWith("--"))// Kommentar
							continue;

						ErrLineNo++;
						string[] elems = line.Trim().Split('=');
						if (elems.Length == 2)
						{
							switch (elems[0].Trim().ToLower())
							{
								case "swopmode": ReadSwopMode(elems[1]); break;
								case "descr": ReadDescription(elems[1]); break;
								case "model": ReadModelType(elems[1]); break;
								case "set": ReadSet(elems[1]); break;
								case "param": ReadParam(elems[1]); break;
								case "date": ReadDate(elems[1]); break;
								default: ErrMsg = "unbekannte Anweisung: '" + line + "'"; break;
							}
							if (!string.IsNullOrEmpty(ErrMsg))
							{
								ErrMsg = $"Zeile: { ErrLineNo}: {ErrMsg}";
								return false;
							}
						}
					}
				}

			}
			catch(Exception e)
			{
				ErrMsg = "Fehler: " + e.Message;
				return false;
			}

			return true;
		}
	}
}

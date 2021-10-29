using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swat.defs;
using System.IO;
using System.Globalization;
using swat.sim;

namespace swat.iodata
{
	public class MonitoringData
	{
		#region Variable
		string _location;
		int _year;

		public ModelType Model { get; private set; }
		public bool MustSave { get; private set; }
		public double[] Adults { get; }
		public double[] Eggs { get; }
		public string ErrorMsg { get; private set; }
		public string Filename { get; private set; }

		public int FirstVirtMonAdult { get; private set; }
		public int FirstVirtMonEgg { get; private set; }
		#endregion

		#region Construction

		private MonitoringData()
		{
			Adults = new double[366];
			Eggs = new double[366];

			for (int i = 0; i < 366; i++)
			{
				Adults[i] = Eggs[i] = double.NaN;
			}
		}

		public static MonitoringData CreateNew(string location, int year, ModelType modelType)
		{
			return new MonitoringData
			{
				_location = location,
				_year = year,
				Model = modelType,
				Filename = Path.Combine(WorkspaceData.GetPathMonitoring, $"{location}-{year}" + ModelBase.GetMonitoringExt(modelType))
			};
		}

		#endregion

		#region Properties


		public int LastMonitoringIndex
		{
			get
			{
				int lastMonEgg = GetLastMonitoringIndex(Eggs);
				int lastMonAdults = GetLastMonitoringIndex(Adults);
				return (lastMonEgg > lastMonAdults) ? lastMonEgg : lastMonAdults;

			}
		}

		public int FirstVirtMonIndex
		{
			get
			{
				return (FirstVirtMonEgg > 0) ? FirstVirtMonEgg : FirstVirtMonAdult;
			
			}
		}

		public string Title(WorkspaceData wd)
		{
			 return $"Monitoring {wd.CurrentModelName} {_location} {_year}"; 
		}

		public bool HasData
		{
			get
			{
				for (int i = 0; i < 366; i++)
				{
					if (!double.IsNaN(Adults[i]) || !double.IsNaN(Eggs[i]))
						return true;
				}
				return false;
			}
		}



		private string MonitoringFlightDescr
		{
			get
			{
				int firstMon = -1;
				int lastMon = 0;
				int numMon = 0;
				//bool isStarted = false;

				for (int i = 0; i < 366; i++)
				{
					if (!double.IsNaN(Adults[i]) )
					{
						if ((Adults[i]) < 0)
						{
							continue;		//todo: Überprüfen - ist richtig?

						}
						else
						{
							numMon++;
							lastMon = i;
							if (firstMon < 0)
								firstMon = i;
						}
					}
				}

				if (numMon < 1)
					return "";

				DateTime dt = new DateTime(_year, 1, 1);

				string termin = (numMon > 1) ? "Termine" : "Termin";

				return $"Flug: {numMon} {termin} ({dt.AddDays(firstMon).ToString("dd.MM")} - {dt.AddDays(lastMon).ToString("dd.MM")})   ";
			}
		}

		//todo:Überarbeiten: nur 1. u. letzter Termin + Anzahl
		private string MonitoringEggsDescr
		{
			get
			{
				int firstMon = -1;
				int lastMon = 0;
				int numMon = 0;
				bool isStarted = false;

				for (int i = 0; i < 366; i++)
				{
					if (!double.IsNaN(Eggs[i]))
					{
						if ((Eggs[i]) < 0)
						{
							isStarted = true;
							continue;
						}
						if (isStarted)
						{
							numMon++;
							lastMon = i;
							if (firstMon < 0)
								firstMon = i;
						}
						else
							isStarted = true;
					}
				}

				if (numMon < 1)
					return "";

				DateTime dt = new DateTime(_year, 1, 1);

				string termin = (numMon > 1) ? "Termine" : "Termin";

				return $"Eiablage: {numMon} {termin} ({dt.AddDays(firstMon).ToString("dd.MM")} - {dt.AddDays(lastMon).ToString("dd.MM")})";
			}
		}


		public string MonitoringDatesDescr
		{
			get
			{
				string flight = MonitoringFlightDescr;
				string eggs = MonitoringEggsDescr;
				if (string.IsNullOrEmpty(flight) && string.IsNullOrEmpty(eggs))
					return " - ";
				else
					return flight + eggs;
			}
		}


		#endregion

		#region Eingabe

		public double SetValue(int dayIndex, int colIndex, string txt)
		{
			double val = double.NaN;

			if (!string.IsNullOrWhiteSpace(txt))
			{ 
				if(txt.ToLower().StartsWith("s"))
					val = -1;
				else
					double.TryParse(txt.Replace('.', ','), out  val);
			}

			return SetValue(dayIndex, colIndex, val);
		}

		public double SetValue(int dayIndex, int colIndex, double val)
		{
			int maxDays = ((_year % 4) == 0) ? 366 : 365;

			if ((dayIndex < 0) || (dayIndex >= maxDays))
				return double.NaN;

			if ((colIndex < 1) || (colIndex > 2))
				return double.NaN;

			switch (colIndex)
			{
				case 1:
					if (val < 0.0)
						val = -1;
					if (val != Adults[dayIndex])
						MustSave = true;
					Adults[dayIndex] = val;
					break;
				case 2:
					if (val < 0.0) 
						val = -1;
					if (val != Eggs[dayIndex])
						MustSave = true;
					Eggs[dayIndex] = val;
					break;


			}
			return val;

		}

		#endregion

		#region IO
		private double ScanExpr(string expr)
		{
			if (!double.TryParse(expr.Replace('.', ','), out double result))
				result = double.NaN;

			return result;
		}

		public bool ReadFromFile()
		{
			for (int i = 0; i < 366; i++)
			{
				Adults[i] = Eggs[i] = double.NaN;
			}

			int lineNo = 0;
			try
			{
				var filestream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var file = new StreamReader(filestream, Encoding.UTF8, true, 1024);
				char[] delim = new char[] { ';', '\t' };

				string line;
				while ((line = file.ReadLine()) != null)
				{
					lineNo++;

					if (lineNo <= 2)
						continue;// 1+2. Zeile nur Spaltenüberschriften
					

					string[] words = line.Split(delim);
					int dayIndex=0;

					for (int i = 0; i < words.Length; i++)
					{
						switch (i)
						{
							case 0:
								if ((!DateTime.TryParseExact(words[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) ||
									(date.Year!= _year))
								{
									ErrorMsg = $"ungültige Datumsangabe in Zeile {lineNo}:  {words[0]}";
									return false;
								}
								dayIndex = date.DayOfYear-1;
								break;

							case 1:
								Adults[dayIndex] = ScanExpr(words[1]);
								break;

							case 2:
								Eggs[dayIndex] = ScanExpr(words[2]);
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				ErrorMsg = $"Fehler beim Einlesen der Monitoring-Daten aus:\r\n'{Filename}' \r\nGrund: {e.Message} \r\n";
				MustSave = false;

				return false;
			}

			MustSave = false;
			return true;
		}

		public bool WriteToFile()
		{
			string delim = ";";
			ErrorMsg = null;
			int maxDays = ((_year % 4) == 0) ? 366 : 365;
			DateTime nDate = new DateTime(_year, 1, 1);

			try
			{
				using (StreamWriter sw = new StreamWriter(Filename, false, Encoding.UTF8))
				{
					sw.WriteLine(_location);
					sw.WriteLine($"Date{delim}Adults{delim}Eggs");

					for(int d=0; d<maxDays; d++)
					{
						sw.Write($"{nDate:dd.MM.yyyy}{delim}");

						if (double.IsNaN(Adults[d]))
							sw.Write($" {delim}");
						else
							sw.Write(Adults[d].ToString("0.##", CultureInfo.CurrentCulture) + $"{delim}");

						if (double.IsNaN(Eggs[d]))
							sw.WriteLine($" ");
						else
							sw.WriteLine(Eggs[d].ToString("0.##", CultureInfo.CurrentCulture));

						nDate = nDate.AddDays(1.0);
					}

				}
				MustSave = false;

			}
			catch (Exception e)
			{
				ErrorMsg = e.Message;
				return false;
			}

			return (ErrorMsg == null);
		}

		#endregion

		#region virtuelles Monitoring für Prognose


		public int FirstVirtMonitoring { get; private set; }

		public MonitoringData GetWithVirtualMonitoring()
		{
			MonitoringData vmd = CreateNew(_location,_year, Model);
			for (int i = 0; i < 366; i++)
			{
				vmd.Adults[i] =Adults[i];
				vmd.Eggs[i] = Eggs[i];
			}


			int lastMonEgg = GetLastMonitoringIndex(vmd.Eggs);
			if (lastMonEgg > 0)
			{
				for (int i = lastMonEgg+7; i < 366; i += 7)
				{ 
					vmd.Eggs[i] = 0.0;
					if (vmd.FirstVirtMonEgg == 0)
						vmd.FirstVirtMonEgg = i;
				}
			}

			int lastMonAdult = GetLastMonitoringIndex(vmd.Adults);
			if (lastMonAdult > 0)
			{
				for (int i = lastMonAdult; i < 366; i += 7)
				{ 
					vmd.Adults[i] = 0.0;

					if (vmd.FirstVirtMonAdult == 0)
						vmd.FirstVirtMonAdult = i;
				}
			}

			vmd.FirstVirtMonitoring = (vmd.FirstVirtMonEgg > 0) ? vmd.FirstVirtMonEgg : vmd.FirstVirtMonAdult;

			return vmd;
		}

		private int GetLastMonitoringIndex(double[] data)
		{
			int todayIndex = DateTime.Now.DayOfYear;
			int li;
			for (li = data.Length-1; li>0; li--)
			{
				if (!Double.IsNaN(data[li]))
					break;
			}

			if (li == 0)
				return 0;
			else
				return (li < todayIndex) ? todayIndex : li;
		}

		#endregion
	}
}

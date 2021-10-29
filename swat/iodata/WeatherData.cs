using swat.defs;
using swat.views.dlg;
using swat.vm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TTP.Engine3;

namespace swat.iodata
{
	public class WeatherData
	{

		#region Variable

		string _location;
		int _year;
		WeatherSource _origin;

		List<string> _dwdStations;
		string _dwdStation;
		bool _isDwdUpdated;

		double[] _airTemps;
		double[] _soilTemps;
		double[] _airHum;
		double[] _prognAir;
		double[] _prognSoil;
		double[] _prognHum;


		double _airTempAdjustment = 0.0;
		double _soilTempAdjustment = 0.0;
		//double _windSpeedAdjustment = 1.0;
		bool _useSummerSoil = false;

		string _errorMsg;
		bool _mustSaveData;
		WorkspaceData _workspace;



		#endregion

		#region Construction

		public WeatherData(int year, WorkspaceData workspace)
		{
			_year = year;
			_workspace = workspace;
			
			_airTemps = new double[366];
			_soilTemps = new double[366];
			_airHum = new double[366];
			_prognAir = new double[366];
			_prognSoil = new double[366];
			_prognHum = new double[366];


			InitData();

			_dwdStations = new List<string>();
		}

		private void InitData()
		{
			for (int i=0; i<366; i++)
			{
				_airTemps[i]=_soilTemps[i] = _airHum[i] =  _prognAir[i] = _prognSoil[i] = _prognHum[i] = double.NaN;
			}
		}

		public void SetLocation(string workspaceCmd)
		{
			int li = workspaceCmd.LastIndexOf('.');
			if (li >= 0)
				workspaceCmd = workspaceCmd.Substring(0, li);


			li = workspaceCmd.LastIndexOf('-');
			if (li<0)
				return;

			if(workspaceCmd.StartsWith("dwd-", StringComparison.CurrentCultureIgnoreCase))
			{
				Origin = WeatherSource.Dwd;
				_location = DwdStation = workspaceCmd.Substring(4,li-4);
			}
			else
			{
				Origin = WeatherSource.HomeGrown;
				_location = workspaceCmd.Substring(0,li);
				DwdStation = "";
			}
		}

		#endregion

		#region Properties

		public bool UseSummerSoil
		{
			get { return _useSummerSoil; }
			set { _useSummerSoil = value; }
		}

		public double AirTempAdjustment
		{
			get {	return _airTempAdjustment;}
			set { _airTempAdjustment = value; }
		}

		public double SoilTempAdjustment
		{
			get { return _soilTempAdjustment; }
			set { _soilTempAdjustment = value; }
		}

		//public double WindSpeedAdjustment
		//{
		//	get { return _windSpeedAdjustment; }
		//	set { _windSpeedAdjustment = value; }
		//}


		public double[] SoilTemps
		{
			get { return _soilTemps; }
		}

		public double[] AirTemps
		{
			get { return _airTemps; }
		}

		public double[] PrognAirTemps
		{
			get { return _prognAir; }
		}

		public double[] PrognSoilTemps
		{
			get { return _prognSoil; }
		}

		public double[] AirHums
		{
			get { return _airHum; }
		}

		public double[] PrognHums
		{
			get { return _prognHum; }
		}

		public string Location
		{
			get { return _location; }
		}

		public int Year
		{
			get { return _year; }
			set { _year = value; }
		}

		public string Title
		{
			get { return $"Wetterdaten {Location} {Year}"; }
		}

		public string ErrorMsg
		{
			get { return _errorMsg; }
		}

		public bool HasError
		{
			get { return (_errorMsg != null); }
		}

		public WeatherSource Origin
		{
			get { return _origin; }
			set { _origin = value; }
		}

		public static List<string> DwdStations
		{
			get {	return  new List<string>(); }// Todo:Dwd Stationen Liste
		}

		public string DwdStation
		{
			get { return _dwdStation; }
			set
			{
				_dwdStation = value;
				_origin = (string.IsNullOrEmpty(value)) ? WeatherSource.HomeGrown : WeatherSource.Dwd;
			}
		}

		public string Filename
		{
			get
			{
				return (Origin == WeatherSource.HomeGrown) ?
					GetFilename(WeatherSource.HomeGrown, Location, Year) :
					GetFilename(WeatherSource.Dwd, DwdStation, Year);
			}
		}

		public bool MustSave
		{
			get { return _mustSaveData;}
		}

		#endregion
		

		#region Datenbeschaffung

		public double SetValue(int dayIndex, int colIndex, string txt)
		{
			if(string.IsNullOrWhiteSpace(txt))
				return SetValue(dayIndex, colIndex, double.NaN);
			
			if (!double.TryParse(txt.Replace('.', ','), out  double val))
			{
				val = double.NaN;
			}

			return SetValue(dayIndex, colIndex, val);

		}

		public double SetValue(int dayIndex, int colIndex, double val)
		{
			int maxDays = ((_year % 4) == 0) ? 366 : 365;

			if ((dayIndex < 0) || (dayIndex >= maxDays))
				return double.PositiveInfinity;

			if ((colIndex < 1) || (colIndex > 6))
				return double.PositiveInfinity;

			switch (colIndex)
			{
				case 1:
					if ((val < -40.0) || (val > 70))
						val = double.NaN; 
					if (val != _airTemps[dayIndex])
						_mustSaveData = true;
					_airTemps[dayIndex] = val;
					break;

				case 2:
					if ((val < -40.0) || (val > 70))
						val = double.NaN;
					if (val != _soilTemps[dayIndex])
						_mustSaveData = true;
					_soilTemps[dayIndex] = val;
					break;

				case 3:
					if ((val < 0.0) || (val > 100))
						val = double.NaN;
					if (val != _airHum[dayIndex])
						_mustSaveData = true;
					_airHum[dayIndex] = val;
					break;

				case 4:
					if ((val < -40.0) || (val > 70))
						val = double.NaN;
					if (val != _prognAir[dayIndex])
						_mustSaveData = true;
					_prognAir[dayIndex] = val;
					break;

				case 5:
					if ((val < -40.0) || (val > 70))
						val = double.NaN;
					if (val != _prognSoil[dayIndex])
						_mustSaveData = true;
					_prognSoil[dayIndex] = val;
					break;

				case 6:
					if ((val < -0.0) || (val > 100))
						val = double.NaN;
					if (val != _prognHum[dayIndex])
						_mustSaveData = true;
					_prognHum[dayIndex] = val;
					break;

			}
			return val;

		}

		public void DwdImport()
		{
			_isDwdUpdated = true;
      }

		public void DwdUpdate()
		{
			_isDwdUpdated = true;

		}

		public bool CanQueryDwd()
		{
			if ((Origin != WeatherSource.Dwd)||(_year != DateTime.Now.Year))
				return false;
			else
				return !_isDwdUpdated;

		}

		public bool CreateNewFile(string location)
		{
			if (Origin == WeatherSource.HomeGrown)
			{
				_location = location;
				if (File.Exists(Filename))
				{
					DlgMessage.Show("Fehler beim Generieren der Wetter-Datei", "Eine Datei mit diesem Namen existiert bereits", MessageLevel.Error);
					return false;
				}
			}

			try {
				using (File.Create(Filename)) { }
			}
			catch (Exception e)
			{
				DlgMessage.Show("Fehler beim Generieren der Wetter-Datei", e.Message, MessageLevel.Error);
				return false;

			}
			return true;
		}

		private void WriteElem( StreamWriter w, double elem, bool linefeed)
		{
			string delim = ";";
			if (linefeed)
			{
				if (double.IsNaN(elem))
					w.WriteLine($" ");
				else
					w.WriteLine(elem.ToString("0.0", CultureInfo.CurrentCulture));
			}
			else
			{
				if (double.IsNaN(elem))
					w.Write($" {delim}");
				else
					w.Write(elem.ToString("0.0", CultureInfo.CurrentCulture) + $"{delim}");

			}
		}

		public bool WriteToFile()
		{
			_errorMsg = null;
			string delim = ";";
			_errorMsg = null;

			int nDays = ((_year % 4) == 0) ? 366 : 365;
			DateTime nDate = new DateTime(_year, 1, 1);

			try
			{
				using (StreamWriter sw = new StreamWriter(Filename, false, Encoding.UTF8))
				{
					sw.WriteLine(_location);
					sw.WriteLine($"Date{delim}Air{delim}Soil{delim}Hum{delim}Progn Air{delim}Progn Soil{delim}Progn Hum");

					for (int d = 0; d < nDays; d++)
					{
						sw.Write($"{nDate:dd.MM.yyyy}{delim}");
						WriteElem(sw, _airTemps[d], false);
						WriteElem(sw, _soilTemps[d], false);
						WriteElem(sw, _airHum[d], false);
						WriteElem(sw, _prognAir[d], false);
						WriteElem(sw, _prognSoil[d], false);
						WriteElem(sw, _prognHum[d], true);

						nDate = nDate.AddDays(1.0);
					}
				}
				_mustSaveData = false;
				_workspace.HasValidPopulationData = false;
			}
			catch (Exception e)
			{
				_errorMsg = $"Fehler beim Speichern der Wetter-Daten.\r\n Grund: {e.Message}";
				return false;
			}
			return true;
		}


		private double ScanExpr(string expr)
		{
			if (!double.TryParse(expr.Replace('.', ','), out double result))
				result = double.NaN;

			return result;
		}

		public bool ReadFromFile()
		{
			InitData();

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
					int dayIndex = 0;

					for (int i = 0; i < words.Length; i++)
					{
						switch (i)
						{
							case 0:
								if ((!DateTime.TryParseExact(words[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) ||
									(date.Year != _year))
								{
									_errorMsg = $"Fehler beim Einlesen der Wetter-Daten aus:\r\n'{Filename}' \r\nGrund: ungültige Datumsangabe in Zeile {lineNo}:  {words[0]}";
									return false;
								}
								dayIndex = date.DayOfYear-1;
								break;

							case 1:
								_airTemps[dayIndex] = ScanExpr(words[1]);
								break;

							case 2:
								_soilTemps[dayIndex] = ScanExpr(words[2]);
								break;

							case 3:
								 _airHum[dayIndex] = ScanExpr(words[3]);
								break;

							case 4:
								_prognAir[dayIndex] = ScanExpr(words[4]);
								break;

							case 5:
								_prognSoil[dayIndex] = ScanExpr(words[5]);
								break;

							case 6:
								_prognHum[dayIndex] = ScanExpr(words[6]);
								break;
						}
					}
				}
			}
			catch (Exception e)
			{
				_errorMsg = $"Fehler beim Einlesen der Wetter-Daten aus:\r\n'{Filename}' \r\nGrund: {e.Message} \r\n";
				_mustSaveData = false;
				return false;
			}
			_mustSaveData = false;
			return true;
		}

		#endregion


		#region Datenaufbereitung für Modell

		public double[] GetSimAirTemp()
		{
			double[] air = new double[366];

			for (int i=0;i<366;i++)
			{
				air[i] = _airTemps[i];
				if (double.IsNaN(air[i]))
					air[i] = _prognAir[i];

				if (!double.IsNaN(air[i]))
				{ 
					air[i] +=  _airTempAdjustment;
					continue;
				}
				// keine Lufttemperaturen? - durch Bodentemp ersetzen	
				//air[i] = _soilTemps[i];
				//if (double.IsNaN(air[i]))
				//	air[i] = _prognSoil[i];

				//if (!double.IsNaN(air[i]))
				//	air[i] += _soilTempAdjustment;
			}

			return air;
		}

		public double[] GetSimSoilTemp() 
		{
			double[] soil = new double[366];

			// ab Juni nur noch mit Lufttemp arbeiten?
			int switchDate = (_useSummerSoil) ? 367 : new DateTime(_year, 6, 1).DayOfYear;
	
			for (int i = 0; i < 366; i++)
			{
				if (i < switchDate)
				{ 
					soil[i] = _soilTemps[i];
					if (double.IsNaN(soil[i]))
						soil[i] = _prognSoil[i];

					if (!double.IsNaN(soil[i]))
					{
						soil[i] += _soilTempAdjustment;
						continue;
					}

					// keine Bodentemperaturen? - durch Lufttemp ersetzen	
					//soil[i] = _airTemps[i];
					//if (double.IsNaN(soil[i]))
					//	soil[i] = _prognAir[i];

					//if (!double.IsNaN(soil[i]))
					//	soil[i] += _airTempAdjustment;
				}
				else
				{
					soil[i] = _airTemps[i];
					if (double.IsNaN(soil[i]))
						soil[i] = _prognAir[i];

					if (!double.IsNaN(soil[i]))
						soil[i] += _airTempAdjustment;
				}
			}
			return soil;
		}

		public double[] GetSimHum()
		{
			double[] airHum = new double[366];

			// keine Luftfeuchte? - durch 100% ersetzen	

			for (int i = 0; i < 366; i++)
			{
				airHum[i] = _airHum[i];
				if (double.IsNaN(airHum[i]))
				{ 
					airHum[i] = (double.IsNaN(_prognHum[i])) ? 100.0 : _prognHum[i];
				}
			}
			return airHum;
		}

		public double[] GetSimVpd()
		{
			double[] airVpd = new double[366];
			double[] t = GetSimAirTemp();
			double[] h = GetSimHum();

			for (int i = 0; i < 366; i++)
			{
				airVpd[i] = TtpHklFunc.VapPressDefP(t[i], h[i]);

				if (double.IsNaN(airVpd[i]))
					airVpd[i] = 0.0;
			}

			return airVpd;
		}

		public int GetFirstPossibleSimIndex(int seekIndex)
		{
			double[] t = GetSimAirTemp();
			double[] b = GetSimSoilTemp();


			if (double.IsNaN(t[seekIndex]) || double.IsNaN(b[seekIndex]))
			{
				while(seekIndex < 365)
				{
					seekIndex++;
					bool isGap = double.IsNaN(t[seekIndex]) || double.IsNaN(b[seekIndex]);
					if (isGap)
						return seekIndex;
				}
			}
			else
			{ 
				while (seekIndex > 0)
				{
					seekIndex--;
					bool isGap = double.IsNaN(t[seekIndex]) || double.IsNaN(b[seekIndex]);
					if (isGap)
						break;
				}
				return seekIndex;
			}

			return -1;
		}

		public int GetLastPossibleSimIndex(int seekIndex)
		{
			double[] t = GetSimAirTemp();
			double[] b = GetSimSoilTemp();


			while (seekIndex < 365)
			{
				seekIndex++;
				bool isGap = double.IsNaN(t[seekIndex]) || double.IsNaN(b[seekIndex]);
				if (isGap)
					return seekIndex - 1;
			}
			return seekIndex;
		}

		public int GetPrognStartIndex(int simStartIndex)
		{
			if (_year < DateTime.Today.Year) // für vergangene Jahre kein Prognosezeitraum
				return 365;// new DateTime(_year, 12, 31).DayOfYear - 1;

			int today = DateTime.Today.DayOfYear;

			for (int i = simStartIndex; i<today; i++)
			{
				if (double.IsNaN(_airTemps[i]) || double.IsNaN(_soilTemps[i]))
					return i;
			}

			return today; 
		}

		public TtpTimeRange GetActualTimeSpan()
		{
			//int first = 0;
			//int last = _airTemps.Length - 1;
			//for (int i = 0; i < _airTemps.Length; i++)
			//{
			//	if (!double.IsNaN(_airTemps[i]))
			//	{
			//		first = i;
			//		break;
			//	}
			//}

			//for (int i = first; i < _airTemps.Length; i++)
			//{
			//	if (double.IsNaN(_airTemps[i]))
			//	{
			//		last = i - 1;
			//		break;
			//	}
			//}

			//if (last < first)
			//{
			//	return new TtpTimeRange();
			//}

			//TtpTime start = new TtpTime("1.1." + _year);
			//start.Inc(TtpEnPattern.Pattern1Day, first);
			//TtpTimeRange tr = new TtpTimeRange(start, TtpEnPattern.Pattern1Day, last - first);
			//return tr;
			int first = FirstActualIndex;
			int last = LastActualIndex;
			if (last < first)
			{
				return new TtpTimeRange();
			}

			TtpTime start = new TtpTime("1.1." + _year);
			start.Inc(TtpEnPattern.Pattern1Day, first);
			TtpTimeRange tr = new TtpTimeRange(start, TtpEnPattern.Pattern1Day, last - first);
			return tr;
		}

		public int FirstActualIndex
		{
			get
			{
				for (int i = 0; i < 365; i++)
				{
					if (!double.IsNaN(_airTemps[i]) && !double.IsNaN(_soilTemps[i]))
						return i;
				}
				return 365;
			}
		}

		public int LastActualIndex
		{
			get
			{
				for (int i=365;i>=0;i--)
				{
					if (!double.IsNaN(_airTemps[i]) && !double.IsNaN(_soilTemps[i]))
						return i;
				}
				return 365;
			}
		}

		public bool HasGaps
		{
			get
			{
				for (int i = FirstActualIndex; i < LastActualIndex; i++)
				{ 
					if (double.IsNaN(_airTemps[i]) || double.IsNaN(_soilTemps[i]))
						return true;
				}
				return false;
			}
		}
		#endregion

		#region Helper

		public string GetFilename(WeatherSource origin, string location, int year)
		{
			return (origin == WeatherSource.HomeGrown) ?
					Path.Combine(WorkspaceData.GetPathWeather, $"{location}-{year}" + WorkspaceData.ExtWeather) :
					Path.Combine(WorkspaceData.GetPathWeather, $"DWD-{location}-{year}" + WorkspaceData.ExtWeather);
		}

		//private TtpTimeRange DetectAvailableTimeSpan()
		//{
		//	int first = 0;
		//	int last = _airTemps.Length-1;
		//	for (int i=0; i<_airTemps.Length;i++)
		//	{
		//		if(!double.IsNaN(_airTemps[i]))
		//		{
		//			first = i;
		//			break;
		//		}
		//	}

		//	for (int i = first; i < _airTemps.Length; i++)
		//	{
		//		if (double.IsNaN(_airTemps[i]))
		//		{
		//			last = i-1;
		//			break;
		//		}
		//	}

		//	if (last < first)
		//	{
		//		return new TtpTimeRange();
		//	}
		//	TtpTime start = new TtpTime("1.1." + _year);
		//	start.Inc(TtpEnPattern.Pattern1Day, first);
		//	TtpTimeRange tr = new TtpTimeRange(start, TtpEnPattern.Pattern1Day, last - first);
		//	return tr;
		//}
		#endregion


	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace SwatImporter
{
	public class DwdImporter
	{
		#region variable
		//Format bis Mai 2017
		//private static readonly DwdColumnDeclarator[] _colDecl =
		//{
		//	new DwdColumnDeclarator(-1,true, false,0,0,"","","MESS_DATUM", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true,0, 1,	"Lufttemperatur",				"°C",		"LUFTTEMPERATUR", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true,20, 10,"Luftfeuchte",					"%rF",	"REL_FEUCHTE", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 20,"Bodentemperatur 2 cm",		"°C",		"ERDBODENTEMPERATUR", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 21,"Bodentemperatur 5 cm",		"°C",		"ERDBODENTEMPERATUR", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 22,"Bodentemperatur 10 cm",	"°C",		"ERDBODENTEMPERATUR", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 23,"Bodentemperatur 20 cm",	"°C",		"ERDBODENTEMPERATUR", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 30,"Luftdruck Stationshöhe",	"hPa",	"LUFTDRUCK_STATIONSHOEHE", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 31,"Luftdruck korrigiert",		"hPa",	"LUFTDRUCK_REDUZIERT", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true,18, 40,"Wolken Bedeckungsgrad",	"",		"GESAMT_BEDECKUNGSGRAD", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 50,"Niederschlagshöhe",			"mm",		"NIEDERSCHLAGSHOEHE", TtpEnQueryCalcType.SUM, 1.0),
		//	new DwdColumnDeclarator(-1,false,true,11, 41,"Sonnenscheindauer",			"h",		"STUNDENSUMME_SONNENSCHEIN", TtpEnQueryCalcType.SUM, 0.016667),
		//	new DwdColumnDeclarator(-1,false,true, 0, 60,"Windgeschwindigkeit",		"m/s",	"WINDGESCHWINDIGKEIT", TtpEnQueryCalcType.MEAN, 1.0),
		//	new DwdColumnDeclarator(-1,false,true, 0, 61,"Windrichtung",				"°",		"WINDRICHTUNG", TtpEnQueryCalcType.PRESENT, 1.0),
		//};

		// Format ab Juni2017
		private static readonly DwdColumnDeclarator[] _colDecl =
		{
			new DwdColumnDeclarator(-1,true, false,0,0,"","","MESS_DATUM", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true,0, 1,  "Lufttemperatur",          "°C",    "TMK", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true,20, 10,"Luftfeuchte",             "%rF",   "UPM", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true, 0, 21,"Bodentemperatur 5 cm",    "°C",    "V_TE005M", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true, 0, 22,"Bodentemperatur 10 cm",   "°C",    "V_TE010M", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true, 0, 23,"Bodentemperatur 20 cm",   "°C",    "V_TE020M", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true, 0, 30,"Luftdruck Stationshöhe",  "hPa",   "PM", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true,0, 40,"Wolken Bedeckungsgrad",   "",      "NM", TtpEnQueryCalcType.MEAN, 1.0),
			new DwdColumnDeclarator(-1,false,true, 0, 50,"Niederschlagshöhe",       "mm",    "RSK", TtpEnQueryCalcType.SUM, 1.0),
			new DwdColumnDeclarator(-1,false,true,0, 41,"Sonnenscheindauer",       "h",     "SDK", TtpEnQueryCalcType.SUM, 0.016667),
			new DwdColumnDeclarator(-1,false,true, 0, 60,"Windgeschwindigkeit",     "m/s",   "FM", TtpEnQueryCalcType.MEAN, 1.0),
		};

		private enum ColumnType
		{
			Datum,
			Lt,
			Rf,
			Bt5,
			Bt10,
			Bt20,
			Dr,
			Wb,
			Ns,
			Sd,
			Wg,
		}

		private readonly string _outputPath;
		private readonly string _mesGroup;
		private readonly string _textFile;
		private string _errorMessage;
		private string[] _mesLines;
		private readonly Sensors _sensors;
		private readonly List<DwdColumnDeclarator> _columns;
		private List<ConvertedLine> _convertedLines;

		#endregion

		#region constructor

		public DwdImporter(string textFile, string outputPath, string mesGroup, Sensors sens)
		{
			_textFile = textFile;
			_outputPath = outputPath;
			_mesGroup = mesGroup;
			_sensors = sens;
			_columns = new List<DwdColumnDeclarator>();
		}

		#endregion

		#region Properties

		public string ErrorMessage { get { return _errorMessage; } }

		#endregion

		#region methoden
		public bool Execute()
		{
			if (!TestOutputPath())
				return false;

			if (!ReadTextFile())
				return false;

			if (!AnalyseTextHeader())
				return false;

			if (!ConvertTextLines())
				return false;

			if (!WriteToMesFiles())
				return false;

			return true;
		}

		private bool TestOutputPath()
		{
			return IsDirectoryWritable(_outputPath);
		}

		private bool ReadTextFile()
		{
			try
			{
				_mesLines = File.ReadAllLines(_textFile);
				return true;
			}
			catch (Exception e)
			{
				_errorMessage = e.Message;
				return false;
			}
		}

		private bool AnalyseTextHeader()
		{
			if ((_mesLines == null) || (_mesLines.Length < 2))
			{
				_errorMessage = "Fehler bei der Analyse der Metadaten (Zeilen)";
				return false;
			}

			string[] cols = _mesLines[0].Split(';');
			AssignColumns(cols);

			if (_columns.Count < 2)
			{
				_errorMessage = "Fehler bei der Analyse der Metadaten (Spalten)";
				return false;
			}

			return true;
		}

		private void AssignColumns(string[] cols)
		{
			for (int n = 0; n < cols.Length; n++)
			{
				for (int d = 0; d < _colDecl.Length; d++)
				{
					if (string.Compare(cols[n].Trim(), _colDecl[d].DwdHeader, true) == 0)
					{
						_columns.Add(_colDecl[d]);
						_columns[_columns.Count - 1].ColIndex = n;
					}
				}
			}
		}


		private bool ConvertTextLines()
		{
			_convertedLines = new List<ConvertedLine>(100000);
			for (int i = 1; i < _mesLines.Length; i++)
			{
				ConvertedLine newLine = ConvertedLine.Create(_mesLines[i], _columns);
				if (newLine != null)
					_convertedLines.Add(newLine);
			}

			if (_convertedLines.Count == 0)
			{
				_errorMessage = "keine nutzbaren Daten gefunden";
				return false;
			}

			return true;

		}


		private bool WriteToMesFiles()
		{
			int dataIndex = 0;
			foreach (DwdColumnDeclarator decl in _columns)
			{
				if (decl.IsValue)
				{
					MesWriter writer = new MesWriter(_outputPath, _mesGroup, decl, _convertedLines, dataIndex);
					if (!writer.Write())
					{
						_errorMessage = writer.ErrorMessage;
						return false;
					}
					dataIndex++;
				}
			}
			return true;
		}

		private bool IsDirectoryWritable(string dirPath)
		{
			try
			{
				using (FileStream fs = File.Create(
				Path.Combine(dirPath, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
				{ }
				return true;
			}
			catch
			{
				_errorMessage = "Ausgabepfad ist nicht beschreibbar.";
				return false;
			}
		}


		#endregion

	}
}

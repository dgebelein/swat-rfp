using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwatImporter
{
	class StationList
	{
		List<StationRow> _stationElems;

		public string ErrMsg { get; set; }

		public bool Read()
		{
			List<StationRow> _elems = new List<StationRow>();
			string fn = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dwd-liste.csv");

			int lineNo = 0;
			try
			{
				var filestream = new FileStream(fn, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				var file = new StreamReader(filestream, Encoding.UTF8, true, 1024);
				char[] delim = new char[] { ';', '\t' };

				string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());


				string line;
				while ((line = file.ReadLine()) != null)
				{
					lineNo++;

					if (lineNo <= 1)
						continue;// 1. Zeile nur Spaltenüberschriften

					StationRow row = new StationRow();
					string[] words = line.Split(delim);

					for (int i = 0; i < words.Length; i++)
					{
						switch (i)
						{
							case 0: row.Id = ScanInt(words[0]); break;
							case 1:
								row.Name = words[1].Trim();
								foreach (char c in invalid)
								{
									row.Name = row.Name.Replace(c.ToString(), " ");
								}
								break;
							case 2: row.Land = words[2].Trim(); break;
							case 3: row.NN = ScanDouble(words[5]); break;
							case 4: row.GeoLat = ScanDouble(words[6]); break;
							case 5: row.GeoLong = ScanDouble(words[7]); break;
						}
					}
					_elems.Add(row);
				}
			}
			catch (Exception e)
			{
				ErrMsg = $"Fehler beim Einlesen der Stationsdaten aus:' {fn}' Grund: {e.Message}";
				return false;
			}

			_stationElems = _elems.OrderBy(a => a.Land).ThenBy(a => a.Name).ToList();
			return true;
		}


		private int ScanInt(string expr)
		{
			if (!int.TryParse(expr, out int result))
				result = -1;

			return result;
		}

		private double ScanDouble(string expr)
		{
			if (!double.TryParse(expr.Replace('.', ','), out double result))
				result = double.NaN;

			return result;
		}

		public List<string> GetSelectionElems()
		{
			List<string> selectionElems = new List<string>(_stationElems.Count);
			foreach (StationRow row in _stationElems)
			{
				selectionElems.Add(row.Land + " / " + row.Name);
			}
			return (selectionElems);
		}

		public int GetDwdId(int selectionIndex)
		{
			return _stationElems[selectionIndex].Id;
		}

		public int GetDwdId(string stationName)
		{
			int n = _stationElems.FindIndex(x => x.Name == stationName);
			return (n >= 0) ? _stationElems[n].Id : -1;
		}

		public string GetDwdStationName(int dwdIndex)
		{
			int n = _stationElems.FindIndex(x => x.Id == dwdIndex);
			return (n >= 0) ? _stationElems[n].Name : "error";
		}

		public string GetSelectedStationName(int selectionIndex)
		{
			return _stationElems[selectionIndex].Name ;
		}



	}
}

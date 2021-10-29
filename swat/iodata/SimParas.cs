using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace swat.iodata
{

	public class SimParamElem : ICloneable
	{
		public object Obj     { get; set; }
		public Type ObjType   { get; set; }
		public bool IsChanged { get; set; }
		public String Descr   { get; set; } 
		public bool IsSelected{ get; set; }
		public double MinVal  { get; set; }
		public double MaxVal  { get; set; }

		public virtual object Clone()
		{
			return this.MemberwiseClone();
		}

	}

	public  class SimParamData
	{
		#region variable		

		Dictionary<string, SimParamElem> _paramDict;
		List<string> _warnings;
		private bool _mustSave;

		public String Filename { get; set; }
		
		#endregion

		#region construction

		public SimParamData()
		{
			_paramDict = new Dictionary<string, SimParamElem>();
			_warnings = new List<string>();
		}


		#endregion

		#region Initialisierung

		public SimParamData Clone()
		{
			SimParamData cs = new SimParamData();
			foreach (string key in _paramDict.Keys)
			{
				SimParamElem e = (SimParamElem) _paramDict[key].Clone();
				cs._paramDict.Add(key, e);
			}

			return cs;
		}

		public SimParamData Copy()
		{
			SimParamData cs = new SimParamData();
			foreach (string key in _paramDict.Keys)
			{
				cs.AddOrReplaceItem(key, _paramDict[key]); // todo: hier werden nur Referenzen kopiert -
			}

			return cs;
		}

		public void AddItemDictionary(Dictionary<string, SimParamElem> dict, bool enforceUnchanged = false)
		{
			foreach (string key in dict.Keys)
			{
				AddOrReplaceItem(key, dict[key], enforceUnchanged);
			}
		}

		public void AddParamData(SimParamData paramData, bool enforceUnchanged = false)
		{
			AddItemDictionary(paramData.ParamDict, enforceUnchanged);
		}

		// für interne Initialisierung
		public void InitItem(string key, object obj, Type objType, double minVal, double maxVal, string descr, bool isChanged = false)
		{
			if(!_paramDict.ContainsKey(key))
			{
				SimParamElem elem = new SimParamElem
				{
					Obj = obj,
					ObjType = objType,
					Descr = descr,
					MinVal = minVal,
					MaxVal = maxVal,
					IsChanged = isChanged
				};
				_paramDict.Add(key, elem);
			}
		}

		// Elementwerte kopieren - keine Referenzen
		public  void AddOrReplaceItem(string key,SimParamElem newElem, bool enforceUnchanged = false)
		{
			if (!_paramDict.ContainsKey(key))
				InitItem(key, newElem.Obj, newElem.ObjType,newElem.MinVal, newElem.MaxVal, newElem.Descr, (enforceUnchanged)? false : newElem.IsChanged);
			else
			{
				SimParamElem oldElem = _paramDict[key];
				bool changed =  (enforceUnchanged) ? 
					false:
					oldElem.IsChanged || (oldElem.Obj.ToString() != newElem.Obj.ToString());

				if (changed)
					_mustSave = true;
				SimParamElem elem = new SimParamElem
				{
					Obj = newElem.Obj,
					ObjType = oldElem.ObjType,
					Descr = oldElem.Descr,
					IsChanged = changed,
					IsSelected = oldElem.IsSelected,
					MinVal = oldElem.MinVal,
					MaxVal = oldElem.MaxVal
				};
				_paramDict[key] = elem;
			}
		}

		#endregion

		#region Properties

		public bool MustSave
		{
			get { return _mustSave; }
		}

		public bool HasWarnings
		{
			get { return (_warnings.Count > 0); }
		}

		public List<string> Warnings
		{
			get { return _warnings; }
		}

		public Dictionary<string, SimParamElem> ParamDict
		{
			get { return _paramDict; }
			set { _paramDict = value; }
		}

		public string ErrorMsg
		{
			get
			{
				if (!HasWarnings)
					return null;
				string err = _warnings[0];
				for (int i = 1; i < _warnings.Count; i++)
				{
					err += "\r\n" + _warnings[i];
				}

				return err;
			}
		}

		#endregion

		#region Rückgabe Werte

		public Dictionary<string, SimParamElem> GetParamDict(string section)
		{
			Dictionary<string, SimParamElem> dict = new Dictionary<string, SimParamElem>();
			foreach (string s in _paramDict.Keys)
			{
				if (s.StartsWith(section))
					dict.Add(s, _paramDict[s]);
			}
			return dict;
		}

		public bool HasChangedParams
		{
			get
			{
				foreach (string key in _paramDict.Keys)
				{
					if (_paramDict[key].IsChanged)
						return true;
				}

				return false;
			}
		}

		public int GetNumChangedParams(string section)
		{
			int num = 0;
			foreach (string key in _paramDict.Keys)
			{
				if (key.StartsWith(section) && _paramDict[key].IsChanged)
					num++;
			}

			return num;
		}

		public int GetNumSelectedParams()
		{
			int num = 0;
			foreach (string key in _paramDict.Keys)
			{
				if (_paramDict[key].IsSelected)
					num++;
			}

			return num;
		}

		public SimParamElem GetParamElem(string key)
		{
			return (_paramDict.ContainsKey(key)) ? _paramDict[key] : null;
		}


		public object GetValue(string key)
		{
			SimParamElem tt = _paramDict[key];
			return tt.Obj;		
		}

		#endregion

		#region Einlesen

		public void SetToUnchanged(string key)
		{
			if (_paramDict.ContainsKey(key))
				_paramDict[key].IsChanged = false;
		}

		private void SetValue(string section, string line)
		{
			if (string.IsNullOrEmpty(section))
			{
				_warnings.Add($"Eintrag ohne zugehörige Sektion: {line}");
				return;
			}

			string[] elems = line.Split('=');
			if (elems.Length < 2)
			{
				_warnings.Add($"keine Wertzuweisung: {line}");
				return;
			}

			string key = section + '.' + elems[0].Trim();
			if (!_paramDict.ContainsKey(key))
			{
				_warnings.Add($"unbekannter Parameter: {section}.{line}");
				return;
			}

			SimParamElem param = _paramDict[key];

			object val = GetConvertedElement(key, elems[1].Trim());
			if (val == null)
			{
				string strType = param.ObjType.ToString();
				_warnings.Add($"keine Wertzuweisung möglich - erwartet wird {strType}: {line}");
				return;
			}

			AddOrReplaceItem(key, new SimParamElem { Obj = val, ObjType = param.ObjType });

		}

		// todo:  variable Parameter überarbeiten
		public object GetConvertedElement(string key, string elem)
		{
			SimParamElem tt = GetParamElem(key);
			if (tt == null)
				return null;

			Type elemType = tt.ObjType;

			switch (Type.GetTypeCode(elemType))
			{
				case TypeCode.Boolean:
						if (bool.TryParse(elem, out bool b))
							return b;
						else
							return null;
					
				case TypeCode.Int32:
						if (Int32.TryParse(elem, out  int x))
						{
							if (x < tt.MinVal)
								x = (Int32)tt.MinVal;
							if (x > tt.MaxVal)
								x = (Int32)tt.MaxVal;
							return x;
						}
							
						else
							return null;

				case TypeCode.Double:
						if (Double.TryParse(elem, NumberStyles.Float,CultureInfo.InvariantCulture, out  double d))
						{
							if (d < tt.MinVal)
								d = tt.MinVal;
							if (d > tt.MaxVal)
								d = tt.MaxVal;
							return d;
						}
						else
							return null;

				case TypeCode.String: return elem.Trim();

				default: return null;
			}
		}

		public bool ReadFromFile()
		{
			_warnings.Clear();
			string pathName = Path.Combine(WorkspaceData.GetPathParameters, Filename);

			if (!File.Exists(pathName))
				return true;

			string[] lines;
			try
			{
				lines = File.ReadAllLines(pathName, Encoding.UTF8);
			}
			catch (Exception e)
			{
				_warnings.Add($"Datei '{pathName}' konnte nicht gelesen werden.\r\nGrund: {e.Message}");
				return false;
			}

			string section = "";
			char[] cTrim = { '[', ' ', ']' };

			foreach (string line in lines)
			{
				if (line.Trim().StartsWith(";"))
					continue;

				Regex regex = new Regex(@"^\[.*\]$");
				Match match = regex.Match(line);
				if (match.Success)
				{
					section = match.Value.Trim(cTrim);
					continue;
				}
				if (String.IsNullOrEmpty(line.Trim()))
					continue;
				SetValue(section, line);

			}
			_mustSave = false;
			return (_warnings.Count == 0);
		}

		#endregion

		#region Schreiben

		public bool WriteToFile()
		{

			_mustSave = false; // zum Start wegen mögl Exceptions

			string pathName = Path.Combine(WorkspaceData.GetPathParameters, Filename);

			//Sortieren
			var list = _paramDict.Keys.ToList();
			list.Sort();

			string section = "";
			_warnings.Clear();

			try
			{
				using (StreamWriter sw = new StreamWriter(File.Open(pathName, FileMode.Create), Encoding.UTF8))
				{
					foreach (var key in list)
					{
						if (!_paramDict[key].IsChanged)
							continue;

						string[] names = key.Split('.');


						if (names[0] != section)
						{
							section = names[0];
							sw.WriteLine($"[{section}]");

						}
						if (_paramDict[key].ObjType == typeof(double))
						{
							sw.WriteLine($"{names[1]} = {((double)(_paramDict[key].Obj)).ToString("0.###", CultureInfo.InvariantCulture)}");
						}
						else
							sw.WriteLine($"{names[1]} = {((IConvertible)_paramDict[key].Obj).ToString(CultureInfo.InvariantCulture)}");

					}
					sw.Close();
				}
			}
			catch (Exception e)
			{
				_warnings.Add($"Datei '{pathName}' konnte nicht geschrieben werden.\r\nGrund: {e.Message}");
				return false;
			}

			return true;
		}

		#endregion


	}
}

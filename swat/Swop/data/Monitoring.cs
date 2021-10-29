using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

//namespace Swop.data
//{
//	public class Monitoring
//	{
//		#region Variable

//		double[] _adults;
//		double[] _eggs;
//		string _errorMsg;
//		public string Location { get; set; }
		
//		#endregion

//		#region Construction

//		public  Monitoring()
//		{
//			_adults = new double[366];
//			_eggs = new double[366];

//			for (int i = 0; i < 366; i++)
//			{
//				_adults[i] = _eggs[i] = double.NaN;
//			}
//		}


//		#endregion


//		#region Properties

//		public string ErrorMsg
//		{
//			get { return _errorMsg; }
//		}


//		public bool HasData
//		{
//			get
//			{
//				for (int i = 0; i < 366; i++)
//				{
//					if (!double.IsNaN(_adults[i]) || !double.IsNaN(_eggs[i]))
//						return true;
//				}
//				return false;
//			}
//		}

//		public double[] Adults
//		{
//			get { return _adults; }
//		}

//		public double[] Eggs
//		{
//			get { return _eggs; }
//		}
//		#endregion


//		#region IO

//		private double ScanExpr(string expr)
//		{
//			if (!double.TryParse(expr.Replace('.', ','), out double result))
//				result = double.NaN;

//			return result;
//		}

//		public bool ReadFromFile(string Filename)
//		{

//			Location = Path.GetFileNameWithoutExtension(Filename);

//			for (int i = 0; i < 366; i++)
//			{
//				_adults[i] = _eggs[i] = double.NaN;
//			}

//			int lineNo = 0;
//			try
//			{
//				var filestream = new FileStream(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
//				var file = new StreamReader(filestream, Encoding.UTF8, true, 1024);
//				char[] delim = new char[] { ';', '\t' };

//				string line;
//				while ((line = file.ReadLine()) != null)
//				{
//					lineNo++;

//					if (lineNo <= 2)
//						continue;// 1+2. Zeile nur Spaltenüberschriften


//					string[] words = line.Split(delim);
//					int dayIndex = 0;

//					for (int i = 0; i < words.Length; i++)
//					{
//						switch (i)
//						{
//							case 0:
//								if (!DateTime.TryParseExact(words[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) 
//								{
//									_errorMsg = $"ungültige Datumsangabe in Zeile {lineNo}:  {words[0]}";
//									return false;
//								}
//								dayIndex = date.DayOfYear - 1;
//								break;

//							case 1:
//								_adults[dayIndex] = ScanExpr(words[1]);
//								break;

//							case 2:
//								_eggs[dayIndex] = ScanExpr(words[2]);
//								break;
//						}
//					}
//				}
//			}
//			catch (Exception e)
//			{
//				_errorMsg = $"Fehler beim Einlesen der Monitoring-Daten aus:\r\n'{Filename}' \r\nGrund: {e.Message} \r\n";
//				return false;
//			}

//			return true;
//		}

//		#endregion
//	}
//}

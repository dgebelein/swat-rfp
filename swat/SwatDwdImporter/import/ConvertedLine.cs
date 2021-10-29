using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace SwatImporter
{
	internal class ConvertedLine
	{
		public long Time { get; set; }
		public double[] Values { get; set; }

		private ConvertedLine(int numValues)
		{
			Values = new double[numValues];
			for (int i = 0; i < numValues; i++)
				Values[i] = double.NaN;
		}

		public static ConvertedLine Create(string line, List<DwdColumnDeclarator> columns)
		{
			int dateIndex = -1;
			int valNum = 0;

			foreach (DwdColumnDeclarator d in columns)
			{
				if (d.IsDateTime)
					dateIndex = d.ColIndex;

				if (d.IsValue)
					valNum++;
			}

			if ((dateIndex < 0) || (valNum == 0))
				return null;

			try
			{
				string[] elems = line.Split(';');
				if ((GetYear(elems[dateIndex])) < 1980)   //TtpTime kann keine Zeit vor 1980 auswerten
					return null;

				ConvertedLine newLine = new ConvertedLine(valNum)
				{
					Time = GetTimeStamp(elems[dateIndex])
				};
				GetValues(newLine, elems, columns);
				return newLine;
			}
			catch
			{
				return null;
			}

		}

		private static int GetYear(string timeString)
		{
			int year = -1;

			string ts = timeString.Trim().Substring(0, 4);
			if (int.TryParse(ts, out year))
				return year;
			return -1;
		}

		private static long GetTimeStamp(string timeString)
		{
			TtpTime tm = new TtpTime();
			string s = timeString.Trim();

			string f = string.Format("{2}.{1}.{0}", s.Substring(0, 4), s.Substring(4, 2), s.Substring(6, 2));
			tm.Set(f);

			if (!tm.IsValid)
				return (-1);
			return tm.Ticks;
		}

		private static void GetValues(ConvertedLine cl, string[] elems, List<DwdColumnDeclarator> columns)
		{
			int index = 0;

			foreach (DwdColumnDeclarator col in columns)
			{
				if (col.IsValue)
				{
					string elem = elems[col.ColIndex].Trim();
					if (elem != "-999")
						double.TryParse(elem, NumberStyles.Float, CultureInfo.InvariantCulture, out cl.Values[index]);
					index++;
				}
			}
		}


	}
}

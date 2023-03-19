using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace swatSim
{
	public class ReadCmd
	{
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
					if (++n > rep)
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

		static public  TtpTimeRange GetTimeRangeFromShort(string line, int year)
		{
			TtpTimeRange tr = new TtpTimeRange();
			string st = ReadCmd.IsolateContent(line, "#d:");
			if (String.IsNullOrWhiteSpace(line))
			{
				return tr;
			}

			string[] elems = line.Trim().Split('-');

			if (elems.Length != 2)
			{
				return tr;
			}

			if (DateTime.TryParseExact(elems[0].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime first) &&
				DateTime.TryParseExact(elems[1].Trim(), "dd.MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime last))
			{
				//TtpTime start = new TtpTime($"1.1.{year}");
				//start.Inc(TtpEnPattern.Pattern1Day, first.DayOfYear - 1);
				//TtpTime end = new TtpTime($"1.1.{year}");
				//end.Inc(TtpEnPattern.Pattern1Day, last.DayOfYear - 1);
				//tr= new TtpTimeRange(start, end, TtpEnPattern.Pattern1Day);

				TtpTime start = new TtpTime($"{elems[0].Trim()}.{year}");
				TtpTime end = new TtpTime($"{elems[1].Trim()}.{year}");
				tr = new TtpTimeRange(start, end, TtpEnPattern.Pattern1Day);
			}

			return tr;

		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
	}
}

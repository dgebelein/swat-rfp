using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swat.iodata
{
	class Utility
	{
		//name hat die Form:Pfad\fn_x.ext; -> es wird das nächsthöhere freie x zurückgegeben
		public static int FindValidFileIndex(string path,string pattern)
		{
			try
			{
				string[] repFiles = Directory.GetFiles(path, pattern);
				if (repFiles.Length == 0)
					return 0;

				int maxIndex = 0;
				foreach (string fn in repFiles)
				{
					int pos = fn.LastIndexOf('_');
					int posp = fn.LastIndexOf('.');
					if (pos != -1)
					{
						int fi = Convert.ToInt32(fn.Substring(pos + 1, posp - pos - 1));
						if (fi > maxIndex)
							maxIndex = fi;
					}
				}
				return maxIndex + 1;
			}
			catch
			{
				return 0;
			}
		}

		public static bool HasWriteAccessToFolder(string folderPath)
		{
			try
			{
				// Attempt to get a list of security permissions from the folder. 
				// This will raise an exception if the path is read only or do not have access to view the permissions. 
				System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

	}
}

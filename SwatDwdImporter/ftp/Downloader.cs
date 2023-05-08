using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Net;

namespace SwatImporter
{

	public enum Sensors
	{
		Clima,
		Soil
	};

	public enum Topicality
	{
		Historical,
		Recent
	}

	class Downloader
	{
		#region variables

		private readonly string[] _ftpSensors = new string[] { "kl", "soil_temperature"};
		private readonly string[] _ftpFileSpec = new string[] { "KL", "EB"};
		private readonly string[] _ftpTopicality = new string[] { "historical", "recent" };
		private readonly string _ftpRoot;
		private readonly string _workDir;
		private readonly Topicality _topicality;
		private Dictionary<int, string>[] _ftpDicts;
		public string ErrorMsgExt { get; private set; }
		public string ErrorMessage { get; private set; }
		public bool HasValidData { get; private set; }


		#endregion

		#region Constructor

		public Downloader(string ftpUrl, string workDir, Topicality topicality)
		{
			_ftpRoot = ftpUrl;
			_workDir = workDir;
			//todo: workdir muss existieren und beschreibbar sein sonst exception
			_topicality = topicality;
		}

		#endregion

		#region download-directories

		public bool BuildDirectories()
		{
			_ftpDicts = new Dictionary<int, string>[2];
			for (int s = 0; s <= 1; s++)
			{
				_ftpDicts[s] = BuildSensorDictionary((Sensors)s);
				if (_ftpDicts[s] == null)
				{
					ErrorMessage = "Fehler: Sensordaten nicht in Ftp-Verzeichnis gefunden.";
					HasValidData = false;
					return false;
				}
			}

			HasValidData = true;
			return true;
		}

		private Dictionary<int, string> BuildSensorDictionary(Sensors sens)
		{

			List<string> ftpFiles = GetDirContentList(GetFtpDirName(sens, _topicality), sens);
			if (ftpFiles == null)
			{
				return null;
			}

			Dictionary<int, string> dict = new Dictionary<int, string>();
			foreach (string fn in ftpFiles)
			{
				int nr = ExtractNumberFromFilename(fn);
				if (nr > 0)
					dict[nr]= fn;
				else
					ErrorMsgExt = "nicht auswertbarer Dateiname: " + fn;
			}

			return dict;
		}

		#endregion

		#region public methods

		public bool GetAndUnzipFile(Sensors sens, Topicality topic, int stationId)
		{
			string ftpFilename = GetFtpZipName(sens, stationId);
			if (ftpFilename == null)
				return false;


			string ftpPath = GetFtpDirName(sens, topic) + @"/" + ftpFilename;
			if (!GetBinFile(ftpPath, _workDir))
			{
				return false;
			}

			string zipDirectory = Path.Combine(_workDir, stationId.ToString());
			string zipFilename = Path.Combine(_workDir, Path.GetFileName(ftpFilename));
			return (UnzipFile(zipFilename, zipDirectory));
		}

		#endregion

		#region Download

		public string GetFtpDirName(Sensors sens, Topicality topic)
		{
			return _ftpRoot + @"/" + _ftpSensors[(int)sens] + @"/" + _ftpTopicality[(int)topic];
		}

		public string GetFtpZipName(Sensors sens, int stationId)
		{
			try
			{
				if (_ftpDicts != null && _ftpDicts[(int)sens].TryGetValue(stationId, out string fn))
					return fn.Substring(fn.IndexOf("tageswerte", StringComparison.InvariantCultureIgnoreCase));

				else
					return null;
			}
			catch
			{
				return null;
			}
		}

		//public bool GetBinFile(string fileName, string outputDir)
		//{
		//	const int bufferSize = 2048;
		//	try
		//	{
		//		FtpWebRequest request = (FtpWebRequest)WebRequest.Create(fileName);
		//		request.Credentials = new NetworkCredential("anonymous", "anonymous");
		//		request.UseBinary = true; // Use binary to ensure correct dlv!
		//		request.Method = WebRequestMethods.Ftp.DownloadFile;

		//		FtpWebResponse response = (FtpWebResponse)request.GetResponse();
		//		Stream responseStream = response.GetResponseStream();
		//		FileStream writer = new FileStream(Path.Combine(outputDir, Path.GetFileName(fileName)), FileMode.Create);

		//		byte[] buffer = new byte[2048];

		//		int readCount = responseStream.Read(buffer, 0, bufferSize);
		//		while (readCount > 0)
		//		{
		//			writer.Write(buffer, 0, readCount);
		//			readCount = responseStream.Read(buffer, 0, bufferSize);
		//		}

		//		responseStream.Close();
		//		response.Close();
		//		writer.Close();
		//		return true;
		//	}
		//	catch (Exception e)
		//	{
		//		ErrorMsgExt = e.Message;
		//		return false;
		//	}
		//}

		public bool GetBinFile(string fileName, string outputDir)
		{
			const int bufferSize = 2048;
			try
			{
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fileName);
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				Stream responseStream = response.GetResponseStream();
				FileStream writer = new FileStream(Path.Combine(outputDir, Path.GetFileName(fileName)), FileMode.Create);

				byte[] buffer = new byte[2048];

				int readCount = responseStream.Read(buffer, 0, bufferSize);
				while (readCount > 0)
				{
					writer.Write(buffer, 0, readCount);
					readCount = responseStream.Read(buffer, 0, bufferSize);
				}

				responseStream.Close();
				response.Close();
				writer.Close();
				return true;
			}
			catch (Exception e)
			{
				ErrorMsgExt = e.Message;
				return false;
			}
		}

		public List<string> GetDirContentList(string dirName, Sensors sens)
		{
			List<string> rawList = new List<string>();

			try
			{
				//Create Http request
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(dirName);
				request.KeepAlive = false;
				HttpWebResponse response = (HttpWebResponse)request.GetResponse();
				Stream responseStream = response.GetResponseStream();
				StreamReader reader = new StreamReader(responseStream);

				while (!reader.EndOfStream)
				{
					rawList.Add(reader.ReadLine());
				}

				//Clean-up
				reader.Close();
				response.Close();

				return ExtractFilenames(rawList, sens);
			}
			catch (Exception e)
			{
				ErrorMsgExt = e.Message;
				return null;
			}

		}
		//public List<string> GetDirContentList(string dirName, Sensors sens)
		//{
		//	List<string> rawList = new List<string>();

		//	try
		//	{
		//		//Create FTP request
		//		FtpWebRequest request = (FtpWebRequest)WebRequest.Create(dirName);

		//		request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
		//		request.Credentials = new NetworkCredential("anonymous", "anonymous");
		//		request.UsePassive = true;
		//		request.UseBinary = true;
		//		request.KeepAlive = false;


		//		FtpWebResponse response = (FtpWebResponse)request.GetResponse();
		//		Stream responseStream = response.GetResponseStream();
		//		StreamReader reader = new StreamReader(responseStream);

		//		while (!reader.EndOfStream)
		//		{
		//			rawList.Add(reader.ReadLine());
		//		}

		//		//Clean-up
		//		reader.Close();
		//		response.Close();

		//		return ExtractFilenames(rawList, sens);
		//	}
		//	catch (Exception e)
		//	{
		//		ErrorMsgExt = e.Message;
		//		return null;
		//	}

		//}

		#endregion

		#region Unzip
		//------------------------------------------------------------------------------------------------------------------------------------------------

		public bool UnzipFile(string fileName, string outputPath)
		{
			try
			{
				//Achtung: outputPath darf noch nicht existieren!
				if (Directory.Exists(outputPath))
					Directory.Delete(outputPath, true);

				ZipFile.ExtractToDirectory(fileName, outputPath);
				return true;
			}
			catch (Exception e)
			{
				ErrorMessage = e.Message;
				return false;
			}
		}

		#endregion

		#region Utilities

		private List<string> ExtractFilenames(List<string> code, Sensors sens)
		{
			List<string> files = new List<string>();
			string filter = "_" + _ftpFileSpec[(int)sens] + "_";
			foreach (string line in code)
			{
				string[] elems = line.Split('"');
				foreach (string elem in elems)
				{
					if ((elem.EndsWith(".zip")) && elem.Contains(filter))
					{
						files.Add(Path.GetFileName(elem));
						break;
					}
				}

			}
			return files;
		}

		public static int ExtractNumberFromFilename(string fn)
		{
			int number = -1;
			try
			{
				string[] elems = fn.Split('_');
				if (elems.Length > 3)
					int.TryParse(elems[2], out number);
				return number;
			}
			catch (Exception)
			{
				return -1;
			}
		}

		#endregion
	}
}

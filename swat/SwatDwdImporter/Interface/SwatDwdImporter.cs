using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SwatImporter;

namespace SwatImporter
{
	public class SwatDwdImporter
	{
		StationList _stationList;
		bool _hasValidList;

		string _ftpUrl;
		string _workFolder;
		string _mesFolder;

		Downloader _downloader;

		public string LogMsg{get;set;}

		public SwatDwdImporter(string ftpUrl, string workFolder, string mesFolder)
		{
			_ftpUrl = ftpUrl;
			_workFolder = workFolder;
			_mesFolder = mesFolder;
			_stationList = new StationList();
			_hasValidList = _stationList.Read();
		}

		public List<string> GetSelectionElems()
		{
			if(_hasValidList)
			{
				return _stationList.GetSelectionElems();
			}
			else
			{
				return new List<string>
				{
					_stationList.ErrMsg
				};
			}
		}

		public string GetDwdStationName(int dwdIndex)
		{
			return _stationList.GetDwdStationName(dwdIndex);
		}

		public string GetSelectedStationName(int selectionIndex)
		{
			return _stationList.GetSelectedStationName(selectionIndex);
		}

		public int GetSelectedStationIndex(int selectionIndex)
		{
			return _stationList.GetDwdId(selectionIndex);
		}

		public int GetStationDwdIndex(string stationName)
		{
			return _stationList.GetDwdId((stationName.StartsWith("DWD-"))? stationName.Substring(4): stationName);
		}

		public bool ReadFtpFolder(int dwdIndex, Topicality topic)
		{
			 _downloader = new Downloader(_ftpUrl, _workFolder, topic);

			if (!_downloader.BuildDirectories())
			{
				LogMsg = _downloader.ErrorMessage;
				return false;
			}

			LogMsg = "OK";
			return true;
		}

		public void ImportSensorGroup(int stationId, Sensors sens, Topicality topicality)
		{

			if (_downloader.GetAndUnzipFile(sens, topicality, stationId))
			{
				LogMsg = ConvertToMes(stationId, sens);
				Directory.Delete(Path.Combine(_workFolder, stationId.ToString()), true);
			}
			else
				LogMsg = "Daten nicht verfügbar";
		}

		private string ConvertToMes(int stationId, Sensors sens)
		{
			string[] textFiles = Directory.GetFiles(Path.Combine(_workFolder, stationId.ToString()), @"produkt_*.txt");
			if (textFiles.Length < 1)
				return "Daten nicht verfügbar";


			DwdImporter importer = new DwdImporter(textFiles[0], _mesFolder, GetDwdStationName(stationId), sens);
			return (importer.Execute())?
				"OK":
				importer.ErrorMessage;
		}
	}
}

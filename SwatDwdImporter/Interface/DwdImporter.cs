using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TTP.DwdLib;

namespace SwatDwdImporter
{
	public class DwdImporter
	{
		StationList _stationList;
		bool _hasValidList;

		string _ftpUrl;
		string _workFolder;
		string _mesFolder;

		DwdDownloader _downloader;

		StringBuilder _log;


		public DwdImporter(string ftpUrl, string workFolder, string mesFolder)
		{
			_ftpUrl = ftpUrl;
			_workFolder = workFolder;
			_mesFolder = mesFolder;
			_stationList = new StationList();
			_hasValidList = _stationList.Read();
			_log = new StringBuilder();
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
			return _stationList.GetDwdId((stationName.StartsWith("dwd-"))? stationName.Substring(4): stationName);
		}

		public bool ImportAllYears(int dwdIndex)
		{
			 _downloader = new DwdDownloader(_ftpUrl, _workFolder, Topicality.Historical);

			if (!_downloader.BuildDirectories())
			{
				_log.Append("Fehler: " + _downloader.ErrorMessage);
				//_executionLog[0].LogMessage = "Fehler: " + _downloader.ErrorMessage;
				//e.Cancel = true;
				return false;
			}

			ImportSensorGroup(dwdIndex, Sensors.Air, Topicality.Historical);
			ImportSensorGroup(dwdIndex, Sensors.Soil, Topicality.Historical);
			ImportSensorGroup(dwdIndex, Sensors.Wind, Topicality.Historical);

			return true;
		}

		public bool UpdateCurrentYear(int StationIndex)
		{
			return true;
		}

		private void ImportSensorGroup(int stationId, Sensors sens, Topicality topicality)
		{

			if (_downloader.GetAndUnzipFile(sens, topicality, stationId))
			{
				_log.Append(" " + ConvertToMes(stationId, sens)); //_executionLog[index + 1].LogMessage += " " + ConvertToMes(index, sens);
				Directory.Delete(Path.Combine(_workFolder, stationId.ToString()), true);
			}
		}

		private string ConvertToMes(int stationId, Sensors sens)
		{
			string[] textFiles = Directory.GetFiles(Path.Combine(_workFolder, stationId.ToString()), @"produkt_*.txt");
			if (textFiles.Length < 1)
				return sens.ToString() + ": Datei fehlt ";


			TTP.DwdLib.DwdImporter importer = new TTP.DwdLib.DwdImporter(textFiles[0], _mesFolder, GetDwdStationName(stationId), sens);
			if (importer.Execute())
				return sens.ToString();

			return sens.ToString() + ": " + importer.ErrorMessage;
		}
	}
}

using SwatImporter;
using swatSim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace swat.iodata
{
	class DwdWeatherCreator
	{

		#region Variable

		WeatherData _weatherData;
		SwatDwdImporter _dwdImporter;
		BackgroundWorker _bkWorker;
		int _dwdStationId;
		bool _isCurrentYear;

		public bool Success { get; set; }

		#endregion

		#region Construction
		
		public DwdWeatherCreator(WeatherData weatherData, SwatDwdImporter dwdImporter,int dwdStationId, ProgressChangedEventHandler progressChangedMethod, RunWorkerCompletedEventHandler progressCompletedMethod, bool onlyUpdate= false)
		{
			 _weatherData = weatherData;
			_dwdImporter = dwdImporter;
			_dwdStationId = dwdStationId;
			_isCurrentYear = (weatherData.Year == DateTime.Now.Year);

			_bkWorker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true,
				WorkerReportsProgress = true
			};

			if(onlyUpdate)
				_bkWorker.DoWork += DoTheUpdate;
			else
				_bkWorker.DoWork += DoTheImport;

			_bkWorker.ProgressChanged += progressChangedMethod;
			_bkWorker.RunWorkerCompleted += progressCompletedMethod;
		}

		#endregion

		#region Properties - Logs

		public string LogReadHistFtpFolder { get; set; }
		public string LogImportHistAir { get; set; }
		public string LogImportHistSoil { get; set; }
		public string LogCreatePrognosis { get; set; }

		public string LogReadActFtpFolder { get; set; }
		public string LogImportActAir { get; set; }
		public string LogImportActSoil { get; set; }
		public string LogCreateSimulationYear { get; set; }

		#endregion

		#region  Thread-Steuerung
		
		private bool CanContinueWorker(DoWorkEventArgs e)
		{
			_bkWorker.ReportProgress(0); // aktualisiert die Ansicht

			if (_bkWorker.CancellationPending || !Success)
			{
				e.Cancel = true;
				Success = false;
				return false;
			}
			else
			{
				e.Cancel = false;
				return true;
			}
		}

		public void Execute()
		{
			_bkWorker.RunWorkerAsync();
		}

		public void Cancel()
		{
			_bkWorker.CancelAsync();
		}

		#endregion

		#region Datenimport

		private void DoTheUpdate(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (!ReadRecentData(e))
					return;

				_dwdImporter.ImportSensorGroup(_dwdStationId, Sensors.Clima, Topicality.Recent);
				_dwdImporter.ImportSensorGroup(_dwdStationId, Sensors.Soil, Topicality.Recent);
				CreateSimulationYearData();
				LogCreateSimulationYear = "OK";
				_bkWorker.ReportProgress(0); // wg. update view
				_weatherData.WriteToFile();
				Success = true;
			}
			catch(Exception ex)
			{
				LogCreateSimulationYear = ex.Message;
				Success = false;
				e.Cancel = true;
				return;
			}
		}

		private void DoTheImport(object sender, DoWorkEventArgs e)
		{
			try
			{
				// historische Daten - für Prognose und Vorjahre 
				if (!ReadHistoricalData(e))
					return;

				if (_isCurrentYear)
				{
					CreatePrognosisData();
					LogCreatePrognosis = "OK";
				}
				else
					LogCreatePrognosis = "nicht notwendig";

				if (!CanContinueWorker(e))
					return;

				// aktuelle Daten - "dieses Jahr"
				if(_isCurrentYear)
				{
					if (!ReadRecentData(e))
						return;
				}
				else
				{
					LogReadActFtpFolder = "nicht notwendig";
					LogImportActAir = "nicht notwendig";
					LogImportActSoil = "nicht notwendig";
					_bkWorker.ReportProgress(0); // wg. update view
				}

				CreateSimulationYearData();
				LogCreateSimulationYear = "OK";
				_bkWorker.ReportProgress(0); // wg. update view

				_weatherData.WriteToFile();
			}
			catch
			{
				Success = false;
				e.Cancel = true;
				return;
			}
		}



		private bool ReadHistoricalData(DoWorkEventArgs e)
		{
			Success = _dwdImporter.ReadFtpFolder(_dwdStationId, Topicality.Historical);
			LogReadHistFtpFolder = _dwdImporter.LogMsg;
			if (!CanContinueWorker(e))
				return false;

			_dwdImporter.ImportSensorGroup(_dwdStationId, Sensors.Clima, Topicality.Historical);
			LogImportHistAir = _dwdImporter.LogMsg;
			if (!CanContinueWorker(e))
				return false;

			_dwdImporter.ImportSensorGroup(_dwdStationId, Sensors.Soil, Topicality.Historical);
			LogImportHistSoil = _dwdImporter.LogMsg;

			return CanContinueWorker(e);
		}

		private bool ReadRecentData(DoWorkEventArgs e)
		{
			Success = _dwdImporter.ReadFtpFolder(_dwdStationId, Topicality.Recent);
			LogReadActFtpFolder = _dwdImporter.LogMsg;
			if (!CanContinueWorker(e))
				return false;

			_dwdImporter.ImportSensorGroup(_dwdStationId, Sensors.Clima, Topicality.Recent);
			LogImportActAir = _dwdImporter.LogMsg;
			if (!CanContinueWorker(e))
				return false;

			_dwdImporter.ImportSensorGroup(_dwdStationId, Sensors.Soil, Topicality.Recent);
			LogImportActSoil = _dwdImporter.LogMsg;

			return CanContinueWorker(e);
		}

		#endregion
		
		#region Mes-Dateien auswerten

		private double[] ReadMes(int year, int mesIndex)
		{
			TtpTime tmStart = new TtpTime("1.1." + year.ToString());
			TtpTimeRange timeRange = new TtpTimeRange(tmStart, TtpEnPattern.Pattern1Year, 1);
			timeRange.Pattern = TtpEnPattern.Pattern1Day;

			TtpMes mes = new TtpMes();
			TtpAnalogData anData = new TtpAnalogData();
			anData.SetQuery(timeRange, new TtpQueryInstruction("[Mean]"));
			string fn = Path.Combine(WorkspaceData.GetPathWeatherMes, _dwdImporter.GetDwdStationName(_dwdStationId) + "-" + mesIndex + ".mes");
			if (mes.Attach(fn))
			{
				mes.GetValues(anData);
			}
			return anData.AllData;
		}

		private double[] ReadAirMes(int year)
		{
			return ReadMes(year, 1);
		}

		private double[] ReadPrecMes(int year)
		{
			return ReadMes(year, 50);

		}

		private double[] ReadSoilMes(int year)
		{
			int[] mesIndices = { 21, 22, 23 }; // die erste vorhandene Bodentemp aus: 5, 10, 20cm Tiefe
			double[] val= null;
			for (int i = 0; i < 3; i++)
			{
				val = ReadMes(year, mesIndices[i]);
				if (!double.IsNaN(val[0]))
					return val;
			}
			return val;
		}

		private void CreatePrognosisData()
		{
			int startYear =  Math.Max(_weatherData.Year - 10, 1980);

			// Lufttemperatur
			double[] sumsAir = new double[365];
			int[] divisorsAir = new int[365];
			for (int year = startYear; year < _weatherData.Year; year++)
			{
				double[] val = ReadAirMes(year);
				for (int d=0; d<365; d++)
				{
					if(!double.IsNaN(val[d]))
					{
						sumsAir[d] += val[d];
						divisorsAir[d]++;
					}
				}
			}

			for (int d = 0; d < 365; d++)
			{
				_weatherData.PrognAirTemps[d] = (divisorsAir[d] > 0) ?
					sumsAir[d] / divisorsAir[d] :
					Double.NaN;
			}

			//Bodentemperatur
			double[] sumsSoil = new double[365];
			int[] divisorsSoil = new int[365];
			for (int year = startYear; year < _weatherData.Year; year++)
			{
				double[] val = ReadSoilMes(year);
				for (int d = 0; d < 365; d++)
				{
					if (!double.IsNaN(val[d]))
					{
						sumsSoil[d] += val[d];
						divisorsSoil[d]++;
					}
				}
			}

			for (int d = 0; d < 365; d++)
			{
				_weatherData.PrognSoilTemps[d] = (divisorsSoil[d] > 0) ?
					sumsSoil[d] / divisorsSoil[d] :
					double.NaN;
			}

			// Niederschlag
			for (int d = 0; d < 365; d++)
			{
				_weatherData.PrognPrecs[d] = Double.NaN;
			}
		}


		private void CreateSimulationYearData()
		{
			double[] airTemps = ReadAirMes(_weatherData.Year);
			double[] soilTemps = ReadSoilMes(_weatherData.Year);
			double[] prec = ReadPrecMes(_weatherData.Year);

			for (int d = 0; d < 365; d++)
			{
				// es werden nur Lücken aufgefüllt - nie Werte überschrieben!
				if (double.IsNaN(_weatherData.AirTemps[d])) _weatherData.AirTemps[d] = airTemps[d];
				if (double.IsNaN(_weatherData.SoilTemps[d])) _weatherData.SoilTemps[d]= soilTemps[d];
				if (double.IsNaN(_weatherData.Precs[d])) _weatherData.Precs[d] = prec[d];
			}
		}

		#endregion

	}
}

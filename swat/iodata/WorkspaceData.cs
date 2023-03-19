//using swat.defs;
using SwatPresentations;
using swatSim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace swat.iodata
{
	public class WorkspaceData
	{
		#region variable

		WeatherData _dataWeather;
		MonitoringData[] _dataMonitoring;
		PopulationData _dataPopulation;
		ModelBase _currentModel;
		FlyType _currentFlyType;
		List<string> _flyNames;


		public const string ExtWorkspace = ".swat";
		public const string ExtWeather = ".swat-wea";
		public const string ExtSimParameters = ".swat-par";

		public string Name { get; set; }
		public string Location { get; set; }
		public int SimulationYear { get; private set; }
		public string WeatherFile { get; set; }
		public string SimParaFile { get; set; }
		public string MonitoringFile { get; set; }
		public string IOError { get; set; }
		public string CurrentWorkspaceName { get; set; }
		public SimParamData DefaultParameters { get; set; }

		public string Notes { get; set; }
		#endregion

		#region Construction + Init

		public WorkspaceData(int year)
		{
			_flyNames = new List<string>() { "Kohlfliege", "Möhrenfliege", "Zwiebelfliege" };// nicht als const weil später durch Lokalisierung variabel
			SetGlobalProperties();
			SimulationYear = year;

			_dataWeather = new WeatherData(SimulationYear);
			_dataPopulation = new PopulationData(10);


			CreateMonitoringData("");  // wird später überschrieben - hier nur wegen UI-Element-Binding
			CreateSimParas();
			InitChangedDefaultParams();
			CurrentFlyType = FlyType.DR;
		}

		private void SetGlobalProperties()
		{
			Application.Current.Properties["ExtWorkspace"] = ExtWorkspace;
			Application.Current.Properties["ExtWeather"] = ExtWeather;
			Application.Current.Properties["ExtSimParameters"] = ExtSimParameters;
			Application.Current.Properties["PathWeather"] = GetPathWeather;
			Application.Current.Properties["PathParameters"] = GetPathParameters;
			Application.Current.Properties["PathMonitoring"] = GetPathMonitoring;
		}

		private void CreateMonitoringData(string location) 
		{
			int numModels = Enum.GetNames(typeof(FlyType)).Length;

			_dataMonitoring = new MonitoringData[numModels];

			for (int i = 0; i < numModels; i++)
			{
				_dataMonitoring[i] = MonitoringData.CreateNew(location, SimulationYear, (FlyType)i);
			}

		}

		private void CreateSimParas() // mit Standardparametern initialisieren
		{
			CodedModelParameters = new SimParamData();
			int numModels = Enum.GetNames(typeof(FlyType)).Length;

			for (int i = 0; i < numModels; i++)
			{
				ModelBase model = CreateModel((FlyType)i); 
				CodedModelParameters.AddParamData(model.CodedParams);
			}
		}


		void InitChangedDefaultParams()
		{
			DefaultParameters = CodedModelParameters.Clone();
			string fnDefParams = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "swat-default.swat-par"); 
			if (!DefaultParameters.ReadFromFile(fnDefParams))
			{
				DlgMessage.Show("Initialisierungs-Fehler: ", DefaultParameters.Warnings[0], SwatPresentations.MessageLevel.Error);

				//DlgMessage.Show("Initialisierungs-Fehler: ", "Datei 'swat-default.swat-par' nicht im Programm-Ordner gefunden", SwatPresentations.MessageLevel.Error);
			}

			var list = DefaultParameters.ParamDict.Keys.ToList();
			foreach (var key in list)
			{
				DefaultParameters.ParamDict[key].IsChanged = false;
			}

			DataSetParameters = DefaultParameters.Clone();

		}

		#endregion

		#region Properties


		public static string GetFilename(string location, int year)
		{
			 return Path.Combine(GetPathWorkspace, location + "-" + year + ExtWorkspace); 
		}


		public ModelBase CurrentModel
		{
			get { return _currentModel; }
		}

		public FlyType CurrentFlyType
		{
			get { return _currentFlyType; }
			set
			{
				_currentFlyType = value;
				_currentModel = CreateModel(_currentFlyType, DataSetParameters);
			}
		}

		public List<string> FlyNameList
		{
			get { return _flyNames; }
		}

		public string CurrentModelName
		{
			get { return _flyNames[(int)CurrentFlyType]; }
		}

	
		public string Title // für Grafik-Titel
		{
			get{ return Name + " " + CurrentModelName;}
		}

		public WeatherData WeatherData
		{
			get { return _dataWeather; }
			set { _dataWeather = value; }
		}

		public MonitoringData[] Monitoring
		{
			get { return _dataMonitoring; }
			set { _dataMonitoring = value; }
		}

		public MonitoringData CurrentMonitoringData
		{
			get { return _dataMonitoring[(int)CurrentFlyType]; }
		}

		public PopulationData CurrentPopulationData
		{
			get { return _dataPopulation; }
			set { _dataPopulation=value; }
		}



		//public SimParamData CurrentDefaultParameters
		//{
		//	get {	return _currentModel.CodedParams;}
		//}

		public SimParamData CurrentWorkingParameters
		{
			get { return DataSetParameters.GetModelParams(_currentModel.GetParamPrefix()); }
		}


		public SimParamData CodedModelParameters { get; private set; }
		public SimParamData DataSetParameters { get; private set; }

		public bool HasValidWeatherData
		{
			get {	return (string.IsNullOrEmpty(WeatherFile)) ? false : true;}
		}

		public bool HasMonitoringData
		{
			get {return CurrentMonitoringData.HasData;}
		}

		public bool HasValidPopulationData
		{
			get
			{
				return (CurrentPopulationData == null) ? false : CurrentPopulationData.HasValidData;
			}
			set
			{
				if (CurrentPopulationData != null)
					CurrentPopulationData.HasValidData = value;
			}
		}

		public void InvalidatePopulationData()
		{
			if (_dataPopulation != null)
				_dataPopulation.HasValidData = false;
	
		}

		#endregion

		#region Read/Write

		public static string GetPathWorkspace
		{
			get
			{
				string dataPath = (string) Properties.Settings.Default["DataFolder"];
				if (string.IsNullOrEmpty(dataPath) || !Utility.HasWriteAccessToFolder(dataPath))
					return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat");
				else
				{
					return Path.Combine(dataPath, "Swat");
				}
			}
		}

		public static string GetPathOptimization
		{
			get { return Path.Combine(GetPathWorkspace, "Optimization"); }
		}

		public static string GetPathMonitoring
		{
			get
			{
				return GetPathWorkspace;
				//return Path.Combine(GetPathWorkspace, "Monitoring");
			}
		}

		public static string GetPathWeather
		{
			get
			{
				return GetPathWorkspace;
				//return Path.Combine(GetPathWorkspace, "Weather");
			}
		}

		public static string GetPathWeatherWork
		{
			get { return Path.Combine(GetPathWorkspace, "Weather","Work"); }
		}

		public static string GetPathWeatherMes
		{
			get { return Path.Combine(GetPathWorkspace, "Weather", "Mes"); }
		}

		public static string GetPathDwdUrl
		{
			get { return Properties.Settings.Default.FtpUrl; }
		}

		public static string GetPathNotes
		{
			get
			{
				return GetPathWorkspace;
				//return Path.Combine(GetPathWorkspace, "Notes");
			}
		}

		public static string GetPathReports
		{
			get { return Path.Combine(GetPathWorkspace, "Reports"); }
		}

		public static string GetPathSwop
		{
			get { return Path.Combine(GetPathWorkspace, "Swop"); }
		}

		public static string GetPathParameters
		{
			get
			{
				return GetPathWorkspace;
				//return Path.Combine(GetPathWorkspace, "Parameters");
			}
		}

		public static WorkspaceData Read(string name)
		{
			int year = DateTime.Today.Year; // Zuweisung nur aus formalen Gründen
			string ioError="";
			string filename = Path.Combine(GetPathWorkspace, name + ExtWorkspace);
			Dictionary<string, string> dictWorkspace = new Dictionary<string, string>();

			try
			{
				using (StreamReader sr = new StreamReader(filename, Encoding.UTF8, true))
				{
					string line;
					while((line = sr.ReadLine())!= null)
					{ 
						string[] elems = line.Trim().Split('=');
						if (elems.Length == 2)
						{
							dictWorkspace.Add(elems[0].Trim().ToLower(), elems[1].Trim());
						}
					}
				}

				if (!dictWorkspace.ContainsKey("location"))
					throw (new Exception("Eintrag 'Location' fehlt"));

				if (dictWorkspace.ContainsKey("year")) {
					if (int.TryParse(dictWorkspace["year"], out int  y))
					{ 
						if ( (y < 1980) || (y>2039) )
							throw (new Exception("Swat kann nur Zeitangaben zwischen 1980 und 2039 verarbeiten."));
						year = y;
					}
					else
						throw (new Exception("Eintrag Year: keine Jahresangabe"));
				}
				else
					throw (new Exception("Eintrag 'Year' fehlt"));

				if (!dictWorkspace.ContainsKey("weather"))
					throw (new Exception("Eintrag 'Weather' fehlt"));
			}
			catch (Exception e)
			{
				ioError = $"Fehler in Swat-Anweisungsdatei '{name + ExtWorkspace}':\n{e.Message}";
			}



			if (string.IsNullOrEmpty(ioError))
			{
				WorkspaceData wsp = new WorkspaceData(year)
				{
					Name = name,
					Location = dictWorkspace["location"],
					WeatherFile = dictWorkspace["weather"],
					SimParaFile = (dictWorkspace.ContainsKey("simpara")) ? dictWorkspace["simpara"] : $"{name}" + ExtSimParameters
				};

				wsp.ReadWorkspaceElements();
				wsp.CurrentFlyType = wsp._currentFlyType;	//Achtung: muss sein, damit variierte Parameter übernommen werden
				return wsp;
			}
			else
			{
				WorkspaceData wsp = new WorkspaceData(year)
				{
					IOError = ioError
				};
				return wsp;
			}

		}

		public void Write()
		{
			IOError = "";
			WriteWorkspaceElements();
			IOError = CreateCmdFile(Location, SimulationYear, WeatherFile, SimParaFile);
		}

		public static string CreateCmdFile(string location, int year, string weatherFile, string paraFile)
		{
			try
			{
				string fn = GetFilename(location, year);
				using (StreamWriter sw = new StreamWriter(fn, false, Encoding.UTF8))
				{
					sw.WriteLine("Location = " + location);
					sw.WriteLine("Year = " + year);
					sw.WriteLine("Weather = " + weatherFile);
					if (!string.IsNullOrEmpty(paraFile))
						sw.WriteLine("SimPara = " + paraFile);
				}
			}
			catch (Exception e)
			{
				return e.Message;
			}

			return null;
		}

		private ErrorType ReadWorkspaceElements()
		{
			_dataWeather.SetLocation(WeatherFile);

			if(File.Exists(_dataWeather.Filename))
			{ 
				if(!_dataWeather.ReadFromFile())
				{
					IOError += _dataWeather.ErrorMsg;
				}
			}

			CreateMonitoringData(Location);
			foreach (MonitoringData md in _dataMonitoring)
			{
				if (File.Exists(md.Filename))
				{
					if (!md.ReadFromFile())
						IOError += md.ErrorMsg;
				}
			}

			// 1. verfügbares Modell auswählen
			foreach (MonitoringData md in _dataMonitoring)
			{
				if (md.HasData)
				{ 
					CurrentFlyType = md.Model;
					break;
				}
			}

			DataSetParameters = DefaultParameters.Clone();
			DataSetParameters.Filename = SimParaFile;
			if ( !DataSetParameters.ReadFromFile())
			{
				IOError += DataSetParameters.ErrorMsg;
			}

			string notesName = Path.Combine(WorkspaceData.GetPathNotes, Name + " - Notes.txt");
			if (!File.Exists(notesName))
			{ 
				Notes= "";
			}
			else { 
				try
				{
					Notes=(File.ReadAllText(notesName)).Trim();
				}
				catch
				{
					Notes = "";
				}
			}


			return (string.IsNullOrEmpty(IOError))? ErrorType.OK : ErrorType.Warning;
		}

		private void WriteWorkspaceElements()
		{
			if (_dataWeather.MustSave)
			{ 
				_dataWeather.WriteToFile();
				HasValidPopulationData = false;
				IOError += _dataWeather.ErrorMsg;
			}

			foreach(MonitoringData md in _dataMonitoring)
			{
				if(md.MustSave)
				{
					md.WriteToFile();
					IOError += md.ErrorMsg;
				}
			}

			if(DataSetParameters.HasChangedParams)
			{
				if (SimParaFile == null)
					SimParaFile = $"{Name}" + ExtSimParameters;

				DataSetParameters.Filename = SimParaFile;
				if (!DataSetParameters.WriteToFile())
					IOError += DataSetParameters.ErrorMsg;
			}
		}


		#endregion

		#region Modellberechnung
		// wird eigentlich nur gebraucht, um Zugriff auf Modellinterna zu ermöglichen
		public ModelBase CreateModel(FlyType md, SimParamData simParam = null)
		{
			double[] randomNumbers = new double[1000000]; // Erzeugung von Zufallszahlen-Folge  wegen Identität mit swop-Familie
			Random rand = new Random(96);
			for (int i = 0; i < randomNumbers.Length; i++)
			{
				randomNumbers[i] = rand.NextDouble();
			}

			ModelBase model;
			switch (md)
			{
				case FlyType.DR: model = new ModelDR(Title, _dataWeather, CurrentPopulationData, simParam); break;
				case FlyType.PR: model = new ModelPR(Title, _dataWeather, CurrentPopulationData, simParam); break;
				case FlyType.DA: model = new ModelDA(Title, _dataWeather, CurrentPopulationData, simParam); break;

				default: model = new ModelDR(Title, _dataWeather, CurrentPopulationData, simParam); break; 
			}

			model.SetRandomNumbers(randomNumbers);

			return model;
		}

		public void CalculatePopulation()
		{
			if (HasValidPopulationData)
				return;

			//var watch = System.Diagnostics.Stopwatch.StartNew();

			
			_currentModel = CreateModel(CurrentFlyType, DataSetParameters);
			_currentModel.RunSimulation();

			//watch.Stop();
			//var elapsedMs = watch.ElapsedMilliseconds;
		}
		#endregion

	}
}

using Microsoft.Win32;
using SwatPresentations;
using swatSim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwopCompare
{
	class CmpData
	{

		#region var
		public List<CmpSet> CompareSets { get; private set; }
		public FlyType ModelType { get; private set; }
		public ModelBase Model { get; private set; }
		public string SwatWorkDir { get; private set; }

		public string Description { get; private set; }

		public string ErrMessage { get; private set; }
		public string CommandFilename { get; private set; }
		public bool HasValidData { get { return string.IsNullOrEmpty(ErrMessage); } }

		#endregion

		#region ctor

		public CmpData()
		{
			CompareSets = new List<CmpSet>();
			AssignWorkDir();
		}

		private void AssignWorkDir()
		{
			string cfgFn = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "swop.cfg");
			try
			{

				string[] fileLines = File.ReadAllLines(cfgFn);
				SwatWorkDir = fileLines[ReadCmd.GetLineNo(fileLines, "SwatDir") + 1].Trim();

			}
			catch (Exception e)
			{
				SwatWorkDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat");
			}


			string swopDir = Path.Combine(SwatWorkDir, "Swop");
			if (!Directory.Exists(swopDir))
			{
				DlgMessage.Show("Swop Konfigurationsfehler", $"das Arbeitsverzeichnis {swopDir} existiert nicht", MessageLevel.Error);
			}
		}

		#endregion

		#region properties

		#endregion



		#region read command-file
		public bool LoadCommandFile()
		{
			OpenFileDialog dlg = new OpenFileDialog
			{
				InitialDirectory = Path.Combine(SwatWorkDir,"Swop"),
				Filter = "Swop Cmp-Files (*.swat-cmp)|*.swat-cmp|All files (*.*)|*.*"
			};

			if (dlg.ShowDialog() == true)
			{
				ReadCommandFile(dlg.FileName);
				return true;
			}
			else
				return false;
		}

		private string[] GetCommandLines(string filename)
		{
			List<string> cmdLines = new List<string>();
			try
			{
				string[] lines = File.ReadAllLines(filename);
				foreach(string li in lines)
				{
					string s = li.Trim();
					if (string.IsNullOrWhiteSpace(s) || s.StartsWith("--"))
						continue;
					else
						cmdLines.Add(s);

				}
				return cmdLines.ToArray();
			}
			catch (Exception e)
			{
				ErrMessage = e.Message;
				return null;
			}
		}

		void ReadModelType(string[] lines)
		{
			int sl = ReadCmd.GetLineNo(lines, "[Model]");
			if (sl < 0)
			{
				ErrMessage = $"\r\nSektion [Model] nicht gefunden";
				return;
			}

			for (int n = sl + 1; n < lines.Length; n++)
			{
				if (lines[n].StartsWith("["))
					break;
				string line = lines[n].Trim();

				foreach (FlyType m in Enum.GetValues(typeof(FlyType)))
				{
					if (string.Compare(line, m.ToString(), true) == 0)
					{
						ModelType = m;
						Model = CreateSimulationModel(ModelType, null, null); // Modell wird beim Einlesen der Parameter gebraucht
					}
				}
			}

			if(Model== null)
				ErrMessage = $"Modell-Angabe falsch";
		}

		void ReadDescription(string[] lines)
		{
			int sl = ReadCmd.GetLineNo(lines,"[Descr]");
			if (sl < 0)
			{
				Description = "";
				return;
			}

			for (int n = sl + 1; n < lines.Length; n++)
			{
				if (lines[n].StartsWith("["))
					break;
				Description = lines[n].Trim();
			}

		}

		void ReadCommandFile(string filename)
		{
			ErrMessage = "";
			CompareSets.Clear();

			string[]lines = GetCommandLines(filename);
			if (HasValidData)
			{
				ReadModelType(lines);
				ReadDescription(lines);

				int setNo = 0;
				while (HasValidData)
				{
					int startLine = ReadCmd.GetLineNo(lines, "[Set]", setNo);
					if (startLine < 0)
						break;
					CmpSet cmpSet = new CmpSet(Model, SwatWorkDir);
					if (cmpSet.Read(lines, startLine+1))
					{
						CompareSets.Add(cmpSet);
					}
					else
					{
						ErrMessage = cmpSet.ErrorMessage;
						break;
					}
					setNo++;
				}
			}

			if (HasValidData) 
			{ 
				CommandFilename = filename;
			}
			else
			{
				CommandFilename = "Command-File?";
				//DlgMessage.Show("Swop-Compare - Error", ErrMessage, MessageLevel.Error);
			}
		}

		#endregion

		#region helpers

		public  ModelBase CreateSimulationModel(FlyType flyType, WeatherData wd, SimParamData simParam)
		{
			ModelBase model;
			switch (flyType)
			{
				case FlyType.DR: model = new ModelDR("", wd, null, simParam); break;
				case FlyType.PR: model = new ModelPR("", wd, null, simParam); break;
				case FlyType.DA: model = new ModelDA("", wd, null, simParam); break;
				default: throw new Exception("Modelloptimierung für dieses Modell nicht implementiert");
			}
			return model;
		}
		#endregion

	}
}

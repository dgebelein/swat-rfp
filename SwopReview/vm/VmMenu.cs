using Microsoft.Win32;
using SwatPresentations;
using swatSim;
using SwopReview.views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using System.Windows.Input;
using TTP.UiUtils;

namespace SwopReview
{
	public class VmMenu :VmBase
	{
		#region variable

		VmBase _vmReport;
		bool _hasValidData;
		int _selectedViewIndex;
		bool _hasCode;
		string _swatWorkDir;

		RelayCommand _openFileCommand;
		RelayCommand _printReportCommand;
		RelayCommand _saveReportCommand;
		RelayCommand _createParameterFilesCommand;
		RelayCommand _clearParameterFilesBoxesCommand;
		RelayCommand _setAllParameterFilesBoxesCommand;

		RelayCommand _plotCommand;
		RelayCommand _plotClearSetBoxesCommand;
		RelayCommand _plotClearLapBoxesCommand;
		RelayCommand _plotSetAllLapBoxesCommand;

		RelayCommand _meshPanelCommand;
		RelayCommand _meshPanelClearLapBoxesCommand;
		RelayCommand _meshPanelSetAllLapBoxesCommand;

		RelayCommand _colorMeshCommand;
		RelayCommand _meshClearLapBoxesCommand;
		RelayCommand _meshSetAllLapBoxesCommand;



		#endregion

		#region Commands

		public ICommand OpenFileCommand { get { return _openFileCommand; } }
		public ICommand PrintReportCommand { get { return _printReportCommand; } }
		public ICommand SaveReportCommand { get { return _saveReportCommand; } }
		public ICommand CreateParameterFilesCommand { get { return _createParameterFilesCommand; } }
		public ICommand ClearParameterFileBoxesCommand { get { return _clearParameterFilesBoxesCommand; } }
		public ICommand SetAllParameterFileBoxesCommand { get { return _setAllParameterFilesBoxesCommand; } }

		public ICommand ShowPlotCommand { get { return _plotCommand; } }
		public ICommand PlotClearSetBoxesCommand { get { return _plotClearSetBoxesCommand; } }
		public ICommand PlotClearLapBoxesCommand { get { return _plotClearLapBoxesCommand; } }
		public ICommand PlotSetAllLapBoxesCommand { get { return _plotSetAllLapBoxesCommand; } }

		public ICommand ShowMeshPanelCommand { get { return _meshPanelCommand; } }
		public ICommand MeshPanelClearLapBoxesCommand { get { return _meshPanelClearLapBoxesCommand; } }
		public ICommand MeshPanelSetAllLapBoxesCommand { get { return _meshPanelSetAllLapBoxesCommand; } }

		public ICommand ShowColorMeshCommand { get { return _colorMeshCommand; } }
		public ICommand MeshClearLapBoxesCommand { get { return _meshClearLapBoxesCommand; } }
		public ICommand MeshSetAllLapBoxesCommand { get { return _meshSetAllLapBoxesCommand; } }



		#endregion

		#region Construction

		public VmMenu(SwopData sd, UserControl view): base(sd, view)
		{

			_openFileCommand = new RelayCommand(param => this.OpenSwopFile());
			_printReportCommand = new RelayCommand(param => this.PrintReport());
			_saveReportCommand = new RelayCommand(param => this.SaveReport());
			_createParameterFilesCommand= new RelayCommand(param => this.CreateParameterFiles());

			_clearParameterFilesBoxesCommand = new RelayCommand(param => this.ClearParameterFileBoxes());
			_setAllParameterFilesBoxesCommand = new RelayCommand(param => this.SetAllParameterFileBoxes());

			_plotCommand = new RelayCommand(param => this.ShowPlot(true));
			_plotClearSetBoxesCommand= new RelayCommand(param => this.PlotClearSetBoxes());
			_plotClearLapBoxesCommand = new RelayCommand(param => this.PlotClearLapBoxes());
			_plotSetAllLapBoxesCommand = new RelayCommand(param => this.PlotSetAllLapBoxes());


			_meshPanelCommand = new RelayCommand(param => this.ShowMeshPanel(true), param => this.CanShowMeshPanel);
			_meshPanelClearLapBoxesCommand = new RelayCommand(param => this.MeshPanelClearLapBoxes());
			_meshPanelSetAllLapBoxesCommand = new RelayCommand(param => this.MeshPanelSetAllLapBoxes());

			_colorMeshCommand = new RelayCommand(param => this.ShowColorMesh(true),param => this.CanShowColorMesh);
			_meshClearLapBoxesCommand = new RelayCommand(param => this.MeshClearLapBoxes());
			_meshSetAllLapBoxesCommand = new RelayCommand(param => this.MeshSetAllLapBoxes());

			AssignWorkDir();
		}

		private void AssignWorkDir()
		{
			string cfgFn = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "swop.cfg");
			try
			{

				string[] fileLines = File.ReadAllLines(cfgFn);
				_swatWorkDir = fileLines[ReadCmd.GetLineNo(fileLines, "SwatDir") + 1].Trim();

			}
			catch (Exception e)
			{
				_swatWorkDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat");
			}


			string swopDir = Path.Combine(_swatWorkDir, "Swop");
			if (!Directory.Exists(swopDir))
			{
				DlgMessage.Show("Swop Konfigurationsfehler", $"das Arbeitsverzeichnis {swopDir} existiert nicht", MessageLevel.Error);
			}
		}
		#endregion


		#region Properties


		public VmBase ViewReportDataContext
		{
			get { return _vmReport; } 
			set 
			{
				_vmReport = value;
				OnPropertyChanged("ViewReportDataContext");
			}
		}

		public string SwopLogName
		{
			get { return _swopData.SwopLogName; }
		}

		public string SwopLogError
		{
			get { return _swopData.ErrMessage; }
		}

		public Visibility  VisErrorLog
		{
			get { return _swopData.HasErrors ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisReportCommands
		{
			get { return (_swopData.HasValidData && _selectedViewIndex == 0) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisPlotCommands
		{
			get { return (_swopData.HasValidData && _selectedViewIndex == 1) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisMeshPanelCommands
		{
			get { return (_swopData.HasValidData &&   _swopData.HasMeshData && _selectedViewIndex == 2) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisColorMeshCommands
		{
			get { return (_swopData.HasValidData && _swopData.HasMeshData && _selectedViewIndex == 3) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisSimResultCommands
		{
			get { return (_swopData.HasValidData && _selectedViewIndex == 4) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public int SelectedTabIndex
		{
			get { return _selectedViewIndex; }
			set
			{
				_selectedViewIndex = value;
				SetMenuAction();
				ShowView();

				OnPropertyChanged("VisReportCommands");
				OnPropertyChanged("VisPlotCommands");
				OnPropertyChanged("VisMeshPanelCommands");
				OnPropertyChanged("VisColorMeshCommands");
				OnPropertyChanged("VisSimResultCommands");

			}
		}

		private void InitMenues()
		{
			((viewCmd)ViewVisual).InitReportMenu();
			((viewCmd)ViewVisual).InitPlotMenu(null); 
			((viewCmd)ViewVisual).InitMeshPanelMenu(null); 
			((viewCmd)ViewVisual).InitMeshMenu(null);
			((viewCmd)ViewVisual).InitSimResultMenu(null);

		}

		private void SetMenuAction()
		{
			switch (_selectedViewIndex)
			{
				case 0: ((viewCmd)ViewVisual).InitReportMenu(); break;
				case 1: ((viewCmd)ViewVisual).InitPlotMenu(ShowPlot); break;
				case 2: ((viewCmd)ViewVisual).InitMeshPanelMenu(ShowMeshPanel); break;
				case 3: ((viewCmd)ViewVisual).InitMeshMenu(ShowColorMesh); break;
				case 4: ((viewCmd)ViewVisual).InitSimResultMenu(ShowSimResult); break;

			}

		}
			
		private void ShowView()
		{
			switch (_selectedViewIndex)
			{
				case 0: break;//((viewCmd)ViewVisual).InitReportMenu(); break;
				case 1: ShowPlot(true); break;
				case 2: ShowMeshPanel(true); break;
				case 3: ShowColorMesh(true); break;
				case 4: ShowSimResult(true); break;
			}
		}

		#endregion

		#region Create Menu

		#endregion

		#region Read Swop-File

		string GetPathSwop
		{
			get { return Path.Combine(_swatWorkDir,"Swop"); }
		}

		public void OpenSwopFile()
		{
			OpenFileDialog dlg = new OpenFileDialog
			{
				InitialDirectory = GetPathSwop,
				Filter = "Swop Log-Files (*.swp-log)|*.swp-log|All files (*.*)|*.*"
			};

			if (dlg.ShowDialog() == true)
			{
				_hasValidData = ReadLogFile(dlg.FileName);
			}

			if (_hasValidData)
			{
				if (!_hasCode)// achtung: sonst klappt es mit den Umlauten nicht! 
					ViewReportDataContext = new VmReport(_swopData);
				Reporter reporter = new Reporter(_swopData);
				((VmReport)ViewReportDataContext).SetTheCode(reporter.CreateTheReport());
				_hasCode = true;
				//}			

				SelectedTabIndex = 0;
				SelectedMeshParameter = null;

				OnPropertyChanged("SwopLogName");
				OnPropertyChanged("SwopLogError");

				OnPropertyChanged("VisReportCommands");
				OnPropertyChanged("VisErrorLog");
				OnPropertyChanged("VisPlotCommands");
				OnPropertyChanged("VisMeshPanelCommands");
				OnPropertyChanged("VisColorMeshCommands");
				OnPropertyChanged("VisSimResultCommands");

				OnPropertyChanged("SelectedTabIndex");
				InitMenues();
			}

		}

		bool ReadLogFile(string fn)
		{
			return _swopData.Read(fn);
		}

		#endregion


		#region Protokoll

		void PrintReport()
		{
			((VmReport)ViewReportDataContext).PrintReport();

		}

		void SaveReport()
		{
			((VmReport)ViewReportDataContext).SaveReport();

		}

		void ClearParameterFileBoxes()
		{
			((viewCmd)ViewVisual).ClearParameterFileBoxes();
		}

		void SetAllParameterFileBoxes()
		{
			((viewCmd)ViewVisual).SetAllParameterFileBoxes();
		}


		int[] GetSelectedParameterFiles()
		{
			List<int> ss = new List<int>();
			for (int i = 0; i < _swopData.OptSets.Count; i++)
			{
				if (((viewCmd)ViewVisual).IsParameterFileSelected(i))
					ss.Add(i);
			}
			return ss.ToArray();
		}


		void CreateParameterFiles()
		{
			string outputPath;
			using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
			{
				System.Windows.Forms.DialogResult result = fbd.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
				{
					outputPath = fbd.SelectedPath;
				}
				else return;
			}

			//ParamCreator pc = new ParamCreator(_swopData);
			foreach(int setId in GetSelectedParameterFiles())
			{
				if (!ParamCreator.CreateParamFile(_swopData, setId, outputPath))
					return;
			}

			DlgMessage.Show("Parameter-Files",$"{GetSelectedParameterFiles().Length} Files written", MessageLevel.Info);
		}
		#endregion

		#region Plot

		private bool CanShowPlot
		{
			get {return _swopData.HasValidData;}
		}

		void ShowPlot(bool doRescale)
		{
			if (!CanShowPlot)
				return;

			int paramId = ((viewCmd)ViewVisual).GetPlotParameterChoice();
			bool absoluteErrors = ((viewCmd)ViewVisual).IsPlotErrorAbsolute();
			int[] setIds = GetSelectedPlotSets();
			List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedPlotLaps();

			ViewVisualDataContext = new VmPlot(_swopData, absoluteErrors, paramId, setIds, optLaps);
		}

		int[] GetSelectedPlotSets()
		{
			List<int> ss = new List<int>();
			for(int i=0;i<= _swopData.OptSets.Count;i++)
			{
				if (((viewCmd)ViewVisual).IsPlotSetSelected(i))
					ss.Add(i);
			}

			return ss.ToArray();
		}

		void PlotClearSetBoxes()
		{
			((viewCmd)ViewVisual).ClearPlotSetBoxes();
			ShowPlot(false);
		}

		void PlotClearLapBoxes()
		{
			((viewCmd)ViewVisual).ClearPlotLapBoxes();
			ShowPlot(false);

		}

		void PlotSetAllLapBoxes()
		{
			((viewCmd)ViewVisual).SetAllPlotLapBoxes();
			ShowPlot(false);
		}

		#endregion


		#region MeshPanel

		public string SelectedMeshParameter { get; set; }

		private bool CanShowMeshPanel
		{
			get
			{
				if (_swopData.HasValidData && _swopData.HasMeshData)
				{
					List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedMeshPanelLaps();
					return (optLaps.Count > 0);
				}
				return false;
			}
		}

		void ShowMeshPanel(bool doRescale)
		{
			if (!CanShowMeshPanel)
				return;

			_swopData.ClearErrorLimits(); // Neu-Skalierung erzwingen
			
			List<ParamCombi> pcl = _swopData.GetParamCombisSelected(_swopData.OptParameters, SelectedMeshParameter);
			bool absoluteErrors = ((viewCmd)ViewVisual).IsMeshPanelErrorAbsolute();
			List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedMeshPanelLaps();
			int setId = GetSelectedMeshPanelSet();

			ViewVisualDataContext = new VmPanelMesh(_swopData, absoluteErrors, pcl, setId, optLaps);
		}

		void MeshPanelClearLapBoxes()
		{
			((viewCmd)ViewVisual).ClearMeshPanelLapBoxes();
			ShowMeshPanel(true);
		}

		void MeshPanelSetAllLapBoxes()
		{
			((viewCmd)ViewVisual).SetAllMeshPanelLapBoxes();
			ShowMeshPanel(true);
		}

		int GetSelectedMeshPanelSet()
		{
			for (int i = 0; i <= _swopData.OptSets.Count; i++)
			{
				if (((viewCmd)ViewVisual).IsMeshPanelSetSelected(i))
					return i;
			}

			return 0;
		}

		#endregion


		#region Mesh

		
		private bool CanShowColorMesh
		{
			get 
			{
				if(_swopData.HasValidData && _swopData.HasMeshData)
				{ 
					List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedMeshLaps();
					return (optLaps.Count > 0);
				}
				return false;
			}
		}

		void ShowColorMesh(bool doRescale)
		{
			if (!CanShowColorMesh)
				return;

			if (doRescale)
				_swopData.ClearErrorLimits();

			ParamCombi pc =  _swopData.ParamCombinations[((viewCmd)ViewVisual).GetMeshParameterChoice()];
			bool absoluteErrors = ((viewCmd)ViewVisual).IsMeshErrorAbsolute();
			List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedMeshLaps();
			int setId = GetSelectedMeshSet();

			ViewVisualDataContext = new VmColorMesh(_swopData, absoluteErrors, pc,setId, optLaps);
		}


		void MeshClearLapBoxes()
		{
			((viewCmd)ViewVisual).ClearMeshLapBoxes();
			ShowColorMesh(true);
		}

		void MeshSetAllLapBoxes()
		{
			((viewCmd)ViewVisual).SetAllMeshLapBoxes();
			ShowColorMesh(true);
		}


		int GetSelectedMeshSet()
		{
			for (int i = 0; i <= _swopData.OptSets.Count; i++)
			{
				if (((viewCmd)ViewVisual).IsMeshSetSelected(i))
					return i;
			}

			return 0;
		}

		#endregion


		#region SimResult

		private bool CanShowSimResult
		{
			get
			{
				if (_swopData.HasValidData)
				{
					List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedMeshLaps();
					return (optLaps.Count > 0);
				}
				return false;
			}
		}

		void ShowSimResult(bool dummy)
		{
			if (!CanShowSimResult)
				return;


			int setId = GetSelectedSimResultSet();

			VmSimResult simResult = new VmSimResult(_swopData, setId);
			if (simResult != null)
				ViewVisualDataContext = simResult;

		}

		int GetSelectedSimResultSet()// achtung: common fällt weg desh. 1 Set zu viel
		{
			for (int i = 0; i < _swopData.OptSets.Count; i++)
			{
				if (((viewCmd)ViewVisual).IsSimResultSetSelected(i))
					return i;
			}

			return 0;
		}

		#endregion
	}
}

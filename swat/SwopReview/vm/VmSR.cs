using Microsoft.Win32;
using SwatPresentations;
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
	public class VmSR :VmBase
	{
		#region variable

		VmBase _vmReport;
		bool _hasValidData;
		int _selectedViewIndex;
		bool _hasCode;

		//string _swopLogName="";

		RelayCommand _openFileCommand;
		RelayCommand _printReportCommand;
		RelayCommand _saveReportCommand;
		RelayCommand _createParameterFilesCommand;
		RelayCommand _clearParameterFilesBoxesCommand;
		RelayCommand _setAllParameterFilesBoxesCommand;


		RelayCommand _plotCommand;
		RelayCommand _clearSetBoxesCommand;
		RelayCommand _clearLapBoxesCommand;
		RelayCommand _setAllLapBoxesCommand;


		#endregion

		#region Commands

		public ICommand OpenFileCommand { get { return _openFileCommand; } }
		public ICommand PrintReportCommand { get { return _printReportCommand; } }
		public ICommand SaveReportCommand { get { return _saveReportCommand; } }
		public ICommand CreateParameterFilesCommand { get { return _createParameterFilesCommand; } }
		public ICommand ClearParameterFileBoxesCommand { get { return _clearParameterFilesBoxesCommand; } }
		public ICommand SetAllParameterFileBoxesCommand { get { return _setAllParameterFilesBoxesCommand; } }

		public ICommand ShowPlotCommand { get { return _plotCommand; } }
		public ICommand ClearSetBoxesCommand { get { return _clearSetBoxesCommand; } }
		public ICommand ClearLapBoxesCommand { get { return _clearLapBoxesCommand; } }
		public ICommand SetAllLapBoxesCommand { get { return _setAllLapBoxesCommand; } }

		
		#endregion

		#region Construction

		public VmSR(SwopData sd, UserControl view): base(sd, view)
		{
			_openFileCommand = new RelayCommand(param => this.OpenSwopFile());
			_printReportCommand = new RelayCommand(param => this.PrintReport());
			_saveReportCommand = new RelayCommand(param => this.SaveReport());
			_createParameterFilesCommand= new RelayCommand(param => this.CreateParameterFiles());

			_clearParameterFilesBoxesCommand = new RelayCommand(param => this.ClearParameterFileBoxes());
			_setAllParameterFilesBoxesCommand = new RelayCommand(param => this.SetAllParameterFileBoxes());

			_plotCommand = new RelayCommand(param => this.ShowPlot());
			_clearSetBoxesCommand= new RelayCommand(param => this.ClearSetBoxes());
			_clearLapBoxesCommand = new RelayCommand(param => this.ClearLapBoxes());
			_setAllLapBoxesCommand = new RelayCommand(param => this.SetAllLapBoxes());

		}

		#endregion


		#region Properties


		public VmBase ViewReportDataContext
		{
			get { return _vmReport; } // return this
			set // kann weg
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

		public Visibility VisPlotCommands
		{
			get { return (_swopData.HasValidData && _selectedViewIndex == 0) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility VisReportCommands
		{
			get { return (_swopData.HasValidData && _selectedViewIndex == 1) ? Visibility.Visible : Visibility.Collapsed; }
		}

		public int SelectedTabIndex
		{
			get { return _selectedViewIndex; }
			set
			{
				_selectedViewIndex = value;
				OnPropertyChanged("VisPlotCommands");
				OnPropertyChanged("VisReportCommands");
			}
		}


		#endregion

		#region Create Menu

		#endregion

		#region Read Swop-File

		public static string GetPathSwop
		{
			get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Swat","Swop"); }
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
			OnPropertyChanged("SwopLogName");
			OnPropertyChanged("SwopLogError");

			OnPropertyChanged("VisErrorLog");
			OnPropertyChanged("VisPlotCommands");
			OnPropertyChanged("VisReportCommands");

			((viewCmd)ViewVisual).InitPlotMenu();
			((viewCmd)ViewVisual).InitReportMenu();


			if (_hasValidData)
			{
				if(!_hasCode)// achtung: sonst klappt es mit den Umlauten nicht! 
					ViewReportDataContext = new VmReport(_swopData);
				Reporter reporter = new Reporter(_swopData);
				((VmReport)ViewReportDataContext).SetTheCode(reporter.CreateTheReport());
				_hasCode = true;
			 }




		}

		bool ReadLogFile(string fn)
		{
			//_swopLogName = fn;
			//_swopData = new SwopData();
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

			ParamCreator pc = new ParamCreator(_swopData);
			foreach(int setId in GetSelectedParameterFiles())
			{
				if (!pc.CreateParamFile(setId, outputPath))
					return;
			}

			DlgMessage.Show("Parameter-Files",$"{GetSelectedParameterFiles().Length} Files written", MessageLevel.Info);




		}
		#endregion

		#region Plot

		void ShowPlot()
		{
			int paramId = ((viewCmd)ViewVisual).GetParameterChoice();
			bool absoluteErrors = ((viewCmd)ViewVisual).IsErrorAbsolute();
			int[] setIds = GetSelectedSets();
			List<string> optLaps = ((viewCmd)ViewVisual).GetSelectedLaps();

			ViewVisualDataContext = new VmXY(_swopData, absoluteErrors, paramId, setIds, optLaps);

		}

		#endregion

		#region Plot

		int[] GetSelectedSets()
		{
			List<int> ss = new List<int>();
			for(int i=0;i<= _swopData.OptSets.Count;i++)
			{
				if (((viewCmd)ViewVisual).IsSetSelected(i))
					ss.Add(i);
			}

			return ss.ToArray();

		}

		void ClearSetBoxes()
		{
			((viewCmd)ViewVisual).ClearSetBoxes();
		}

		void ClearLapBoxes()
		{
			((viewCmd)ViewVisual).ClearLapBoxes();
		}

		void SetAllLapBoxes()
		{
			((viewCmd)ViewVisual).SetAllLapBoxes();
		}

		#endregion
	}
}

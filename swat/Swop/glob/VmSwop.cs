using Microsoft.Win32;
//using Swop.data;
using Swop.optimizer;
//using Swop.sim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TTP.UiUtils;
using swatSim;

namespace Swop.glob
{
	public class VmSwop :INotifyPropertyChanged
	{

		GlobData _gd;
		RelayCommand _openCommand;
		RelayCommand _abortCommand;
		RelayCommand _executeCommand;

		MultiOptimizer _optimizer;

		bool _canOptimize;
		bool _isOptimizationRunning;


		public VmSwop()
		{
			_openCommand = new RelayCommand(param => this.OpenCommandFile(), param => this.CanOpenCommandFile);
			_abortCommand = new RelayCommand(param => this.AbortOptimization(), param => this.CanAbortOptimization);
			_executeCommand = new RelayCommand(param => this.ExecuteOptimization(), param => this.CanExecuteOptimization);
			_gd = new GlobData();
		}

		public ICommand OpenCommand { get { return _openCommand; } }
		public ICommand AbortCommand { get { return _abortCommand; } }
		public ICommand ExecuteCommand { get { return _executeCommand; } }

		public string CmdName
		{
			get { return _gd.CmdFilename; }
		}
		// Texte für Binding UI
		public string CmdErrorText
		{
			get { return _gd.ErrorMessage; }
		}

		public string BestText
		{
			get { return _gd.BestText; }
		}

		public string PrologText
		{
			get {return _gd.PrologText;}
		}


		public string OptStep{ get { return _gd.OptStep; }}
		public string StepEval { get { return _gd.StepEval; } }
		public string OptLap { get { return _gd.OptLap; } }
		public string LapEval { get { return _gd.LapEval; } }
		public string EndText { get { return _gd.EndText; } }
		public string RemainingSteps { get { return _gd.RemainingSteps; } }
		public string TotalBestEval { get { return _gd.TotalBestEval; } }


		bool CanAbortOptimization
		{
			get { return _isOptimizationRunning; }
		}

		void AbortOptimization()
		{
			_optimizer.AbortOptimization();
			_isOptimizationRunning = false;
		}

		bool CanExecuteOptimization
		{
			get { return _canOptimize && !_isOptimizationRunning; }
		}

		void ExecuteOptimization()
		{
			_optimizer.ExecuteOptimization();
			_isOptimizationRunning = true;
		}

		bool CanOpenCommandFile
		{
			get { return !_isOptimizationRunning; }
		}

		void OpenCommandFile()
		{
			OpenFileDialog dlg = new OpenFileDialog
			{
				InitialDirectory = GlobData.GetPathSwop,
				Filter = "Swop files (*.swop)|*.swop|All files (*.*)|*.*"
			};

			if (dlg.ShowDialog()== true)
			{
				_canOptimize = _gd.ReadInstructions(dlg.FileName);
				_optimizer = new MultiOptimizer(_gd, OptimizerProgressChanged, OptimizerCompleted);
				if (_canOptimize)
				{ 
					_gd.CreatePrologText(_optimizer);
					_gd.WriteSwopPrologText();
				}
				else
					_gd.PrologText = _gd.ErrorMessage;
			}

			OnPropertyChanged("CmdName");
			//OnPropertyChanged("CmdErrorText");
			//OnPropertyChanged("ActionText");
			OnPropertyChanged("BestText");
			OnPropertyChanged("PrologText");

		}

		#region Backgroundworker-Methoden

		void OptimizerProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage == 0)
			{ 
				OnPropertyChanged("PrologText");
				return;
			}

			if (e.ProgressPercentage == 50)
			{
				//OnPropertyChanged("ActionText");
				OnPropertyChanged("OptStep");
				OnPropertyChanged("StepEval");
				OnPropertyChanged("OptLap");
				OnPropertyChanged("LapEval");
				OnPropertyChanged("RemainingSteps");
				OnPropertyChanged("TotalBestEval");

				OnPropertyChanged("BestText");
				return;
			}

			if (e.ProgressPercentage == 100)
			{
				OnPropertyChanged("EndText");
				//_gd.LogAction();
				//_gd.LogEnd();
			}

		}

		void OptimizerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_isOptimizationRunning = false;
			//_canUpdateParameters = true;
			CommandManager.InvalidateRequerySuggested();
		}

		#endregion

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			try
			{
				PropertyChangedEventHandler handler = this.PropertyChanged;
				if (handler != null)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
			catch { }// wegen   Tooltip-Fehler: so wird exception abgefangen - aber tooltip bleibt leer


		}

		#endregion

	}
}

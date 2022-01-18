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
using System.Windows;

namespace Swop.glob
{
	public class VmSwop :INotifyPropertyChanged
	{

		//GlobData SwopData;
		RelayCommand _openCommand;
		RelayCommand _abortCommand;
		RelayCommand _executeCommand;

		MultiOptimizer _optimizer;
		MultiCombiner  _combiner;


		bool _canExecute;
		bool _isExecutionRunning;
		public GlobData SwopData { get; private set; }


		public VmSwop()
		{
			_openCommand = new RelayCommand(param => this.OpenCommandFile(), param => this.CanOpenCommandFile);
			_abortCommand = new RelayCommand(param => this.AbortExecution(), param => this.CanAbortExecution);
			_executeCommand = new RelayCommand(param => this.ExecuteOptimization(), param => this.CanExecuteOptimization);
			SwopData = new GlobData();
		}

		public ICommand OpenCommand { get { return _openCommand; } }
		public ICommand AbortCommand { get { return _abortCommand; } }
		public ICommand ExecuteCommand { get { return _executeCommand; } }
		// Binding UI
		public string CmdName{get { return SwopData.CmdFilename; }}
		public string CmdErrorText	{	get { return SwopData.ErrorMessage; }}
		public string BestText{	get { return SwopData.BestText; }}
		public string PrologText{	get {return SwopData.PrologText;}}
		public Visibility VisOptimization 
		{	
			get { if ((SwopData.WorkMode == SwopWorkMode.LEAST) || (SwopData.WorkMode == SwopWorkMode.SHRINK))
						return Visibility.Visible;
					else
						return Visibility.Collapsed; 
			}	
		}
		public Visibility VisCombination { get { return (SwopData.WorkMode == SwopWorkMode.COMBI) ? Visibility.Visible : Visibility.Collapsed; } }
		public string OptStep{ get { return SwopData.OptStep; }}
		public string StepEval { get { return SwopData.StepEval; } }
		public string OptLap { get { return SwopData.OptLap; } }
		public string LapEval { get { return SwopData.LapEval; } }
		public string EndText { get { return SwopData.EndText; } }
		public string RemainingSteps { get { return SwopData.RemainingSteps; } }
		public string TotalBestEval { get { return SwopData.TotalBestEval; } }
		public string CombiStep { get { return SwopData.CombiStep; } }
		public string CombiStepsRemaining { get { return SwopData.CombiStepsRemaining; } }



		bool CanAbortExecution
		{
			get { return _isExecutionRunning; }
		}

		void AbortExecution()
		{
			if ((SwopData.WorkMode == SwopWorkMode.LEAST) || (SwopData.WorkMode == SwopWorkMode.SHRINK))
				_optimizer.AbortExecution();
			else
				_combiner.AbortExecution();

			_isExecutionRunning = false;
		}

		bool CanExecuteOptimization
		{
			get { return _canExecute && !_isExecutionRunning; }
		}

		void ExecuteOptimization()
		{
			if ((SwopData.WorkMode == SwopWorkMode.LEAST) || (SwopData.WorkMode == SwopWorkMode.SHRINK))
				_optimizer.ExecuteOptimization();
			else
				_combiner.ExecuteCombination();

			_isExecutionRunning = true;
		}

		bool CanOpenCommandFile
		{
			get { return !_isExecutionRunning; }
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
				_canExecute = SwopData.ReadInstructions(dlg.FileName);

				//if (SwopData.SwopMode == SwopWorkMode.OPTI)
				//	_optimizer = new MultiOptimizer(SwopData, OptimizerProgressChanged, OptimizerCompleted);
				//else
				//	_combiner = new MultiCombiner(SwopData, CombinerProgressChanged, CombinerCompleted);

				if (_canExecute)
				{
					if ((SwopData.WorkMode == SwopWorkMode.LEAST) || (SwopData.WorkMode == SwopWorkMode.SHRINK))
					{
						_optimizer = new MultiOptimizer(SwopData, OptimizerProgressChanged, OptimizerCompleted);
						SwopData.CreatePrologDisplayText(_optimizer);
					}
					else { 
						_combiner = new MultiCombiner(SwopData, CombinerProgressChanged, CombinerCompleted);
						SwopData.CreatePrologDisplayText(_combiner);
					}

					SwopData.WritePrologLogText();
					SwopData.EndText = "";
				}
				else
					SwopData.PrologText = SwopData.ErrorMessage;
			}

			OnPropertyChanged("CmdName");
			OnPropertyChanged("BestText");
			OnPropertyChanged("PrologText");
			OnPropertyChanged("EndText");
			OnPropertyChanged("VisOptimization");
			OnPropertyChanged("VisCombination");


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
			}
		}

		void OptimizerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_isExecutionRunning = false;
			CommandManager.InvalidateRequerySuggested();
		}

		void CombinerProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (e.ProgressPercentage == 0)
			{
				OnPropertyChanged("PrologText");
				return;
			}

			if (e.ProgressPercentage == 50)
			{
				OnPropertyChanged("CombiStep");
				OnPropertyChanged("StepEval");
				OnPropertyChanged("CombiStepsRemaining");
				return;
			}

			if (e.ProgressPercentage == 100)
			{
				OnPropertyChanged("EndText");
			}

		}

		void CombinerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_isExecutionRunning = false;
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

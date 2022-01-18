using swat.iodata;
using swatSim;
using swat.optimizer;
using swat.Optimizer;
using swat.views.dlg;
using swat.views.sheets;
using SwatPresentations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TTP.UiUtils;

namespace swat.vm
{
	public class VmOptimizationControl:VmBase
	{
		#region Variable

		RelayCommand _selectParametersCommand;
		RelayCommand _executeCommand;
		RelayCommand _abortCommand;
		RelayCommand _acceptCommand;
		RelayCommand _reportCommand;
		RelayCommand _notesCommand;

		//RelayCommand _transmitCommand; // Dialog Parameterauswahl




		OptimizationData _opData;

		bool _isOptimizationRunning;
		bool _canUpdateParameters;


		// für  Parameter-Auswahl Dialog
		SimParamData _modelParameters;
		DataTable _dtModelParams = null;
		const int MaxOptParams = 10;


		// Kommunikation mit grafik
		public event EventHandler <OptEventArgs> BestParametersChanged;

		#endregion

		#region Contruction + Init

		public VmOptimizationControl(VmSwat vmSwat) : base(vmSwat)
		{
			_opData = new OptimizationData(Workspace, OptimizerProgressChanged, OptimizerCompleted);

			_modelParameters = Workspace.CurrentWorkingParameters.Copy();

			_selectParametersCommand = new RelayCommand(param => this.SelectParameters(), param => this.CanSelectParameters);
			_executeCommand = new RelayCommand(param => this.ExecuteOptimization(), param => this.CanExecuteOptimization);
			_abortCommand = new RelayCommand(param => this.AbortOptimization(), param => this.CanAbortOptimization);
			_acceptCommand = new RelayCommand(param => this.AcceptOptimization(), param => this.CanAcceptOptimization);
			_reportCommand = new RelayCommand(param => this.ReportOptimization(), param => this.CanReportOptimization);
			_notesCommand = new RelayCommand(param => this.ShowNotes());
			UseRelationEval = true;

			ViewVisual = new ViewOptimizationControl();
		}

		#endregion

		#region Commands

		public ICommand SelectParametersCommand { get { return _selectParametersCommand; } }
		public ICommand ExecuteCommand { get { return _executeCommand; } }
		public ICommand AbortCommand { get { return _abortCommand; } }
		public ICommand AcceptCommand { get { return _acceptCommand; } }
		public ICommand ReportCommand { get { return _reportCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }




		bool CanSelectParameters
		{
			get { return !_isOptimizationRunning; }
		}

		bool CanExecuteOptimization
		{
			get { return _opData.HasParameters && !_isOptimizationRunning; }
		}

		bool CanAbortOptimization
		{
			get { return _isOptimizationRunning; }
		}

		bool CanAcceptOptimization
		{
			get { return _canUpdateParameters; }
		}

		bool CanReportOptimization
		{
			get { return _canUpdateParameters; }
		}

		public bool CanTransmitParameters
		{
			get { return (_modelParameters.GetNumSelectedParams()> 0) && (_opData.FirstEvalIndex < _opData.LastEvalIndex); }
		}

		#endregion


		#region Properties

		public string Model
		{
			get { return Workspace.CurrentModelName; }
		}

		public string Workspacename
		{
			get { return Workspace.Name; }
		}

		public string ParameterText
		{
			get { return _opData.ParamHeaderText; }
		}

		public string OriginText
		{
			get { return  _opData.OriginalValuesText; }
		}

		public string BestText
		{
			get { return _opData.BestValuesText; }
		}

		public string CurrentText
		{
			get { return  _opData.CurrentText; }
		}

		public bool OptimizationRunning
		{
			get { return _isOptimizationRunning; }
		}

		public DataTable ParameterTable
		{
			get { return _dtModelParams; }
		}

		#endregion


		#region Methoden

		void SelectParameters()
		{
			CreateDlgTable(true);
			if (DlgOptimizationParameters.Show(this))
			{
				_canUpdateParameters = false;

				_opData.SetOptimizerParams(_modelParameters);
				OnPropertyChanged("ParameterText");
				OnPropertyChanged("OriginText");
				OnPropertyChanged("CurrentText");
				OnPropertyChanged("BestText");
			}
		}

		void ExecuteOptimization()
		{
			_opData.ExecuteOptimization();
			_isOptimizationRunning = true;
			_canUpdateParameters = false;
		}

		void AbortOptimization()
		{
			_opData.AbortOptimization();
			_isOptimizationRunning = false;
		}

		void AcceptOptimization()
		{
			Workspace.HasValidPopulationData = false;
			Workspace.DataSetParameters.AddParamData(_opData.BestParameters);
			Workspace.CurrentModel.InitModelParameters(Workspace.DataSetParameters);
			Workspace.DataSetParameters.WriteToFile();
			_canUpdateParameters = false;
		}

		void ReportOptimization()
		{
			_opData.SaveReport();
		}

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		void OptimizerProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			OnPropertyChanged("CurrentText");
			OnPropertyChanged("BestText");
			OnPropertyChanged("OriginText"); // wegen Nachtrag Eval-Wert mit Ausgangsparametern
			if (e.UserState != null)
			{
				OptEventArgs args = new OptEventArgs { PresData = (PresentationsData)e.UserState };
				OnBestParametersChanged(args);
			}
		}

		void OptimizerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			_isOptimizationRunning = false;
			_canUpdateParameters = true;
			CommandManager.InvalidateRequerySuggested();
		}

		#endregion

		#region Properties Auswahl-Dialog

		public DateTime EvalRangeStart
		{
			get
			{
				DateTime d = DateTime.Parse("1.1." + Workspace.SimulationYear);
				return d.AddDays(_opData.FirstEvalIndex);
			}
			set
			{

				_opData.FirstEvalIndex = (value.Year == Workspace.SimulationYear) ?
					value.DayOfYear - 1 :0;

				OnPropertyChanged("EvalRangeStart");
			}
		}

		public DateTime EvalRangeEnd
		{
			get
			{
				DateTime d = DateTime.Parse("1.1." + Workspace.SimulationYear);
				return d.AddDays(_opData.LastEvalIndex);
			}
			set
			{
				_opData.LastEvalIndex = (value.Year == Workspace.SimulationYear) ?
					value.DayOfYear - 1 : 364;

				OnPropertyChanged("EvalRangeEnd");
			}
		}

		public bool UseRelationEval
		{
			get
			{
				return _opData.UseRelationEval;
			}
			set
			{
				_opData.UseRelationEval = value;
				OnPropertyChanged("UseRelationEval");
			}
		}
		#endregion

		#region Methoden Auswahl-dialog

		private void CreateDlgTable(bool firstInit)
		{
				if (Workspace == null)
					return;

			_dtModelParams = new DataTable();
			_dtModelParams.Columns.Add("ParamSelected", typeof(bool));
			_dtModelParams.Columns.Add("ParamKey", typeof(String));
			_dtModelParams.Columns.Add("ParamValue", typeof(String));
			_dtModelParams.Columns.Add("ParamHelp", typeof(String));
			_dtModelParams.Columns.Add("RowColor", typeof(String));

			foreach (string s in _modelParameters.ParamDict.Keys)
			{
				DataRow row = _dtModelParams.NewRow();
				SimParamElem elem = _modelParameters.ParamDict[s];

				if (!elem.IsSelected) // auf Ausgangswert zurücksetzen, wenn de-selektiert
				{
					elem = Workspace.CurrentWorkingParameters.GetParamElem(s);
				}
				row["ParamSelected"] =  elem.IsSelected;

				row["ParamKey"] = s;
					row["ParamHelp"] = elem.Descr;
					if (Type.GetTypeCode(elem.ObjType) == TypeCode.Double)
						row["ParamValue"] = ((Double)elem.Obj).ToString("0.0##", CultureInfo.InvariantCulture);
					else
						row["ParamValue"] = elem.Obj.ToString();

					if (elem.Obj == null)
						row["RowColor"] = "Red";
					else
					{
					if (elem.IsSelected)
						row["RowColor"] = "LawnGreen";
					else
						row["RowColor"] = (elem.IsChanged) ? "Yellow" : "White";
					}

				_dtModelParams.Rows.Add(row);
			}
			_dtModelParams.RowChanged += Row_Changed;

			OnPropertyChanged("ParameterTable");
		}

		private void Row_Changed(object sender, DataRowChangeEventArgs e)
		{

			//int i = 0;
			//CheckAuswahl();
		}

		public int GetIndex(string key)
		{
			return _modelParameters.ParamDict.Keys.ToList().IndexOf(key);
		}

		public string SetAndValidateEditText(string key, string txt)
		{
			// todo: Plausibilitätskontrolle Parameter
			SimParamElem origParam = _modelParameters.ParamDict[key];
			if (!origParam.IsSelected)
			{
				CreateDlgTable(false);
				return origParam.Obj.ToString();
			}

			object obj = Workspace.DataSetParameters.GetConvertedElement(key, txt.Replace(',', '.'));
			if (obj != null)
			{
				if (obj.ToString() != origParam.Obj.ToString())
				{
					origParam.IsChanged = true;
					origParam.Obj = obj;
				}
				CreateDlgTable(false);
			}

			return (obj == null) ? "???" : obj.ToString();
		}

		public void SetSelected(string key, bool isChecked)
		{
			if (_modelParameters.GetNumSelectedParams() > MaxOptParams)
				isChecked = false;

			SimParamElem origParam = _modelParameters.ParamDict[key];
			origParam.IsSelected = isChecked;
			CreateDlgTable(false);
		}


		#endregion

		#region Events

		protected virtual void OnBestParametersChanged(OptEventArgs e)
		{
			EventHandler<OptEventArgs> handler = BestParametersChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}


		#endregion

	}
}

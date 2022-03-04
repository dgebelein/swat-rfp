using swat.iodata;
using swatSim;
using swat.views.dlg;
using swat.views.sheets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using TTP.UiUtils;
using System.Windows;

namespace swat.vm
{
	class VmParameterGrid:VmBase
	{
		#region Variable

		RelayCommand _updateCommand;
		RelayCommand _defaultCommand;
		RelayCommand _importCommand;
		RelayCommand _notesCommand;


		VmBase _parentPanel;

		SimParamData _workParameters;
		SimParamData _defaultParameters;


		DataTable _dtParams = null;

		private bool _isAnyParameterChanged;
		private bool _isdefaulted;
		int _selectedIndex;


		#endregion


		#region Constructor

		public VmParameterGrid(VmSwat vmSwat, VmBase parentPanel):base(vmSwat)
      {
			_parentPanel = parentPanel;
			ViewVisual = new ViewParameterGrid();
			_updateCommand = new RelayCommand(param => this.UpdateParameters(), param => this.CanUpdateParameters);
			_defaultCommand = new RelayCommand(param => this.SetDefaultParameters(), param => this.CanSetDefaultParameters);
			_importCommand = new RelayCommand(param => this.ImportParameters());
			_notesCommand = new RelayCommand(param => this.ShowNotes());

			_defaultParameters = Workspace.DefaultParameters.Clone();
			_workParameters = Workspace.CurrentWorkingParameters;
			
			InitTable();
		}

		public void InitTable()
		{
			if (Workspace == null)
				return;

			int legitCode = 0;
			if((bool)Application.Current.Properties["ExtUser"])
				legitCode=1;
			if ((bool)Application.Current.Properties["SuperUser"])
				legitCode = 10;
			if ((bool)Application.Current.Properties["Developer"])
				legitCode = 100;

			//if(_parameters == null)
			//	_parameters = Workspace.CurrentWorkingParameters;
			//_defaultParameters = Workspace.CurrentModel.DefaultParams;
			//_defaultParameters = Workspace.DefaultParameters; // neu 5.1.21


			_dtParams = new DataTable();
			_dtParams.Columns.Add("ParamKey", typeof(String));
			_dtParams.Columns.Add("ParamValue", typeof(String));
			_dtParams.Columns.Add("ParamHelp", typeof(String));
			_dtParams.Columns.Add("RowColor", typeof(String));

			foreach (string s in _workParameters.ParamDict.Keys)
			{
				DataRow row = _dtParams.NewRow();
				SimParamElem elem = _workParameters.ParamDict[s];
				if (elem.Legit > legitCode) // nur Parameter mit entsprechender Berechtigung anzeigen
					continue;

				row["ParamKey"] = s;
				row["ParamHelp"] = elem.Descr;
				if (Type.GetTypeCode(elem.ObjType) ==  TypeCode.Double)
					row["ParamValue"] = ((Double)elem.Obj).ToString("0.0##",CultureInfo.InvariantCulture);
				else
					row["ParamValue"] = elem.Obj.ToString();

				if (elem.Obj == null)
					row["RowColor"] = "Red";
				else
					row["RowColor"] = (elem.IsChanged) ? "Yellow" : "White";

				_dtParams.Rows.Add(row);
			}
			OnPropertyChanged("ParameterTable");
		}


		#endregion

		#region Properties

		public ICommand UpdateCommand { get { return _updateCommand; } }
		public ICommand DefaultCommand { get { return _defaultCommand; } }
		public ICommand ImportCommand { get { return _importCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }


		public DataTable ParameterTable
		{
			get { return _dtParams; }
		}

		public int SelectedIndex
		{

			get { return _selectedIndex; }
			set {
				_selectedIndex = value;
				//OnPropertyChanged("SelectedIndex");
			}
		}

		public string Model
		{
			get { return Workspace.CurrentModelName; }
		}

		public string Workspacename
		{
			get { return Workspace.Name; }
		}
		#endregion


		#region Methods


		public bool CanUpdateParameters
		{
			get {return _isAnyParameterChanged;}
		}

		public void UpdateParameters()
		{
			Workspace.HasValidPopulationData = false;
			Workspace.DataSetParameters.AddParamData(_workParameters,_isdefaulted);
			Workspace.CurrentModel.InitModelParameters(Workspace.DataSetParameters);
			Workspace.DataSetParameters.WriteToFile();
			_isAnyParameterChanged = false;
			_isdefaulted = false;

			_vmSwat.UpdateMenuContent();
		}

		private bool CanSetDefaultParameters
		{
			get
			{
				foreach (string s in _workParameters.ParamDict.Keys)
				{
					if (_workParameters.ParamDict[s].IsChanged)
						return true;
				}
				return false;
			}
		}

		private void SetDefaultParameters()
		{
			Workspace.CurrentModel.InitModelParameters(Workspace.DefaultParameters); // neu 5.1.21
			SimParamData sd = new SimParamData();

			foreach (string key in _workParameters.ParamDict.Keys)
			{
				SimParamElem e = (SimParamElem)_defaultParameters.ParamDict[key].Clone();
				sd.ParamDict.Add(key, e);
			}
			_workParameters = sd;

			_isdefaulted = true;
			_isAnyParameterChanged = true;
			InitTable();
			_vmSwat.UpdateMenuContent();
		}


		public string SetAndValidateEditText(string key, string txt)
		{
			// todo: Plausibilitätskontrolle Parameter
			SimParamElem origParam = _workParameters.ParamDict[key];
			SimParamElem defParam = _defaultParameters.ParamDict[key];


			object obj = Workspace.CodedModelParameters.GetConvertedElement(key, txt.Replace(',','.'));
			if (obj != null)
			{ 
				if (obj.ToString() != origParam.Obj.ToString())
				{ 
					origParam.Obj = obj;
					_isAnyParameterChanged = true;
				}
				if(obj.ToString() == defParam.Obj.ToString())
				{
					origParam.IsChanged = false;
					_workParameters.SetToUnchanged(key);
				}
				else
					origParam.IsChanged = true;

				InitTable();
			}
			
			return  (obj == null)? "???": obj.ToString();
		}

		public int GetIndex(string key)
		{
			return _workParameters.ParamDict.Keys.ToList().IndexOf(key);
		}


		public bool IsChangedParameter(string key)
		{
			return _workParameters.ParamDict[key].IsChanged;
		}

		#endregion



		#region Import Parameter

		private void ImportParameters()
		{
			string paraFile = DlgImportParameters.Show(GetParameterFileList(), Workspace.CurrentModelName);
			if(paraFile != null)
			{
				SimParamData paras = Workspace.CurrentWorkingParameters;
				paras.Filename = paraFile + WorkspaceData.ExtSimParameters;
				paras.ReadFromFile();

				string prefix = Workspace.CurrentModel.GetParamPrefix();
				Dictionary<string, SimParamElem> paraDict = paras.GetParamDict(prefix);
				_workParameters.AddItemDictionary(paraDict);
				_isAnyParameterChanged = true;
				_isdefaulted = false;
				InitTable();

				_vmSwat.UpdateMenuContent();

			}
		}

		private List<string> GetParameterFileList()
		{
			List<string> wsList = new List<string>();

			string[] files = Directory.GetFiles(WorkspaceData.GetPathParameters, "*" + WorkspaceData.ExtSimParameters);

			foreach (string s in files)
			{
				wsList.Add(Path.GetFileNameWithoutExtension(s));
			}

			return wsList;
		}
		#endregion

		#region Notes

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		#endregion

	}
}

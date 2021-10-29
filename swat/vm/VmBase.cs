using swat.iodata;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace swat.vm
{
	public class VmBase : INotifyPropertyChanged, IDataErrorInfo
	{
		#region Variable

		protected VmSwat _vmSwat;
		protected VmBase _dataContextView;
		protected UserControl _viewVisual;
		protected Dictionary<string, string> _errorMessages;

		#endregion

		#region Construction

		protected VmBase(VmSwat vmSwat)
		{
			_vmSwat = vmSwat;
			_errorMessages = new Dictionary<string, string>();
		}

		#endregion

		#region Properties

		protected WorkspaceData Workspace { get { return ((VmSwat)_vmSwat).Workspace; } } // nur zum Abkürzen

		public UserControl ViewVisual
		{
			get { return _viewVisual; }
			set
			{
				_viewVisual = value;
				OnPropertyChanged("ViewVisual");
			}
		}

		public VmBase ViewVisualDataContext
		{
			get { return _dataContextView; } // return this ?
			set // kann weg ?
			{
				_dataContextView = value;
				OnPropertyChanged("ViewVisualDataContext");
			}
		}


		#endregion

		#region virtuals

		public virtual bool RespondToViewChange()
		{
			return true;
		}

		public virtual Visibility  VisibilityState
		{
			get { return Visibility.Visible; }
		}

		public virtual void UpdateMenuContent()
		{
		}

		public virtual void UpdateEventRoutings()
		{ }

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

		#region IDataErrorInfo- Validierung

		string IDataErrorInfo.Error
		{
			get { return null; }
		}

		protected virtual String GetValidationError(string propertyName)
		{
			return this.GetValidationString(propertyName);
		}


		string IDataErrorInfo.this[string propertyName]
		{
			get { return this.GetValidationError(propertyName); }
		}


		#endregion

		#region Validation

		public string GetErrorMessage(String key)
		{
			if (!_errorMessages.TryGetValue(key, out string error))
				return null;
			else
				return error;
		}

		protected String GetValidationString(String ElemName)
		{
			return GetErrorMessage(ElemName);
		}

		protected void ResetErrorList()
		{
			_errorMessages.Clear();
		}

		public bool ContainsError(String key)
		{
			return _errorMessages.ContainsKey(key);
		}

		public void AddErrorMessage(String elemKey, String msg)
		{
			if (!ContainsError(elemKey))
				_errorMessages.Add(elemKey, msg);
		}

		public bool HasErrors
		{
			get { return (_errorMessages.Count > 0); }
		}

		public bool CanApply
		{
			get { return _errorMessages.Count == 0; }
		}

		#endregion
	}
}

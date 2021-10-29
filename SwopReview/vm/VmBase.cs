using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace SwopReview
{
	public class VmBase : INotifyPropertyChanged
	{
		protected SwopData _swopData;

		protected VmBase _dataContextView;
		protected UserControl _viewVisual;
		protected Dictionary<string, string> _errorMessages;

		protected VmBase(SwopData sd, UserControl view)
		{
			_swopData = sd;
			_viewVisual = view;
		}


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
			get { return _dataContextView; } // return this
			set 
			{
				_dataContextView = value;
				OnPropertyChanged("ViewVisualDataContext");
			}
		}

		public SwopData SwpData
		{
			get { return _swopData; }
		}




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
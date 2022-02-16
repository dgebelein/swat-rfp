using SwatPresentations;
using swatSim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TTP.UiUtils;

namespace SwopCompare
{
	class VmCompare : INotifyPropertyChanged
	{
		#region var
			
		public CmpData Data { get; private set; }
		VisGenerator _visGen;
		protected ViewLeft _viewLeft;
		protected UserControl _viewRight;
		private PresentationsData _graphData;
		EvalMethod _quantorMethod;

		RelayCommand _loadFileCommand;
		RelayCommand _printCommand;
		RelayCommand _quantRelativeCommand;
		RelayCommand _quantAbsoluteCommand;
		RelayCommand _quantNormalizeCommand;


		#endregion

		#region ctor

		public VmCompare()
		{
			_loadFileCommand = new RelayCommand(param => this.LoadCommandFile());
			_printCommand = new RelayCommand(param => this.Print());
			_quantRelativeCommand = new RelayCommand(param => this.QuantRelative(), param => this.CanQuantRelative);
			_quantAbsoluteCommand = new RelayCommand(param => this.QuantAbsolute(), param => this.CanQuantAbsolute);
			_quantNormalizeCommand = new RelayCommand(param => this.QuantNormalize(), param => this.CanQuantNormalize);

			Data = new CmpData();
			_visGen = new VisGenerator(Data);
			_viewLeft = new ViewLeft();
		}

		#endregion

		#region binding properties
		public ICommand LoadFileCommand { get { return _loadFileCommand; } }

		public ViewLeft LeftView
		{
			get { return _viewLeft; }
		}

		public UserControl RightView
		{
			get { return _viewRight; }
			set
			{
				_viewRight = value;
				OnPropertyChanged("RightView");
			}
		}

		public string CommandFileName { get { return Path.GetFileName(Data.CommandFilename); } }
		public ICommand PrintCommand { get { return _printCommand; } }

		public Visibility VisQuantCommands { get { return Visibility.Visible; } } // aktiviert die quantifizierungspunkte im contextmenü
		public ICommand QuantRelativeCommand { get { return _quantRelativeCommand; } }
		public ICommand QuantAbsoluteCommand { get { return _quantAbsoluteCommand; } }
		public ICommand QuantNormalizeCommand { get { return _quantNormalizeCommand; } }

		bool CanQuantRelative { get { return (_quantorMethod != EvalMethod.Relation); } }
		bool CanQuantAbsolute { get { return (_quantorMethod != EvalMethod.AbsDiff); } }
		bool CanQuantNormalize { get { return (_quantorMethod != EvalMethod.Nothing); } }

		#endregion

		#region read command-File (left view)

		public void LoadCommandFile()
		{

			Data.LoadCommandFile();
			if(!Data.HasValidData)
			{				
				DlgMessage.Show("Swop-Compare - Error", Data.ErrMessage, MessageLevel.Error);

			}

			InitMenu();
			ShowResult();
		}

		void InitMenu()
		{
			_viewLeft.CreateSimSetPanel(ShowResult);			
			OnPropertyChanged("CommandFileName");
 
		}

		#endregion

		#region   response (right view)

		void ShowResult()
		{
			if (!Data.HasValidData)
				return;

			int setId = GetSelectedSimResultSet();
			if(setId >=0)
			{
				_quantorMethod = EvalMethod.Relation;
				_graphData = _visGen.GeneratePresentationsData(setId, _quantorMethod);
				RightView= PresentationCreator.Create(PresentationType.Optimization, _graphData, false);
			}
		}

		void ShowResult(EvalMethod quant)
		{
			if (!Data.HasValidData)
				return;

			int setId = GetSelectedSimResultSet();
			if (setId >= 0)
			{
				_quantorMethod = quant;
				_graphData = _visGen.GeneratePresentationsData(setId, _quantorMethod);
				RightView = PresentationCreator.Create(PresentationType.Optimization, _graphData, false);
			}
		}

		int GetSelectedSimResultSet()
		{
			for (int i = 0; i < Data.CompareSets.Count; i++)
			{
				if ((LeftView).IsSimResultSetSelected(i))
					return i;
			}

			return -1;
		}

		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.Optimization, _graphData, true);
			printPres.PrintView();
		}
		private void QuantRelative()
		{
			ShowResult(EvalMethod.Relation);
		}

		private void QuantAbsolute()
		{
			ShowResult(EvalMethod.AbsDiff);
		}

		private void QuantNormalize()
		{
			ShowResult(EvalMethod.Nothing);
		}

		#endregion


		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			try
			{
				this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			catch { }


		}

		#endregion
	}
}

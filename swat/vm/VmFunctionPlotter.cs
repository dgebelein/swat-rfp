using SwatPresentations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.UiUtils;
using swat;
using swat.views.sheets;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace swat.vm
{

	//enum FunctionType
	//{
	//	Probit,
	//	Logit,
	//	Fertility,
	//	DevRate,
	//	Aestiv
	//}



	class VmFunctionPlotter:VmBase
	{
		RelayCommand _printCommand;
		RelayCommand _transitionCommand;
		RelayCommand _aestivCommand;
		RelayCommand _devRatesCommand;
		RelayCommand _fertilityCommand;

		UserControl _viewPlotter;
		PlotterData _plotterData;


		//FunctionType _funcType;


		public VmFunctionPlotter(VmSwat vmSwat) : base(vmSwat)
		{
			_printCommand = new RelayCommand(param => this.Print());
			_transitionCommand = new RelayCommand(param => this.ShowTransition());
			_aestivCommand = new RelayCommand(param => this.ShowAestiv());
			_devRatesCommand = new RelayCommand(param => this.ShowDevRates());
			_fertilityCommand = new RelayCommand(param => this.ShowFertility());

			_plotterData = new PlotterData();
			ViewPlotter= PresentationCreator.Create(PresentationType.FunctionPlot, _plotterData, false);
			ViewVisual = new ViewFunctionPlotter();
			RefreshView();

		}

		#region Binding Properties

		public ICommand PrintCommand { get { return _printCommand; } }

		public ICommand ShowTransitionCommand { get { return _transitionCommand; } }
		public ICommand ShowAestivCommand { get { return _aestivCommand; } }
		public ICommand ShowDevRatesCommand { get { return _devRatesCommand; } }
		public ICommand ShowFertilityCommand { get { return _fertilityCommand; } }


		public object DC_FunctionPlotter
		{
			get { return this; }
		}

		public UserControl ViewPlotter
		{
			get { return _viewPlotter; }
			set
			{
				_viewPlotter = value;
				OnPropertyChanged("ViewPlotter");
			}
		}

		public Visibility[] SVisibility
		{
			get { return _plotterData.ParamVisibilities; }
		}
		
		public Visibility CheckBoxVisibility
		{
			get { return _plotterData.CheckBoxVisibility; }
		}

		public double S0Value
		{
			get { return _plotterData.ParamValues[0]; }
			set
			{
				_plotterData.ParamValues[0] = value;
				OnPropertyChanged("S0Value");
				RefreshView();
			}
		}

		public double S1Value
		{
			get { return _plotterData.ParamValues[1]; }
			set
			{
				_plotterData.ParamValues[1] = value;
				OnPropertyChanged("S1Value");
				RefreshView();
			}
		}


		public double S2Value
		{
			get { return _plotterData.ParamValues[2]; }
			set
			{
				_plotterData.ParamValues[2] = value;
				OnPropertyChanged("S2Value");
				RefreshView();
			}
		}

		public double S3Value
		{
			get { return _plotterData.ParamValues[3]; }
			set
			{
				_plotterData.ParamValues[3] = value;
				OnPropertyChanged("S3Value");
				RefreshView();
			}
		}

		public double S4Value
		{
			get { return _plotterData.ParamValues[4]; }
			set
			{
				_plotterData.ParamValues[4] = value;
				OnPropertyChanged("S4Value");
				RefreshView();
			}
		}
		public bool ShowKumSum
		{
			get { return _plotterData.ShowCumSum; }
			set
			{
				_plotterData.ShowCumSum = value;
				OnPropertyChanged("ShowCumSum");
				RefreshView();
			}
		}

		public string[] SHeader
		{
			get { return _plotterData.ParamHeaders; }
		}

		public double[] SMinimum
		{
			get { return _plotterData.ParamMini; }
		}


		public double[] SMaximum
		{
			get { return _plotterData.ParamMaxi; }
		}

		public double[] STick
		{
			get { return _plotterData.ParamTic; }
		}

		public string ParamTitle
		{
			get { return"Parameter für "+ _plotterData.Title; }
		}



		#endregion


		#region  Create Grafic-Data

		void RefreshView()
		{
			_plotterData.CalcNew();
			ViewPlotter = PresentationCreator.Create(PresentationType.FunctionPlot, _plotterData, false);
			OnPropertyChanged("SVisibility");
			OnPropertyChanged("SMinimum");
			OnPropertyChanged("SMaximum");
			OnPropertyChanged("STick");
			OnPropertyChanged("SHeader");

			OnPropertyChanged("CheckBoxVisibility");
			OnPropertyChanged("ShowKumSum"); 
			OnPropertyChanged("ParamTitle");

			OnPropertyChanged("S0Value");
			OnPropertyChanged("S1Value");
			OnPropertyChanged("S2Value");
			OnPropertyChanged("S3Value");
			OnPropertyChanged("S4Value");
		}

		#region Auswahl Funktionen

		private void ShowTransition()
		{
			_plotterData.FuncType = FunctionType.Transition;
			RefreshView();
		}

		private void ShowAestiv()
		{
			_plotterData.FuncType = FunctionType.Aestiv;
			RefreshView();
		}

		private void ShowDevRates()
		{
			_plotterData.FuncType = FunctionType.DevRate;
			RefreshView();
		}

		private void ShowFertility()
		{
			_plotterData.FuncType = FunctionType.Fertility;
			RefreshView();
		}


		#endregion


		#endregion

		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.FunctionPlot, _plotterData, true);
			printPres.PrintView();
		}

	}
}

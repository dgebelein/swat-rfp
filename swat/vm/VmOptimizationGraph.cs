using swat.Optimizer;
using SwatPresentations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using TTP.UiUtils;

namespace swat.vm
{
	class VmOptimizationGraph:VmBase
	{

		#region Variable

		VmBase _parentPanel;

		PresentationsData _presentationData;

		RelayCommand _printCommand;

		#endregion

		#region Construction

		public VmOptimizationGraph(VmSwat vmSwat, VmBase parentPanel) : base(vmSwat)
		{
			_parentPanel = parentPanel;
			_printCommand = new RelayCommand(param => this.Print());
		}

		#endregion

		#region EventHandling

		public void BestParameterChangedHandler(object sender, OptEventArgs e)
		{
			if(e.PresData != null)
			{ 
				_presentationData = e.PresData;
				ViewVisual = PresentationCreator.Create(PresentationType.Optimization, _presentationData, false);
			}

		}
		#endregion

		#region Properties für Binding

		public ICommand PrintCommand { get { return _printCommand; } }
		public Visibility VisQuantCommands { get { return Visibility.Collapsed; } } // entfernt die quantifizierungsmethoden aus contextmenü

		#endregion

		#region Methoden  aus Contextmenü


		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.Optimization, _presentationData, true);
			printPres.PrintView();
		}

		#endregion


	}
}

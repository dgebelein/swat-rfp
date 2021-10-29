using swat.views.dlg;
using SwatPresentations;
using swat.views.panels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.vm
{
	class VmPanelOptimization:VmBase
	{
		#region Variable

		VmOptimizationControl _vmControl;
		VmOptimizationGraph _vmGraph;

		#endregion

		#region construction + overridables

		public VmPanelOptimization(VmSwat vmSwat) : base(vmSwat)
		{
			_vmControl = new VmOptimizationControl(vmSwat);
			_vmGraph = new VmOptimizationGraph(vmSwat, this);
			_vmControl.BestParametersChanged += _vmGraph.BestParameterChangedHandler;

			ViewVisual = new ViewPanelOptimization();
		}

		public override bool RespondToViewChange()
		{
			if (_vmControl.OptimizationRunning)
			{
				DlgMessage.Show("Modell-Optimierung läuft", "Umschalten  erst nach Ende oder Abbruch der Optimierung möglich!", MessageLevel.Info);
				return false;
			}
			else
				return true;
		}

		#endregion


		#region properties

		public object DC_Control
		{
			get { return _vmControl; }
		}

		public object DC_Graph
		{
			get { return _vmGraph; }
		}

		#endregion

	}
}

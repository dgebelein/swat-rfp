using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.vm
{
	class VmOptimizationParameters:VmBase
	{
		public VmOptimizationParameters(VmSwat vmSwat) : base(vmSwat)
		{
			//_vmControl = new VmOptimizationControl(vmSwat);
			//_vmGraph = new VmOptimizationGraph(vmSwat, this);
			////vmGrid.MonitoringDataChanged += vmGraph.MonitoringDataChangedHandler;

			//ViewVisual = new ViewPanelOptimization();
		}
	}
}

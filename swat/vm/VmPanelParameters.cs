using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swat.views.panels;
using SwatPresentations;
//using swat.views.dlg;

namespace swat.vm
{
	class VmPanelParameters:VmBase
	{

		#region Variable

		VmParameterGrid _vmGrid;
		//VmParamFunc _vmGraph;

		#endregion

		#region construction

		public VmPanelParameters(VmSwat vmSwat) : base(vmSwat)
		{
			_vmGrid = new VmParameterGrid(vmSwat,this);
			//_vmGraph = new VmParamFunc(vmSwat, this);
			//vmGrid.MonitoringDataChanged += vmGraph.MonitoringDataChangedHandler;

			ViewVisual = new ViewPanelParameters();
		}


		public override bool RespondToViewChange()
		{
			if (_vmGrid.CanUpdateParameters)
			{
				bool? response = DlgRememberSave.Show(Workspace.Name, "veränderte Parameter sind noch nicht gesichert!", "Jetzt sichern?", MessageLevel.Warning);
				if (response == null)
					return false;

				if (response == true)
					_vmGrid.UpdateParameters();
			}
			return true;
		}
	


		#endregion

		#region properties

		public object DC_Parameter
		{
			get { return _vmGrid; }
		}

		//public object DC_FuncView
		//{
		//	get { return _vmGraph; }
		//}

		#endregion
	}
}

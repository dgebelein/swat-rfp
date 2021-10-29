using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swat.views.panels;
using SwatPresentations;
//using swat.views.dlg;

namespace swat.vm
{
	class VmPanelMonitoring : VmBase
	{

		#region Variable

		VmMonitoringGrid vmGrid;
		VmMonitoringGraph vmGraph;

		#endregion

		#region construction

		public VmPanelMonitoring(VmSwat vmSwat) : base(vmSwat)
		{
			vmGrid = new VmMonitoringGrid(vmSwat);
			vmGraph = new VmMonitoringGraph(vmSwat, null);
			vmGrid.MonitoringDataChanged += vmGraph.MonitoringDataChangedHandler;

			ViewVisual = new ViewPanelWeather();
		}

		public override bool RespondToViewChange()
		{
			if (Workspace.CurrentMonitoringData.MustSave)
			{
				bool? response = DlgRememberSave.Show(Workspace.Name, "veränderte Monitoringdaten sind noch nicht gesichert!", "Jetzt sichern?", MessageLevel.Warning);
				if (response == null)
					return false;

				if (response == true)
					Workspace.CurrentMonitoringData.WriteToFile();
				else
					Workspace.CurrentMonitoringData.ReadFromFile(); // alten Zustand restaurieren
			}
			return true;
		}

		#endregion

		#region properties

		public object DC_WeatherGrid
		{
			get { return vmGrid; }
		}

		public object DC_WeatherGraph
		{
			get { return vmGraph; }
		}

		#endregion

	}
}

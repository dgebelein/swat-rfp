using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using swat.views.panels;
using SwatPresentations;
//using swat.views.dlg;

namespace swat.vm
{
	class VmPanelWeather:VmBase
	{

		#region Variable

		VmWeatherGrid vmGrid;
		VmWeatherGraph vmGraph;

		#endregion

		#region construction

		public VmPanelWeather(VmSwat vmSwat) : base(vmSwat)
		{
			vmGrid = new VmWeatherGrid(vmSwat);
			vmGraph = new VmWeatherGraph(vmSwat,null, false);
			vmGrid.WeatherDataChanged += vmGraph.WeatherDataChangedHandler;

			ViewVisual = new ViewPanelWeather();
		}

		public override bool RespondToViewChange()
		{
			if (Workspace.WeatherData.MustSave)
			{
				bool? response = DlgRememberSave.Show(Workspace.Name, "Wetterdaten sind noch nicht gesichert!", "Jetzt sichern?", MessageLevel.Warning);
				if (response == null)
					return false;

				if (response == true)
				{ 
					Workspace.WeatherData.WriteToFile();
					Workspace.HasValidPopulationData = false;
				}
				else
					Workspace.WeatherData.ReadFromFile(); // alten Zustand restaurieren
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

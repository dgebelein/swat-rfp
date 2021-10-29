using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace swat.cmd
{

	public enum CmdResponse
	{
		Nothing,
		WorkspaceSelect,
		ShowPrognosis,
		ShowComparison,
		EditParameters,
		ShowWeatherPanel,
		ShowPopDynPanel,
		ShowMonitoringPanel,
		ShowOptimizationPanel,
		ShowFunctions
	};

	public class CmdItem
	{
		protected bool _isNode;
		public CmdResponse Response { get; set; } // Response bei Nodes  nur  als Identifier zum Aktivieren/Deaktivieren
		public string Name { get; set; }
		public string Text { get; set; }
		public Brush TextColor { get; set; }
		public int Heigth { get; set; }
		

		public ToolTip CmdTooltip
		{ 
			get { return BuildTooltip(); }
		}

		private ToolTip BuildTooltip()
		{
			return  new ToolTip();
		}
	}
}

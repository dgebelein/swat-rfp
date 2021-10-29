using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace swat.cmd
{
	class CmdMenu
	{
		public ObservableCollection<CmdItem> Items { get; set; }
		public CmdMenu()
		{
			Items = new ObservableCollection<CmdItem>
			{
				new CmdItem { Text = "Projekt", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.WorkspaceSelect },
				new CmdItem { Text = "Wetterdaten", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.ShowWeatherPanel },
				new CmdItem { Text = "Monitoring", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.ShowMonitoringPanel },
				new CmdItem { Text = "Populationdynamik", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.ShowPopDynPanel },
				new CmdItem { Text = "Vergleich/Prognose", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.ShowPrognosis },
				new CmdItem { Text = "Modellparameter ändern", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.EditParameters },
				new CmdItem { Text = "Modellparameter optimieren", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.ShowOptimizationPanel },
				new CmdItem { Text = "Funktionsplotter", TextColor = Brushes.White, Heigth = 25, Response = CmdResponse.ShowFunctions }
			};
		}
	}
}

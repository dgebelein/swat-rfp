using swat.vm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace swat
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public readonly VmSwat Vm;

		public MainWindow()
		{
			this.WindowState = System.Windows.WindowState.Maximized;
			InitializeComponent();
			Vm = new VmSwat();
			DataContext = Vm;
		}

		private void HelpClicked(object sender, MouseButtonEventArgs e)
		{
			//Vm.ShowHelp(HelpView); // für Hilfe wieder an
			//Vm.ShowHelp(HelpBrowser);


		}
	}
}

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

namespace SwopReview
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public readonly VmSR Vm;

		public MainWindow()
		{
			this.WindowState = System.Windows.WindowState.Maximized;
			InitializeComponent();
			SwopData sd = new SwopData();
			Vm = new VmSR(sd, CmdView);
			DataContext = Vm;
		}
	}
}

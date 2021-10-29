using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using swat.vm;

namespace swat.views.dlg
{
	/// <summary>
	/// Interaktionslogik für DlgUpdateDwdData.xaml
	/// </summary>
	public partial class DlgUpdateDwdData : Window
	{
		public DlgUpdateDwdData()
		{
			InitializeComponent();
		}

		//private DlgUpdateDwdData(VmWorkspace dc)
		//{
		//	DataContext = dc;
		//	InitializeComponent();
		//	dc.StartUpdateDwdWeather();
		//}

		private DlgUpdateDwdData(VmDlgUpdateDwdData dc)
		{
			DataContext = dc;
			InitializeComponent();
			dc.StartUpdateDwdWeather();
		}

		//public static void Show(VmWorkspace dc)
		//{
		//	DlgUpdateDwdData dlg = new DlgUpdateDwdData(dc);
		//	dlg.ShowDialog();
		//}

		public static void Show(VmDlgUpdateDwdData dc)
		{
			DlgUpdateDwdData dlg = new DlgUpdateDwdData(dc);
			dlg.ShowDialog();
		}

		private void CmdClose(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

using swat.vm;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace swat.views.dlg
{
	/// <summary>
	/// Interaktionslogik für DlgImportDwdData.xaml
	/// </summary>
	public partial class DlgImportDwdData : Window
	{
		private DlgImportDwdData(VmNewWorkspace dc)
		{
			DataContext = dc;
			dc.CreateDwdWeatherFile();

			InitializeComponent();
		}

		public static void Show(VmNewWorkspace dc)
		{
			DlgImportDwdData dlg = new DlgImportDwdData(dc);
			dlg.ShowDialog();
		}

		private void CmdClose(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}

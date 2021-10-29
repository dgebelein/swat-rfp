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
	/// Interaktionslogik für DlgImportParameters.xaml
	/// </summary>
	public partial class DlgImportParameters : Window
	{
	
		private bool _retVal;

		private DlgImportParameters()
		{
			InitializeComponent();
		}

		public static string Show(List<string> workspaceList, string model)
		{
			DlgImportParameters dlg = new DlgImportParameters();
			dlg._textModel.Text = model;
			dlg.WorkspaceBox.ItemsSource = workspaceList;
			if (workspaceList.Count > 0)
				dlg.WorkspaceBox.SelectedItem = workspaceList[0];
			
			dlg.ShowDialog();

			return (dlg._retVal == false) ? null : (string)dlg.WorkspaceBox.SelectedItem;
		}

		private void CmdLoad(object sender, RoutedEventArgs e)
		{
			_retVal = true;
			Close();
		}

		private void CmdEsc(object sender, RoutedEventArgs e)
		{
			_retVal = false;
			Close();
		}
	}
}

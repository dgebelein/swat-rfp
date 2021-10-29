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

namespace SwatPresentations
{
	/// <summary>
	/// Interaktionslogik für DlgLoadWorkspace.xaml
	/// </summary>
	public partial class DlgLoadWorkspace : Window
	{
		private bool _retVal;

		private DlgLoadWorkspace()
		{
			InitializeComponent();
		}

		public static string Show(List<string> workspaceList,string currentWorkspaceName)
		{
			DlgLoadWorkspace dlg = new DlgLoadWorkspace();
			dlg.WorkspaceBox.ItemsSource = workspaceList;
			if (workspaceList.Count > 0)
			{
				dlg.WorkspaceBox.SelectedItem = (string.IsNullOrEmpty(currentWorkspaceName)) ?
						workspaceList[0] : 
						currentWorkspaceName;
			}
			dlg.ShowDialog();

			return (dlg._retVal== false) ? null :(string) dlg.WorkspaceBox.SelectedItem;
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

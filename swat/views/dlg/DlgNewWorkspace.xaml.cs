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

namespace swat.views.dlg
{
	/// <summary>
	/// Interaktionslogik für DlgNewWorkspace.xaml
	/// </summary>
	public partial class DlgNewWorkspace : Window
	{
		bool _retVal;

		private DlgNewWorkspace(object dc)
		{
			DataContext = dc;
			InitializeComponent();

			DataContext = null;	// weil sonst RadioButton nicht gecheckt wird
			DataContext = dc;
		}

		public static bool Show(VmNewWorkspace dc)
		{
			DlgNewWorkspace dlg = new DlgNewWorkspace(dc);
			dlg.ShowDialog();

			return (dlg._retVal);
		}


		private void CanCreateNew(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute =  ((VmNewWorkspace)(DataContext)).CanCreateWorkspace;
		}

		private void CreateNew(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
			_retVal =((VmNewWorkspace)(DataContext)).CreateWorkspace();
		}


		private void CanCloseDlg(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CloseDlg(object sender, ExecutedRoutedEventArgs e)
		{
			_retVal = false;
			Close();
		}

	}
}

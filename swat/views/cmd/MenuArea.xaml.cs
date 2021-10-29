using swat.cmd;
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
using swat.vm;

namespace swat.views.cmd
{
	/// <summary>
	/// Interaktionslogik für MenuArea.xaml
	/// </summary>
	public partial class MenuArea : UserControl
	{
		public MenuArea()
		{
			InitializeComponent();
		}

		//verhindert horizontales Scrollen des gesamten Views beim Anklicken eines Eintrags
		private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
		{
			e.Handled = true;
		}

		private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			if (e.NewValue is CmdItem)
				((VmSwat)(DataContext)).SelectedMenuItem = (CmdItem)e.NewValue;

		}


	}
}

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

namespace swat.views.sheets
{
	/// <summary>
	/// Interaktionslogik für ViewHelp.xaml
	/// </summary>
	public partial class ViewHelp : UserControl
	{
		public ViewHelp()
		{
			InitializeComponent();
			//helpBrowser.Navigate("http://www.wpf-tutorial.com");
		}

		private void helpBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		{
			//txtUrl.Text = e.Uri.OriginalString;
		}
		//protected void WindowUnloaded(object sender, RoutedEventArgs e)
		//{
		//	//// wichtig: kann hier entfallen -> prüfen!
		//	//_aweBrowser.Close();
		//	//WebCore.Update();
		//}

		//private void txtUrl_KeyUp(object sender, KeyEventArgs e)
		//{
		//	if (e.Key == Key.Enter)
		//		helpBrowser.Navigate(txtUrl.Text);
		//}

		//private void wbSample_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
		//{
		//	txtUrl.Text = e.Uri.OriginalString;
		//}

		private void BrowseBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((helpBrowser != null) && (helpBrowser.CanGoBack));
		}

		private void BrowseBack_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			helpBrowser.GoBack();
		}

		private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((helpBrowser != null) && (helpBrowser.CanGoForward));
		}

		private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			helpBrowser.GoForward();
		}

		private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		//private void GoToPage_Executed(object sender, ExecutedRoutedEventArgs e)
		//{
		//	helpBrowser.Navigate(txtUrl.Text);
		//}
	}
}

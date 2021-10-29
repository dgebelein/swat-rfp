using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace swat.views.cmd
{
	/// <summary>
	/// Interaktionslogik für CmdView.xaml
	/// </summary>
	public partial class CmdView : UserControl
	{
		public CmdView()
		{
			InitializeComponent();
		}
		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			try
			{
				Hyperlink thisLink = (Hyperlink)sender;
				string navigateUri = thisLink.NavigateUri.ToString();
				Process.Start(new ProcessStartInfo(navigateUri));
				e.Handled = true;
			}
			catch (Exception)
			{
				

			}
		}
	}
}

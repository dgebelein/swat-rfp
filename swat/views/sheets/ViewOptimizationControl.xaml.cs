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

namespace swat.views.sheets
{
	/// <summary>
	/// Interaktionslogik für ViewOptimizationControl.xaml
	/// </summary>
	public partial class ViewOptimizationControl : UserControl
	{
		public ViewOptimizationControl()
		{
			InitializeComponent();
		}

		private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox tb = sender as TextBox;
			//tb.OnTextChanged(e);
			tb.CaretIndex = tb.Text.Length;
			tb.ScrollToEnd();
		}
	}
}

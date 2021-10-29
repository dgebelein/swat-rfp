using Swop.glob;
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

namespace Swop
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>

	public partial class MainWindow : Window
	{

		public readonly VmSwop Vm;

		public MainWindow()
		{
			this.WindowState = System.Windows.WindowState.Maximized;
			InitializeComponent();
			Vm = new VmSwop();
			DataContext = Vm;
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

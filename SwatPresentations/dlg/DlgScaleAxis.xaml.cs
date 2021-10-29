using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace SwatPresentations
{
	/// <summary>
	/// Interaktionslogik für DlgScaleAxis.xaml
	/// </summary>
	public partial class DlgScaleAxis : Window
	{
		private bool? _retVal;
		private string _title;
		private bool _isAuto;
		private double _minimum;
		private double _maximum;


		public DlgScaleAxis()
		{
			InitializeComponent();
		}

		private DlgScaleAxis(string title, bool isAuto, double minimum, double maximum)
		{
			InitializeComponent();
			_title = title;
			_minimum = minimum;
			_maximum = maximum;
			_isAuto = isAuto;
			DataContext = this;
		}

		private void This_Loaded(object sender, RoutedEventArgs e)
		{
			DlgHeader.Text = _title;
			AutoButton.IsChecked = _isAuto;
			FixedButton.IsChecked = !_isAuto;
		}

		//Rückgabewerte: 
		// beide NaN: Abbruch
		// ein Wert NaN: Auto
		// beide Werte gültig: neue feste Skalierung
		public static Tuple<ScaleDlgResult, double, double> Show(string title, bool isAuto, double minimum, double maximum)
		{
			DlgScaleAxis dlg = new DlgScaleAxis(title, isAuto, minimum, maximum);
			dlg.ShowDialog();
			if(dlg._retVal == true)
			{
				if(dlg.AutoButton.IsChecked == true)
					return new Tuple<ScaleDlgResult, double, double>(ScaleDlgResult.Auto, double.NaN, double.NaN);
				else
					return new Tuple<ScaleDlgResult, double, double>(ScaleDlgResult.Fixed, dlg._minimum, dlg._maximum);
			}
			return new Tuple<ScaleDlgResult,double, double>(ScaleDlgResult.Escape, double.NaN, double.NaN);
		}

		private void CanScale(object sender, CanExecuteRoutedEventArgs e)
		{
			if (AutoButton.IsChecked == true)
				e.CanExecute = true;
			else
				e.CanExecute = (!double.IsNaN(ScaleMin) && !double.IsNaN(ScaleMax) && (ScaleMin < ScaleMax));

		}

		private void CmdScale(object sender, ExecutedRoutedEventArgs e)
		{
			This._retVal = true;
			This.Close();
		}

		private void CanEsc(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void CmdEsc(object sender, RoutedEventArgs e)
		{
			This._retVal = false;
			This.Close();
		}

		public double ScaleMin
		{
			get { return _minimum; }
			set { _minimum = value; }
		}

		public double ScaleMax
		{
			get { return _maximum; }
			set { _maximum = value; }
		}
	}
}

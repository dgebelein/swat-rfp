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
using System.Windows.Shapes;

namespace SwopCompare
{
	/// <summary>
	/// Interaktionslogik für ViewLeft.xaml
	/// </summary>
	public partial class ViewLeft : UserControl
	{
		Action ShowResultAction;

		public ViewLeft()
		{
			InitializeComponent();
		}
		void OnShowResult(object sender, RoutedEventArgs e)
		{
				ShowResultAction();
		}

		public void CreateSimSetPanel(Action ShowResult)
		{
			_simSetSelector.Children.Clear();

			if (ShowResult != null)
			{
				this.ShowResultAction = ShowResult;
			}

			if (DataContext == null)
				return;
			VmCompare vm = ((VmCompare)(DataContext));
			if (!vm.Data.HasValidData)
				return;

			List<CmpSet> sets = vm.Data.CompareSets;
			
			int p = 1;
			foreach (CmpSet sw in sets)
			{
				_simSetSelector.Children.Add(CreateRadio($"S{p++}: {sw.Monitoring.Location}","", false, true));
			}
			((RadioButton)(_simSetSelector.Children[0])).IsChecked = true;
		}


		RadioButton CreateRadio(string txt, string group, bool isChecked, bool doShowResult = false)
		{
			RadioButton btn = new RadioButton
			{
				Content = txt,
				GroupName = group,
				IsChecked = isChecked,
				Margin = new Thickness(5),

			};
			if (doShowResult)
				btn.Click += OnShowResult;

			return btn;
		}

		public bool IsSimResultSetSelected(int setNo)
		{
			return (((RadioButton)(_simSetSelector.Children[setNo])).IsChecked == true);
		}
	}
}

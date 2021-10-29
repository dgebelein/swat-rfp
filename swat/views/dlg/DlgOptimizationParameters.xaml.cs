using swat.vm;
using System;
using System.Collections.Generic;
using System.Data;
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
using TTP.UiUtils;

namespace swat.views.dlg
{
	/// <summary>
	/// Interaktionslogik für DlgOptimizationParameters.xaml
	/// </summary>
	public partial class DlgOptimizationParameters : Window
	{

		bool _retVal;

		private DlgOptimizationParameters(object dc)
		{
			DataContext = dc;
			InitializeComponent();

			//DataContext = null;  // weil sonst RadioButton nicht gecheckt wird
			//DataContext = dc;
		}

		public static bool Show(VmOptimizationControl dc)
		{
			DlgOptimizationParameters dlg = new DlgOptimizationParameters(dc);
			dlg.ShowDialog();

			return (dlg._retVal);
		}


		private void CanTransmit(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ((VmOptimizationControl)(DataContext)).CanTransmitParameters;
		}

		private void Transmit(object sender, ExecutedRoutedEventArgs e)
		{
			_retVal = true;
			Close();
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


		#region edit Cell

		private void DataGrid_GotFocus(object sender, RoutedEventArgs e)
		{
			bool oneClick = Properties.Settings.Default.OneClickEdit;
			if (!oneClick)
				return;

			// Lookup for the source to be DataGridCell
			if (e.OriginalSource.GetType() == typeof(DataGridCell))
			{
				// Starts the Edit on the row;
				DataGrid grd = (DataGrid)sender;
				grd.BeginEdit(e);
			}
		}

		private void dgBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			if (e.Column.DisplayIndex == 2) //nur zulassen, wenn selektiert
			{
				var rowview = (DataRowView)e.Row.Item;
				bool b = ((bool)rowview.Row.ItemArray[0]);
				e.Cancel = (b != true);
			}
			else
				e.Cancel = false;

		}

		private void dgCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			if (e.EditAction == DataGridEditAction.Cancel)
				return;

			var rowview = (DataRowView)e.Row.Item;
			string paramKey = (String)rowview.Row.ItemArray[1];
			if (e.Column.DisplayIndex == 0)
			{
				bool? ch = ((CheckBox)e.EditingElement).IsChecked;
				if (!ch.HasValue)
					ch = false;

				((VmOptimizationControl)DataContext).SetSelected(paramKey,(bool) ch);
			}
			else
			{ 
				String txt = ((TextBox)e.EditingElement).Text;
				((TextBox)e.EditingElement).Text = ((VmOptimizationControl)DataContext).SetAndValidateEditText(paramKey, txt);

				int nextIndex = ((VmOptimizationControl)DataContext).GetIndex(paramKey); // wegen Neuaufbau Tabelle notwendig
				UIHelpers.SelectCellByIndex(_dgParameters, nextIndex, 2);
			}
		}

		#endregion
	}
}

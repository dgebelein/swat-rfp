using swat.vm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TTP.UiUtils;

namespace swat.views.sheets
{
	/// <summary>
	/// Interaktionslogik für ViewParameterGrid.xaml
	/// </summary>
	public partial class ViewParameterGrid : UserControl
	{
		public ViewParameterGrid()
		{
			InitializeComponent();
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

		private void dgCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			if (e.EditAction == DataGridEditAction.Cancel)
				return;

			var rowview = (DataRowView)e.Row.Item;
			string paramKey = (String)rowview.Row.ItemArray[0];
			String txt = ((TextBox)e.EditingElement).Text;
			((TextBox)e.EditingElement).Text = ((VmParameterGrid)DataContext).SetAndValidateEditText(paramKey, txt);
			
			int nextIndex = ((VmParameterGrid)DataContext).GetIndex(paramKey); // wegen Neuaufbau Tabelle notwendig
			UIHelpers.SelectCellByIndex(_dgParameters, nextIndex, 0);
		}








			#endregion

		}
	}


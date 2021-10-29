using swat.vm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
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

namespace swat.views.sheets
{
	/// <summary>
	/// Interaktionslogik für ViewWeatherGrid.xaml
	/// </summary>
	public partial class ViewWeatherGrid : UserControl
	{
		//bool _isCellEditing = false;
		//DataGridColumn _editCol;
		//int _editRow;

		public ViewWeatherGrid()
		{
			InitializeComponent();
		}


		private void DataGrid_columnHeaderClick(object sender, RoutedEventArgs e)
		{
			var columnHeader = sender as DataGridColumnHeader;
			if (columnHeader != null)
			{
				_dgWeather.SelectedCells.Clear();
				foreach (var item in _dgWeather.Items)
				{
					_dgWeather.SelectedCells.Add(new DataGridCellInfo(item, columnHeader.Column));
				}
			}
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

		//private void dgBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		//{
		//	//// Schreibschutz  und "kein Eintrag vor den Start"
		//	//int mesIndex = e.Column.DisplayIndex;
		//	//var rowview = (DataRowView)e.Row.Item;

		//	////TtpTime tm = new TtpTime((String)rowview.Row.ItemArray[0]);

		//	////e.Cancel = !((VmTable)DataContext).CanEditCell(tm, mesIndex);
		//	//e.Cancel = false;
		//	//_isCellEditing = !e.Cancel;
		//}

		private void dgCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			if (e.EditAction == DataGridEditAction.Cancel)
				return;

			int colIndex = e.Column.DisplayIndex;
			var rowview = (DataRowView)e.Row.Item;
			DateTime dt = DateTime.ParseExact((String)rowview.Row.ItemArray[0],"dd.MM.yyyy", CultureInfo.InvariantCulture);
			int rowIndex = dt.DayOfYear - 1;

			String txt = ((TextBox)e.EditingElement).Text;
			((TextBox)e.EditingElement).Text = ((VmWeatherGrid)DataContext).SetAndValidateEditText(rowIndex, colIndex, txt);

			//if (_dgWeather.CurrentCell.Column == null)//kommt vor, wenn z.B. der Editierzeitraum verändert wurde...
			//{
			//	_isCellEditing = false;
			//	return;
			//}
			// zwischenspeichern in lokalen Variablen, weil CurrentCell in CurrentCellChanged nicht verfügbar ist
			////	_editCol = _xGrid.CurrentCell.Column.DisplayIndex;
			//_editCol = _dgWeather.CurrentCell.Column;
			//_editRow = _dgWeather.ItemContainerGenerator.IndexFromContainer(e.Row);
		}


		// ist notwendig, weil während des Editierens einer Zelle kein Refresh der Items möglich ist
		//private void dgCurrentCellChanged(object sender, EventArgs e)
		//{
		//	//if ((!_isCellEditing) || ((DataRowView)_dgWeather.CurrentItem) == null)
		//	//	return;

		//	//_isCellEditing = false;
		//	//if (!((DataRowView)_dgWeather.CurrentItem).IsEdit)  // sonst exception bei wechsel des Zell-Focus durch doppelklick!
		//	//{
		//	//	_dgWeather.Items.Refresh();
		//	//	int nextRow = (_editRow < (_dgWeather.Items.Count - 1)) ? _editRow + 1 : _editRow;   // (auf canUserAddRows= false achten!)

		//	//	int editCol = _dgWeather.Columns.IndexOf(_editCol);
		//	//	if (editCol < 0)  // tritt auf, wenn zuletzt editierte Spalte gelöscht wurde
		//	//		editCol = 1;

		//	//	_dgWeather.CurrentCell = new DataGridCellInfo(_dgWeather.Items[nextRow], _dgWeather.Columns[editCol]);
		//	//}
		//	//_dgWeather.BeginEdit();

		//}
		#endregion





	}
}

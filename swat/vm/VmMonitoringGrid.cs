using swatSim;
using swat.iodata;
using swat.views.dlg;
using swat.views.sheets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TTP.UiUtils;
using SwatPresentations;

namespace swat.vm
{
	class VmMonitoringGrid : VmBase
	{
		#region variable
		RelayCommand _updateCommand;
		RelayCommand _copyDataCommand;
		RelayCommand _pasteDataCommand;
		RelayCommand _deleteDataCommand;
		RelayCommand _notesCommand;


		DataTable _dtMonitoring = null;
		public int LastRowWithContent { get; set; }

		MonitoringData _mData;
		char[] _delim = new char[] { ';', '\t' };

		public event EventHandler MonitoringDataChanged;

		#endregion

		#region construction + Init

		public VmMonitoringGrid(VmSwat vmSwat) : base(vmSwat)
		{
			_mData = Workspace.Monitoring[(int)Workspace.CurrentFlyType];

			ViewVisual = new ViewMonitoringGrid();

			_updateCommand = new RelayCommand(param => this.UpdateMonitoringFile(), param => this.CanUpdateMonitoringFile);
			_copyDataCommand = new RelayCommand(param => this.CopyData(), param => this.CanCopyData);
			_pasteDataCommand = new RelayCommand(param => this.PasteData(), param => this.CanPasteData);
			_deleteDataCommand = new RelayCommand(param => this.DeleteData(), param => this.CanDeleteData);
			_notesCommand = new RelayCommand(param => this.ShowNotes());

			InitTable();
		}

		private void InitTable()
		{
			if (Workspace == null)
				return;

			_dtMonitoring = new DataTable();
			_dtMonitoring.Columns.Add("Date", typeof(String));
			_dtMonitoring.Columns.Add("Adults", typeof(String));
			_dtMonitoring.Columns.Add("Eggs", typeof(String));

			int maxDays = ((Workspace.SimulationYear % 4) == 0) ? 366 : 365;
			LastRowWithContent = 0;

			DateTime dt = new DateTime(Workspace.SimulationYear, 1, 1);
			for (int n = 0; n < maxDays; n++)
			{
				DataRow row = _dtMonitoring.NewRow();
				row["Date"] = dt.ToString("dd.MM.yyyy");
				if (!double.IsNaN(_mData.Adults[n]))
				{ 
					row["Adults"] = (_mData.Adults[n] <0.0) ? "start": _mData.Adults[n].ToString("0.##", CultureInfo.CurrentCulture);
					LastRowWithContent = n;
				}

				if (!double.IsNaN(_mData.Eggs[n]))
				{ 
					row["Eggs"] = (_mData.Eggs[n] < 0.0) ? "start" : _mData.Eggs[n].ToString("0.##", CultureInfo.CurrentCulture);
					LastRowWithContent = n;
				}

				dt = dt.AddDays(1.0);
				_dtMonitoring.Rows.Add(row);
			}
		}

		public override void UpdateMenuContent()
		{
			InitTable();
			OnPropertyChanged("MonitoringTable");
		}

		#endregion

		#region Properties Commands

		public ICommand UpdateCommand { get { return _updateCommand; } }
		public ICommand CopyDataCommand { get { return _copyDataCommand; } }
		public ICommand PasteDataCommand { get { return _pasteDataCommand; } }
		public ICommand DeleteDataCommand { get { return _deleteDataCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }


		bool CanUpdateMonitoringFile
		{
			get { return _mData.MustSave; }
		}

		#endregion

		#region Binding Properties

		public override Visibility VisibilityState
		{
			get
			{
				return (Workspace.Name != null)? Visibility.Visible : Visibility.Collapsed;
			}
		}

		public DataTable MonitoringTable
		{
			get { return _dtMonitoring; }
			set
			{
				_dtMonitoring = value;
				OnPropertyChanged("MonitoringTable");
			}
		}

		public string Modell
		{
			get { return Workspace.CurrentModelName; }
		}

		#endregion

		#region Methods

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		private void UpdateMonitoringFile()
		{
			if (!_mData.WriteToFile())
			{
				DlgMessage.Show("Fehler beim Speichern der Monitoring-Daten", _mData.ErrorMsg, SwatPresentations.MessageLevel.Error);
			}

			_vmSwat.UpdateMenuContent();
		}


		public string SetAndValidateEditText(int rowIndex, int colIndex, string txt)
		{
			double val = _mData.SetValue(rowIndex, colIndex, txt);
			OnMonitoringDataChanged(EventArgs.Empty);

			if (double.IsNaN(val))
				return ("");
			else
				return (val < 0.0) ? "start" : val.ToString("0.##", CultureInfo.CurrentCulture);
		}

		#endregion

		#region ContextMenue

		private bool CanCopyData
		{
			get { return (((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells.Count > 0); }
		}

		private bool CanPasteData
		{
			get
			{
				if (!Clipboard.ContainsData(DataFormats.Text) || (((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells.Count != 1))
					return false;

				DataGridCellInfo ci = ((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells[0];
				int colId = ci.Column.DisplayIndex;
				return (colId > 0 && colId <= 2);
			}
		}

		private bool CanDeleteData
		{
			get { return (((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells.Count > 0); }
		}

		private String GetCellContent(DataGridCellInfo ci, int colNo)
		{
			DataRowView rv = (DataRowView)ci.Item;
			return ((DataRow)rv.Row).ItemArray[colNo].ToString();

		}

		private int GetRowId(DataGridCellInfo ci)
		{
			string datum = GetCellContent(ci, 0);
			DateTime dt;
			if (DateTime.TryParseExact(datum, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out  dt)||
				 DateTime.TryParseExact(datum, "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out  dt))
				return dt.DayOfYear - 1;
			else
				return -1;
		}

		private void CopyData()
		{
			IList<DataGridCellInfo> selectedCells = ((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells;

			DataGridCellInfo ci = selectedCells[0];
			int activeCol = ci.Column.DisplayIndex;
			StringBuilder sb = new StringBuilder(GetCellContent(ci, activeCol), 10000);
			for (int n = 1; n < selectedCells.Count; n++)
			{
				ci = selectedCells[n];
				int newCol = ci.Column.DisplayIndex;
				if (newCol <= activeCol)
				{
					sb.AppendLine();
					sb.Append(GetCellContent(ci, newCol));
				}
				else
				{
					sb.AppendFormat("\t{0}", GetCellContent(ci, newCol));
				}
				activeCol = newCol;
			}

			DataObject data = new DataObject();
			data.SetData(DataFormats.Text.ToString(), sb.ToString());
			Clipboard.SetDataObject(data, true);
		}



		private bool ClipboardDataHasDates()
		{
			String cb = Clipboard.GetText();

			using (StringReader sr = new StringReader(cb))
			{
				string line = sr.ReadLine();
				if (!string.IsNullOrEmpty(line))
				{ 
					string[] elems = line.Trim().Split(_delim);
					if ((elems.Length > 0) && 
						DateTime.TryParseExact(elems[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)||
						DateTime.TryParseExact(elems[0], "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
					{
						return true;
					}
				}
			}

			return false;
		}

		private List<double[]> ClipboardRawValues
		{
			get
			{
				String cb = Clipboard.GetText();

				List<double[]> rows = new List<double[]>();

				using (StringReader sr = new StringReader(cb))
				{
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						//cbLines.Add(line);
						double[] row = new double[] { double.NaN, double.NaN };
						string[] elems = line.Trim().Split(_delim);
						int c = 0;
						foreach (string e in elems)
						{
							if (DateTime.TryParseExact(e, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)||
								DateTime.TryParseExact(e, "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date1))
								continue;  // datum verwerfen
							if (c <= 1)
							{
								if (!Double.TryParse(e.Replace('.', ','), out row[c]))
									row[c] = double.NaN;
							}
							c++;
						}
						rows.Add(row);
					}
				}
				return rows;
			}
		}

		private double[] ClipboardValuesWithDates
		{
			get
			{
				double[] cbValues = new double[366];

				for (int i = 0; i < 366; i++)
				{
					cbValues[i] = double.PositiveInfinity;
				}

				String cb = Clipboard.GetText();

				using (StringReader sr = new StringReader(cb))
				{
					string line;
					while ((line = sr.ReadLine()) != null)
					{
						string[] elems = line.Trim().Split(_delim);
						if (elems.Length > 1)
						{
							int id = -1;
							DateTime date;
							if (DateTime.TryParseExact(elems[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out  date)||
								DateTime.TryParseExact(elems[0], "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out  date))
							{
								if (date.Year == Workspace.SimulationYear)
								{
									id = date.DayOfYear - 1;
								}
							}

							if (id >= 0)
							{
								if (!Double.TryParse(elems[1].Replace('.', ','), out cbValues[id]))
									cbValues[id] = double.NaN; // wegen evtl. doppelt vorkommenden Datumseinträgen
							}
						}
					}
				}

				return cbValues;
			}
		}

		private void PasteRawValues(int startRowId,int startColId)
		{
			List<double[]> cc = ClipboardRawValues;

			foreach (double[] ds in cc)
			{
				int actCol = startColId;
				foreach (double d in ds)
				{
					if (!double.IsNaN(d))
					{
						SetAndValidateEditText(startRowId, actCol, double.IsNaN(d) ? "" : d.ToString());
					}
					actCol++;
				}
				startRowId++;
			}
		}

		private void PasteValuesWithDates(int colId)
		{
			double[] cbData = ClipboardValuesWithDates;
			
			for ( int i=0; i< cbData.Length; i++)
			{
				if(!double.IsPositiveInfinity(cbData[i]))
					SetAndValidateEditText(i, colId, double.IsNaN(cbData[i]) ? "" : cbData[i].ToString());
			}
		}

		private void PasteData()
		{
			if (!Clipboard.ContainsData(DataFormats.Text))
				return;

			// wohin? Ausgangspunkt ermitteln
			DataGridCellInfo ci = ((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells[0];
			int rowId = GetRowId(ci);
			int colId = ci.Column.DisplayIndex;

			if (ClipboardDataHasDates())
			{
				PasteValuesWithDates(colId);
			}
			else
			{
				PasteRawValues(rowId, colId);
			}

			// Tabelle neu aufbauen
			InitTable();
			OnPropertyChanged("MonitoringTable");

		}

		private void DeleteData()
		{
			IList<DataGridCellInfo> selectedCells = ((ViewMonitoringGrid)_viewVisual)._dgMonitoring.SelectedCells;

			for (int n = 0; n < selectedCells.Count; n++)
			{
				DataGridCellInfo ci = selectedCells[n];
				int col = ci.Column.DisplayIndex;
				if (col <= 0)
					continue;
				int row = GetRowId(ci);
				if (row >= 0)
					SetAndValidateEditText(row, col, "");
			}
			InitTable();
			OnPropertyChanged("MonitoringTable");
		}

		#endregion
		
		#region Events

		protected virtual void OnMonitoringDataChanged(EventArgs e)
		{
			MonitoringDataChanged?.Invoke(this, e);
		}


		#endregion

	}
}

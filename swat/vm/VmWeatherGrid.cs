using swatSim;
using swat.iodata;
using swat.views.dlg;
using swat.views.sheets;
using SwatImporter;
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
	public class VmWeatherGrid:VmBase
	{
		RelayCommand _updateCommand;
		//RelayCommand _queryDwdDataCommand;
		RelayCommand _copyDataCommand;
		RelayCommand _pasteDataCommand;
		RelayCommand _deleteDataCommand;
		RelayCommand _notesCommand;
		RelayCommand _updateDwdWeatherCommand;



		DataTable _dtWeather = null;
		//DwdWeatherCreator _dwdWeatherCreator;

		char[] _delim = new char[] { ';', '\t' };

		public event EventHandler WeatherDataChanged;

		#region construction + Init

		public VmWeatherGrid(VmSwat vmSwat):base(vmSwat)
      {
			ViewVisual = new ViewWeatherGrid();

			_updateCommand = new RelayCommand(param => this.UpdateWeatherFile(), param => this.CanUpdateWeatherFile);
			//_queryDwdDataCommand = new RelayCommand(param => this.QueryDwdData(), param => this.CanQueryDwdData);
			_copyDataCommand = new RelayCommand(param => this.CopyData(), param => this.CanCopyData);
			_pasteDataCommand = new RelayCommand(param => this.PasteData(), param => this.CanPasteData);
			_deleteDataCommand = new RelayCommand(param => this.DeleteData(), param => this.CanDeleteData);
			_notesCommand = new RelayCommand(param => this.ShowNotes());
			_updateDwdWeatherCommand= new RelayCommand(param => this.UpdateDwdWeather());


			InitTable();
		}

		private void InitTable()
		{
			if (Workspace == null)
				return;

			_dtWeather = new DataTable();
			_dtWeather.Columns.Add("Date", typeof(String));
			_dtWeather.Columns.Add("Air", typeof(String));
			_dtWeather.Columns.Add("Soil", typeof(String));
			_dtWeather.Columns.Add("Hum", typeof(String));
			_dtWeather.Columns.Add("PrognAir", typeof(String));
			_dtWeather.Columns.Add("PrognSoil", typeof(String));
			_dtWeather.Columns.Add("PrognHum", typeof(String));
			_dtWeather.Columns.Add("RowColor", typeof(String));


			int maxDays = ((Workspace.SimulationYear % 4) == 0) ? 366 : 365;

			DateTime dt = new DateTime(Workspace.SimulationYear, 1, 1);
			for (int n=0;n< maxDays;n++)
			{
				DataRow row = _dtWeather.NewRow();
				row["Date"] = dt.ToString("dd.MM.yyyy");
				if (!double.IsNaN(Workspace.WeatherData.AirTemps[n]))
					row["Air"] = Workspace.WeatherData.AirTemps[n].ToString("0.0", CultureInfo.CurrentCulture);
				if (!double.IsNaN(Workspace.WeatherData.SoilTemps[n]))
					row["Soil"] = Workspace.WeatherData.SoilTemps[n].ToString("0.0", CultureInfo.CurrentCulture);
				if (!double.IsNaN(Workspace.WeatherData.Precs[n]))
					row["Hum"] = Workspace.WeatherData.Precs[n].ToString("0.0", CultureInfo.CurrentCulture);
				if (!double.IsNaN(Workspace.WeatherData.PrognAirTemps[n]))
					row["PrognAir"] = Workspace.WeatherData.PrognAirTemps[n].ToString("0.0", CultureInfo.CurrentCulture);
				if (!double.IsNaN(Workspace.WeatherData.PrognSoilTemps[n]))
					row["PrognSoil"] = Workspace.WeatherData.PrognSoilTemps[n].ToString("0.0", CultureInfo.CurrentCulture);
				if (!double.IsNaN(Workspace.WeatherData.PrognPrecs[n]))
					row["PrognHum"] = Workspace.WeatherData.PrognPrecs[n].ToString("0.0", CultureInfo.CurrentCulture);
				row["RowColor"] = GetRowColor(n);
				dt = dt.AddDays(1.0);
				_dtWeather.Rows.Add(row);

			}
		}

		private string GetRowColor( int day)
		{
			if(!Workspace.WeatherData.HasGaps)
				return "white";

			if(Workspace.WeatherData.Year == DateTime.Now.Year)
			{
				if((day > Workspace.WeatherData.FirstActualIndex) &&(day < Workspace.WeatherData.LastActualIndex))
				{
					if (double.IsNaN(Workspace.WeatherData.AirTemps[day]) || double.IsNaN(Workspace.WeatherData.SoilTemps[day]) ||
						double.IsNaN(Workspace.WeatherData.PrognAirTemps[day]) || double.IsNaN(Workspace.WeatherData.PrognSoilTemps[day]))
						return "HotPink";
				}

			}
			else // prognosewerte werden nicht verwendet
			{
				if (double.IsNaN(Workspace.WeatherData.AirTemps[day]) || double.IsNaN(Workspace.WeatherData.SoilTemps[day]))
					return "HotPink";
			}
			return "white";
		}

		public override void UpdateMenuContent()
		{
			InitTable();
			OnPropertyChanged("WeatherTable");
		}
		#endregion

		#region Properties

		public ICommand UpdateCommand { get { return _updateCommand; } }
		//public ICommand QueryDwdDataCommand { get { return _queryDwdDataCommand; } }
		public ICommand CopyDataCommand { get { return _copyDataCommand; } }
		public ICommand PasteDataCommand { get { return _pasteDataCommand; } }
		public ICommand DeleteDataCommand { get { return _deleteDataCommand; } }
		public ICommand NotesCommand { get { return _notesCommand; } }
		public ICommand UpdateDwdWeatherCommand { get { return _updateDwdWeatherCommand; } }



		public Visibility VisUpdateDwdWeatherButton
		{
			// aktuelles Jahr? letzte Daten älter als 2 Tage?
		
			get {
				if ((Workspace.SimulationYear == DateTime.Now.Year) &&(Workspace.WeatherData.LastActualIndex < (DateTime.Now.DayOfYear - 3)))
					return Visibility.Visible;
				else
					return Visibility.Hidden;
			}
		}

		public override Visibility VisibilityState
		{
			get
			{
				if (Workspace.HasValidWeatherData)
					return Visibility.Visible;
				else return Visibility.Collapsed;
			}
		}

		public DataTable WeatherTable
		{
			get { return _dtWeather; }
			set
			{
				_dtWeather = value;
				OnPropertyChanged("WeatherTable");
			}
		}

		public string Weatherfile
		{
			get { return Path.GetFileNameWithoutExtension(Workspace.WeatherFile); }
		}

		bool CanUpdateWeatherFile
		{
			get { return  Workspace.WeatherData.MustSave ; }
		}

		//bool CanQueryDwdData
		//{
		//	get { return true; }
		//}

		//public Visibility VisDwdUpdate
		//{
		//	get { return ((Workspace.WeatherData.Origin == WeatherSource.Dwd)&& Workspace.WeatherData.CanQueryDwd() )? Visibility.Visible : Visibility.Collapsed; }
		//}

		#endregion

		#region Methods

		private void UpdateDwdWeather()
		{
			VmDlgUpdateDwdData vmDlg = new VmDlgUpdateDwdData(_vmSwat);
			vmDlg.DoTheUpdate();
			Workspace.WeatherData.ReadFromFile();
			InitTable();

			OnPropertyChanged("WeatherTable");
			OnPropertyChanged("VisUpdateDwdWeatherButton");
			OnWeatherDataChanged(EventArgs.Empty); // für Grafik
		}



		private void UpdateWeatherFile()
		{
			if (!Workspace.WeatherData.WriteToFile())
			{
				DlgMessage.Show("Fehler beim Speichern der Wetter-Daten", Workspace.WeatherData.ErrorMsg, SwatPresentations.MessageLevel.Error);
			}
			else
			{
				Workspace.CurrentModel.InitModelParameters(Workspace.DefaultParameters); //neu 5.1.21
				Workspace.InvalidatePopulationData();
			}

			_vmSwat.UpdateMenuContent();
		}

		public string SetAndValidateEditText(int rowIndex,int colIndex, string txt)
		{
			string fmt = ((colIndex == 3) || (colIndex == 6)) ? "00" : "0.0";

			double val = Workspace.WeatherData.SetValue(rowIndex, colIndex, txt);
			OnWeatherDataChanged(EventArgs.Empty);
			if (double.IsNaN(val))
				return ("");
			else
				return (double.IsInfinity(val)) ? "???" : val.ToString(fmt, CultureInfo.CurrentCulture);
		}

		#endregion

		#region ContextMenue

		private void ShowNotes()
		{
			DlgNotes.Show(Workspace);
		}

		private bool CanCopyData
		{
			get { return (((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells.Count > 0); }
		}

		private bool CanPasteData
		{
			get
			{
				if (!Clipboard.ContainsData(DataFormats.Text) || (((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells.Count !=1))
						return false;

				DataGridCellInfo ci = ((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells[0];
				int colId = ci.Column.DisplayIndex;
				return (colId > 0 && colId <= 6);
			}
		}

		private bool CanDeleteData
		{
			get { return (((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells.Count > 0); }
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
			IList<DataGridCellInfo> selectedCells = ((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells;

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
						DateTime.TryParseExact(elems[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)||
						DateTime.TryParseExact(elems[0], "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date1))
					{
						return true;
					}
				}
			}

			return false;
		}

		//private List<double[]> GetClipboardContent()
		//{
		//	String cb = Clipboard.GetText();

		//	char[] delim = new char[] { ';', '\t' };

		//	List<string> cbLines = new List<string>();

		//	using (StringReader sr = new StringReader(cb))
		//	{
		//		string line;
		//		while ((line = sr.ReadLine()) != null)
		//		{
		//			cbLines.Add(line);
		//		}
		//	}

		//	List<double[]> rows = new List<double[]>();

		//	foreach (string line in cbLines)
		//	{
		//		double[] row = new double[] {double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity };
		//		string[] elems = line.Split(delim, 6, StringSplitOptions.None);
		//		int c = 0;
		//		foreach (string e in elems)
		//		{
		//			if (DateTime.TryParseExact(e, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
		//				continue;  // datum verwerfen
		//			if (c <= 4)
		//			{
		//				if (!Double.TryParse(e.Replace('.', ','), out row[c]))
		//                   row[c] = double.NaN;
		//			}
		//			c++;
		//		}
		//		rows.Add(row);
		//	}
		//	return rows;
		//}


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
						double[] row = new double[] { double.NaN, double.NaN, double.NaN, double.NaN, double.NaN};
						string[] elems = line.Trim().Split(_delim);
						int c = 0;
						foreach (string e in elems)
						{
							if (DateTime.TryParseExact(e, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)||
								DateTime.TryParseExact(e, "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date1))
								continue;  // datum verwerfen
							if (c <= 4)
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

		private double[,] ClipboardValuesWithDates
		{
			get
			{
				double[,] cbValues = new double[366,5];

				for (int d = 0; d < 366; d++)
				{
					for(int i=0; i<5; i++)
						cbValues[d,i] = double.PositiveInfinity;
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
								DateTime.TryParseExact(elems[0], "dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
							{
								if (date.Year == Workspace.SimulationYear)
								{
									id = date.DayOfYear - 1;
								}
							}

							if (id >= 0)
							{
								int c = -1;
	
								foreach (string e in elems)
								{
									c++;
									if ((c < 1) || (c > 5))
										continue;

									if (Double.TryParse(e.Replace('.', ','), out double val))
										cbValues[id, c-1] = val;
									else
										cbValues[id,c-1] = double.NaN; // wegen evtl. doppelt vorkommenden Datumseinträgen u. ungültigen zahlen
								
								}
							}
						}
					}
				}

				return cbValues;
			}
		}

		private void PasteRawValues(int startRowId, int startColId)
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
			double[,] cbData = ClipboardValuesWithDates;

			for (int d = 0; d < cbData.Length/5; d++)
			{
				for (int n=0; n<5; n++)
				if (!double.IsPositiveInfinity(cbData[d,n]))
					SetAndValidateEditText(d, colId+n, double.IsNaN(cbData[d,n]) ? "" : cbData[d,n].ToString());
			}
		}

		private void PasteData()
		{
			if (!Clipboard.ContainsData(DataFormats.Text))
				return;

			// wohin? Ausgangspunkt ermitteln
			DataGridCellInfo ci = ((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells[0];
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
			OnPropertyChanged("WeatherTable");

		}
		//private void PasteData()
		//{
		//	if (!Clipboard.ContainsData(DataFormats.Text))
		//		return;

		//	// wohin? Ausgangspunkt ermitteln
		//	DataGridCellInfo ci = ((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells[0];

		//	int startRowId = GetRowId(ci);
		//	int startColId = ci.Column.DisplayIndex;

		//	// Clipboard lesen und zerlegen
		//	List<double[]> cc = GetClipboardContent();

		//	// Werte einfügen
		//	foreach (double[]ds in cc)
		//	{
		//		int actCol = startColId;
		//          foreach (double d in ds)
		//		{
		//			if (!double.IsInfinity(d))
		//			{
		//			 SetAndValidateEditText(startRowId, actCol, double.IsNaN(d)? "" : d.ToString());
		//			}
		//			actCol++;
		//		}
		//		startRowId++;
		//       }

		//	// Tabelle neu aufbauen
		//	InitTable();
		//	OnPropertyChanged("WeatherTable");

		//}

		private void DeleteData()
		{
			IList<DataGridCellInfo> selectedCells = ((ViewWeatherGrid)_viewVisual)._dgWeather.SelectedCells;


			for (int n = 0; n < selectedCells.Count; n++)
			{
				DataGridCellInfo ci = selectedCells[n];
				int col = ci.Column.DisplayIndex;
				if (col <= 0)
					continue;
				int row = GetRowId(ci);
				if(row>=0)
					SetAndValidateEditText(row, col, "");
			}
			InitTable();
			OnPropertyChanged("WeatherTable");

      }

		#endregion

		#region Events
		protected virtual void OnWeatherDataChanged(EventArgs e)
		{
			EventHandler handler = WeatherDataChanged;
			if (handler != null)
			{
				handler(this, e);
			}
		}


		#endregion

	}
}

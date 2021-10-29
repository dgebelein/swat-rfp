using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	public class PresentationsData
	{
		#region Variable

		private TtpTimeRange _timeRange;
		protected List<PresentationRow> _dataRows;	
		
		protected TtpScaleInfo _leftAxisInfo;
		protected TtpScaleInfo _rightAxisInfo;
		protected TtpScaleInfo _xAxisInfo;

		public string Title { get; set; }
		public string YLegend { get; set; }
		public int ZoomFactor { get; set; }
		public int ZoomFactorRight { get; set; }
		public bool IsZoomed { get; set; }  // zoom mit Maus in Scatterplot
		public TtpTime DisplayTime { get; set; }
		public TtpTimeRange HighlightTimeRange { get; set; }

		public Double ZoomX0 { get; set; }
		//{
		//	get { return _zoomX0; }
		//	set { _zoomX0 = value; }
		//}

		public Double ZoomX1 { get; set; }
		//{
		//	get { return _zoomX1; }
		//	set { _zoomX1 = value; }
		//}

		public Double ZoomYL0 { get; set; }
		//{
		//	get { return _zoomYL0; }
		//	set { _zoomYL0 = value; }
		//}

		public Double ZoomYL1 { get; set; }
		//{
		//	get { return _zoomYL1; }
		//	set { _zoomYL1 = value; }
		//}

		//für Funktionsplotter zum Erzwingen von Grenzen
		public double XScaleMin { get; set; }
		public double XScaleMax { get; set; }
		public double YScaleMin { get; set; }
		public double YScaleMax { get; set; }

		#endregion

		#region Construction
		public PresentationsData()
		{
			_dataRows = new List<PresentationRow>();
			_leftAxisInfo = new TtpScaleInfo(TtpEnAxis.Left);
			//RightAxisInfo = new TtpScaleInfo(TtpEnAxis.Right);
			_xAxisInfo = new TtpScaleInfo(TtpEnAxis.X);

			ZoomFactor = 1;
		}

		#endregion


		#region Properties

		public TtpTimeRange TimeRange
		{
			get { return _timeRange; }
			set
			{
				_timeRange = value;
				_timeRange.Pattern = TtpEnPattern.Pattern1Day;
			}
		}



		public int NumRows
		{
			get { return (_dataRows == null) ? 0 : _dataRows.Count; }
		}



		public TtpScaleInfo LeftAxisInfo 
		{
			get { return _leftAxisInfo; }
			set
			{
				if (!_leftAxisInfo.Equals(value))
				{
					_leftAxisInfo = value;
				}
			}
		}

		public TtpScaleInfo XAxisInfo 
		{
			get { return _xAxisInfo; }
			set
			{
				if (!_xAxisInfo.Equals(value))
				{
					_xAxisInfo = value;
				}
			}
		}

		public bool HasRightAxis
		{
			get
			{
				foreach (PresentationRow row in _dataRows)
				{
					if (row.Axis == TtpEnAxis.Right)
						return true;
				}
				return false;
			}
		}

		#endregion


		#region methods

		public virtual void ClearData()
		{
			_dataRows.Clear();
		}

		public void AddRow(PresentationRow row)
		{
			_dataRows.Add(row);
		}



		public PresentationRow GetRow(int num)
		{
			return _dataRows[num];  // todo: index begrenzen
		}

		public void CalcAxes()
		{
			double maxVal = 0.0;
			int startIndex = TimeRange.Start.DayOfYear-1;
			
			foreach( PresentationRow row in _dataRows)
			{
				if (!row.IsVisible || row.Axis != TtpEnAxis.Left)
					continue;

				int num = Math.Min(TimeRange.GetNumPatterns(), row.Values.Length);
				for (int i = 0; i < num; i++)
					if (row.Values[startIndex+i] > maxVal)
						maxVal = row.Values[startIndex + i];
			}
			_leftAxisInfo.CalcAxisLimits(0, maxVal);
		}

		public void CalcXYAxes()
		{
			double minX = double.NaN;
			double maxX = double.NaN;
			double minY = double.NaN;
			double maxY = double.NaN;
			if (_dataRows.Count > 0)
			{
				minX = GetMinVal(_dataRows[0],  minX);
				maxX = GetMaxVal(_dataRows[0], maxX);
			}
			if (_dataRows.Count > 1)
			{
				for (int i = 1; i < _dataRows.Count; i++)
				{

					minY = GetMinVal(_dataRows[i], minY);
					maxY = GetMaxVal(_dataRows[i], maxY);
				}
			}

			_leftAxisInfo.CalcAxisLimits(minY, maxY);
			_xAxisInfo.CalcAxisLimits(minX, maxX);

		}

		private double GetMinVal(PresentationRow row, double initVal)
		{
			if (!row.IsVisible)
				return initVal;

			double minVal = initVal;

			foreach ( double v in row.Values)
			{
				if (double.IsNaN(minVal))
					minVal = v;
				else
				{ 
					if (v < minVal)
						minVal = v;
				}
			}
			return minVal;

		}

		private double GetMaxVal(PresentationRow row,double initVal)
		{
			if (!row.IsVisible)
				return initVal;

			double maxVal = initVal;


			foreach (double v  in row.Values)
			{
				if (double.IsNaN(maxVal))
					maxVal = v;
				else
				{
					if (v > maxVal)
						maxVal = v;
				}
			}
			return maxVal;

		}


		public void CalcAgeClassAxes()
		{
			double maxVal = 0.0;
			foreach (PresentationRow row in _dataRows)
			{
				double mx = row.Values.Max();
				if (mx > maxVal)
					maxVal = mx;
			}
			_leftAxisInfo.CalcAxisLimits(0, maxVal);
		}

		public void Draw(ChartGrid chart)
		{
			foreach (PresentationRow row in _dataRows)
			{
				if(row.IsVisible)
				{ 
					TrendLine tl = new TrendLine(TimeRange, row);
					tl.Draw(chart);
				}
			}
		}

		public void DrawScatter(ChartGrid chart)
		{
			if(_dataRows.Count > 1)
			{
				PresentationRow xRow = _dataRows[0];
				for (int r = 1; r< _dataRows.Count;r++)
				{
					PresentationRow yRow = _dataRows[r];
					TrendLine tl = new TrendLine(xRow, yRow);
					tl.Draw(chart);
				}
			}
		}

		public void DrawAgeClasses(ChartGrid chart)
		{
			foreach (PresentationRow row in _dataRows)
			{
				AgeClassBoxes boxes = new AgeClassBoxes(row);
				boxes.Draw(chart);
			}
		}

		public PresentationRow GetNearestRow(ChartGrid chart, System.Windows.Point pt)
		{
			TtpTime tmPoint = chart.GetPointTime(pt.X, _timeRange);
			int index = tmPoint.DayOfYear-1;

			Double minDistance = Double.NaN;
			PresentationRow bestRow = null;
			foreach(PresentationRow row in _dataRows)
			{
				if (!row.IsVisible)
					continue;

				double dist = GetYDistance(row, chart, index, pt.Y);
				if (!Double.IsNaN(dist))
				{
					if (Double.IsNaN(minDistance) || (dist <= minDistance))
					{
						bestRow = row; ;
						minDistance = dist;
					}
				}
			}
			return bestRow;
		}

		private Double GetYDistance( PresentationRow row,ChartGrid chart, int index, Double yPos)
		{
			double val = row.Values[index];

			if (Double.IsNaN(val)|| (val == 0.0))
				return Double.NaN;
			else
				return Math.Abs(yPos - chart.NormalizeY(val, row.Axis));

		}


		#endregion


		#region Legend

		public void CreateLegendPanel(StackPanel panel, TtpEnAxis axis,
												Double widthSymbolColumn, Double widthTextColumn,
												Brush textColor)
		{
			panel.Children.Clear();

			// auf jeden Fall ein Element mit der richtigen Breite zuweisen - sonst stimmt evtl. Layout nicht
			Border border = new Border
			{
				Background = Brushes.Transparent,
				Width = widthSymbolColumn + widthTextColumn,
				Height = 1
			};
			panel.Children.Add(border);

			List<string> legTexts = new List<string>();

			foreach (PresentationRow row in _dataRows)
			{
				if(!legTexts.Contains(row.Legend) &&(axis == row.Axis) && !string.IsNullOrEmpty(row.Legend))
				{
					legTexts.Add(row.Legend);
					panel.Children.Add(LegendElement.Create(row, textColor, widthSymbolColumn, widthTextColumn));
				}
			}
		}

		public void CreateXYLegendPanel(StackPanel panel, 
												  Double widthSymbolColumn, Double widthTextColumn,
												  Brush textColor)
		{
			panel.Children.Clear();

			// auf jeden Fall ein Element mit der richtigen Breite zuweisen - sonst stimmt evtl. Layout nicht
			Border border = new Border
			{
				Background = Brushes.Transparent,
				Width = widthSymbolColumn + widthTextColumn,
				Height = 1
			};
			panel.Children.Add(border);

			List<string> legTexts = new List<string>();

			if(_dataRows.Count > 1)
			{
				for(int i=1;i < _dataRows.Count;i++)
				{
					PresentationRow row = _dataRows[i];
					if (!legTexts.Contains(row.Legend) && !string.IsNullOrEmpty(row.Legend))
					{
						legTexts.Add(row.Legend);
						panel.Children.Add(LegendElement.Create(row, textColor, widthSymbolColumn, widthTextColumn));
					}
				}

			}
		}

		public virtual void CreateXLegend(Grid grid, Brush textColor)
		{
			grid.Children.Clear();

			PresentationRow row = _dataRows[0];

			TextBlock tb = new TextBlock
			{
				Text = row.Legend,
				Foreground = textColor,
				TextAlignment = TextAlignment.Center,
				Margin = new Thickness(0, 35, 0, 0)
			};

			grid.Children.Add(tb);
			
		}

		public void CreateYLegend(Grid grid, Brush textColor) // für Scatterplot
		{
			grid.Children.Clear();

			TextBlock tb = new TextBlock
			{
				Text = YLegend,
				Foreground = textColor,
				TextAlignment = TextAlignment.Center,
				Margin = new Thickness(10, 0, 0, 0)
			};

			tb.LayoutTransform = new RotateTransform(270);
			grid.Children.Add(tb);

		}

		public virtual void ToggleVisibilty(int index)
		{
			string txt = GetRow(index).Legend;
			foreach (PresentationRow row in _dataRows)
			{
				if(txt == row.Legend)
					row.IsVisible = !row.IsVisible;
			}
		}


		#endregion

	}
}

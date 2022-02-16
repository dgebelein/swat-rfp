using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TTP.Engine3;
using TTP.TtpCommand3;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Globalization;

namespace SwatPresentations
{
	class TrendLine
	{

		#region variable

		public TtpTimeRange TimeRange;
		public PresentationRow Row;
		private Point[] _points;

		#endregion

		#region Constructor

		public TrendLine(TtpTimeRange timeRange, PresentationRow row)
		{
			Row = row;
			TimeRange = timeRange;

			TimeRange.Pattern = TtpEnPattern.Pattern1Day;
			int num = Math.Min( TimeRange.GetNumPatterns(),row.Values.Length);
			int  day = TimeRange.Start.DayOfYear - 1;

			_points = new Point[num];	

			if((Row.LineType == TtpEnLineType.LinePoint)|| (Row.LineType == TtpEnLineType.AreaDiff))
			{
				bool isFirst = false;
				for (int p = 0; p < num; p++)
				{
					if (row.Values[day] < 0)
					{
						_points[p] = new Point(p, double.NaN);
						isFirst = true;
					}
					else
					{
						if (double.IsNaN(row.Values[day]))
						{
							if(isFirst)
								_points[p] = new Point(p, double.NaN);
							else
								_points[p] = new Point(double.NaN, double.NaN);

						}
						else
						{ 
							_points[p] = new Point(p, row.Values[day]);
							isFirst = false;
						}
					}

					day++;
				}
			}
			else
			{
				for (int p=0; p < num; p++)
				{
					_points[p] = new Point(p, row.Values[day++]);
				}
			}
		}

		public TrendLine(PresentationRow xRow, PresentationRow yRow) // für Xy-Grafik
		{
			Row = yRow;

			int num = yRow.Values.Length;

			_points = new Point[num];

			for (int p = 0; p < num; p++)
			{
				_points[p] = new Point(xRow.Values[p], yRow.Values[p]);
			}
			
		}

		#endregion


		#region Draw

		public  void Draw(ChartGrid chart)
		{
			if (!Row.IsVisible)
				return;

			Path trendPath= null;
			switch (Row.LineType)
			{
				case TtpEnLineType.Line:
					trendPath = CreateAnalogLinePath(chart,Row.Thicknes); break;
				case TtpEnLineType.AreaDiff:
					trendPath = CreateAnalogAreaPath(chart, 0.0); break;
				case TtpEnLineType.LinePoint:
					Path symbolPath = CreateAnalogSymbolPath(chart);
					chart.Canvas.Children.Add(symbolPath);
					trendPath = CreateAnalogLinePath(chart, Row.Thicknes); break;
				case TtpEnLineType.Point:
					Path xyPath = CreateAnalogSymbolPath(chart);
					chart.Canvas.Children.Add(xyPath);
					break;
				case TtpEnLineType.Chart:
					trendPath = CreateAnalogBoxesPath(chart, 0.0); break;
				case TtpEnLineType.Limit:
					trendPath = CreateMarkerPath(chart); break;

				default: break;
			}

			if (trendPath != null)
				chart.Canvas.Children.Add(trendPath);
		}

		private Path CreateAnalogLinePath(ChartGrid chart, double thickness)
		{
			StreamGeometry sg = new StreamGeometry();
			using (StreamGeometryContext sgc = sg.Open())
			{
				bool startNewFigure = true;
				for (int n = 0; n < _points.Length; n++)
				{
					if (Double.IsNaN(_points[n].X))
						continue;

					if (Double.IsNaN(_points[n].Y))
					{
						startNewFigure = true;
						continue;
					}

					Point pt = new Point(chart.NormalizeX(_points[n].X), chart.NormalizeY(_points[n].Y, Row.Axis));

					if (startNewFigure)
						sgc.BeginFigure(pt, true, false);
					else
						sgc.LineTo(pt, true, false);

					startNewFigure = false;
				}
			}

			sg.Freeze();

			Path trendLine = new Path
			{
				Stroke = Row.Color,
				StrokeThickness = thickness,
				Data = sg
			};

			return trendLine;

		}

		private Path CreateAnalogAreaPath(ChartGrid chart, Double limit)
		{
			Double yBase = Double.IsNaN(limit) ? chart.NormalizeY(0.0, Row.Axis) : chart.NormalizeY(limit, Row.Axis);

			Point pt = new Point(chart.ScaleXMin, yBase);
			bool isGap = true;

			StreamGeometry sg = new StreamGeometry();
			using (StreamGeometryContext sgc = sg.Open())
			{
				sgc.BeginFigure(pt, true, true);
				for (int n = 0; n < _points.Length; n++)
				{
					if (Double.IsNaN(_points[n].X))     // nicht im Raster
						continue;

					if (Double.IsNaN(_points[n].Y))
					{
						pt.Y = yBase;
						sgc.LineTo(pt, true, false);

						pt.X = chart.NormalizeX(_points[n].X);
						pt.Y = yBase;
						sgc.LineTo(pt, true, false);
						isGap = true;
					}
					else
					{
						if (isGap)
						{
							pt.X = chart.NormalizeX(_points[n].X);
							sgc.LineTo(pt, true, false);
						}
						pt.X = chart.NormalizeX(_points[n].X);
						pt.Y = chart.NormalizeY(_points[n].Y, Row.Axis);
						sgc.LineTo(pt, true, false);
						isGap = false;
					}
				}

				pt.Y = yBase;
				sgc.LineTo(pt, true, false);
			}

			sg.Freeze();

			SolidColorBrush sb = Row.Color as SolidColorBrush;
			Color fillColor = sb.Color;
			fillColor.A = 64;
			Brush fillBrush = new SolidColorBrush(fillColor);

			Path trendArea = new Path
			{
				Stroke = Row.Color,
				Fill = fillBrush,
				StrokeThickness = 0.5,
				Data = sg
			};

			return trendArea;
		}


		private Path CreateAnalogSymbolPath(ChartGrid chart)
		{
			GeometryGroup gg = new GeometryGroup
			{
				FillRule = FillRule.Nonzero
			};

			//Ellipse circle = new Ellipse();

			for (int n = 0; n < _points.Length; n++)
			{

				if (Double.IsNaN(_points[n].X) || Double.IsNaN(_points[n].Y))
					continue;

				if (_points[n].Y < 0.0)
					continue;

				EllipseGeometry ell = new EllipseGeometry(new Point(), 4, 4, new TranslateTransform(chart.NormalizeX(_points[n].X), chart.NormalizeY(_points[n].Y, Row.Axis)));
				gg.Children.Add(ell);
				
			}

			gg.Freeze();

			Path symbols = new Path();
			Color c = ((SolidColorBrush)Row.Color).Color;
			c.A = 255; // kann hier auch transparent gemacht werden
			SolidColorBrush symbolBrush = new SolidColorBrush(c);
			symbols.Fill = symbolBrush;
			symbols.Data = gg;

			return symbols;
		}

		private Path CreateAnalogBoxesPath(ChartGrid chart, Double yBase)
		{
			if (Double.IsNaN(yBase))
			{
				yBase = (Row.Axis == TtpEnAxis.Left) ? chart.ScaleYLMin : chart.ScaleYRMin;
			}

			Double boxWidth = chart.NormalizeX(1.0) - chart.NormalizeX(0.0);
			Double boxTop;

			GeometryGroup gg = new GeometryGroup();

			Rect rect = new Rect();
			rect.Width = boxWidth;

			for (int n = 0; n < _points.Length; n++)
			{

				if (Double.IsNaN(_points[n].X) || Double.IsNaN(_points[n].Y))
					continue;

				if (_points[n].Y >= yBase)
				{
					boxTop = chart.NormalizeY(_points[n].Y, Row.Axis);
					rect.Height = chart.NormalizeY(yBase, Row.Axis) - boxTop;
				}
				else
				{
					boxTop = chart.NormalizeY(yBase, Row.Axis);
					rect.Height = chart.NormalizeY(_points[n].Y, Row.Axis) - boxTop;
				}

				RectangleGeometry rec = new RectangleGeometry(rect, 0, 0, new TranslateTransform(chart.NormalizeX(_points[n].X - 0.5), boxTop));
				gg.Children.Add(rec);
			}

			gg.Freeze();

			Color fillColor = ((SolidColorBrush)Row.Color).Color; 
			fillColor.A = 64;
			Brush fillBrush = new SolidColorBrush(fillColor);

			Path trendBoxes = new Path
			{
				Stroke = Row.Color,
				Fill = fillBrush,
				StrokeThickness = 0.5,
				Data = gg
			};
			return trendBoxes;

		}

		private Path CreateMarkerPath(ChartGrid chart)
		{
			double h = chart.ScaleYLMax / 10.0;
			StreamGeometry sg = new StreamGeometry();
			using (StreamGeometryContext sgc = sg.Open())
			{
				for (int n = 0; n < _points.Length; n++)
				{
					if (Double.IsNaN(_points[n].Y))
						continue;

					Point startp = new Point(chart.NormalizeX(n), chart.NormalizeY(0, Row.Axis));
					Point endp = new Point(chart.NormalizeX(n), chart.NormalizeY(h, Row.Axis));

					sgc.BeginFigure(startp, false, false);
					sgc.LineTo(endp, true, false);
				}
			}

			sg.Freeze();

			Path trendLine = new Path
			{
				Stroke = Row.Color,
				StrokeThickness = 2.0,
				Data = sg
			};

			return trendLine;
		}

		#endregion


	}
}

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
	class AgeClassBoxes
	{
		#region variable

		public PresentationRow _row;
		private Point[] _points;

		#endregion

		#region Constructor

		public AgeClassBoxes(PresentationRow row)
		{
			_row = row;
			int num = _row.Values.Length;
			_points = new Point[num];
			double xPos = row.StartIndex;

			for (int p = 0; p < num; p++)
			{
				_points[p] = new Point(xPos + p, _row.Values[p]);
			}
		}

		#endregion

		#region Draw

		public void Draw(ChartGrid chart)
		{
			chart.Canvas.Children.Add(CreateAnalogBoxesPath(chart));
		}

	
		private Path CreateAnalogBoxesPath(ChartGrid chart)
		{
			Double boxWidth = chart.NormalizeX(1.0) - chart.NormalizeX(0.0);

			GeometryGroup gg = new GeometryGroup();

			Rect rect = new Rect
			{
				Width = boxWidth
			};

			for (int n = 0; n < _points.Length; n++)
			{
				double boxTop = chart.NormalizeY(_points[n].Y, _row.Axis);
				rect.Height = chart.NormalizeY(0.0, _row.Axis) - boxTop;

				RectangleGeometry rec = new RectangleGeometry(rect, 0, 0, new TranslateTransform(chart.NormalizeX(_points[n].X), boxTop));

				gg.Children.Add(rec);
			}

			gg.Freeze();

			Color fillColor = ((SolidColorBrush)_row.Color).Color;
			fillColor.A = 64;

			Path acBoxes = new Path
			{
				Stroke = _row.Color,
				StrokeThickness = 1.5,
				Fill= new SolidColorBrush(fillColor),
				Data = gg
			};
			return acBoxes;

		}
		#endregion
	}
}

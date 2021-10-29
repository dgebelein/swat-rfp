
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	class HighlightArea
	{
		private Canvas _canvas;
		private Brush _backgroundBrush;
		private TtpTimeRange _trHighlighted;

		public HighlightArea(Canvas canvas, Brush backgroundBrush, TtpTimeRange tr)
		{
			_canvas = canvas;
			_backgroundBrush = Brushes.Transparent;
			_trHighlighted = tr;
		}

		public void Draw(ChartGrid chart)
		{
			Rectangle rect = new Rectangle
			{
				Width =  chart.NormalizeX(_trHighlighted.GetNumPatterns()) - chart.NormalizeX(0.0),
				Height = chart.NormalizeY(0, TtpEnAxis.Left)- chart.NormalizeY(100, TtpEnAxis.Left),
				Fill = _backgroundBrush
			};


			Canvas.SetLeft(rect, chart.NormalizeX(_trHighlighted.Start.DayOfYear));
			Canvas.SetTop(rect, chart.NormalizeY(50, TtpEnAxis.Left));

			_canvas.Children.Add(rect);

		}
	}
}
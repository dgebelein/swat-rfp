using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
//using TTP.TtpCommand3;
using System.Globalization;

namespace SwatPresentations
{
	public class VerticalAxis
	{
		private Canvas _canvas;
		private Double _topMargin;
		private Double _bottomMargin;
		private Double _scaleMin = 0.0;
		private Double _scaleMax = 100.0;
		private Double _firstTick = 0.0;
		private Double _deltaTicks = 10.0;
		private bool _isLeftAxis = true;
		private Brush _textColor;
		private String _textValueFormat = "### ### ##0.###";

		public VerticalAxis(Canvas canvas, Double topMargin, Double bottomMargin, Brush textColor)
		{
			_canvas = canvas;
			_bottomMargin = bottomMargin;
			_topMargin = topMargin;
			_textColor = textColor;
		}

		public void SetParameters(bool isLeftAxis, Double scaleMin, Double scaleMax, Double firstTick, Double deltaTicks, bool hideLabels = false)
		{
			_isLeftAxis = isLeftAxis;

			if (scaleMin <= scaleMax)
			{
				_scaleMin = scaleMin;
				_scaleMax = scaleMax;
			}
			else
			{
				_scaleMin = scaleMax;
				_scaleMax = scaleMin;
			}
			if (_scaleMin == _scaleMax)
				_scaleMax += 1.0;

			_firstTick = firstTick;
			_deltaTicks = Math.Abs(deltaTicks);
		}

		public void Draw(bool showLabels = true)
		{
			DrawAxis(null, showLabels);
		}

		public void Draw(String[] labels)
		{
			DrawAxis(labels, true);
		}

		private void DrawAxis(String[] labels, bool showLabels)
		{
			if (Double.IsNaN(_scaleMin))
				return;

			Rectangle rect = new Rectangle
			{
				Width = _canvas.Width,
				Height = _canvas.Height - _topMargin - _bottomMargin,
				Stroke = Brushes.Transparent,
				Fill = Brushes.Transparent
			};
			Canvas.SetTop(rect, _topMargin);
			_canvas.Children.Add(rect);

			if (!showLabels)
				return;

			Double yTick = _firstTick;
			int labelIndex = 0;
			while (yTick <= _scaleMax)
			{
				Line tickLine = new Line
				{
					Stroke = _textColor,
					X1 = NormalizeX(0),
					Y1 = NormalizeY(yTick),
					X2 = NormalizeX(5),
					Y2 = NormalizeY(yTick)
				};
				//_canvas.Children.Add(tickLine);

				TextBlock tb = new TextBlock
				{
					Foreground = _textColor
				};
				if (labels == null)
				{
					tb.Text = yTick.ToString(_textValueFormat, CultureInfo.InvariantCulture);
				}
				else
				{
					tb.Text = (labelIndex < labels.Length) ? labels[labelIndex] : "";
					labelIndex++;
				}

				if (_isLeftAxis)
					Canvas.SetRight(tb, NormalizeX(_canvas.Width - 10));
				else
					Canvas.SetLeft(tb, NormalizeX(10));

				tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
				Canvas.SetTop(tb, NormalizeY(yTick) - tb.DesiredSize.Height / 2);

				_canvas.Children.Add(tb);
				yTick += _deltaTicks;
			}
		}

		private Double NormalizeX(Double x)
		{
			if (Double.IsNaN(_canvas.Width))
				_canvas.Width = 60;

			return (_isLeftAxis ? _canvas.Width - x : x);
		}

		private Double NormalizeY(Double y)
		{
			if (Double.IsNaN(_canvas.Height))
				_canvas.Height = 100;

			return ((_canvas.Height - _bottomMargin) - (y - _scaleMin) * (_canvas.Height - _topMargin - _bottomMargin) / (_scaleMax - _scaleMin));
		}

		private Point NormalizePoint(Point pt)
		{
			return new Point(NormalizeX(pt.X), NormalizeY(pt.Y));
		}

	}
}

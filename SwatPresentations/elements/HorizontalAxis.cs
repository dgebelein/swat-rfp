﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows;

namespace SwatPresentations
{
	public class HorizontalAxis
	{
		private Canvas _canvas;
		private Double _leftMargin;
		private Double _rightMargin;
		private Double _scaleMin = 0.0;
		private Double _scaleMax = 100.0;
		private Double _firstTick = 0.0;
		private Double _deltaTicks = 10.0;
		private Brush _textColor;
		private String _textValueFormat = "### ### ##0.0";

		public HorizontalAxis(Canvas canvas, Double leftMargin, Double rightMargin, Brush textColor)
		{
			_canvas = canvas;
			_leftMargin = leftMargin;
			_rightMargin = rightMargin;
			_textColor = textColor;
		}

		private string GetLabelFormat()
		{ 
			string fmt = _textValueFormat;
			double range = _scaleMax - _scaleMin;
			double nk = -Math.Log10(range);
			for (int i = 0; i < nk; i++)
				fmt += "#";
			return fmt;
		}

		public void SetParameters(Double scaleMin, Double scaleMax, Double firstTick, Double deltaTicks)
		{
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


		public void Draw(bool showTicks = false)
		{
			if (Double.IsNaN(_scaleMin))
				return;

			Rectangle rect = new Rectangle
			{
				Width = _canvas.Width - _leftMargin - _rightMargin,
				Height = _canvas.Height,
				Stroke = Brushes.Transparent,
				Fill = Brushes.Transparent
			};
			Canvas.SetLeft(rect, _leftMargin);
			_canvas.Children.Add(rect);

			string labelFmt = GetLabelFormat();

			Double xTick = _firstTick;
			while (xTick < (_scaleMax + _deltaTicks / 2.0)) // wg. Rundungsfehlern
			{
				if (showTicks)
				{
					Line tickLine = new Line
					{
						Stroke = _textColor,

						X1 = NormalizeX(xTick),
						Y1 = NormalizeY(0),
						X2 = NormalizeX(xTick),
						Y2 = NormalizeY(5)
					};
					_canvas.Children.Add(tickLine);
				}
				TextBlock tb = new TextBlock
				{
					Foreground = _textColor,
					Text = xTick.ToString(labelFmt, CultureInfo.InvariantCulture)
				};

				tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
				Canvas.SetTop(tb, NormalizeY(5 + tb.DesiredSize.Height / 2));
				Canvas.SetLeft(tb, NormalizeX(xTick) - tb.DesiredSize.Width / 2);

				_canvas.Children.Add(tb);
				xTick += _deltaTicks;
			}
		}

		private Double NormalizeX(Double x)
		{
			return _leftMargin + (x - _scaleMin) * (_canvas.Width - _leftMargin - _rightMargin) / (_scaleMax - _scaleMin);
		}


		private Double NormalizeY(Double y)
		{
			return y;
		}

		private Point NormalizePoint(Point pt)
		{
			return new Point(NormalizeX(pt.X), NormalizeY(pt.Y));
		}
	}  // class
}  // namespace

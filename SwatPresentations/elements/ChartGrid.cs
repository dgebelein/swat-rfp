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
	public enum GridCanvasType
	{
		TimeCanvas,
		XYCanvas,
		MeshCanvas
	};


	public class ChartGrid
	{
		private Canvas _canvas;
		private Brush _backgroundBrush;
		private Brush _foregroundBrush;
		private Brush _highlightBrush;

		private TtpTimeRange _timeRange;
		private TtpTimeRange _highlightedTimeRange;


		private Double _scaleXMin = 0.0;
		private Double _scaleXMax = 100.0;
		private Double _firstXTick = 0.0;
		private Double _deltaXTicks = 10.0;

		private Double _scaleYLMin = 0.0;
		private Double _scaleYLMax = 100.0;
		private Double _firstYLTick = 0.0;
		private Double _deltaYLTicks = 10.0;

		private Double _scaleYRMin = 0.0;
		private Double _scaleYRMax = 100.0;
		//private Double _firstYRTick = 0.0;
		//private Double _deltaYRTicks = 10.0;



		public ChartGrid(Canvas canvas, Brush backgroundBrush, Brush foregroundBrush)
		{
			_canvas = canvas;
			//_backgroundBrush = Brushes.Transparent;
			_backgroundBrush = backgroundBrush;
			_foregroundBrush = foregroundBrush;
			_highlightBrush = null;
		}

		public void SetHighlightArea( TtpTimeRange highlightedTimeRange,  Brush highlightBrush)
		{
			_highlightedTimeRange = highlightedTimeRange;
			_highlightBrush = highlightBrush;
		}

		public void SetScaling(TtpTimeRange timeRange,
									  Double scaleYLMin, Double scaleYLMax, 
									  Double scaleYRMin, Double scaleYRMax)
		{
			_timeRange = timeRange;
			SetXScaling();// scaleXMin, scaleXMax, firstXTick, deltaXTicks);
			SetYScaling(TtpEnAxis.Left, scaleYLMin, scaleYLMax);
			SetYScaling(TtpEnAxis.Right, scaleYRMin, scaleYRMax);

		}

		public void SetScaling(Double scaleXMin, Double scaleXMax, Double deltaXTicks,
							  Double scaleYLMin, Double scaleYLMax, 
							  Double scaleYRMin, Double scaleYRMax)
		{
			
			SetXScaling(scaleXMin, scaleXMax, deltaXTicks);
			SetYScaling(TtpEnAxis.Left, scaleYLMin, scaleYLMax);
			SetYScaling(TtpEnAxis.Right, scaleYRMin, scaleYRMax);

		}

		public void SetXScaling() //Double scaleXMin, Double scaleXMax, Double firstXTick, Double deltaXTicks)
		{
			_scaleXMin = 0;
			_scaleXMax = _timeRange.GetNumPatterns() - 1;
		}

		public void SetXScaling(Double scaleXMin, Double scaleXMax, Double deltaXTicks)
		{
			if (scaleXMin <= scaleXMax)
			{
				_scaleXMin = scaleXMin;
				_scaleXMax = scaleXMax;
			}
			else
			{
				_scaleXMin = scaleXMax;
				_scaleXMax = scaleXMin;
			}

			if (_scaleXMin == _scaleXMax)
				_scaleXMax += 1.0;

			
			_deltaXTicks = Math.Abs(deltaXTicks);
		}

		public void SetYScaling(TtpEnAxis axis, Double scaleYMin, Double scaleYMax)
		{
			if (axis == TtpEnAxis.Left)
			{
				if (scaleYMin <= scaleYMax)
				{
					_scaleYLMin = scaleYMin;
					_scaleYLMax = scaleYMax;
				}
				else
				{
					_scaleYLMin = scaleYMax;
					_scaleYLMax = scaleYMin;
				}
				if (_scaleYLMin == _scaleYLMax)
					_scaleYLMax += 1.0;
			}

			if (axis == TtpEnAxis.Right)
			{
				if (scaleYMin <= scaleYMax)
				{
					_scaleYRMin = scaleYMin;
					_scaleYRMax = scaleYMax;
				}
				else
				{
					_scaleYRMin = scaleYMax;
					_scaleYRMax = scaleYMin;
				}
				if (_scaleYRMin == _scaleYRMax)
					_scaleYRMax += 1.0;
			}

		}

		public void SetXScaling(double xMin, double xMax)
		{
			_scaleXMin = xMin;
			_scaleXMax = xMax;
		}

		public Double ScaleXMin
		{
			get { return _scaleXMin; }
		}

		public Double ScaleXMax
		{
			get { return _scaleXMax; }
		}

		public Double ScaleYLMin
		{
			get { return _scaleYLMin; }
		}

		public Double ScaleYLMax
		{
			get { return _scaleYLMax; }
		}

		public Double ScaleYRMin
		{
			get { return _scaleYRMin; }
		}

		public Double ScaleYRMax
		{
			get { return _scaleYRMax; }
		}
		public Canvas Canvas
		{
			get { return _canvas; }
		}

		private void DrawHighlightedArea()
		{
			if (_highlightBrush == null)
				return;

			TtpTimeRange tr = TtpTimeRange.And(_highlightedTimeRange, _timeRange);
			tr.Pattern = TtpEnPattern.Pattern1Day;
			double x0 = NormalizeX(_timeRange.GetIndex(_highlightedTimeRange.Start));
			double x1 = NormalizeX(_timeRange.GetIndex(_highlightedTimeRange.End));

			Rectangle rect = new Rectangle
			{
				Width = x1-x0,
				Height = _canvas.Height,
				Stroke = Brushes.Transparent,
				Fill = _highlightBrush
			};
			Canvas.SetLeft(rect, x0);
			_canvas.Children.Add(rect);
		}

		public void DrawCanvas(GridCanvasType canvasType = GridCanvasType.TimeCanvas)
		{
			Rectangle rect = new Rectangle
			{
				Width = _canvas.Width,
				Height = _canvas.Height,
				Stroke = _foregroundBrush,
				Fill = _backgroundBrush
			};
			_canvas.Children.Add(rect);

			DrawHighlightedArea();

			// horizontale Linien 
			Double yTick = _firstYLTick;
			while (yTick <= _scaleYLMax)
			{
				Line tickLine = new Line
				{
					Stroke = _foregroundBrush,
					StrokeDashArray = new DoubleCollection(new double[2] { 1, 2 }),
					X1 = 0,
					Y1 = NormalizeY(yTick, TtpEnAxis.Left),
					X2 = _canvas.Width,
					Y2 = NormalizeY(yTick, TtpEnAxis.Left)
				};
				_canvas.Children.Add(tickLine);

				yTick += _deltaYLTicks;
			}

			// vertikale Linien 
			switch (canvasType)
			{
				case GridCanvasType.TimeCanvas: AddVertTimeLines(); break;
				case GridCanvasType.XYCanvas:	AddVertXLines(); break;
				case GridCanvasType.MeshCanvas: break;
			}
		}

		public void DrawMesh()
		{
			Rectangle rect = new Rectangle
			{
				Width = _canvas.Width,
				Height = _canvas.Height,
				Stroke = _foregroundBrush,
				Fill = _backgroundBrush
			};
			_canvas.Children.Add(rect);


			// horizontale Linien 
			Double yTick = _firstYLTick;
			while (yTick <= _scaleYLMax)
			{
				Line tickLine = new Line
				{
					Stroke = _foregroundBrush, 
					StrokeThickness=2.0,
					X1 = 0,
					Y1 = NormalizeY(yTick, TtpEnAxis.Left),
					X2 = _canvas.Width,
					Y2 = NormalizeY(yTick, TtpEnAxis.Left)
				};
				_canvas.Children.Add(tickLine);

				yTick += _deltaYLTicks;
			}

			// vertikale Linien 
			Double xTick = _firstXTick;
			while (xTick <= _scaleXMax)
			{
				Line tickLine = new Line
				{
					Stroke = _foregroundBrush,
					Y1 = 0,
					X1 = NormalizeX(xTick),
					Y2 = _canvas.Height,
					X2 = NormalizeX(xTick)
				};
				_canvas.Children.Add(tickLine);

				xTick += _deltaXTicks;
			}

		}

		private void AddVertTimeLines()
		{
			TtpTimeRange labelRange = new TtpTimeRange(_timeRange.Start, _timeRange.End, TtpEnPattern.Pattern1Month);
			bool bSmall = (labelRange.GetNumPatterns() < 3);
			int deltaTicks = (bSmall) ? 10 : 30;

			int lastTick = _timeRange.GetNumPatterns() - 1;
			int xTickPos = 0;
			TtpTime labelTime = _timeRange.Start;
			while (xTickPos <= lastTick)
			{
				Line tickLine = new Line
				{
					Stroke = _foregroundBrush,
					StrokeDashArray = new DoubleCollection(new double[2] { 1, 2 }),
					X1 = NormalizeX(xTickPos),
					Y1 = 0,
					X2 = NormalizeX(xTickPos),
					Y2 = _canvas.Height
				};
				_canvas.Children.Add(tickLine);

				if (bSmall)
				{
					xTickPos += deltaTicks;
					labelTime.Inc(_timeRange.Pattern, deltaTicks);
				}
				else
				{
					TtpTime oldTime = labelTime;
					labelTime.Inc(TtpEnPattern.Pattern1Month, 1);
					xTickPos += labelTime.GetDifferenceTo(oldTime, TtpEnPattern.Pattern1Day);
				}
			}
		}

		private void AddVertXLines()
		{
			// vertikale Linien 
			Double xTick = _firstXTick;
			while (xTick < _scaleXMax)
			{
				Line tickLine = new Line
				{
					Stroke = _foregroundBrush,
					StrokeDashArray = new DoubleCollection(new double[2] { 1, 2 }),

					X1 = NormalizeX(xTick),
					Y1 = 0,
					X2 = NormalizeX(xTick),
					Y2 = _canvas.Height
				};
				_canvas.Children.Add(tickLine);

				xTick += _deltaXTicks;
			}
		}

		public Double NormalizeX(Double x)
		{
			if (Double.IsNaN(_canvas.Width))
				_canvas.Width = 60;

			return (x - _scaleXMin) * _canvas.Width / (_scaleXMax - _scaleXMin);
		}

		public Double NormalizeY(Double y, TtpEnAxis axis)
		{
			if (Double.IsNaN(_canvas.Height))
				_canvas.Height = 100;

			return (axis == TtpEnAxis.Left) ?
						_canvas.Height - (y - _scaleYLMin) * _canvas.Height / (_scaleYLMax - _scaleYLMin) :
						_canvas.Height - (y - _scaleYRMin) * _canvas.Height / (_scaleYRMax - _scaleYRMin);

		}

		public Double NormalizeXDist(Double x)
		{
			if (Double.IsNaN(_canvas.Width))
				_canvas.Width = 60;

			return x  * _canvas.Width / (_scaleXMax - _scaleXMin);
		}

		public Double NormalizeYDist(Double y, TtpEnAxis axis)
		{
			if (Double.IsNaN(_canvas.Height))
				_canvas.Height = 100;

			return (axis == TtpEnAxis.Left) ?
						(y * _canvas.Height / (_scaleYLMax - _scaleYLMin)) :
						(y * _canvas.Height / (_scaleYRMax - _scaleYRMin));
		}

		public Point NormalizePoint(Point pt, TtpEnAxis axis)
		{
			return new Point(NormalizeX(pt.X), NormalizeY(pt.Y, axis));
		}

		public TtpTime GetPointTime(Double xPos, TtpTimeRange trRange)
		{
			TtpTime tm = trRange.Start;
			tm.Inc(trRange.Pattern, GetPointIndexX(xPos));
			return tm;
		}

		public int GetPointIndexX(Double xPos)
		{
			Double x = ScaleXMin + xPos * (ScaleXMax - ScaleXMin) / Canvas.Width;
			if (x < 0)
				x = 0;

			return (int)Math.Round(x);
		}



		public Double GetYCoord(Double ptY, TtpEnAxis axis)
		{
			return (axis == TtpEnAxis.Left) ?
				_scaleYLMin + (_canvas.Height - ptY) * (_scaleYLMax - _scaleYLMin) / _canvas.Height :
				_scaleYRMin + (_canvas.Height - ptY) * (_scaleYRMax - _scaleYRMin) / _canvas.Height;
		}


		public Double GetXCoord(Double ptX)
		{
			return _scaleXMin + ptX * (_scaleXMax - _scaleXMin) / _canvas.Width;
		}


	}
}

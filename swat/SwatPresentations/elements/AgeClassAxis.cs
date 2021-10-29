using System;
using TTP.Engine3;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;

namespace SwatPresentations
{
	class AgeClassAxis
	{
		#region  Variable

		private Canvas _canvasAxis;
		private Double _leftMargin;
		private Double _rightMargin;
		private Brush _foregroundBrush;
		private Double _scaleMin;
		private Double _scaleMax;
		string[] _labels = new string[] { "Eier", "Larven", "Puppen", "Fliegen" };
		int[] _numAc = new int[] { 5, 10, 10, 10};


		#endregion


		#region Kunstruktion u. Basisparameter

		public AgeClassAxis(Canvas canvasChart, Canvas canvasAxis, Double leftMargin, Double rightMargin, Brush textColor)
		{
			_canvasAxis = canvasAxis;
			_leftMargin = leftMargin;
			_rightMargin = rightMargin;
			_foregroundBrush = textColor;
			_scaleMin = 0;
			_scaleMax = 35;
		}

		#endregion


		#region Scaling


		private Double NormalizeX(Double x)
		{
			return _leftMargin + (x - _scaleMin) * (_canvasAxis.Width - _leftMargin - _rightMargin) / (_scaleMax - _scaleMin);
		}


		private Double NormalizeY(Double y)
		{
			return y;
		}


		private Point NormalizePoint(Point pt)
		{
			return new Point(NormalizeX(pt.X), NormalizeY(pt.Y));
		}


		#endregion

		#region Draw

		public void Draw()
		{
			double xPos = 0;

			Rectangle rect = new Rectangle
			{
				Width = _canvasAxis.Width,
				Height = _canvasAxis.Height,
				Stroke = Brushes.Transparent,
				Fill = Brushes.Transparent
			};
			_canvasAxis.Children.Add(rect);

			for(int i=0; i<4; i++)
			{
				xPos += _numAc[i];
				TextBlock tb = new TextBlock
				{
					Foreground = _foregroundBrush,
					Text = _labels[i]
				};

				tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
				Canvas.SetTop(tb, NormalizeY(5 + tb.DesiredSize.Height / 2));
				Canvas.SetLeft(tb, NormalizeX(xPos - _numAc[i]/2) - tb.DesiredSize.Width / 2);

				_canvasAxis.Children.Add(tb);

			}
		}



			#endregion
	}
}

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
	public class MeshRects
	{
		#region variable

		List<PresentationRect> _rects;
		double _rWidth, _rHeigth;
		Rectangle[] _rectangles;
		SolidColorBrush[] _colorBrushes;

		#endregion

		#region Constructor

		public MeshRects(List<PresentationRect> rects)
		{
			_rects = rects;
			_rectangles = new Rectangle[_rects.Count];
			_colorBrushes = CreateBrushes();
		}

		public MeshRects(List<PresentationRect> rects, SolidColorBrush[] brushes)
		{
			_rects = rects;
			_rectangles = new Rectangle[_rects.Count];
			_colorBrushes = brushes;
		}

		#endregion

		#region statics
		public static SolidColorBrush[] CreateBrushes()
		{
			SolidColorBrush[] brushes= new SolidColorBrush[255];
			
			for (int i=0; i<255; i++)
			{
				brushes[i]= new SolidColorBrush(ColorTools.GetShortRainbow((double)i/255.0));
			}

			return brushes;
		}

		#endregion

		#region Draw

		public void Draw(ChartGrid chart, TtpScaleInfo colorScaleInfo, double zoomFaktor=1.0)
		{
			if (_rects.Count == 0)
				return;

			_rWidth = chart.NormalizeXDist(_rects[0].Width);
			_rHeigth = chart.NormalizeYDist(_rects[0].Height, TtpEnAxis.Left);

			int n=0;
			foreach (PresentationRect mr in _rects)
			{
				Brush fill=GetRectColor(mr, colorScaleInfo, zoomFaktor);
				_rectangles[n++] = CreateRectangle(chart, mr, fill);
			}
			AddRectanglesToChart(chart);
		}

		public void DrawColorZoomed(TtpScaleInfo colorScaleInfo, double zoomFaktor = 1.0)
		{
			if (_rects.Count == 0)
				return;

			int n = 0;
			foreach (PresentationRect mr in _rects)
			{
				if (!double.IsNaN(mr.ZValue))
				{
					Brush fill = GetRectColor(mr, colorScaleInfo, zoomFaktor);
					_rectangles[n].Fill = fill;
				}
				n++;
			}
		}

		private Brush GetRectColor(PresentationRect mr, TtpScaleInfo colorScaleInfo, double zoom)
		{
			if (double.IsNaN(mr.ZValue))
				return Brushes.Transparent;
			else
			{
				double v = (mr.ZValue - colorScaleInfo.ActualScaleMin) /( (colorScaleInfo.ActualScaleMax - colorScaleInfo.ActualScaleMin)*zoom) * 255 ;
				v = Math.Min(v, 254);
				return _colorBrushes[(int)v];
				//return new SolidColorBrush(ColorTools.GetShortRainbow(v));
			}
		}

		private Rectangle CreateRectangle (ChartGrid chart,PresentationRect rct, Brush fillColor )
		{
			Rectangle r = new Rectangle
			{
				Width = _rWidth,
				Height = _rHeigth,
				Fill = fillColor,
				Stroke = rct.FrameColor,
				StrokeThickness = 1.0
			};
			Canvas.SetLeft(r, chart.NormalizeX(rct.X));
			Canvas.SetTop(r, chart.NormalizeY(rct.Y , TtpEnAxis.Left)- _rHeigth);
			return r;
		}

		private void AddRectanglesToChart(ChartGrid chart)
		{
			foreach(Rectangle r in _rectangles)
				chart.Canvas.Children.Add(r);
		}
		#endregion
	}
}

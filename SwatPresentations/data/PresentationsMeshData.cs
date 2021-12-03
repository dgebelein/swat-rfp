using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	public class PresentationsMeshData : PresentationsData
	{
		#region Variable

		protected List<PresentationRect> _dataRects;
		protected SolidColorBrush[] _colorBrushes;
		protected TtpScaleInfo _zAxisInfo;
		protected ChartGrid _chartMesh;

		private MeshRects _meshRects;
		private TextBlock _maxXLabel;
		private Slider _zoomSlider;

		public bool HasPanelData { get; set; }
		public string XLegend { get; set; }
		public string ZLegend { get; set; }
		public double ColorZoom { get; set; }
		public double MinimumZ { get; set; }
		public double MaximumZ { get; set; }
		public SolidColorBrush[] ColorBrushes { get; set; }


		#endregion

		#region Contruction
		public PresentationsMeshData()
		{
			_dataRects = new List<PresentationRect>();
			ZAxisInfo = new TtpScaleInfo(TtpEnAxis.X);
		}

		#endregion

		#region Properties
		public int NumRects
		{
			get { return (_dataRects == null) ? 0 : _dataRects.Count;}
		}

		public TtpScaleInfo ZAxisInfo /*{ get; set; }*/
		{
			get { return _zAxisInfo; }
			set
			{
				if (!_zAxisInfo.Equals(value))
				{
					_zAxisInfo = value;
				}
			}
		}

		#endregion

		#region  Methods /Data

		public override void ClearData()
		{
			_dataRects.Clear();
		}

		public void AddRect(PresentationRect rct)
		{
			_dataRects.Add(rct);
		}

		public void CalcMeshAxes()
		{
			// kleinstes, größtes x,y,value
			double minX = double.NaN;
			double maxX = double.NaN;
			double minY = double.NaN;
			double maxY = double.NaN;

			foreach (PresentationRect rct in _dataRects)
			{
				if (double.IsNaN(minX))
				{
					minX = rct.X;
					maxX = rct.X;
					minY = rct.Y;
					maxY = rct.Y;
				}

				minX = (double.IsNaN(rct.X)) ? minX : Math.Min(minX, rct.X);
				maxX = (double.IsNaN(rct.X)) ? maxX : Math.Max(maxX, rct.X);
				minY = (double.IsNaN(rct.Y)) ? minY : Math.Min(minY, rct.Y);
				maxY = (double.IsNaN(rct.Y)) ? maxY : Math.Max(maxY, rct.Y);
			}

			if (!double.IsNaN(maxX))
			{
				maxX += _dataRects[0].Width;
				maxY += _dataRects[0].Height;
			}

			_leftAxisInfo.ActualScaleMin = (maxY > minY) ? minY :minY - 1.0;
			_leftAxisInfo.ActualScaleMax = (maxY > minY) ? maxY : minY + 1.0;
			_xAxisInfo.ActualScaleMin = (maxX > minX) ? minX : minX - 1.0; ;
			_xAxisInfo.ActualScaleMax = (maxX > minX) ? maxX : minX + 1.0;
			_zAxisInfo.ActualScaleMin = MinimumZ;
			_zAxisInfo.ActualScaleMax = (MaximumZ > MinimumZ) ? MaximumZ : MinimumZ + 1.0;

			ColorZoom = 1.0;
		}

		public List<string> GetRetrievalStrings(double x, double y)
		{
			List<string> rs = new List<string>();

			foreach (PresentationRect rct in _dataRects)
			{
				if ((x >= rct.X) && (x < rct.X+rct.Width) && (y >= rct.Y)&& (y <rct.Y+rct.Height))
				{
					rs.Add( $"X = {rct.X.ToString("0.###", CultureInfo.InvariantCulture)+" -  "+(rct.X + rct.Width).ToString("0.###", CultureInfo.InvariantCulture)}");
					rs.Add( $"Y = {rct.Y.ToString("0.###", CultureInfo.InvariantCulture) + " -  " + (rct.Y + rct.Height).ToString("0.###", CultureInfo.InvariantCulture)}");
					rs.Add( $"Error = {rct.ZValue.ToString("0.###", CultureInfo.InvariantCulture)}");
					break;
				}

			}
			return rs;
		}


		#endregion

		#region Methods/Draw

		public void DrawMeshRects(ChartGrid chart)
		{
			if (ColorBrushes == null)
				ColorBrushes = MeshRects.CreateBrushes();

			_chartMesh = chart;
			_meshRects = new MeshRects(_dataRects, ColorBrushes);
			_meshRects.Draw(chart, _zAxisInfo);
		}

		public void DrawColorZoomedMeshRects()
		{
			//_chartMesh.SetVisMode(Visibility.Collapsed);
			_meshRects.DrawColorZoomed(_zAxisInfo, ColorZoom);
			//_chartMesh.SetVisMode(Visibility.Visible);

		}

		public override void CreateXLegend(Grid grid, Brush textColor)
		{
			grid.Children.Clear();

			TextBlock tb = new TextBlock
			{
				Text = XLegend,
				Foreground = textColor,
				TextAlignment = TextAlignment.Center,
				Margin = new Thickness(0, 35, 0, 0)
			};

			grid.Children.Add(tb);
		}

		public void  DrawColorBarArea(Canvas canvas, Brush textColor)
		{
			ChartGrid colorChart = new ChartGrid(canvas, Brushes.Transparent, Brushes.Transparent);
			colorChart.SetScaling(0, 3, 1, 0, 3, 0, 100);

			double heigth = colorChart.NormalizeYDist(1, TtpEnAxis.Left);
			double width = colorChart.NormalizeX(1);
			double xStart = colorChart.NormalizeX(1);
			double yStart = colorChart.NormalizeY(2, TtpEnAxis.Left);
			
			// Überschrift
			TextBlock tb = new TextBlock
			{
				Text = ZLegend,
				Foreground = textColor,
				Margin = new Thickness(0,0,0,90), 
			};
			tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
			Canvas.SetLeft(tb, canvas.Width / 2 - tb.DesiredSize.Width / 2);
			Canvas.SetBottom(tb, yStart+heigth);
			colorChart.Canvas.Children.Add(tb);

			// Min u Max Z- Werte

			_maxXLabel = new TextBlock //Klassenvariable wg. Aktualisierung durch Z-Zoom
			{
				Text = _zAxisInfo.ActualScaleMax.ToString("   0.###"),
				Foreground = textColor,
				Margin = new Thickness(0, 0, 0, 10),
			};
			_maxXLabel.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
			Canvas.SetLeft(_maxXLabel, canvas.Width / 2 - _maxXLabel.DesiredSize.Width / 2);
			Canvas.SetBottom(_maxXLabel, yStart + heigth);
			colorChart.Canvas.Children.Add(_maxXLabel);

			TextBlock minL = new TextBlock
			{
				Text = _zAxisInfo.ActualScaleMin.ToString("0.###"),
				Foreground = textColor,
				Margin = new Thickness(0, 0, 10, 0),
			};
			minL.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
			Canvas.SetLeft(minL, canvas.Width / 2 - minL.DesiredSize.Width / 2);
			Canvas.SetTop(minL, yStart + heigth);
			colorChart.Canvas.Children.Add(minL);

			DrawColorBarGradient(colorChart, xStart, yStart, width, heigth);
			DrawSlider(colorChart, xStart, yStart, width, heigth);
		}

		private void DrawColorBarGradient(ChartGrid chart, double startX, double startY, double width, double heigth)
		{
			double h = heigth / 500.0;

			for ( int i=0; i<500; i++)
			{
				Rectangle rect = new Rectangle
				{
					Width = width,
					Height = 3,
					Stroke = Brushes.Transparent,
					Fill = new SolidColorBrush(ColorTools.GetShortRainbow(i/500.0))
				};
				Canvas.SetLeft(rect, startX);
				Canvas.SetBottom(rect, startY+(h*i));
				chart.Canvas.Children.Add(rect);
			}
		}

		private void DrawZoomedMaxLimitLabel()
		{
			double m = _zAxisInfo.ActualScaleMin +  (_zAxisInfo.ActualScaleMax - _zAxisInfo.ActualScaleMin) * ColorZoom;
			_maxXLabel.Text = (ColorZoom < 1.0) ? $"> " + m.ToString("0.###") : m.ToString("0.###");
		}

		private void DrawSlider(ChartGrid chart, double startX, double startY, double width, double heigth)
		{
			_zoomSlider = new Slider
			{
				Minimum = 0.01,
				Maximum = 1.0,
				Height = heigth,
				Orientation = Orientation.Vertical,
				Value=ColorZoom,
			};

			Canvas.SetLeft(_zoomSlider, startX + width);
			Canvas.SetTop(_zoomSlider, startY );
			chart.Canvas.Children.Add(_zoomSlider);
		}

		private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			ColorZoom = ((Slider)sender).Value;
			DrawZoomedMaxLimitLabel();				
			DrawColorZoomedMeshRects();
		}

		public void AddSliderHandlers()
		{
			_zoomSlider.ValueChanged += ZoomSlider_ValueChanged;
		}

		#endregion
	}
}

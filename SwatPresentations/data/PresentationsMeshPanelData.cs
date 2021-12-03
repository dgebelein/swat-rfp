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
	public class PresentationsMeshPanelData : PresentationsData
	{
		#region Variable

		public List<PresentationsMeshData> _meshWindowsData;

		public double PanelColorZoom { get; set; }
		private Slider _zoomSlider;
		private StackPanel _sliderPanel;
		double _zoom;
		//SolidColorBrush[] _colorBrushes;

		#endregion
		#region Construction

		public PresentationsMeshPanelData()
		{
			//_colorBrushes = MeshRects.CreateBrushes();
			_meshWindowsData = new List<PresentationsMeshData>();
		}

		#endregion

		#region Colorslider

		public void DrawColorSlider(StackPanel colorSliderPanel)
		{
			_sliderPanel = colorSliderPanel;
			TextBlock minLabel = colorSliderPanel.Children[0] as TextBlock;
			TextBlock maxLabel = colorSliderPanel.Children[2] as TextBlock;
			Canvas canvas= colorSliderPanel.Children[1] as Canvas;

			ChartGrid colorChart = new ChartGrid(canvas, Brushes.Transparent, Brushes.Transparent);
			colorChart.SetScaling(0, 100, 50, 0, 30, 0, 5);
			
			minLabel.Text =_meshWindowsData[0].MinimumZ.ToString("0.###");
			maxLabel.Text = _meshWindowsData[0].MaximumZ.ToString("0.###");
			_zoom = 1.0;

			double heigth = colorChart.NormalizeYDist(30, TtpEnAxis.Left);
			double width = colorChart.NormalizeXDist(100);
			double xStart = colorChart.NormalizeX(0);
			double yStart = colorChart.NormalizeY(30, TtpEnAxis.Left);

			DrawColorBarGradient(colorChart, xStart, yStart, width, heigth);
			DrawSlider(colorChart, xStart, yStart, width, heigth);
		}

		private void DrawColorBarGradient(ChartGrid chart, double startX, double startY, double width, double heigth)
		{
			double w = width / 500.0;

			for (int i = 0; i < 500; i++)
			{
				Rectangle rect = new Rectangle
				{
					Width = 1,
					Height = heigth,
					Stroke = null, 
					Fill = new SolidColorBrush(ColorTools.GetShortRainbow(i / 500.0))

				};
				Canvas.SetTop(rect, startY);
				Canvas.SetLeft(rect, startX + (w * i));
				chart.Canvas.Children.Add(rect);
			}
		}

		private void DrawSlider(ChartGrid chart, double startX, double startY, double width, double heigth)
		{
			_zoomSlider = new Slider
			{
				Minimum = 0.01,
				Maximum = 1.0,
				Width = width,
				Orientation = Orientation.Horizontal,
				Value = ColorZoom,
				TickFrequency = 10 // hat  ohne IsSnapToTickEnabled nur Auswirkung auf Tastaturbedienung
				
			};

			Canvas.SetLeft(_zoomSlider, startX);
			Canvas.SetBottom(_zoomSlider, startY-15);
			chart.Canvas.Children.Add(_zoomSlider);
		}

		#endregion

		#region ColorZoom
		public double ColorZoom
		{ 
			get { return _zoom; } 
			set 
			{ 
				_zoom = value;
				DrawZoomedMaxLimitLabel();
				ZoomMeshWindows();
			}
		}

		private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			ColorZoom = ((Slider)sender).Value;
		}

		public void AddSliderHandlers()
		{
			_zoomSlider.ValueChanged += ZoomSlider_ValueChanged;
		}

		private void DrawZoomedMaxLimitLabel()
		{
			TextBlock maxLabel = _sliderPanel.Children[2] as TextBlock;
			double m = _meshWindowsData[0].MinimumZ + (_meshWindowsData[0].MaximumZ - _meshWindowsData[0].MinimumZ) * ColorZoom;
			maxLabel.Text = (ColorZoom < 1.0) ? $"> " + m.ToString("0.###") : m.ToString("0.###");
		}

		public void AddMeshWindowData(PresentationsMeshData meshData)
		{
			_meshWindowsData.Add(meshData);
		}

		void ZoomMeshWindows()
		{ 
			foreach(PresentationsMeshData singleMeshData in _meshWindowsData)
			{
				singleMeshData.ColorZoom = ColorZoom;
				singleMeshData.DrawColorZoomedMeshRects();
			}
		}

		#endregion
	}
}

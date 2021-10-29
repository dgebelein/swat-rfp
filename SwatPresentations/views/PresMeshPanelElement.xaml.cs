using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	public partial class PresMeshPanelElement : SwatPresentation
	{
		#region Variable

		double _fixX0;
		double _fixX1;
		double _fixY0;
		double _fixY1;
		double _fixZ0;
		double _fixZ1;

		#endregion


		#region Construction

		public PresMeshPanelElement(PresentationsData data, bool isPrint)
		{
			_sourceData = data;
			_isPrint = isPrint;
			_fixX0 = _fixX1 = _fixY0 = _fixY1 = _fixZ0 = _fixZ1 = double.NaN;

			InitializeComponent();

			if (isPrint)
			{
				_backgroundColor = Brushes.White;
				_textColor = Brushes.Black;
			}
			else
			{
				_backgroundColor = Brushes.Black;
				_textColor = Brushes.White;
				_hittedSeries = -1;
			}
		}

		#endregion

		#region Properties

		PresentationsMeshData MyData
		{
			get { return (PresentationsMeshData)SourceData; }
		}
		#endregion


		#region Drawing

		private void ChartSizeChanged(object sender, SizeChangedEventArgs e)
		{
			_xLeftAxisCanvas.Width = _xLeftAxisGrid.ActualWidth;
			_xLeftAxisCanvas.Height = _xLeftAxisGrid.ActualHeight;

			_xRightAxisCanvas.Width = _xRightAxisGrid.ActualWidth;
			_xRightAxisCanvas.Height = _xRightAxisGrid.ActualHeight;

			_xXAxisCanvas.Width = _xXAxisGrid.ActualWidth;
			_xXAxisCanvas.Height = _xXAxisGrid.ActualHeight;

			_xChartCanvas.Width = _xChartGrid.ActualWidth;
			_xChartCanvas.Height = _xChartGrid.ActualHeight;

			ShowChart();

		}

		public override void ShowChart()
		{
			if (SourceData == null)
				return;

			_drawing.Background = _backgroundColor;

			_xLeftAxisCanvas.Children.Clear();
			_xRightAxisCanvas.Children.Clear();
			_xXAxisCanvas.Children.Clear();
			_xChartCanvas.Children.Clear();

			DrawChartContent();

			// am Ende noch einen Rahmen um das Zeichenfeld...
			Rectangle rect = new Rectangle
			{
				Width = _xChartCanvas.Width,
				Height = _xChartCanvas.Height,
				Stroke = _textColor,
				StrokeThickness = 1,
				Fill = Brushes.Transparent
			};
			_xChartCanvas.Children.Add(rect);
		}

		private void DrawChartContent()
		{
			_title.Text = SourceData.Title;
			_title.Foreground = _textColor;

			// Grid - Gitter mit Standardeinstellung
			_chartGrid = new ChartGrid(_xChartCanvas, _backgroundColor, _textColor);
			_chartGrid.SetScaling(0, 100, 10, 0, 100, 0, 100);
			//_chartGrid.DrawMesh();

			// Achsen
			_vlAxis = new VerticalAxis(_xLeftAxisCanvas, 0, 0, _textColor);
			_xAxis = new HorizontalAxis(_xXAxisCanvas, _xLeftAxisCanvas.Width, _xRightAxisCanvas.Width, _textColor);


			if (MyData.NumRects > 0)
			{
				AssignScaling();

				// linke Achse
				Double deltaTick = (SourceData.LeftAxisInfo.ActualScaleMax - SourceData.LeftAxisInfo.ActualScaleMin) / 5;
				_vlAxis.SetParameters(true,
											SourceData.LeftAxisInfo.ActualScaleMin,
											SourceData.LeftAxisInfo.ActualScaleMax,
											SourceData.LeftAxisInfo.ActualScaleMin,
											deltaTick);

				_chartGrid.SetYScaling(TtpEnAxis.Left, SourceData.LeftAxisInfo.ActualScaleMin, SourceData.LeftAxisInfo.ActualScaleMax);
				_vlAxis.Draw(false, true);

				// X-Achse
				deltaTick = (SourceData.XAxisInfo.ActualScaleMax - SourceData.XAxisInfo.ActualScaleMin) / 5;
				_xAxis.SetParameters(SourceData.XAxisInfo.ActualScaleMin,
											SourceData.XAxisInfo.ActualScaleMax,
											SourceData.XAxisInfo.ActualScaleMin,
											deltaTick);
				_chartGrid.SetXScaling(SourceData.XAxisInfo.ActualScaleMin,
											 SourceData.XAxisInfo.ActualScaleMax,
											 deltaTick);
				_xAxis.Draw(false);

				//SourceData.CreateYLegend(_xLeftAxisGrid, _textColor);
				SourceData.CreateXLegend(_xXAxisGrid, _textColor);
	//			MyData.DrawColorSlider(xColorSliderCanvas);

				MyData.DrawMeshRects(_chartGrid);
			}
		}


		private void AssignScaling()
		{
			MyData.CalcMeshAxes();

			// Hier gemeinsame Z- Skalierung aus Data !

			TtpScaleInfo xInfo = MyData.XAxisInfo;
			if (!double.IsNaN(_fixX0))
			{
				xInfo.ActualScaleMin = _fixX0;
				xInfo.ActualScaleMax = _fixX1;
				MyData.XAxisInfo = xInfo;
			}

			TtpScaleInfo yInfo = MyData.LeftAxisInfo;
			if (!double.IsNaN(_fixY0))
			{
				yInfo.ActualScaleMin = _fixY0;
				yInfo.ActualScaleMax = _fixY1;
				MyData.LeftAxisInfo = yInfo;
			}
			TtpScaleInfo zInfo = MyData.ZAxisInfo;
			if (!double.IsNaN(_fixZ0))
			{
				zInfo.ActualScaleMin = _fixZ0;
				zInfo.ActualScaleMax = _fixZ1;
				MyData.ZAxisInfo = zInfo;
			}
		}

		#endregion


		#region Mausaktionen

		private void _xChartCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (!_xChartCanvas.IsMouseCaptured)
			{  // Werte anzeigen ...
				_xChartCanvas.CaptureMouse();
				_isRetrieving = true;
				CreateInfoBox();
				ShowDataRetrieval(e.GetPosition(_xChartCanvas));
			}
		}

		private void _xChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
		{
			if (_xChartCanvas.IsMouseCaptured)
			{
				Double x = (e.GetPosition(_xChartCanvas)).X;
				Double y = (e.GetPosition(_xChartCanvas)).Y;
				if ((x < 0.0) || (y < 0.0) || (x > _xChartCanvas.Width) || (y > _xChartCanvas.Height))
					return;

				if (_isRetrieving)
				{
					ShowDataRetrieval(e.GetPosition(_xChartCanvas));
				}
			}
		}

		private void _xChartCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (!_xChartCanvas.IsMouseCaptured)
				return;

			if (_isRetrieving)
			{
				_isRetrieving = false;
				ClearDataRetrieving();
			}
		}

		#endregion

		#region Data Retrieval

		private void CreateInfoBox()
		{
			_drHorLine = new Line
			{
				SnapsToDevicePixels = false,
				Stroke = Brushes.Red,
				StrokeThickness = 1,
				X1 = 0,
				X2 = _xChartCanvas.Width,
				Y1 = 0,
				Y2 = 0
			};

			_drVertLine = new Line
			{
				SnapsToDevicePixels = false,
				Stroke = Brushes.Red,
				StrokeThickness = 1,
				X1 = 0,
				X2 = 0,
				Y1 = 0,
				Y2 = _xChartCanvas.Height
			};


			_drTextPanel = new StackPanel
			{
				Orientation = Orientation.Vertical
			};

			_drTextBorder = new Border
			{
				Opacity = 0.75,
				CornerRadius = new CornerRadius(4),
				Child = _drTextPanel
			};

			_xChartCanvas.Children.Add(_drHorLine);
			_xChartCanvas.Children.Add(_drVertLine);
			_xChartCanvas.Children.Add(_drTextBorder);

		}



		private void ShowDataRetrieval(Point pt)
		{
			List<string> boxText = GetRetrievingInfo(pt);
			if (boxText.Count == 0)
				return;

			Brush boxBackground = new SolidColorBrush(Color.FromArgb(196, 255, 255, 255));

			Canvas.SetLeft(_drVertLine, pt.X);
			Canvas.SetTop(_drHorLine, pt.Y);

			_drTextPanel.Children.Clear();

			for (int n = 0; n < 3; n++)
			{
				TextBlock label = new TextBlock
				{
					Foreground = _backgroundColor,
					Margin = new Thickness(5, 0, 5, 0),
					Text = boxText[n]
				};
				label.TextWrapping = TextWrapping.Wrap;
				_drTextPanel.Children.Add(label);
			}

			_drTextBorder.Background = boxBackground;
			_drTextBorder.Padding = new Thickness(5);

			if ((pt.X + 12 + _drTextPanel.ActualWidth) < _xChartCanvas.Width)
				Canvas.SetLeft(_drTextBorder, pt.X + 12);
			else
				Canvas.SetLeft(_drTextBorder, pt.X - 12 - _drTextPanel.ActualWidth);

			if ((pt.Y + 15 + _drTextPanel.ActualHeight) < _xChartCanvas.Height)
				Canvas.SetTop(_drTextBorder, pt.Y + 15);
			else
				Canvas.SetTop(_drTextBorder, pt.Y - 5 - _drTextPanel.ActualHeight);
		}

		private List<string> GetRetrievingInfo(Point pt)
		{
			double x = _chartGrid.GetXCoord(pt.X);
			double y = _chartGrid.GetYCoord(pt.Y, TtpEnAxis.Left);
			return MyData.GetRetrievalStrings(x, y);
		}

		private void ClearDataRetrieving()
		{
			// Fadenkreuz und Beschriftung löschen
			_xChartCanvas.ReleaseMouseCapture();

			_xChartCanvas.Children.Remove(_drTextBorder);
			_xChartCanvas.Children.Remove(_drVertLine);
			_xChartCanvas.Children.Remove(_drHorLine);

			_drHorLine = null;
			_drVertLine = null;

			_drTextPanel = null;
			_drTextBorder = null;
		}


		#endregion


		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			//MenueBtn.ContextMenu.DataContext = DataContext; // wichtig: Contextmenu-DC wird nicht automatisch übernommen
			//MenueBtn.ContextMenu.IsOpen = true;
		}

		private void root_Loaded(object sender, RoutedEventArgs e)
		{
			//MyData.AddSliderHandlers();
		}
	} // class

}

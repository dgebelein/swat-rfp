
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
	public partial class PresFunctionPlot : SwatPresentation
	{
		#region Variable

		//private HorizontalAxis _xAxis;
		//private PresentationRow _retrievalRow;
		double _fixX0;
		double _fixX1;
		double _fixY0;
		double _fixY1;



		#endregion


		#region Construction

		public PresFunctionPlot(PresentationsData data, bool isPrint)
		{
			_sourceData = data;
			_isPrint = isPrint;
			_fixX0 = data.XScaleMin;
			_fixX1 = data.XScaleMax;
			_fixY0 = data.YScaleMin;
			_fixY1 = data.YScaleMax;

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

			_xLeftLegend.Children.Clear();
			_xRightLegend.Children.Clear();
			_xLeftAxisCanvas.Children.Clear();
			_xRightAxisCanvas.Children.Clear();
			_xXAxisCanvas.Children.Clear();
			_xChartCanvas.Children.Clear();

			if ((SourceData).IsZoomed)
				ShowZoomedContent();
			else
				DrawChartContent();

			//DrawChartContent();

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
			_chartGrid.SetScaling(0, 100, 10, 0, 100,0, 100);
			_chartGrid.DrawCanvas(GridCanvasType.XYCanvas);

			// Achsen
			_vlAxis = new VerticalAxis(_xLeftAxisCanvas, 10, 75, _textColor);
			_xAxis = new HorizontalAxis(_xXAxisCanvas, _xLeftAxisCanvas.Width, _xRightAxisCanvas.Width, _textColor);


			if (SourceData.NumRows > 1)
			{
				//SourceData.CalcXYAxes();
				AssignScaling();

				// linke Achse
				Double deltaTick = (SourceData.LeftAxisInfo.ActualScaleMax - SourceData.LeftAxisInfo.ActualScaleMin) / 10.0;
				_vlAxis.SetParameters(true,
											SourceData.LeftAxisInfo.ActualScaleMin,
											SourceData.LeftAxisInfo.ActualScaleMax,
											SourceData.LeftAxisInfo.ActualScaleMin,
											deltaTick);

				_chartGrid.SetYScaling(TtpEnAxis.Left, SourceData.LeftAxisInfo.ActualScaleMin, SourceData.LeftAxisInfo.ActualScaleMax);


				// X-Achse
				deltaTick = (SourceData.XAxisInfo.ActualScaleMax - SourceData.XAxisInfo.ActualScaleMin) / 10.0;
				_xAxis.SetParameters(SourceData.XAxisInfo.ActualScaleMin,
											SourceData.XAxisInfo.ActualScaleMax,
											SourceData.XAxisInfo.ActualScaleMin,
											deltaTick);
				_chartGrid.SetXScaling(SourceData.XAxisInfo.ActualScaleMin,
											 SourceData.XAxisInfo.ActualScaleMax,
											 deltaTick);

				_vlAxis.Draw();
				SourceData.CreateYLegend(_xLeftAxisGrid, _textColor);
				_xAxis.Draw();

				SourceData.CreateXYLegendPanel(_xLeftLegend, _xLeftAxisCanvas.Width, _xChartCanvas.Width / 2, _textColor);
				SourceData.CreateXLegend(_xXAxisGrid, _textColor);
				
				//Funktionsgraph
				SourceData.DrawScatter(_chartGrid);
			}
		}


		private void ShowZoomedContent()
		{

			_title.Text = SourceData.Title;
			_title.Foreground = _textColor;

			// Grid - Gitter mit Standardeinstellung
			_chartGrid = new ChartGrid(_xChartCanvas, _backgroundColor, _textColor);
			_chartGrid.SetScaling(0, 100,  10, 0, 100, 0, 100);
			_chartGrid.DrawCanvas(GridCanvasType.XYCanvas);

			// Achsen
			_vlAxis = new VerticalAxis(_xLeftAxisCanvas, 10, 75, _textColor);
			_xAxis = new HorizontalAxis(_xXAxisCanvas, _xLeftAxisCanvas.Width, _xRightAxisCanvas.Width, _textColor);


			if (SourceData.NumRows > 1)
			{
				// linke Achse
				Double deltaTick = (SourceData.ZoomYL1 - SourceData.ZoomYL0) / 10.0;
				_vlAxis.SetParameters(true,
											SourceData.ZoomYL0,
											SourceData.ZoomYL1,
											SourceData.ZoomYL0,
											deltaTick);

				_chartGrid.SetYScaling(TtpEnAxis.Left,SourceData.ZoomYL0,SourceData.ZoomYL1);


				// X-Achse
				deltaTick = (SourceData.ZoomX1 - SourceData.ZoomX0) / 10.0;
				_xAxis.SetParameters(SourceData.ZoomX0,
											SourceData.ZoomX1,
											SourceData.ZoomX0,
											deltaTick);
				_chartGrid.SetXScaling(SourceData.ZoomX0,
											 SourceData.ZoomX1,
											 deltaTick);

				_vlAxis.Draw();
				SourceData.CreateYLegend(_xLeftAxisGrid, _textColor);
				_xAxis.Draw();

				SourceData.CreateXYLegendPanel(_xLeftLegend, _xLeftAxisCanvas.Width, _xChartCanvas.Width / 2, _textColor);
				SourceData.CreateXLegend(_xXAxisGrid, _textColor);
				//Punktwolken
				SourceData.DrawScatter(_chartGrid);
			}
		}

		private void AssignScaling()
		{
			SourceData.CalcXYAxes();

			TtpScaleInfo xInfo = SourceData.XAxisInfo;
			if (!double.IsNaN(_fixX0))
			{
				xInfo.ActualScaleMin = _fixX0;
				xInfo.ActualScaleMax = _fixX1;
				SourceData.XAxisInfo = xInfo;
			}

			TtpScaleInfo yInfo = SourceData.LeftAxisInfo;
			if (!double.IsNaN(_fixY0))
			{
				yInfo.ActualScaleMin = _fixY0;
				yInfo.ActualScaleMax = _fixY1;
				SourceData.LeftAxisInfo = yInfo;
			}
		}

		#endregion


		#region Mausaktionen

		private void _xChartCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != 0) ||
				((Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) != 0))
			{  // Zoomen	

				if (!_xChartCanvas.IsMouseCaptured)
				{
					_zoomStartPoint = e.GetPosition(_xChartCanvas);
					_xChartCanvas.CaptureMouse();
					_isZooming = true;
				}
			}

			else if (!_xChartCanvas.IsMouseCaptured)// Werte anzeigen ...
			{
				_xChartCanvas.CaptureMouse();
				_isRetrieving = true;
				//_retrievalRow = SourceData.GetRow(1);

				CreateInfoBox();
				ShowDataRetrieval(e.GetPosition(_xChartCanvas));
			}

		}

#pragma warning disable IDE1006 // Benennungsstile
		private void _xChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
#pragma warning restore IDE1006 // Benennungsstile
		{
			if (_xChartCanvas.IsMouseCaptured)
			{
				Double x = (e.GetPosition(_xChartCanvas)).X;
				Double y = (e.GetPosition(_xChartCanvas)).Y;
				if ((x < 0.0) || (y < 0.0) || (x > _xChartCanvas.Width) || (y > _xChartCanvas.Height))
					return;

				if (_isZooming)
				{
					_zoomEndPoint = e.GetPosition(_xChartCanvas);
					MarkZoomingArea();
				}
				if (_isRetrieving)
				{
					ShowDataRetrieval(e.GetPosition(_xChartCanvas));
				}


			}
		}

#pragma warning disable IDE1006 // Benennungsstile
		private void _xChartCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
#pragma warning restore IDE1006 // Benennungsstile
		{

			if (!_xChartCanvas.IsMouseCaptured)
				return;

			if (_isRetrieving)
			{
				_isRetrieving = false;
				ClearDataRetrieving();
			}

			if (_isZooming)
			{
				_isZooming = false;
				_zoomEndPoint = e.GetPosition(_xChartCanvas);
				ZoomMarkedArea();
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
			String[] boxText;
			Brush boxBackground = new SolidColorBrush(Color.FromArgb(196, 255, 255, 255));

			GetRetrievingInfo(pt, out boxText);

			Canvas.SetLeft(_drVertLine, pt.X);
			Canvas.SetTop(_drHorLine, pt.Y);

			_drTextPanel.Children.Clear();

			for (int n = 0; n < 2; n++)
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

		private void GetRetrievingInfo(Point pt, out string[] s)
		{
			s = new string[2];

			double x = _chartGrid.GetXCoord(pt.X);
			double y = _chartGrid.GetYCoord(pt.Y, TtpEnAxis.Left);
			s[0] = $"X = {x.ToString("0.###", CultureInfo.InvariantCulture) }";
			s[1] = $"Y = {y.ToString("0.###", CultureInfo.InvariantCulture) }";


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


		#region Zooming 

		private void MarkZoomingArea()
		{
			if (_zoomRubberBand == null)
			{
				_zoomRubberBand = new Rectangle
				{
					Stroke = Brushes.Red,
					Fill = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255))
				};
				_xChartCanvas.Children.Add(_zoomRubberBand);
			}

			_zoomRubberBand.Width = Math.Abs(_zoomStartPoint.X - _zoomEndPoint.X);
			_zoomRubberBand.Height = Math.Abs(_zoomStartPoint.Y - _zoomEndPoint.Y);

			double left = Math.Min(_zoomStartPoint.X, _zoomEndPoint.X);
			double top = Math.Min(_zoomStartPoint.Y, _zoomEndPoint.Y);
			Canvas.SetLeft(_zoomRubberBand, left);
			Canvas.SetTop(_zoomRubberBand, top);

		}

		//--------------------------------------------------------------------------------------------

		private void ZoomMarkedArea()
		{
			// x-Achse
			if (_zoomEndPoint.X > _zoomStartPoint.X)
			{
				SourceData.ZoomX0 = _chartGrid.ScaleXMin + (_chartGrid.ScaleXMax - _chartGrid.ScaleXMin) * _zoomStartPoint.X / _xChartCanvas.Width;
				SourceData.ZoomX1 = _chartGrid.ScaleXMin + (_chartGrid.ScaleXMax - _chartGrid.ScaleXMin) * _zoomEndPoint.X / _xChartCanvas.Width;
			}
			else if (_zoomEndPoint.X < _zoomStartPoint.X)
			{
				SourceData.ZoomX1 = _chartGrid.ScaleXMin + (_chartGrid.ScaleXMax - _chartGrid.ScaleXMin) * _zoomStartPoint.X / _xChartCanvas.Width;
				SourceData.ZoomX0 = _chartGrid.ScaleXMin + (_chartGrid.ScaleXMax - _chartGrid.ScaleXMin) * _zoomEndPoint.X / _xChartCanvas.Width;
			}

			// linke Seite
			if (_zoomEndPoint.Y < _zoomStartPoint.Y)
			{
				SourceData.ZoomYL0 = _chartGrid.ScaleYLMin + (_chartGrid.ScaleYLMax - _chartGrid.ScaleYLMin) * (_xChartCanvas.Height - _zoomStartPoint.Y) / _xChartCanvas.Height;
				SourceData.ZoomYL1 = _chartGrid.ScaleYLMin + (_chartGrid.ScaleYLMax - _chartGrid.ScaleYLMin) * (_xChartCanvas.Height - _zoomEndPoint.Y) / _xChartCanvas.Height;
			}
			else if (_zoomEndPoint.Y > _zoomStartPoint.Y)
			{
				SourceData.ZoomYL1 = _chartGrid.ScaleYLMin + (_chartGrid.ScaleYLMax - _chartGrid.ScaleYLMin) * (_xChartCanvas.Height - _zoomStartPoint.Y) / _xChartCanvas.Height;
				SourceData.ZoomYL0 = _chartGrid.ScaleYLMin + (_chartGrid.ScaleYLMax - _chartGrid.ScaleYLMin) * (_xChartCanvas.Height - _zoomEndPoint.Y) / _xChartCanvas.Height;
			}

			SourceData.ZoomX0 = Math.Max(SourceData.ZoomX0, _chartGrid.ScaleXMin);
			SourceData.ZoomX1 = Math.Min(SourceData.ZoomX1, _chartGrid.ScaleXMax);
			SourceData.ZoomYL0 = Math.Max(SourceData.ZoomYL0, _chartGrid.ScaleYLMin);
			SourceData.ZoomYL1 = Math.Min(SourceData.ZoomYL1, _chartGrid.ScaleYLMax);


			if (_zoomRubberBand != null)
			{
				_zoomRubberBand = null;
			}

			_xChartCanvas.ReleaseMouseCapture();

			SourceData.IsZoomed = ((Math.Abs(_zoomEndPoint.X - _zoomStartPoint.X) > 3) && (Math.Abs(_zoomEndPoint.Y - _zoomStartPoint.Y) > 3));
			ShowChart();
		}



		//--------------------------------------------------------------------------------------------

		#endregion

		#region Toggle Visibility

		private void _xLeftLegend_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DoubleClickLegend(_xLeftLegend, e);
		}

		//private void _xRightLegend_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//	DoubleClickLegend(_xRightLegend, e);
		//}

		private void DoubleClickLegend(object sender, MouseButtonEventArgs e)
		{

			_isDoubleClick = false;

			if ((e.ClickCount >= 2) && (e.LeftButton == MouseButtonState.Pressed))
			{
				_isDoubleClick = true;
				int h = DetectHittedSeries(sender, e.GetPosition((UIElement)sender)) + 1; // Serie 0 ist x-Wert
				if (h > 0)
				{
					SourceData.ToggleVisibilty(h);
					ShowChart();
				}
			}
		}


		#endregion

		#region Scaling-Dialoge
		private void ShowXScaleDlg(object sender, MouseButtonEventArgs e)
		{
			double mini = SourceData.XAxisInfo.ActualScaleMin;
			double maxi = SourceData.XAxisInfo.ActualScaleMax;
			Tuple<ScaleDlgResult,double, double> ScX = DlgScaleAxis.Show("Scale X-Axis", false, mini, maxi);

			ScaleDlgResult res = ScX.Item1;
			switch(res)
			{
				case ScaleDlgResult.Escape: 
					return;
				case ScaleDlgResult.Auto:	
					_fixX0 =_fixX1 = double.NaN; 
					break;
				case ScaleDlgResult.Fixed:
					_fixX0 = ScX.Item2;
					_fixX1 = ScX.Item3;
					break;

			}
			SourceData.IsZoomed = false;
			ShowChart();
		}
	

		private void ShowYScaleDlg(object sender, MouseButtonEventArgs e)
		{
			double mini = SourceData.LeftAxisInfo.ActualScaleMin;
			double maxi = SourceData.LeftAxisInfo.ActualScaleMax;
			Tuple<ScaleDlgResult, double, double> ScY = DlgScaleAxis.Show("Scale Y-Axis", false, mini, maxi);

			ScaleDlgResult res = ScY.Item1;
			switch (res)
			{
				case ScaleDlgResult.Escape:
					return;
				case ScaleDlgResult.Auto:
					_fixY0 = _fixY1 = double.NaN;
					break;
				case ScaleDlgResult.Fixed:
					_fixY0 = ScY.Item2;
					_fixY1 = ScY.Item3;
					break;

			}
			SourceData.IsZoomed = false;
			ShowChart();
		}
		#endregion


		#region Event Handling

		//private void SyncDataRetrieval(Point pt)
		//{
		//	if (pt.X >= 0)
		//	{
		//		ClearDataRetrieving(true);
		//		CreateInfoBox();
		//		ShowDataRetrieval(pt, true);
		//	}
		//	else
		//	{
		//		ClearDataRetrieving(true);
		//	}
		//}

		public override void RespondToPresentationEvent(object Sender, PresentationEventArgs e)
		{
			//switch (e.InfoType)
			//{
			//	case EventInfo.TimeRange:
			//		SourceData.TimeRange = e.PresTimeRange;
			//		ShowChart();
			//		break;

			//	case EventInfo.InfoPoint:
			//		SyncDataRetrieval(e.InfoPoint);
			//		break;

			//	default: break;
			//}
		}

		#endregion

		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			MenueBtn.ContextMenu.DataContext = DataContext; // wichtig: Contextmenu-DC wird nicht automatisch übernommen
			MenueBtn.ContextMenu.IsOpen = true;
		}



	} // class

}
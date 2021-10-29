using System;
using System.Collections.Generic;
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

namespace SwatPresentations
{
	/// <summary>
	/// Interaktionslogik für ViewPopDyn.xaml
	/// </summary>
	/// 
	public partial class ViewPopDyn : SwatPresentation
	{


		#region Variable

		private TimeAxis _timeAxis;
		private bool _initialShow = true;

		#endregion


		#region Construction

		public ViewPopDyn()
		{

			InitializeComponent();

			// ist notwendig, weil sonst die Befehle nicht ankommen würden
			// Das Binding über CommandTarget funktioniert in XAML nicht!
			// Achtung: Das geht hier nur deshalb, weil die Resource statisch geladen wird
			ContextMenu ctm = (ContextMenu)FindResource("contextLegend");
			for (int c = 0; c < ctm.Items.Count; c++)
			{
				if (ctm.Items[c] is MenuItem)
					((MenuItem)ctm.Items[c]).CommandTarget = this;
			}

			ctm = (ContextMenu)FindResource("contextLegendArea");
			for (int c = 0; c < ctm.Items.Count; c++)
			{
				if (ctm.Items[c] is MenuItem)
					((MenuItem)ctm.Items[c]).CommandTarget = this;
			}
		}

		#endregion


		#region Properties

		PresentationsData SourceData
		{
			get { return _sourceData; }
		}

		//new TrendStdData SourceData
		//{
		//	get { return (TrendStdData)_sourceData; }
		//}


		#endregion


		#region Drawing

		public override void ShowChart(bool refreshData)
		{
			/*
			if (SourceData == null)
				return;

			_drawing.Background = _backgroundColor;

			_xLeftLegend.Children.Clear();
			_xRightLegend.Children.Clear();
			_xLeftAxisCanvas.Children.Clear();
			_xRightAxisCanvas.Children.Clear();
			_xTimeAxisCanvas.Children.Clear();
			_xChartCanvas.Children.Clear();

			if (((TrendStdData)SourceData).IsZoomed)
				ShowZoomedChart();
			else
				ShowRegularChart(refreshData);

			// am Ende noch einen Rahmen um das Zeichenfeld...
			Rectangle rect = new Rectangle();
			rect.Width = _xChartCanvas.Width;
			rect.Height = _xChartCanvas.Height;
			rect.Stroke = _textColor;
			rect.StrokeThickness = 1;
			rect.Fill = Brushes.Transparent;
			_xChartCanvas.Children.Add(rect);

			// Wenn die Grafik durch einen Link des WebBrowsers erzeugt wurde, erhält sie erst dann wieder
			// MouseWheel-Benachrichtigungen, wenn irgendein Element des Programmfensters den Fokus erhalten hat
			// _xFbtn ist ein fokusierbarer Button, der unsichtbar bleibt und nur die Aufgabe hat den Fokus zu übernehmen

			if (_initialShow)
			{ // nur bei erstmaliger Anzeige, weil sonst z.B. Größenänderungen des Windows nicht mehr funktionieren
				_xFbtn.Focus();
				_initialShow = false;
			}
			*/
			// 
		}

		private void ChartSizeChanged(object sender, SizeChangedEventArgs e)
		{/*
			_xLeftAxisCanvas.Width = _xLeftAxisGrid.ActualWidth;
			_xLeftAxisCanvas.Height = _xLeftAxisGrid.ActualHeight;

			_xRightAxisCanvas.Width = _xRightAxisGrid.ActualWidth;
			_xRightAxisCanvas.Height = _xRightAxisGrid.ActualHeight;

			_xTimeAxisCanvas.Width = _xTimeAxisGrid.ActualWidth;
			_xTimeAxisCanvas.Height = _xTimeAxisGrid.ActualHeight;

			_xChartCanvas.Width = _xChartGrid.ActualWidth;
			_xChartCanvas.Height = _xChartGrid.ActualHeight;

			//Size-Changed-Message wird mehrmals gesendet - nur beim 1.Mal Daten lesen!
			ShowChart(e.PreviousSize.Height == 0);
			*/
		}

		private void ShowRegularChart(bool refreshData)
		{
			/*
			_trendTitle.Text = SourceData.Title;
			_trendTitle.Foreground = _textColor;

			// Zeitachse
			_timeAxis = new TimeAxis(_xChartCanvas, _xTimeAxisCanvas, _xLeftAxisCanvas.Width, _xRightAxisCanvas.Width, _textColor, SourceData.DisplayTimeRange);
			_timeAxis.Draw();

			// Grid - Gitter mit Standardeinstellung
			_chartGrid = new ChartGrid(_xChartCanvas, _backgroundColor, _textColor);
			_chartGrid.SetScaling(_timeAxis.ScaleMin, _timeAxis.ScaleMax, _timeAxis.FirstTick, _timeAxis.DeltaTicks,
									-1, 101, 0, 10, -1, 101, 0, 10);
			_chartGrid.Draw(_isRightZooming);

			// y-Achsen zeichnen
			_vlAxis = new VerticalAxis(_xLeftAxisCanvas, 10, 75, _textColor);
			_vrAxis = new VerticalAxis(_xRightAxisCanvas, 10, 75, _textColor);

			if (SourceData.Trends != null)
			{

				if (refreshData)
					SourceData.RequestData();

				SourceData.Trends.CreateLegendPanel(_dragClassID, _xLeftLegend, TtpEnAxis.Left,
																(ContextMenu)FindResource("contextLegend"),
																_xLeftAxisCanvas.Width, _xChartCanvas.Width / 2, _textColor);
				SourceData.Trends.CreateLegendPanel(_dragClassID, _xRightLegend, TtpEnAxis.Right,
																(ContextMenu)FindResource("contextLegend"),
																_xRightAxisCanvas.Width, _xChartCanvas.Width / 2, _textColor);


				SourceData.CalcAxes();

				// linke Achse
				Double deltaTick = (SourceData.LeftAxisInfo.ActualScaleMax - SourceData.LeftAxisInfo.ActualScaleMin) / 10.0;
				_vlAxis.SetParameters(TtpEnAxis.Left,
											SourceData.LeftAxisInfo.ActualScaleMin - deltaTick / 10,
											SourceData.LeftAxisInfo.ActualScaleMax + deltaTick / 10,
											SourceData.LeftAxisInfo.ActualScaleMin,
											 deltaTick);
				_chartGrid.SetYScaling(TtpEnAxis.Left,
											SourceData.LeftAxisInfo.ActualScaleMin - deltaTick / 10,
											SourceData.LeftAxisInfo.ActualScaleMax + deltaTick / 10,
											SourceData.LeftAxisInfo.ActualScaleMin,
											deltaTick);

				// rechte Achse
				deltaTick = (SourceData.RightAxisInfo.ActualScaleMax - SourceData.RightAxisInfo.ActualScaleMin) / 10.0;
				_vrAxis.SetParameters(TtpEnAxis.Right,
											SourceData.RightAxisInfo.ActualScaleMin - deltaTick / 10,
											SourceData.RightAxisInfo.ActualScaleMax + deltaTick / 10,
											SourceData.RightAxisInfo.ActualScaleMin,
											 deltaTick);
				_chartGrid.SetYScaling(TtpEnAxis.Right,
											SourceData.RightAxisInfo.ActualScaleMin - deltaTick / 10,
											SourceData.RightAxisInfo.ActualScaleMax + deltaTick / 10,
											SourceData.RightAxisInfo.ActualScaleMin,
											deltaTick);

				_vrAxis.Draw();
				_vlAxis.Draw();
				SourceData.Trends.Draw(_chartGrid);
				
			}*/
		}


		#endregion


		#region Mausaktionen

		private void _xChartCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			/*
			if (((Keyboard.GetKeyStates(Key.LeftCtrl) & KeyStates.Down) != 0) ||
				 ((Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) != 0))
			{  // Zoomen	
				_isRightZooming = ((Keyboard.GetKeyStates(Key.RightCtrl) & KeyStates.Down) != 0);
				if (!_xChartCanvas.IsMouseCaptured)
				{
					_zoomStartPoint = e.GetPosition(_xChartCanvas);
					_xChartCanvas.CaptureMouse();
					_isZooming = true;
				}
			}

			else if (((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) != 0) ||
						((Keyboard.GetKeyStates(Key.RightShift) & KeyStates.Down) != 0))
			{  // Kurven-Clone verschieben
				_csSeriesNo = SourceData.Trends.GetNearestSeries(_chartGrid, e.GetPosition(_xChartCanvas));
				if (_csSeriesNo >= 0)
				{
					_xChartCanvas.CaptureMouse();
					_isCloneShifting = true;
					_csStartPoint = e.GetPosition(_xChartCanvas);
					CreateCloneShiftBox();
					_csTrend = SourceData.Trends.Trends[_csSeriesNo].CloneLine(_chartGrid);
					_xChartCanvas.Children.Add(_csTrend);
				}
			}

			else if (!_xChartCanvas.IsMouseCaptured)// Werte anzeigen ...
			{  // Werte anzeigen
				_drSeriesNo = SourceData.Trends.GetNearestSeries(_chartGrid, e.GetPosition(_xChartCanvas));
				if (_drSeriesNo >= 0)
				{
					_xChartCanvas.CaptureMouse();
					_isRetrieving = true;
					CreateInfoBox();
					ShowDataRetrieval(_drSeriesNo, e.GetPosition(_xChartCanvas));
				}
			}
*/
		}

		private void _xChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
		{
			/*
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
					ShowDataRetrieval(_drSeriesNo, e.GetPosition(_xChartCanvas));
				}

				if (_isCloneShifting)
				{
					ShowCloneShifting(e.GetPosition(_xChartCanvas));
				}

			}
			*/
		}

		private void _xChartCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			/*
			if (!_xChartCanvas.IsMouseCaptured)
				return;

			if (_isZooming)
			{
				_isZooming = false;
				_zoomEndPoint = e.GetPosition(_xChartCanvas);
				ZoomMarkedArea();
			}

			if (_isRetrieving)
			{
				_isRetrieving = false;
				ClearDataRetrieving();
			}

			if (_isCloneShifting)
			{
				_isCloneShifting = false;
				ClearCloneShifting();
			}
*/
		}

		#endregion


		#region Zooming 

		private void MarkZoomingArea()
		{
			/*
			if (_zoomRubberBand == null)
			{
				_zoomRubberBand = new Rectangle();
				_zoomRubberBand.Stroke = Brushes.Red;
				_zoomRubberBand.Fill = new SolidColorBrush(Color.FromArgb(64, 255, 255, 255));
				_xChartCanvas.Children.Add(_zoomRubberBand);
			}

			_zoomRubberBand.Width = Math.Abs(_zoomStartPoint.X - _zoomEndPoint.X);
			_zoomRubberBand.Height = Math.Abs(_zoomStartPoint.Y - _zoomEndPoint.Y);

			double left = Math.Min(_zoomStartPoint.X, _zoomEndPoint.X);
			double top = Math.Min(_zoomStartPoint.Y, _zoomEndPoint.Y);
			Canvas.SetLeft(_zoomRubberBand, left);
			Canvas.SetTop(_zoomRubberBand, top);
*/
		}

		private void ZoomMarkedArea()
		{
			/*
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

			// rechte Seite
			if (_zoomEndPoint.Y < _zoomStartPoint.Y)
			{
				SourceData.ZoomYR0 = _chartGrid.ScaleYRMin + (_chartGrid.ScaleYRMax - _chartGrid.ScaleYRMin) * (_xChartCanvas.Height - _zoomStartPoint.Y) / _xChartCanvas.Height;
				SourceData.ZoomYR1 = _chartGrid.ScaleYRMin + (_chartGrid.ScaleYRMax - _chartGrid.ScaleYRMin) * (_xChartCanvas.Height - _zoomEndPoint.Y) / _xChartCanvas.Height;
			}
			else if (_zoomEndPoint.Y > _zoomStartPoint.Y)
			{
				SourceData.ZoomYR1 = _chartGrid.ScaleYRMin + (_chartGrid.ScaleYRMax - _chartGrid.ScaleYRMin) * (_xChartCanvas.Height - _zoomStartPoint.Y) / _xChartCanvas.Height;
				SourceData.ZoomYR0 = _chartGrid.ScaleYRMin + (_chartGrid.ScaleYRMax - _chartGrid.ScaleYRMin) * (_xChartCanvas.Height - _zoomEndPoint.Y) / _xChartCanvas.Height;
			}

			if (_zoomRubberBand != null)
			{
				_zoomRubberBand = null;
			}

			_xChartCanvas.ReleaseMouseCapture();

			IsZoomed = ((Math.Abs(_zoomEndPoint.X - _zoomStartPoint.X) > 3) && (Math.Abs(_zoomEndPoint.Y - _zoomStartPoint.Y) > 3));
			ShowChart(false);
			*/
		}

		private void ShowZoomedChart()
		{
			/*
			_trendTitle.Text = SourceData.Title;
			_trendTitle.Foreground = _textColor;

			_chartGrid = new ChartGrid(_xChartCanvas, _backgroundColor, _textColor);

			// Zeitachse
			_timeAxis = new TimeAxis(_xChartCanvas, _xTimeAxisCanvas, _xLeftAxisCanvas.Width, _xRightAxisCanvas.Width, _textColor, SourceData.DisplayTimeRange);
			_timeAxis.SetZoomParameters(SourceData.ZoomX0, SourceData.ZoomX1);
			_timeAxis.Draw();

			// linke Achse
			Double spacingL = CalcTickSpacing(_chartGrid.Canvas.Height, SourceData.ZoomYL0, SourceData.ZoomYL1, false);
			Double firstTickL = Math.Floor((SourceData.ZoomYL0 + spacingL) / spacingL) * spacingL;

			_vlAxis = new VerticalAxis(_xLeftAxisCanvas, 10, 75, _textColor);
			_vlAxis.SetParameters(TtpEnAxis.Left, SourceData.ZoomYL0, SourceData.ZoomYL1, firstTickL, spacingL);
			_vlAxis.Draw();

			// rechte Achse
			Double spacingR = CalcTickSpacing(_chartGrid.Canvas.Height, SourceData.ZoomYR0, SourceData.ZoomYR1, false);
			Double firstTickR = Math.Floor((SourceData.ZoomYR0 + spacingR) / spacingR) * spacingR;

			_vrAxis = new VerticalAxis(_xRightAxisCanvas, 10, 75, _textColor);
			_vrAxis.SetParameters(TtpEnAxis.Right, SourceData.ZoomYR0, SourceData.ZoomYR1, firstTickR, spacingR);
			_vrAxis.Draw();

			// Grid-Raster

			_chartGrid.SetScaling(_timeAxis.ScaleMin, _timeAxis.ScaleMax, _timeAxis.FirstTick, _timeAxis.DeltaTicks,
										 SourceData.ZoomYL0, SourceData.ZoomYL1, firstTickL, spacingL,
										 SourceData.ZoomYR0, SourceData.ZoomYR1, firstTickR, spacingR);

			SourceData.Trends.CreateLegendPanel(_dragClassID, _xLeftLegend, TtpEnAxis.Left,
															null, // kein ContextMenü im Zoom-Modus
															_xLeftAxisCanvas.Width, _xChartCanvas.Width / 2, _textColor);
			SourceData.Trends.CreateLegendPanel(_dragClassID, _xRightLegend, TtpEnAxis.Right,
															null, // kein ContextMenü im Zoom-Modus
															_xRightAxisCanvas.Width, _xChartCanvas.Width / 2, _textColor);


			_chartGrid.Draw(_isRightZooming);

			if (SourceData.Trends != null)
			{
				SourceData.Trends.Draw(_chartGrid);
			}
			*/
		}


		#endregion


		#region Data Retrieval


		private void CreateInfoBox()
		{
			/*
			_drHorLine = new Line();
			_drHorLine.SnapsToDevicePixels = false;
			_drHorLine.Stroke = Brushes.Red;
			_drHorLine.StrokeThickness = 1;
			_drHorLine.X1 = 0;
			_drHorLine.X2 = _xChartCanvas.Width;
			_drHorLine.Y1 = 0;
			_drHorLine.Y2 = 0;

			_drVertLine = new Line();
			_drVertLine.SnapsToDevicePixels = false;
			_drVertLine.Stroke = Brushes.Red;
			_drVertLine.StrokeThickness = 1;
			_drVertLine.X1 = 0;
			_drVertLine.X2 = 0;
			_drVertLine.Y1 = 0;
			_drVertLine.Y2 = _xChartCanvas.Height;

			_drCrossPoint = new Ellipse();
			_drCrossPoint.Width = 8;
			_drCrossPoint.Height = 8;

			_drTextPanel = new StackPanel();
			_drTextPanel.Orientation = Orientation.Vertical;

			_drTextBorder = new Border();
			_drTextBorder.Opacity = 0.75;
			_drTextBorder.CornerRadius = new CornerRadius(4);
			_drTextBorder.Child = _drTextPanel;

			_xChartCanvas.Children.Add(_drHorLine);
			_xChartCanvas.Children.Add(_drVertLine);
			_xChartCanvas.Children.Add(_drCrossPoint);
			_xChartCanvas.Children.Add(_drTextBorder);
*/
		}

		private void ShowDataRetrieval(int seriesNo, Point pt)
		{
			/*
			Point ptCross = new Point();

			String[] boxText;
			Brush boxBackground = new SolidColorBrush();
			SourceData.Trends.GetRetrievingInfo(seriesNo, _chartGrid, pt, ref ptCross, out boxText, ref boxBackground);

			Canvas.SetLeft(_drVertLine, ptCross.X);
			Canvas.SetTop(_drHorLine, ptCross.Y);

			Canvas.SetLeft(_drCrossPoint, ptCross.X - 4);
			Canvas.SetTop(_drCrossPoint, ptCross.Y - 4);


			_drTextPanel.Children.Clear();

			for (int n = 0; n < 4; n++)
			{
				TextBlock label = new TextBlock();
				label.Foreground = _backgroundColor;
				label.Margin = (n == 2) ? new Thickness(5, 5, 5, 0) : new Thickness(5, 0, 5, 0);
				label.Text = boxText[n];
				label.TextWrapping = TextWrapping.Wrap;
				if (n == 3)
					label.FontWeight = FontWeights.Medium;
				_drTextPanel.Children.Add(label);
			}

			_drTextBorder.Background = boxBackground;
			_drTextBorder.Padding = new Thickness(5);

			_drCrossPoint.Fill = boxBackground;

			if ((pt.X + 12 + _drTextPanel.ActualWidth) < _xChartCanvas.Width)
				Canvas.SetLeft(_drTextBorder, pt.X + 12);
			else
				Canvas.SetLeft(_drTextBorder, pt.X - 12 - _drTextPanel.ActualWidth);

			if ((pt.Y + 15 + _drTextPanel.ActualHeight) < _xChartCanvas.Height)
				Canvas.SetTop(_drTextBorder, pt.Y + 15);
			else
				Canvas.SetTop(_drTextBorder, pt.Y - 5 - _drTextPanel.ActualHeight);
*/
		}

		private void ClearDataRetrieving()
		{
			/*
			// Fadenkreuz und Beschriftung löschen
			_xChartCanvas.ReleaseMouseCapture();

			_xChartCanvas.Children.Remove(_drTextBorder);
			_xChartCanvas.Children.Remove(_drCrossPoint);
			_xChartCanvas.Children.Remove(_drVertLine);
			_xChartCanvas.Children.Remove(_drHorLine);

			_drHorLine = null;
			_drVertLine = null;
			_drCrossPoint = null;
			_drTextPanel = null;
			_drTextBorder = null;
			*/
		}


		#endregion


		#region Clone-Shifting


		private void CreateCloneShiftBox()
		{
			/*
			_csTextPanel = new StackPanel();
			_csTextPanel.Orientation = Orientation.Vertical;


			_csTextBorder = new Border();
			_csTextBorder.Opacity = 0.75;
			_csTextBorder.Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
			_csTextBorder.Child = _csTextPanel;

			_xChartCanvas.Children.Add(_csTextBorder);
			*/
		}


		private void ShowCloneShifting(Point pt)
		{
			/*
			// Trendkurve verschieben
			Canvas.SetLeft(_csTrend, pt.X - _csStartPoint.X);
			Canvas.SetTop(_csTrend, pt.Y - _csStartPoint.Y);


			// InfoBox verschieben
			_csTextPanel.Children.Clear();

			TtpTime startTime = _chartGrid.GetPointTime(_csStartPoint.X, SourceData.TimeRange, true);
			TtpTime endTime = _chartGrid.GetPointTime(pt.X, SourceData.TimeRange, true);
			int timeDiff = endTime.GetDifferenceTo(startTime, TtpEnPattern.Pattern1Min);

			Double yStart = _chartGrid.GetYCoord(_csStartPoint.Y, SourceData.Trends.Trends[_csSeriesNo].Axis);
			Double yEnd = _chartGrid.GetYCoord(pt.Y, SourceData.Trends.Trends[_csSeriesNo].Axis);

			for (int i = 0; i < 2; i++)
			{
				TextBlock label = new TextBlock();
				label.Foreground = _backgroundColor;
				label.Margin = new Thickness(5);
				label.TextWrapping = TextWrapping.Wrap;
				if (i == 0)
					label.Text = (timeDiff / 60).ToString("+ 00;- 00") + ":" + Math.Abs(timeDiff % 60).ToString("00");
				else
					label.Text = (yEnd - yStart).ToString("f3");

				_csTextPanel.Children.Add(label);
			}



			Canvas.SetLeft(_csTextBorder, pt.X - _csTextPanel.ActualWidth / 2);


			if ((pt.Y + 25 + _csTextPanel.ActualHeight) < _xChartCanvas.Height)
				Canvas.SetTop(_csTextBorder, pt.Y + 25);
			else
				Canvas.SetTop(_csTextBorder, pt.Y - 5 - _csTextPanel.ActualHeight);
				*/
		}

		private void ClearCloneShifting()
		{
			/*
			// Beschriftung löschen
			_xChartCanvas.ReleaseMouseCapture();

			_xChartCanvas.Children.Remove(_csTrend);
			_csTrend = null;

			_xChartCanvas.Children.Remove(_csTextBorder);
			_csTextPanel = null;
			_csTextBorder = null;
			*/
		}

		#endregion


		#region Time shifting


		private void TimeShift(object sender, MouseWheelEventArgs e)
		{
			/*
			if (_isZooming || SourceData.IsZoomed) // kein Time-Shifting im Zoom-Modus !
				return;

			TtpTimeRange tr = SourceData.TimeRange;


			// mit Shift
			if (((Keyboard.GetKeyStates(Key.LeftShift) | Keyboard.GetKeyStates(Key.RightShift)) & KeyStates.Down) != 0)
			{
				if (tr.IsFullMonth())
					tr.ShiftFullMonths(e.Delta < 0);
				else
					tr.Scroll(((e.Delta < 0) ? tr.GetNumPatterns() : -tr.GetNumPatterns()), true);
				SourceData.TimeRange = tr;
				return;
			}

			// mit Alt
			if (((Keyboard.GetKeyStates(Key.LeftAlt) | Keyboard.GetKeyStates(Key.RightAlt)) & KeyStates.Down) != 0)
			{
				tr.Scroll(((e.Delta < 0) ? 1 : -1), true);
				SourceData.TimeRange = tr;
				return;
			}

			// ohne Zusatztaste
			int numShiftingPatterns = (tr.Pattern >= TtpEnPattern.Pattern1Day) ? 1 : TtpTime.GetPatternTicks(TtpEnPattern.Pattern1Day) / TtpTime.GetPatternTicks(tr.Pattern); ;
			tr.Scroll(((e.Delta < 0) ? numShiftingPatterns : -numShiftingPatterns), true);
			SourceData.TimeRange = tr;
			return;
			/*
					if ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) != 0) {
						numShiftingPatterns = tr.GetNumPatterns();
					}
					else
						if ((Keyboard.GetKeyStates(Key.LeftAlt) & KeyStates.Down) != 0) {
							numShiftingPatterns = 1;
						}
						else {
							numShiftingPatterns = (tr.Pattern >= TtpEnPattern.Pattern1Day) ?
										 1 :
										 TtpTime.GetPatternTicks(TtpEnPattern.Pattern1Day) / TtpTime.GetPatternTicks(tr.Pattern);
						}

					if (e.Delta < 0)
						tr.Scroll(numShiftingPatterns, true);
					else
						tr.Scroll(-numShiftingPatterns, true);

					SourceData.TimeRange = tr;
			 */
		}

		private void TimeShiftStart(object sender, MouseWheelEventArgs e)
		{
			/*
			if (_isZooming || SourceData.IsZoomed) // kein Time-Shifting im Zoom-Modus !
				return;

			TtpTimeRange tr = SourceData.TimeRange;

			// bei gedrückter SHIFT-Taste um Faktor 10 verstärken
			int step = ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) != 0) ? 10 : 1;

			// mindestens in Tagesschritten verändern
			if (tr.Pattern < TtpEnPattern.Pattern1Day)
				step *= TtpTime.GetPatternTicks(TtpEnPattern.Pattern1Day) / TtpTime.GetPatternTicks(tr.Pattern);

			if (e.Delta < 0)
			{
				if ((tr.GetNumPatterns() - step) >= SourceData.GetMinTimePatterns(tr.Pattern))
					tr.Decrease(step, false);
			}
			else
			{
				if ((tr.GetNumPatterns() + step) <= SourceData.GetMaxTimePatterns(tr.Pattern))
					tr.Increase(step, false);
			}

			SourceData.TimeRange = tr;
			*/
		}

		private void TimeShiftEnd(object sender, MouseWheelEventArgs e)
		{
			/*
			if (_isZooming || SourceData.IsZoomed) // kein Time-Shifting im Zoom-Modus !
				return;

			TtpTimeRange tr = SourceData.TimeRange;

			// bei gedrückter SHIFT-Taste um Faktor 10 verstärken
			int step = ((Keyboard.GetKeyStates(Key.LeftShift) & KeyStates.Down) != 0) ? 10 : 1;

			// mindestens in Tagesschritten verändern
			if (tr.Pattern < TtpEnPattern.Pattern1Day)
				step *= TtpTime.GetPatternTicks(TtpEnPattern.Pattern1Day) / TtpTime.GetPatternTicks(tr.Pattern);

			if (e.Delta < 0)
			{
				if ((tr.GetNumPatterns() + step) <= SourceData.GetMaxTimePatterns(tr.Pattern))
					tr.Increase(step, true);
			}
			else
			{
				if ((tr.GetNumPatterns() - step) >= SourceData.GetMinTimePatterns(tr.Pattern))
					tr.Decrease(step, true);
			}

			SourceData.TimeRange = tr;
			*/
		}


		#endregion


		#region Toggle Visibility

		private void _xLeftLegend_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DoubleClickLegend(_xLeftLegend, e);
		}

		private void _xRightLegend_MouseDown(object sender, MouseButtonEventArgs e)
		{
			DoubleClickLegend(_xRightLegend, e);
		}

		private void DoubleClickLegend(object sender, MouseButtonEventArgs e)
		{
			/*
			_isDoubleClick = false;
			if (SourceData.IsZoomed || _isZooming)
				return;

			if ((e.ClickCount >= 2) && (e.LeftButton == MouseButtonState.Pressed))
			{
				_isDoubleClick = true;
				int h = DetectHittedSeries(sender, e.GetPosition((UIElement)sender));
				if (h >= 0)
				{
					SourceData.Trends.ToggleVisibilty(h);
					SourceData.OnMesDataChanged();
				}
			}
			*/
		}


		#endregion


		#region Drag and Drop

		//--------------------------------------------------------------------------------------------
		// Drop-Legende ordnet die Daten je nach Drop-Koordinaten der linken oder rechten Achse zu
		// Stammen die Daten aus der eigenen Grafik, werden sie ggf. auf der Achse verschoben,
		// Stammen die Daten aus einer anderen Darstellung, wird deren Achsinformation verworfen und
		// die Drop-Koordinaten bestimmen die neue Achszuordnung

		private void LegendDrop(object sender, DragEventArgs e)
		{
			/*
			String cmd = GetDropData(e);
			if (cmd == null)
				return;

			// feststellen, ob auf linken oder rechten Legendenbereich geklickt wurde
			TtpEnAxis targetAxis = (e.GetPosition(_drawing).X < _drawing.ActualWidth / 2) ? TtpEnAxis.Left : TtpEnAxis.Right;

			TtpCmdParserStd parser = new TtpCmdParserStd(_dragClassID);
			parser.SetCommandCode(cmd, false);
			string cmdLine = parser.GetNextCommandLine();
			while (cmdLine != null)
			{
				if (parser.ContainsKeyword(cmdLine, "#MES:") || parser.ContainsKeyword(cmdLine, "#TMES:"))
				{

					int index = -1;
					if (parser.ContainsKeyword(cmdLine, "#PID:"))
					{
						parser.GetClsId(cmdLine, ref index);   // feststellen, ob Datenquelle eigene Grafik ist
					}

					if ((index >= 0) && (SourceData.Trends.Trends[index] != null))
					{     // Quelle ist eigene Grafik -> nur verschieben zwischen den Achsen möglich
						SourceData.Trends.Trends[index].Axis = targetAxis;
					}
					else
					{  // Quelle ist nicht eigene Grafik
						TrendSeries series = new TrendSeries(parser, cmdLine);
						series.Axis = targetAxis;
						SourceData.Trends.Add(series);
					}
				}

				cmdLine = parser.GetNextCommandLine();
			}

			SourceData.OnMesDataChanged();
			*/
		}

		// Drop-Canvas übernimmt nur Daten aus anderen Darstellungen
		// Eventuelle Achszuordungen werden (anders als bei Drop-Legende) aus den übergebenen
		// Daten übernommem
		private void ChartCanvasDrop(object sender, DragEventArgs e)
		{
			/*
			_xChartGrid.Background = Brushes.Transparent;

			String cmd = GetDropData(e);
			if (cmd == null)
				return;

			TtpCmdParserStd parser = new TtpCmdParserStd(_dragClassID);
			parser.SetCommandCode(cmd, false);
			string cmdLine = parser.GetNextCommandLine();
			while (cmdLine != null)
			{
				if (parser.ContainsKeyword(cmdLine, "#MES:") || parser.ContainsKeyword(cmdLine, "#TMES:"))
				{

					int index = -1;
					if (parser.ContainsKeyword(cmdLine, "#PID:"))
					{
						parser.GetClsId(cmdLine, ref index);   // feststellen, ob Datenquelle eigene Grafik ist
					}

					if (index >= 0)
					{     // Quelle ist eigene Grafik -> nichts machen
						return;
					}
					else
					{  // Quelle ist nicht eigene Grafik
						// TrendSeries series = new TrendSeries(cmdLine);		// falsch !!! (12.11.2011)
						// einzelne Zeile reicht bei #TMES: nicht, weil Parser die Definition des temporären Objektes haben muss
						TrendSeries series = new TrendSeries(parser, cmdLine);
						SourceData.Trends.Add(series);   // geht auch mit 	SourceData.AddTrend(cmdLine); - hat dann aber mehrmalige Aktualisierung der Anzeige zur Folge
					}
				}

				cmdLine = parser.GetNextCommandLine();
			}

			SourceData.OnMesDataChanged();
			*/
		}


		#endregion


		#region Commands ContextMenu

		//---------------------------------------------------------------------------
		// Das Fenster kann nur dann auf Tastatureingaben (Shortcuts) reagieren, wenn
		// es den Tastaturfokus hat
		void VisualisationMouseEnter(object sender, MouseEventArgs e)
		{
			_xFbtn.Focus();
		}

		//protected override void PasteTrend(object sender, ExecutedRoutedEventArgs e)
		//{
		//	//SourceData.AddTrend(Clipboard.GetText());
		//}

		//protected override void ShowPropertyTrend(object sender, ExecutedRoutedEventArgs e)
		//{

		//	//TtpPresentationDlg.ShowDialog(PointToScreen(Mouse.GetPosition(this)),
		//	//										TtpPresentationDlgType.TrendProperty,
		//	//										SourceData.GetFocussedTrendCode(_dragClassID),
		//	//										new TtpInjectScriptEventHandler(SourceData.InjectCommands),
		//	//										_dragClassID, SourceData.TimeRange.Pattern);
		//}


		#endregion


	} // class

}

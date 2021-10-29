﻿using System;
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
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	/// <summary>
	/// Interaktionslogik für ViewPopDyn.xaml
	/// </summary>
	/// 
	public partial class PresAgeClasses : SwatPresentation
	{
		#region Variable

		private AgeClassAxis _ageClassAxis;

		#endregion

		#region Construction

		public PresAgeClasses(PresentationsData data, bool isPrint)
		{
			_sourceData = data;
			_isPrint = isPrint;
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

			_xTimeAxisCanvas.Width = _xTimeAxisGrid.ActualWidth;
			_xTimeAxisCanvas.Height = _xTimeAxisGrid.ActualHeight;

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
			_xTimeAxisCanvas.Children.Clear();
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

			_ageClassAxis = new AgeClassAxis(_xChartCanvas, _xTimeAxisCanvas, _xLeftAxisCanvas.Width, _xRightAxisCanvas.Width, _textColor);
			_ageClassAxis.Draw();

			SourceData.CalcAgeClassAxes();

			double zoomedScaleMax = SourceData.LeftAxisInfo.ActualScaleMax * (11 - SourceData.ZoomFactor) / 10.0;

			//Double deltaTick = (zoomedScaleMax - SourceData.LeftAxisInfo.ActualScaleMin) / 10.0;

			_chartGrid = new ChartGrid(_xChartCanvas, _backgroundColor, _textColor);

			_chartGrid.SetYScaling(TtpEnAxis.Left,SourceData.LeftAxisInfo.ActualScaleMin,zoomedScaleMax);
			_chartGrid.SetXScaling(0.0, 35.0);
			SourceData.DrawAgeClasses(_chartGrid);

		}


		#endregion


		#region Mausaktionen

		private void _xChartCanvas_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{

			//if (!_xChartCanvas.IsMouseCaptured)// Werte anzeigen ...
			//{
			//	_xChartCanvas.CaptureMouse();
			//	_isRetrieving = true;
			//	CreateInfoBox();
			//	ShowDataRetrieval(e.GetPosition(_xChartCanvas));
			//}

		}

#pragma warning disable IDE1006 // Benennungsstile
		private void _xChartCanvas_OnMouseMove(object sender, MouseEventArgs e)
#pragma warning restore IDE1006 // Benennungsstile
		{

			//if (_xChartCanvas.IsMouseCaptured)
			//{

			//	Double x = (e.GetPosition(_xChartCanvas)).X;
			//	Double y = (e.GetPosition(_xChartCanvas)).Y;
			//	if ((x < 0.0) || (y < 0.0) || (x > _xChartCanvas.Width) || (y > _xChartCanvas.Height))
			//		return;

			//	if (_isRetrieving)
			//	{
			//		ShowDataRetrieval(e.GetPosition(_xChartCanvas));
			//	}
			//}

		}

		private void _xChartCanvas_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{

			//if (!_xChartCanvas.IsMouseCaptured)
			//	return;

			//if (_isRetrieving)
			//{
			//	_isRetrieving = false;
			//	ClearDataRetrieving();
			//}
		}

		#endregion

		#region Data Retrieval


		//private void CreateInfoBox()
		//{
		//	_drVertLine = new Line
		//	{
		//		SnapsToDevicePixels = false,
		//		Stroke = Brushes.AntiqueWhite,
		//		StrokeThickness = 1,
		//		X1 = 0,
		//		X2 = 0,
		//		Y1 = 0,
		//		Y2 = _xChartCanvas.Height
		//	};

		//	_drTextPanel = new StackPanel
		//	{
		//		Orientation = Orientation.Vertical
		//	};

		//	_drTextBorder = new Border
		//	{
		//		Opacity = 0.75,
		//		CornerRadius = new CornerRadius(4),
		//		Child = _drTextPanel
		//	};

		//	_xChartCanvas.Children.Add(_drVertLine);
		//	_xChartCanvas.Children.Add(_drTextBorder);

		//}

		//private void ShowDataRetrieval(Point pt, bool fromSync = false)
		//{
		//	Brush boxBackground = Brushes.AntiqueWhite;
		//	TtpTime tm = _chartGrid.GetPointTime(pt.X, SourceData.TimeRange);

		//	Canvas.SetLeft(_drVertLine, pt.X);

		//	_drTextPanel.Children.Clear();
		//	TextBlock label = new TextBlock
		//	{
		//		Foreground = _backgroundColor,
		//		Margin = new Thickness(5, 0, 5, 0),
		//		Text = tm.ToString(TtpEnPattern.Pattern1Day),
		//		FontWeight = FontWeights.Medium
		//	};
		//	_drTextPanel.Children.Add(label);

		//	_drTextBorder.Background = boxBackground;
		//	_drTextBorder.Padding = new Thickness(5);

		//	if ((pt.X + 12 + _drTextPanel.ActualWidth) < _xChartCanvas.Width)
		//		Canvas.SetLeft(_drTextBorder, pt.X + 12);
		//	else
		//		Canvas.SetLeft(_drTextBorder, pt.X - 12 - _drTextPanel.ActualWidth);

		//	if ((pt.Y + 15 + _drTextPanel.ActualHeight) < _xChartCanvas.Height)
		//		Canvas.SetTop(_drTextBorder, pt.Y + 15);
		//	else
		//		Canvas.SetTop(_drTextBorder, pt.Y - 5 - _drTextPanel.ActualHeight);

		//	if (!fromSync)
		//		SendRetrievalToSiblings(pt);

		//}

		//private void ClearDataRetrieving(bool fromSync = false)
		//{
		//	// Fadenkreuz und Beschriftung löschen
		//	_xChartCanvas.ReleaseMouseCapture();

		//	_xChartCanvas.Children.Remove(_drTextBorder);
		//	_xChartCanvas.Children.Remove(_drVertLine);

		//	_drVertLine = null;
		//	_drTextPanel = null;
		//	_drTextBorder = null;
		//	if (!fromSync)
		//		SendRetrievalToSiblings(new Point(-1, 0));
		//}

		//private void SendRetrievalToSiblings(Point pt)
		//{
		//	PresentationEventArgs args = new PresentationEventArgs
		//	{
		//		InfoType = EventInfo.InfoPoint,
		//		InfoPoint = pt
		//	};
		//	OnPresentationChanged(this, args);
		//}

		#endregion

		#region Time shifting

		//private void SetTimeRange(TtpTimeRange newRange)
		//{
		//	SourceData.TimeRange = newRange;
		//	ShowChart();
		//}


		private void TimeShift(object sender, MouseWheelEventArgs e)
		{

			//TtpTimeRange tr = SourceData.TimeRange;
			//tr.Pattern = TtpEnPattern.Pattern1Month;
			//if (e.Delta > 0)
			//{
			//	if (tr.Start.Month > 1)
			//		tr.Scroll(-1, true);
			//}
			//else
			//{
			//	if (tr.End.Month != 1)
			//		tr.Scroll(1, true);
			//}
			//SetTimeRange(tr);

		}

		private void TimeShiftStart(object sender, MouseWheelEventArgs e)
		{
			//TtpTimeRange tr = SourceData.TimeRange;
			//tr.Pattern = TtpEnPattern.Pattern1Month;
			//if (e.Delta < 0)
			//{
			//	if (tr.GetNumPatterns() > 1)
			//		tr.Decrease(1, false);
			//}
			//else
			//{
			//	if (tr.Start.Month > 1)
			//		tr.Increase(1, false);
			//}
			//SetTimeRange(tr);

		}

		private void TimeShiftEnd(object sender, MouseWheelEventArgs e)
		{
			//TtpTimeRange tr = SourceData.TimeRange;
			//tr.Pattern = TtpEnPattern.Pattern1Month;

			//if (e.Delta < 0)
			//{
			//	if (tr.End.Month != 1)
			//		tr.Increase(1, true);
			//}
			//else
			//{
			//	if (tr.GetNumPatterns() > 1)
			//		tr.Decrease(1, true);
			//}
			//SetTimeRange(tr);
		}


		#endregion

		#region YZoom

		//private void YZoom(object sender, MouseWheelEventArgs e)
		//{

		//	if (e.Delta < 0)
		//	{
		//		if (SourceData.ZoomFactor > 1)
		//			SourceData.ZoomFactor -= 1;
		//	}
		//	else
		//	{
		//		if (SourceData.ZoomFactor < 10)
		//			SourceData.ZoomFactor += 1;
		//	}

		//	ShowChart();
		//}

		#endregion


		#region Toggle Visibility

		//private void _xLeftLegend_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//	DoubleClickLegend(_xLeftLegend, e);
		//}

		//private void _xRightLegend_MouseDown(object sender, MouseButtonEventArgs e)
		//{
		//	DoubleClickLegend(_xRightLegend, e);
		//}

		//private void DoubleClickLegend(object sender, MouseButtonEventArgs e)
		//{

		//	_isDoubleClick = false;

		//	if ((e.ClickCount >= 2) && (e.LeftButton == MouseButtonState.Pressed))
		//	{
		//		_isDoubleClick = true;
		//		int h = DetectHittedSeries(sender, e.GetPosition((UIElement)sender));
		//		if (h >= 0)
		//		{
		//			SourceData.ToggleVisibilty(h);
		//			ShowChart();
		//		}
		//	}
		//}


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
			switch (e.InfoType)
			{
				//case EventInfo.TimeRange:
				//	SourceData.TimeRange = e.PresTimeRange;
				//	ShowChart();
				//	break;

				case EventInfo.InfoPoint:
						//SourceData.TimeRange = e.PresTimeRange;
						ShowChart();

					//SyncDataRetrieval(e.InfoPoint);
					break;

				default: break;
			}
		}

		#endregion

		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			ContextMenu.DataContext = DataContext; // wichtig: Contextmenu-DC wird nicht automatisch übernommen
			ContextMenu.IsOpen = true;
		}
	} // class
}

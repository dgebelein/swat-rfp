using System;
using TTP.Engine3;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Controls;


namespace SwatPresentations
{
	public class TimeAxis
	{

		#region  Variable

		private Canvas _canvasAxis;
		private Double _leftMargin;
		private Double _rightMargin;
		private Brush _foregroundBrush;
		private TtpTimeRange _timeRange;
		private Double _scaleMin;
		private Double _scaleMax;
		private int _deltaTicks;         // Abstand der Tickmarks auf der Achse


		#endregion


		#region Kunstruktion u. Basisparameter

		public TimeAxis(Canvas canvasChart, Canvas canvasAxis, Double leftMargin, Double rightMargin, Brush textColor, TtpTimeRange timeRange)
		{
			_canvasAxis = canvasAxis;
			_leftMargin = leftMargin;
			_rightMargin = rightMargin;
			_foregroundBrush = textColor;
			_timeRange = timeRange;
			_scaleMin = 0;
			_scaleMax = _timeRange.GetNumPatterns()-1;  

		}

		#endregion


		#region Properties


		public Double ScaleMin
		{
			get { return _scaleMin; }
			set { _scaleMin = value; }
		}


		public Double ScaleMax
		{
			get { return _scaleMax; }
			set { _scaleMax = value; }
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


			Rectangle rect = new Rectangle
			{
				Width = _canvasAxis.Width,
				Height = _canvasAxis.Height,
				Stroke = Brushes.Transparent,
				Fill = Brushes.Transparent
			};
			_canvasAxis.Children.Add(rect);
			DrawDateLabelAxis();
		}


		private void DrawTimeRange()
		{
			// bei Darstellungsrastern < 1 Tag ist der Zeitraum immer um ein Raster erweitert
			//TtpTimeRange tr = _timeRange;
			//if (_timeRange.Pattern < TtpEnPattern.Pattern1Day)
			//	tr.Decrease(1, true);

			TextBlock tb = new TextBlock();
			Double fs = TextBlock.GetFontSize(new TextBlock());
			tb.FontSize = fs * 1.2;
			tb.Text = "Zeitraum: " + _timeRange.ToConsumerString();// + " / " + TtpTime.GetLongPatternString(tr.Pattern);
			tb.Foreground = _foregroundBrush; ;

			tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
			Canvas.SetTop(tb, NormalizeY(30 + tb.DesiredSize.Height / 2));
			Canvas.SetLeft(tb, _canvasAxis.Width / 2 - tb.DesiredSize.Width / 2);
			_canvasAxis.Children.Add(tb);

		}

		private void DrawDateLabelAxis()
		{
			string timeFormat = "dd.MM.yy";

			
			//labelTime.Inc(_timeRange.Pattern, _minTick);

			TtpTimeRange labelRange = new TtpTimeRange(_timeRange.Start, _timeRange.End, TtpEnPattern.Pattern1Month);
			bool bSmall = (labelRange.GetNumPatterns() < 3);
			_deltaTicks = (bSmall) ? 10 : 30;

			int lastTick = _timeRange.GetNumPatterns()-1;
			int xTickPos = 0;
			TtpTime labelTime = _timeRange.Start;
			while (xTickPos <= lastTick)
			{
				//Line tickLine = new Line
				//{
				//	Stroke = _foregroundBrush,
				//	X1 = NormalizeX(xTickPos),
				//	Y1 = NormalizeY(0),
				//	X2 = NormalizeX(xTickPos),
				//	Y2 = NormalizeY(5)
				//};
				//_canvasAxis.Children.Add(tickLine);

				TextBlock tb = new TextBlock
				{
					Foreground = _foregroundBrush,
					Text = labelTime.ToString(timeFormat)
				};

				tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
				Canvas.SetTop(tb, NormalizeY(2.5 + tb.DesiredSize.Height / 2));
				Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);

				_canvasAxis.Children.Add(tb);
				if (bSmall)
				{
					xTickPos += _deltaTicks;
					labelTime.Inc(_timeRange.Pattern, _deltaTicks);
				}
				else
				{
					TtpTime oldTime = labelTime;
					labelTime.Inc(TtpEnPattern.Pattern1Month, 1);
					xTickPos += labelTime.GetDifferenceTo(oldTime, TtpEnPattern.Pattern1Day);

				}
			}

			DrawTimeRange();
		}

		//private void DrawProjectionTimeRangeLabel()
		//{
		//	Double fs = TextBlock.GetFontSize(new TextBlock());
		//	TextBlock tbd = new TextBlock();
		//	tbd.FontSize = fs * 1.0;
		//	tbd.Foreground = _foregroundBrush;

		//	switch (_projectionPattern)
		//	{
		//		case TtpEnPattern.Pattern1Year: tbd.Text = "Projektion auf Jahr / "; break;
		//		case TtpEnPattern.Pattern1Month: tbd.Text = "Projektion auf Monat / "; break;
		//		case TtpEnPattern.Pattern1Week: tbd.Text = "Projektion auf Woche / "; break;
		//		case TtpEnPattern.Pattern1Day: tbd.Text = "Projektion auf Tag / "; break;
		//		default: tbd.Text = ""; break;
		//	}
		//	tbd.Text += "Zeitraum: " + _projectionTimeRange.ToConsumerString() + " / " + TtpTime.GetLongPatternString(_projectionTimeRange.Pattern);

		//	tbd.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//	Canvas.SetTop(tbd, NormalizeY(25 + tbd.DesiredSize.Height / 2));
		//	Canvas.SetLeft(tbd, _canvasAxis.Width / 2 - tbd.DesiredSize.Width / 2);
		//	_canvasAxis.Children.Add(tbd);
		//}


		//private void DrawDateLabelAxis()
		//{
		//	string timeFormat= "dd.MM.yy"; 

		//	TtpTime labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);

		//	//if (_timeRange.GetNumPatterns() <= 62)
		//	//{
		//	//	_deltaTicks = 10;
		//	//}
		//	//else _deltaTicks = 30;

		//	int xTickPos = _minTick;
		//	while (xTickPos <= _maxTick)
		//	{
		//		Line tickLine = new Line
		//		{
		//			Stroke = _foregroundBrush,
		//			X1 = NormalizeX(xTickPos),
		//			Y1 = NormalizeY(0),
		//			X2 = NormalizeX(xTickPos),
		//			Y2 = NormalizeY(5)
		//		};
		//		_canvasAxis.Children.Add(tickLine);

		//		TextBlock tb = new TextBlock
		//		{
		//			Foreground = _foregroundBrush,
		//			Text = labelTime.ToString(timeFormat)
		//		};

		//		tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//		Canvas.SetTop(tb, NormalizeY(5 + tb.DesiredSize.Height / 2));
		//		Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);

		//		_canvasAxis.Children.Add(tb);

		//		xTickPos += _deltaTicks;
		//		labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//	}

		//	DrawTimeRange();
		//}


		//private void DrawDateTimeLabelAxis()
		//{
		//	TtpTime labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);
		//	int xTickPos = _minTick;
		//	bool hasSingleDateFormate = false;

		//	Double fs = TextBlock.GetFontSize(new TextBlock());   // Standardschriftgröße ermitteln

		//	// Uhrzeit	
		//	while (xTickPos <= _maxTick)
		//	{

		//		// Ticks			
		//		Line tickLine = new Line();
		//		tickLine.Stroke = _foregroundBrush;
		//		tickLine.X1 = NormalizeX(xTickPos);
		//		tickLine.Y1 = NormalizeY(0);
		//		tickLine.X2 = NormalizeX(xTickPos);
		//		tickLine.Y2 = NormalizeY(5);
		//		_canvasAxis.Children.Add(tickLine);

		//		// wenn ein Tick genau auf 12:00 fällt, anschließend nur diesen Tick mit Datum beschriften
		//		if ((labelTime.Hour == 12) && (labelTime.Minute == 0) && !_isZoomMode)
		//			hasSingleDateFormate = true;

		//		// Uhrzeit
		//		TextBlock tb = new TextBlock();
		//		tb.FontSize = fs * 0.9;
		//		tb.Text = labelTime.ToString("HH:mm");

		//		tb.Foreground = _foregroundBrush;
		//		tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//		Canvas.SetTop(tb, NormalizeY(tb.DesiredSize.Height / 2));
		//		Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);
		//		_canvasAxis.Children.Add(tb);

		//		xTickPos += _deltaTicks;
		//		labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//	}

		//	// Datum
		//	labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);
		//	xTickPos = _minTick;
		//	while (xTickPos <= _maxTick)
		//	{
		//		// ggf nur die 12:00 Ticks beschriften
		//		if (!hasSingleDateFormate || ((labelTime.Hour == 12) && (labelTime.Minute == 0)))
		//		{
		//			TextBlock tbd = new TextBlock();
		//			tbd.FontSize = fs * 1.1;
		//			tbd.Foreground = _foregroundBrush;
		//			tbd.Text = labelTime.ToString("dd.MM.yy");
		//			tbd.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//			Canvas.SetTop(tbd, NormalizeY(16 + tbd.DesiredSize.Height / 2));
		//			Canvas.SetLeft(tbd, NormalizeX(xTickPos) - tbd.DesiredSize.Width / 2);
		//			_canvasAxis.Children.Add(tbd);
		//		}
		//		xTickPos += _deltaTicks;
		//		labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//	}

		//	DrawTimeRange();

		//}


		//private void DrawProjectionYearAxis()
		//{
		//	string timeFormat = (_timeRange.Pattern == TtpEnPattern.Pattern1Month) ? "MMM" : "dd.MM";
		//	TtpTime labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);

		//	int xTickPos = _minTick;
		//	while (xTickPos <= _maxTick)
		//	{
		//		Line tickLine = new Line();
		//		tickLine.Stroke = _foregroundBrush;

		//		tickLine.X1 = NormalizeX(xTickPos);
		//		tickLine.Y1 = NormalizeY(0);
		//		tickLine.X2 = NormalizeX(xTickPos);
		//		tickLine.Y2 = NormalizeY(5);
		//		_canvasAxis.Children.Add(tickLine);

		//		TextBlock tb = new TextBlock();
		//		tb.Foreground = _foregroundBrush;
		//		tb.Text = labelTime.ToString(timeFormat);

		//		tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//		Canvas.SetTop(tb, NormalizeY(5 + tb.DesiredSize.Height / 2));
		//		Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);

		//		_canvasAxis.Children.Add(tb);

		//		xTickPos += _deltaTicks;
		//		labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//	}

		//	DrawProjectionTimeRangeLabel();
		//}


		//private void DrawProjectionMonthAxis()
		//{
		//	TtpTime labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);
		//	int xTickPos = _minTick;
		//	Double offsetDayLabel = 0;

		//	Double fs = TextBlock.GetFontSize(new TextBlock());   // Standardschriftgröße ermitteln

		//	while (xTickPos <= _maxTick)
		//	{

		//		// Ticks			
		//		Line tickLine = new Line();
		//		tickLine.Stroke = _foregroundBrush;
		//		tickLine.X1 = NormalizeX(xTickPos);
		//		tickLine.Y1 = NormalizeY(0);
		//		tickLine.X2 = NormalizeX(xTickPos);
		//		tickLine.Y2 = NormalizeY(5);
		//		_canvasAxis.Children.Add(tickLine);


		//		// Uhrzeit
		//		if (_isZoomMode || (_projectionTimeRange.Pattern < TtpEnPattern.Pattern1Day))
		//		{
		//			TextBlock tb = new TextBlock();
		//			tb.FontSize = fs * 0.8;
		//			tb.Text = labelTime.ToString("HH:mm");
		//			tb.Foreground = _foregroundBrush;
		//			tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//			Canvas.SetTop(tb, NormalizeY(tb.DesiredSize.Height / 2));
		//			Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);
		//			_canvasAxis.Children.Add(tb);
		//			offsetDayLabel = 11;
		//		}

		//		// Tag
		//		TextBlock tbd = new TextBlock();
		//		tbd.FontSize = fs;
		//		tbd.Foreground = _foregroundBrush;
		//		tbd.Text = labelTime.ToString("dd");
		//		tbd.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//		Canvas.SetTop(tbd, NormalizeY(offsetDayLabel + tbd.DesiredSize.Height / 2));
		//		Canvas.SetLeft(tbd, NormalizeX(xTickPos) - tbd.DesiredSize.Width / 2);
		//		_canvasAxis.Children.Add(tbd);



		//		xTickPos += _deltaTicks;
		//		labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//	}

		//	DrawProjectionTimeRangeLabel();

		//}

		//private void DrawProjectionWeekAxis()
		//{
		//	TtpTime labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);
		//	int xTickPos = _minTick;
		//	Double offsetDayLabel = 0;

		//	Double fs = TextBlock.GetFontSize(new TextBlock());   // Standardschriftgröße ermitteln

		//	if (_isZoomMode)
		//	{
		//		while (xTickPos <= _maxTick)
		//		{

		//			// Ticks			
		//			Line tickLine = new Line();
		//			tickLine.Stroke = _foregroundBrush;
		//			tickLine.X1 = NormalizeX(xTickPos);
		//			tickLine.Y1 = NormalizeY(0);
		//			tickLine.X2 = NormalizeX(xTickPos);
		//			tickLine.Y2 = NormalizeY(5);
		//			_canvasAxis.Children.Add(tickLine);

		//			// Uhrzeit
		//			if (_projectionTimeRange.Pattern < TtpEnPattern.Pattern1Day)
		//			{
		//				TextBlock tb = new TextBlock();
		//				tb.FontSize = fs * 0.8;
		//				tb.Text = labelTime.ToString("HH:mm");
		//				tb.Foreground = _foregroundBrush;
		//				tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//				Canvas.SetTop(tb, NormalizeY(tb.DesiredSize.Height / 2));
		//				Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);
		//				_canvasAxis.Children.Add(tb);
		//				offsetDayLabel = 11;
		//			}

		//			// Tag
		//			TextBlock tbd = new TextBlock();
		//			tbd.FontSize = fs;
		//			tbd.Foreground = _foregroundBrush;
		//			tbd.Text = labelTime.ToString("ddd");
		//			tbd.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//			Canvas.SetTop(tbd, NormalizeY(offsetDayLabel + tbd.DesiredSize.Height / 2));
		//			Canvas.SetLeft(tbd, NormalizeX(xTickPos) - tbd.DesiredSize.Width / 2);
		//			_canvasAxis.Children.Add(tbd);

		//			xTickPos += _deltaTicks;
		//			labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//		}
		//	}
		//	else
		//	{
		//		_deltaTicks = TtpTime.GetPatternTicks(TtpEnPattern.Pattern1Day) / TtpTime.GetPatternTicks(_timeRange.Pattern);
		//		while (xTickPos <= _maxTick)
		//		{

		//			// Ticks			
		//			Line tickLine = new Line();
		//			tickLine.Stroke = _foregroundBrush;
		//			tickLine.X1 = NormalizeX(xTickPos);
		//			tickLine.Y1 = NormalizeY(0);
		//			tickLine.X2 = NormalizeX(xTickPos);
		//			tickLine.Y2 = NormalizeY(5);
		//			_canvasAxis.Children.Add(tickLine);

		//			// Tag
		//			TextBlock tbd = new TextBlock();
		//			tbd.FontSize = fs;
		//			tbd.Foreground = _foregroundBrush;
		//			tbd.Text = labelTime.ToString("ddd");
		//			tbd.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//			Canvas.SetTop(tbd, NormalizeY(offsetDayLabel + tbd.DesiredSize.Height / 2));
		//			Canvas.SetLeft(tbd, NormalizeX(xTickPos + _deltaTicks / 2) - tbd.DesiredSize.Width / 2);
		//			_canvasAxis.Children.Add(tbd);

		//			xTickPos += _deltaTicks;
		//			labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//		}
		//	}

		//	DrawProjectionTimeRangeLabel();

		//}


		//private void DrawProjectionDayAxis()
		//{
		//	TtpTime labelTime = _timeRange.Start;
		//	labelTime.Inc(_timeRange.Pattern, _minTick);
		//	int xTickPos = _minTick;

		//	Double fs = TextBlock.GetFontSize(new TextBlock());   // Standardschriftgröße ermitteln

		//	while (xTickPos <= _maxTick)
		//	{

		//		// Ticks			
		//		Line tickLine = new Line();
		//		tickLine.Stroke = _foregroundBrush;
		//		tickLine.X1 = NormalizeX(xTickPos);
		//		tickLine.Y1 = NormalizeY(0);
		//		tickLine.X2 = NormalizeX(xTickPos);
		//		tickLine.Y2 = NormalizeY(5);
		//		_canvasAxis.Children.Add(tickLine);

		//		// Uhrzeit
		//		TextBlock tb = new TextBlock();
		//		tb.FontSize = fs * 1;
		//		tb.Text = labelTime.ToString("HH:mm");
		//		tb.Foreground = _foregroundBrush;
		//		tb.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
		//		Canvas.SetTop(tb, NormalizeY(tb.DesiredSize.Height / 2));
		//		Canvas.SetLeft(tb, NormalizeX(xTickPos) - tb.DesiredSize.Width / 2);
		//		_canvasAxis.Children.Add(tb);

		//		xTickPos += _deltaTicks;
		//		labelTime.Inc(_timeRange.Pattern, _deltaTicks);
		//	}

		//	DrawProjectionTimeRangeLabel();

		//}

		#endregion


	}
}

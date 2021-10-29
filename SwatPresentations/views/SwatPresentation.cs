using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Printing;
using System.Windows.Markup;
using System.IO;
using System.Xml;

namespace SwatPresentations
{
	public class SwatPresentation : UserControl
	{

		#region Variables

		//SwatPresentation _view;
		protected  PresentationsData _sourceData;
		protected bool _isPrint;

		protected Brush _backgroundColor;      // Hintergrundfarbe (wird beim Drucken geändert) 
		protected Brush _textColor;            // Farbe für Achsen, Legende, Schrift... (wird beim Drucken geändert)

		// data-retrieval
		protected Ellipse _drCrossPoint = null;
		protected int _drSeriesNo = -1;
		protected Line _drHorLine = null;
		protected Line _drVertLine = null;
		protected Border _drTextBorder = null;
		protected StackPanel _drTextPanel = null;
		protected bool _isRetrieving;

		// HitTesting
		protected int _hittedSeries;
		protected bool _isHitLegend;
		protected bool _isDoubleClick;

		protected int _zoomMult = 15; // zoom über Achse u. Mausrad

		// zooming 
		protected Point _zoomStartPoint = new Point();
		protected Point _zoomEndPoint = new Point();
		protected Shape _zoomRubberBand = null;
		protected bool _isZooming;

		// Zeichnungselemente
		protected VerticalAxis _vlAxis;
		protected VerticalAxis _vrAxis;
		protected HorizontalAxis _xAxis;
		protected ChartGrid _chartGrid;

		// Event
		public event EventHandler<PresentationEventArgs> PresentationSendsEvent;

		#endregion

		#region Properties

		public PresentationsData SourceData
		{
			get { return _sourceData; }
		}


		#endregion


		#region Drawing

		public virtual void ShowChart()
		{ }

		protected void YZoom(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
			{
				if (SourceData.ZoomFactor > -_zoomMult)
					SourceData.ZoomFactor -= 1;
			}
			else
			{
				if (SourceData.ZoomFactor < _zoomMult - 1)
					SourceData.ZoomFactor += 1;
			}

			ShowChart();
		}

		protected double CalcZoom()
		{
			if (SourceData.ZoomFactor >= 0)
				return (double)(_zoomMult - SourceData.ZoomFactor) / _zoomMult;
			else
				return -SourceData.ZoomFactor;
		}

		#endregion


		#region Dialoge

		protected void ShowTitleDialog(object sender, MouseButtonEventArgs e)
		{
			//TtpPresentationDlg.ShowDialog(PointToScreen(e.GetPosition(this)),
			//										TtpPresentationDlgType.Title,
			//										SourceData.ToString(),
			//										new TtpInjectScriptEventHandler(SourceData.InjectCommands),
			//										null, null);
		}

		#endregion


		#region Hit Testing


		protected int DetectHittedSeries(object sender, Point mousePos)
		{
			_hittedSeries = -1;
			VisualTreeHelper.HitTest((UIElement)sender,
										null,
										new HitTestResultCallback(DetectHittedSeriesCallback),
										new PointHitTestParameters(mousePos));
			return (_hittedSeries);
		}


		protected HitTestResultBehavior DetectHittedSeriesCallback(HitTestResult result)
		{
			// jeder Legendeneintrag ist von einem Rahmen eingeschlossen, der den Namen "LegendItem_x" trägt
			// das "x" ist der Index des Legendeneintrags

			//	if (result.VisualHit.GetType() == typeof(Border)) {
			if (result.VisualHit.GetType() == typeof(LegendElement))
			{
				_hittedSeries = -1;
				String name = ((Border)result.VisualHit).Name;
				if (!String.IsNullOrEmpty(name))
				{
					int pos = name.IndexOf('_');
					if (pos >= 0)
					{
						if (!int.TryParse(name.Substring(pos + 1), out _hittedSeries))
						{
							_hittedSeries = -1;
						}
					}
				}
				return HitTestResultBehavior.Stop;
			}

			return HitTestResultBehavior.Continue;
		}



		#endregion

		#region Zoom

		//Achtung: beide nachfolgende static-Blöcke müssen in Basisklasse, sonst kommt Fehlermeldung wegen doppelt registrierter DP
		//----------------------------------------------------------------------------------------------------
		// wird gebraucht für Cursor-Styles im XAML-File
		//public static DependencyProperty IsZoomedProperty = DependencyProperty.Register(
		//						"IsZoomed",
		//						typeof(bool), typeof(SwatPresentation),
		//						new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnPropertyChanged)));

		////------------------------------------------------------------------------------------------------

		//private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		//{
		//	SwatPresentation graphics = sender as SwatPresentation;
		//	if (e.Property == IsZoomedProperty)
		//		graphics.IsZoomed = (bool)e.NewValue;
		//}

		////--------------------------------------------------------------------------------------------

		//public override  bool IsZoomed
		//{
		//	get { return (bool)GetValue(IsZoomedProperty); }
		//	set
		//	{
		//		if ((bool)GetValue(IsZoomedProperty) != value)
		//		{
		//			SetValue(IsZoomedProperty, value);
		//			_sourceData.IsZoomed = value;
		//			ShowChart();
		//		}
		//	}
		//}

		//--------------------------------------------------------------------------------------------

		public  bool IsZoomable
		{
			get { return true; }
		}

		#endregion


		#region Event für Synchronisation

		protected virtual void OnPresentationChanged(object source, PresentationEventArgs e)
		{
			EventHandler<PresentationEventArgs> handler = PresentationSendsEvent;
			if (handler != null)
			{
				handler(this, e);
			}
		}

		public virtual void RespondToPresentationEvent(object Sender, PresentationEventArgs e)
		{

		}

		public void ClearEventRoutings()
		{
			PresentationSendsEvent = null;
		}

		#endregion


		#region Print


		public Brush TextColor
		{
			get { return _textColor; }
			set { _textColor = value; }
		}


		public Brush BackgroundColor
		{
			get { return _backgroundColor; }
			set { _backgroundColor = value; }
		}

		//protected void SwitchToPrintColors()
		//{
		//	TextColor = Brushes.Black;
		//	Foreground = Brushes.Black;
		//	BackgroundColor = Brushes.White;
		//}

		protected virtual bool OnPrint()
		{
			return true;
		}

		public void PrintView()
		{
			if (!OnPrint())   // Abfangen des 'Drucken'-Befehls (mit Sonderbehandlung) ermöglichen
				return;

			PrintDialog dialog = new PrintDialog();
			if (dialog.ShowDialog() == true)
			{
				PrintTicket pt = dialog.PrintTicket;
				pt.PageOrientation = PageOrientation.Landscape;

				// Seitenränder
				double xMargin = 0.75 * 96;
				double yMargin = 0.75 * 96;
				Double printableWidth = pt.PageMediaSize.Height.Value - 2 * xMargin; ;
				Double printableHeight = pt.PageMediaSize.Width.Value - 2 * yMargin;

				Size sz = new Size(printableWidth, printableHeight);
				Measure(sz);
				Arrange(new Rect(new Point(xMargin, yMargin), sz));
				UpdateLayout();

				ContainerVisual vis = new ContainerVisual();
				vis.Children.Add(this);
				vis.Transform = new MatrixTransform(1, 0, 0, 1, xMargin, yMargin);

				try
				{ 
					if (String.IsNullOrWhiteSpace(_sourceData.Title))
						dialog.PrintVisual(vis, "Untitled Visualisation");
					else
						dialog.PrintVisual(vis, _sourceData.Title.Replace(' ', '_'));
				}
				catch (Exception e)
				{
					DlgMessage.Show("Fehler beim Drucken: ", e.Message, SwatPresentations.MessageLevel.Error);
				}
			}

		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SwatPresentations
{
	public class SwatPresViewBase : UserControl
	{

		#region Variables

		protected Brush _backgroundColor;      // Hintergrundfarbe;
		protected Brush _textColor;            // Farbe für Achsen, Legende, Schrift... 

		// zooming 
		protected Point _zoomStartPoint = new Point();
		protected Point _zoomEndPoint = new Point();
		protected Shape _zoomRubberBand = null;
		protected bool _isZooming;
		protected bool _isRightZooming;

		// data-retrieval
		protected Ellipse _drCrossPoint = null;
		protected int _drSeriesNo = -1;
		protected Line _drHorLine = null;
		protected Line _drVertLine = null;
		protected Border _drTextBorder = null;
		protected StackPanel _drTextPanel = null;
		protected int _swarmIndex = -1;
		protected bool _isRetrieving;

		// Clone-Shifting
		protected int _csSeriesNo = -1;
		protected bool _isCloneShifting;
		protected Border _csTextBorder = null;
		protected StackPanel _csTextPanel = null;
		protected Point _csStartPoint = new Point();
		protected Path _csTrend = null;

		// HitTesting
		protected int _hittedSeries;

		// Drag and Drop
		protected Point _dragStartPoint;
		protected bool _isDraggingLeftLegend;

		//protected LegendElement _dragLegend;
		protected bool _isDragging;
		protected bool _isHitLegend;
		protected bool _isDoubleClick;

		// Zeichnungselemente

		protected VerticalAxis _vlAxis;
		protected VerticalAxis _vrAxis;
		protected ChartGrid _chartGrid;

		#endregion
		protected void ShowTitleDialog(object sender, MouseButtonEventArgs e)
		{
			//if (IsZoomed) // hier später Meldung anzeigen, dass im Zoom-Modus keine Veränderungen möglich sind
			//	return;

			//TtpPresentationDlg.ShowDialog(PointToScreen(e.GetPosition(this)),
			//										TtpPresentationDlgType.Title,
			//										SourceData.ToString(),
			//										new TtpInjectScriptEventHandler(SourceData.InjectCommands),
			//										null, null);
		}





	}
}

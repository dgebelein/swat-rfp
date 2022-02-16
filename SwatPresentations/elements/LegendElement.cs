using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	public class LegendElement :  Border
	{
		//PresentationsData _presData;
		private PresentationRow _row;
		private Brush _textColor;
		private Brush _textColorGrayed;
		private bool _isVisible;
		private Brush _color;
		private TtpEnLineType _linetype;


		protected LegendElement(Brush textColor)
			//: base(elementCode)
		{
			TextColor = textColor;
		}


		private Brush TextColor
		{
			get { return _textColor; }
			set
			{
				_textColor = value;
				SolidColorBrush b = _textColor as SolidColorBrush;
				_textColorGrayed = (b == null) ? Brushes.Gray : new SolidColorBrush(Color.Multiply(b.Color, (float)0.5));
			}
		}


		private void DrawLegendCircle(Canvas canvas, Brush legendColor)
		{
			Ellipse circle = new Ellipse
			{
				Width = 8,
				Height = 8
			};
			Canvas.SetTop(circle, canvas.Height / 2 - 4);
			Canvas.SetLeft(circle, canvas.Width / 2 - 4);
			circle.Fill = legendColor;
			canvas.Children.Add(circle);
		}


		private void DrawLegendLine(Canvas canvas, Brush legendColor)
		{
			Line line = new Line
			{
				X1 = 0,
				Y1 = 0,
				X2 = 30,
				Y2 = 0,
				Stroke = legendColor,
				StrokeThickness = 2
			};
			Canvas.SetTop(line, canvas.Height / 2);
			Canvas.SetLeft(line, canvas.Width / 2 - 15);
			canvas.Children.Add(line);
		}


		private void DrawLegendBox(Canvas canvas, Brush legendColor)
		{
			Brush fillBrush;

			if (legendColor is SolidColorBrush)
			{
				SolidColorBrush fillColor = (SolidColorBrush)(legendColor).CloneCurrentValue();
				fillColor.Opacity = 0.5;
				fillBrush = fillColor;
			}
			else
			{
				fillBrush = legendColor;
			}

			Rectangle rect = new Rectangle
			{
				Width = 10,
				Height = 10,
				Stroke = legendColor,
				StrokeThickness = 0.5,
				Fill = fillBrush
			};

			Canvas.SetTop(rect, canvas.Height / 2 - 5);
			Canvas.SetLeft(rect, canvas.Width / 2 - 5);
			canvas.Children.Add(rect);
		}

		private void DrawLegendArea(Canvas canvas, Brush legendColor)
		{
			Brush fillBrush;

			if (legendColor is SolidColorBrush)
			{
				SolidColorBrush fillColor = (SolidColorBrush)(legendColor).CloneCurrentValue();
				fillColor.Opacity = 0.25;
				fillBrush = fillColor;
			}
			else
			{
				fillBrush = legendColor;
			}

			Rectangle rect = new Rectangle
			{
				Width = 10,
				Height = 10,
				Stroke = legendColor,
				StrokeThickness = 1,
				Fill = fillBrush
			};

			Canvas.SetTop(rect, canvas.Height / 2 - 5);
			Canvas.SetLeft(rect, canvas.Width / 2 - 5);
			canvas.Children.Add(rect);
		}


		private Canvas CreateLegendSymbol(Double width, Double height)
		{
			Canvas canvas = new Canvas
			{
				Width = width,
				Height = height
			};

			//Brush legendColor = (_row.IsVisible) ?_row.Color : _textColorGrayed;
			Brush legendColor = (_isVisible) ? _color : _textColorGrayed;

			//switch (_row.LineType)
			switch (_linetype)
			{
				case TtpEnLineType.AreaAbove:
				case TtpEnLineType.AreaBelow:
				case TtpEnLineType.AreaDiff:
				case TtpEnLineType.Diff:
				case TtpEnLineType.Limit:
				case TtpEnLineType.Chart:
					DrawLegendBox(canvas, legendColor);
					break;

				case TtpEnLineType.LinePoint:
						DrawLegendLine(canvas, legendColor);
						DrawLegendCircle(canvas, legendColor);
						break;

				case TtpEnLineType.Point:
					DrawLegendCircle(canvas, legendColor);
					break;

				default:
					DrawLegendLine(canvas, legendColor);
					break;
			}
			

			return canvas;
		}

		//-------------------------------------------------------------------------------------------

		protected override void OnContextMenuOpening(ContextMenuEventArgs e)
		{
			//_trends.SetFocusTo(_trend);
		}



		public static LegendElement Create( PresentationRow row,
														Brush textColor, 
														Double widthSymbolColumn, Double widthTextColumn)
		{
			LegendElement le = new LegendElement(textColor)
			{
				_row = row,
				Background = Brushes.Transparent,
				Width = widthSymbolColumn + widthTextColumn,
				Name = "LegendItem_" + row.LegendIndex
			};
			le._color = row.Color;
			le._isVisible = row.IsVisible;
			le._linetype = row.LineType;
			ToolTipService.SetInitialShowDelay(le, 350);
			ToolTipService.SetShowDuration(le, 3000);
			le.ToolTip = string.IsNullOrEmpty(row.LegendTooltip) ? "Doppelklick schaltet Sichtbarkeit um" : row.LegendTooltip;

			TextBlock tb = new TextBlock
			{
				Width = widthTextColumn - 10,
				Text = le._row.Legend,
				Foreground = le._row.IsVisible ? le._textColor : le._textColorGrayed,
				Margin = new Thickness(0, 0, 10, 0)
			};

			StackPanel panel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(0, 5, 0, 0)
			};

			switch (row.Axis)
			{
				case TtpEnAxis.Left:
					tb.TextAlignment = TextAlignment.Left;
					tb.Margin = new Thickness(0, 0, 10, 0);
					panel.Children.Add(le.CreateLegendSymbol(widthSymbolColumn, tb.ActualHeight));
					panel.Children.Add(tb);
					break;
				case TtpEnAxis.Right:
					tb.TextAlignment = TextAlignment.Right;
					tb.Margin = new Thickness(10, 0, 0, 0);
					panel.Children.Add(tb);
					panel.Children.Add(le.CreateLegendSymbol(widthSymbolColumn, tb.ActualHeight));
					break;
				case TtpEnAxis.X:
					tb.TextAlignment = TextAlignment.Center;
					panel.Children.Add(tb);
					break;
			}

			le.Child = panel;
			return le;
		}
	}

}


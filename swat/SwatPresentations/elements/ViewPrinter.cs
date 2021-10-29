using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace SwatPresentations
{
	static class ViewPrinter
	{
		public static void Print( UserControl view, PrintTicket pt)
		{
			//PrintTicket pt = dialog.PrintTicket;
			//pt.PageOrientation = PageOrientation.Landscape;

			//SwitchToPrintColors();

			// Set the margins.
			double xMargin = 0.75 * 96;
			double yMargin = 0.75 * 96;
			Double printableWidth = pt.PageMediaSize.Height.Value - 2 * xMargin; 
			Double printableHeight = pt.PageMediaSize.Width.Value - 2 * yMargin;

			Size sz = new Size(printableWidth, printableHeight);
			view.Measure(sz);
			view.Arrange(new Rect(new Point(xMargin, yMargin), sz));
			view.UpdateLayout();




			string gridXaml = XamlWriter.Save(view);
			//StringReader stringReader = new StringReader(gridXaml);
			//XmlReader xmlReader = XmlReader.Create(stringReader);
			//UserControl newGrid = (UserControl)XamlReader.Load(xmlReader);

			ContainerVisual root = new ContainerVisual();
			root.Children.Add(view);//?
			root.Transform = new MatrixTransform(1, 0, 0, 1, xMargin, yMargin);

			//if (String.IsNullOrWhiteSpace(_sourceData.Title))
			//	dialog.PrintVisual(root, "Untitled Visualisation");
			//else
			//	dialog.PrintVisual(root, _sourceData.Title.Replace(' ', '_'));


		}
	}
}

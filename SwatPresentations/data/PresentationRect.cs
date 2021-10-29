using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TTP.Engine3;
using TTP.TtpCommand3;

namespace SwatPresentations
{
	public class PresentationRect
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Width { get; set; }
		public double Height { get; set; }
		public Brush FrameColor { get; set; }
		public double ZValue { get; set; }
	}
}

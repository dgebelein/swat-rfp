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
	public class PresentationRow
	{
		public string Legend { get; set; }
		public bool IsVisible { get; set; }
		public Brush Color { get; set; }
		public double Thicknes { get; set; }
		public TtpEnAxis Axis { get; set; }
		public TtpEnLineType LineType { get; set; }
		public int LegendIndex { get; set; }
		public int Generation { get; set; }
		public int StartIndex { get; set; }
		public double[] Values { get; set; }
	}
}

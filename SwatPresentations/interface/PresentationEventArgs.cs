using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using TTP.Engine3;

namespace SwatPresentations
{
	public enum EventInfo
	{
		TimeRange,
		InfoPoint
	}
	public enum ScaleDlgResult
	{
		Escape,
		Auto,
		Fixed
	}

	public class PresentationEventArgs : EventArgs
	{
		public EventInfo InfoType { get; set; }
		public TtpTimeRange PresTimeRange{ get; set; }
		public Point  InfoPoint { get; set; }
		public TtpTime DisplayTime { get; set; }
	}
}

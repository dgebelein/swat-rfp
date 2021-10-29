using SwatPresentations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.Optimizer
{
	public class OptEventArgs: EventArgs
	{ 
		public PresentationsData PresData { get; set; }
	}
}

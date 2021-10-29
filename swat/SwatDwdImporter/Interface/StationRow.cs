using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SwatImporter
{
	public class StationRow
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Land { get; set; }
		public double NN { get; set; }
		public double GeoLat { get; set; }
		public double GeoLong { get; set; }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace SwatImporter
{
	internal class DwdColumnDeclarator
	{
		#region variable
		public int ColIndex;
		public bool IsDateTime;
		public bool IsValue;
		public byte RecType;
		public int MesIndex;
		public string MesText;
		public string MesDim;
		public string DwdHeader;
		public TtpEnQueryCalcType DefCalc;
		public double Multiplicator;

		#endregion

		#region Constructor
		public DwdColumnDeclarator(int colIndex, bool isDateTime, bool isValue, byte recType, int mesIndex, string mesText, string mesDim, string dwdHeader, TtpEnQueryCalcType defCalc, double mult)
		{
			ColIndex = colIndex;
			IsDateTime = isDateTime;
			IsValue = isValue;
			RecType = recType;
			MesIndex = mesIndex;
			MesText = mesText;
			MesDim = mesDim;
			DwdHeader = dwdHeader;
			DefCalc = defCalc;
			Multiplicator = mult;
		}
		#endregion
	}
}

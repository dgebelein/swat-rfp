using swatSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwopReview
{
	public class SwopSet
	{

		#region variable
		public int SetIndex { get; private set; }
		public string Weather { get; set; }
		public string Monitoring { get; set; }
		public string EvalTime { get; set; }
		public double Weight { get; set; }
		public List<string> LocalParams { get; set; }
		public double[] ErrValues { get; private set; }
		public double BestErrValue { get; set; }
		public double StartErrValue { get; set; }


		#endregion

		#region Construction
		public SwopSet(int setNo)
		{
			SetIndex = setNo;
		}

		public void InitErrValues(int nStep)
		{
			ErrValues = new double[nStep];
		}

		#endregion

		public void SetErrValue(int index, double errValue)
		{
			if (index < ErrValues.Length)
				ErrValues[index] = errValue;
		}
	}
}

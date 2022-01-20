using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace swatSim
{
	public enum EvalMethod
	{
		Nothing,
		AbsDiff,
		Relation
	}

	public class Evaluator
	{
		public static double GetResidual(double[] progn, double[] monitoring, EvalMethod evalMethod, int firstIndex, int lastIndex)
		{
			double result = 0.0;
			int numPairs = 0;
			for (int i = firstIndex; i < lastIndex; i++)
			{
				if (!double.IsNaN(progn[i]) && (progn[i] >= 0.0))
				{
					result += EvalFkt(progn[i], monitoring[i], evalMethod);
					numPairs++;
				}
			}
			return result / numPairs;
		}

		static private double EvalFkt(double prognVal, double monVal, EvalMethod evalMethod)
		{

			return (Math.Abs(prognVal - monVal));

			//return Math.Sqrt(Math.Abs(prognVal - monVal));

			//if (evalMethod == EvalMethod.AbsDiff)
			//{
			//	return Math.Sqrt(Math.Abs(prognVal - monVal));
			//}
			//else
			//{
			//	prognVal += 1.0;
			//	monVal += 1.0;
			//	return (prognVal > monVal) ? (prognVal / monVal) : (monVal / prognVal);
			//}
		}

	}
}

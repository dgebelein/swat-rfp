using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	public class SimFunctions
	{

		public static double ONeal( double t, double Tmax, double Topt, double Q,double L, double Kmax)
		{
			double w, z, delta;

			if ((t < 0.0) || (t > Tmax))
				return (0.0);

			if (Tmax <= Topt) return (0.0);

			delta = Tmax - Topt;
			w = (Q - 1.0) * delta;
			z = Math.Pow(w, 2) * Math.Pow(1.0 + Math.Sqrt(1 + 40 / w), 2) / 400;

			double zw;

			if (t < Topt)
				zw = (Kmax + L) * Math.Pow((Tmax - t) / delta, z) *  Math.Exp((t - Topt) / delta * z) - L;
			else
				zw = Kmax * Math.Pow((Tmax - t) / delta, z) *		Math.Exp((t - Topt) / delta * z);

			return (zw < 0.0) ? 0.0 : zw;
		}


		public static double Sigmoid(double BioAge, double beta)
		{
			return 1 / (1 + Math.Exp(-beta * (BioAge - 1)));	// Probit
			//return (1 - Math.Exp(-Math.Pow(BioAge, beta))); //Weibul
		}


		public static double FertilityFkt(double bioAge,double startFert, double startSlope, double endFert, double endSlope, double calibFactor)
		{
			// angelehnt an Weibul- ist nicht symmetrisch
			return ((1 - Math.Exp(-Math.Pow(bioAge / startFert, startSlope))) *
						Math.Exp(-Math.Pow(bioAge / (1.0 - endFert), endSlope)) * calibFactor);

			//return ((1/(1+ Math.Exp(-startSlope*(bioAge/startFert - 1)))) *
			//			(1+Math.Exp(-endSlope*(bioAge / (1.0 - endFert)-1))) * calibFactor);
			//return ((1 / (1 + Math.Exp(-startSlope * (bioAge-1) / startFert ))) *
			//			(1 + Math.Exp(-endSlope * (bioAge-1) / (1.0 - endFert))) * calibFactor);

		}

		public static double AestivTemp(double random, double aestThreshold, double aestVar)
		{
			double r = random;

			// Abfangen von Rechenfehlern!
			if (r == 0.0)
				return 0.0;
			if (r == 1.0)
				return 100.0;

			//return aestThreshold + Math.Log(r / (1 - r)) * aestVar;
			return aestThreshold + Logit(r) * aestVar;

		}

		public static double Logit(double p)
		{
			return Math.Log(p / (1 - p));
		}

	}
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Swop.sim
//{
//	class SimFunctions
//	{

//		public static double ONeal(double t, double Tmax, double Topt, double Q, double L, double Kmax)
//		{
//			double w, z, delta;

//			if ((t < 0.0) || (t > Tmax))
//				return (0.0);

//			if (Tmax <= Topt) return (0.0);

//			delta = Tmax - Topt;
//			w = (Q - 1.0) * delta;
//			z = Math.Pow(w, 2) * Math.Pow(1.0 + Math.Sqrt(1 + 40 / w), 2) / 400;

//			double zw;

//			if (t < Topt)
//				zw = (Kmax + L) * Math.Pow((Tmax - t) / delta, z) * Math.Exp((t - Topt) / delta * z) - L;
//			else
//				zw = Kmax * Math.Pow((Tmax - t) / delta, z) * Math.Exp((t - Topt) / delta * z);

//			return (zw < 0.0) ? 0.0 : zw;

//			//if (t < Topt)
//			//	return (Math.Max(0.0, (Kmax + L) * Math.Pow((Tmax - t) / delta, z) *
//			//			  Math.Exp((t - Topt) / delta * z) - L));
//			//else
//			//	return (Math.Max(0.0, Kmax * Math.Pow((Tmax - t) / delta, z) *
//			//						Math.Exp((t - Topt) / delta * z)));
//		}


//		public static double Sigmoid(double BioAge, double beta)
//		{
//			return 1 / (1 + Math.Exp(-beta * (BioAge - 1)));

//			//return (1 - Math.Exp(-Math.Pow(BioAge, beta)));
//		}


//		public static double FertilityFkt(double bioAge, double startFert, double startSlope, double endFert, double endSlope, double calibFactor)
//		{

//			return ((1 - Math.Exp(-Math.Pow(bioAge / startFert, startSlope))) *
//						Math.Exp(-Math.Pow(bioAge / (1.0 - endFert), endSlope)) * calibFactor);

//		}

//		public static double AestivTemp(Random random, double aestThreshold, double aestVar)
//		{

//			//double r = random.NextDouble();
//			//double a= aestThreshold + (Weibul(r, aestVar / 10.0) * 10.0);

//			//return aestThreshold + (Weibull(random.NextDouble(), aestVar / 10.0) * 10.0); 
//			double r = random.NextDouble();

//			// Abfangen von Rechenfehlern!
//			if (r == 0.0)
//				return 0.0;
//			if (r == 1.0)
//				return 100.0;

//			return aestThreshold + Math.Log(r / (1 - r)) * aestVar;
//		}

//	}

//}

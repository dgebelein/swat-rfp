﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	public enum ErrorType
	{
		OK,
		Warning,
		Error
	}
	public enum FlyType
	{
		DR,	//Kohlfliege
		PR,	//Moehrenfliege,
		DA		//Zwiebelfliege
	}

	public enum SwopWorkMode
	{
		OPTI,   //Optimierung
		COMBI   //Parameterkombinationen 
	}
	public enum DevStage:int
	{
		Egg = 0,
		Larva = 1,
		Pupa = 2,
		Fly = 3,
		WiPupa = 4,
		NewEgg = 5,
		ActiveFly = 6,
		AestPup = 7
	}

	public enum WeatherSource
	{
		HomeGrown,
		Dwd
	}

	public struct CombiRec
	{
		public SimParamElem Para;
		public string Key;
		public double MinVal;
		public double MaxVal;
		public Int32 Steps;
		public CombiRec(SimParamElem p, string key, double mini, double maxi, int steps)
		{
			Para = p;
			Key = key;
			MinVal = mini;
			MaxVal = maxi;
			Steps = steps;
		}
	}

}
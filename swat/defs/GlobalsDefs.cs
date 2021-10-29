using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.defs
{
	public enum ErrorType
	{
		OK,
		Warning,
		Error
	}
	public enum ModelType
	{
		DR,	//Kohlfliege
		PR,	//Moehrenfliege,
		DA		//Zwiebelfliege
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



}

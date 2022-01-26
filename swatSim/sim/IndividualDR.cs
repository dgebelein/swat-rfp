﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	class IndividualDR
	{
		#region Variable

		ModelDR model;

		int generation;
		int dayIndex;

		double[] transitionLimits;
		int diapauseIndex;
		bool isDiapauseGen;
		double diapauseTemp;
		double diapauseSlope;

		bool isLarvDia;
		int diapauseDur;
		double diapauseProb;

		double aestTemp; 
		double aestMinAge;
		double aestMaxAge;
		double aestDropDiff;
		bool isAestAsleep;

		double mortLarvaMaxAge;

		double fertCluster;

		DevStage stage;
      double bioAge;
		double repro;

		bool isAlive;

		#endregion


		#region Construction

		private IndividualDR(ModelDR model, DevStage startStage, double startAge, int startDay, int generation, bool isDiapauseGen)
		{
			this.model = model;
			this.stage = startStage;
			this.bioAge = startAge;
			this.dayIndex = startDay;
			this.generation = generation;

			this.transitionLimits = new double[5];

			this.mortLarvaMaxAge = model.MortLarvaMaxAge;
			this.fertCluster = model.FertCluster;

			this.isLarvDia = model.IsLarvDia;
			this.diapauseIndex = model.DiapauseIndex;
			this.diapauseTemp = model.DiapauseTemp;
			this.diapauseSlope = model.DiapauseRise;
			this.diapauseDur = model.DiapauseDur;
			this.diapauseProb = 0.0;
			this.isDiapauseGen = isDiapauseGen;

			this.aestTemp = model.IndividualAestThreshold;
			this.aestMinAge = model.AestMinAge;
			this.aestMaxAge = model.AestMaxAge;
			this.aestDropDiff = model.AestDropDiff;
			this.isAestAsleep = false;

			this.repro = 0.0;
			this.isAlive = true;
		}

		double[] GetTransitionLimits()
		{
			double[] tl = new double[5];

			for ( int stage=0; stage<=4; stage++)
			tl[stage] =  1 + SimFunctions.Logit(model.GetRandom)* model.Transitions[stage];

			return tl;
		}

		#endregion

		#region Simulationsschleife

		public static void CreateAndLive(ModelDR model, DevStage startStage, double startAge, int startDay, int generationNo, bool isDiapauseGeneration)
		{
			if (generationNo >= model.MaxGenerations) // kein neues Individuum bei zu hoher Generationenanzahl (wegen Speicherüberlauf!)
				return;

			IndividualDR indiv = new IndividualDR(model, startStage, startAge, startDay, generationNo, isDiapauseGeneration);
			indiv.transitionLimits = indiv.GetTransitionLimits();


			while (indiv.isAlive)
			{
				model.ReportIndivStatus(indiv.stage, indiv.dayIndex, indiv.generation, indiv.bioAge, indiv.isAestAsleep);
				switch(indiv.stage)
				{
					case DevStage.Egg:		EggDay(indiv); break;
					case DevStage.Larva:		LarvaDay(indiv); break;
					case DevStage.Pupa:		PupaDay(indiv); break;
					case DevStage.WiPupa:	WiPupaDay(indiv); break;
					case DevStage.Fly:		FlyDay(indiv); break;
				}

				if (indiv.dayIndex == indiv.model.LastSimIndex)
					return;
				indiv.dayIndex++;
			}
		}

		#endregion

		#region Simulation Entwicklungsstadien

		private static void EggDay(IndividualDR indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Egg, di))
			{
				indiv.isAlive = false;
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Egg, di);
			if(indiv.bioAge > indiv.transitionLimits[(int)DevStage.Egg])
			{
				indiv.stage = DevStage.Larva;
				indiv.bioAge = 0.0;
			}
		}

		private static void LarvaDay(IndividualDR indiv)
		{
			int di = indiv.dayIndex;

			double mort = (indiv.bioAge > indiv.mortLarvaMaxAge) ?
				indiv.model.GetMortality(DevStage.Larva, di) :
				indiv.model.GetMortality((DevStage)5, di);
			if (indiv.model.GetRandom < mort)
			{
				indiv.isAlive = false;
				return;
			}

			if (indiv.isLarvDia)
				CalcDiapause(indiv, di, indiv.model.GetSoilTemp(di));

			indiv.bioAge +=  indiv.model.GetDevRate(DevStage.Larva, di);
			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Larva])
			{
				indiv.stage = DevStage.Pupa;
				indiv.bioAge = 0.0;
			}
		}

		private static void PupaDay(IndividualDR indiv)
		{
			int di = indiv.dayIndex;

			double mort = indiv.model.GetMortality(DevStage.Pupa, di);
			//if (indiv.isAestAsleep)//Mortalität während Ästivation halbieren
			//		mort /= 2.0;

			if (indiv.model.GetRandom < mort)
			{
				indiv.isAlive = false;
				return;
			}

			double bt = indiv.model.GetSoilTemp(di);			
			CalcDiapause(indiv, di, bt);

			CalcAestivation(indiv, bt);
			if (indiv.isAestAsleep)
				return; // keine weiteren Berechnungen für diesen Tag nötig

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Pupa, di);

			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Pupa])
			{
				if (!indiv.isDiapauseGen && (indiv.model.GetRandom > indiv.diapauseProb))
				{
					indiv.stage = DevStage.Fly;
					indiv.bioAge = 0.0;
					indiv.generation++;
				}
				else
					indiv.isDiapauseGen = true;
			}
		}

		private static void WiPupaDay(IndividualDR indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.WiPupa, di))
			{
				indiv.isAlive = false; 
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.WiPupa, di);

			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.WiPupa])
			{
				indiv.stage = DevStage.Fly;
				indiv.bioAge = 0.0;
				indiv.generation++;
			}
		}

		private static void FlyDay(IndividualDR indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Fly, di))
			{
				indiv.isAlive = false; // vorzeitiger Tod
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Fly, di);
			indiv.repro += indiv.model.GetFertility(di, indiv.bioAge);// * indiv.model.GetVpdRestr(di);
			while(indiv.repro >= indiv.fertCluster)
			{
				for (int i=0;i< indiv.fertCluster; i++)
				{
					indiv.model.ReportIndivStatus(DevStage.NewEgg, di, indiv.generation, 0.0, false);
					CreateAndLive(indiv.model, DevStage.Egg, 0.0, di, indiv.generation, (di >= indiv.diapauseIndex));
				}
				indiv.repro -= indiv.fertCluster;
			}

			// aktive Fliegen berichten
			if (indiv.model.GetRandom < (indiv.model.GetFlightAct(di)))
         {
				indiv.model.ReportIndivStatus(DevStage.ActiveFly, di, indiv.generation, 0.0, false);
			}

			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Fly])
			{
				indiv.isAlive = false; // Tod durch Alter
				return;
			}
		}

		// Diapause nicht durch niedrige Temperaturen ausgelöst, sondern durch hohe Temperaturen spät im Jahr verzögert, wobei
		// die notwendige Temperaturschwelle mit der Zeit ansteigt
		private static void CalcDiapause(IndividualDR indiv, int day, double soilTemp)
		{
			if (day > indiv.diapauseIndex)
			{
				if (indiv.diapauseProb >= 1.0) // Diapausereiz ist nicht reversibel
					return;

				double threshold = indiv.diapauseTemp + indiv.diapauseSlope * Math.Max(1.0, (day - indiv.diapauseIndex)) / indiv.diapauseDur; 
				if (soilTemp < threshold)
					indiv.diapauseProb += 1.0 / indiv.diapauseDur;
				else
				{
					double v = Math.Min((soilTemp - threshold) / indiv.diapauseDur, 1.0 / indiv.diapauseDur);
					indiv.diapauseProb -= v;
					if (indiv.diapauseProb < 0.0)
						indiv.diapauseProb = 0.0;
				}
			}

			// alter Algorithmus: Diapauseauslöung durch Temperaturunterschreitung
			//if (di >= indiv.diapauseIndex)
			//{
			//	double bt = indiv.model.GetSoilTemp(di);
			//	if (bt < indiv.diapauseTemp + 2.0)
			//	{
			//		if (bt < indiv.diapauseTemp - 2.0)
			//			indiv.diapauseProb += 1.0 / indiv.diapauseDur;
			//		else
			//			indiv.diapauseProb += (1.0 - (bt - (indiv.diapauseTemp - 2.0)) / 4.0) / indiv.diapauseDur;

			//	}
			//}
		}


		private static void CalcAestivation(IndividualDR indiv, double soilTemp)
		{
			if ((indiv.bioAge < indiv.aestMinAge) || (indiv.bioAge > indiv.aestMaxAge))
				indiv.isAestAsleep = false;
			else
			{
				if (soilTemp > indiv.aestTemp)
					indiv.isAestAsleep = true;
				else
				{
					if (soilTemp < (indiv.aestTemp - indiv.aestDropDiff))
						indiv.isAestAsleep = false;
				}
			}
		}
		#endregion

	}
}


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
		int firstPupaDay;
		bool isDiapauseGen;

		double[] transitionLimits;

		int diapauseMode;		
		int diapauseIndex;
		double diapauseTemp;
		double diapauseRise;		
		int diapauseDur;

		bool isInDiapause;
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

			// Variable aus Performance-Gründen aus Modell übertragen!
			this.mortLarvaMaxAge = model.MortLarvaMaxAge;
			this.fertCluster = model.FertCluster;

			this.diapauseMode = model.DiaMode;
			this.diapauseIndex = model.DiapauseIndex;
			this.diapauseTemp = model.DiapauseTemp;
			this.diapauseRise = model.DiapauseRise;
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
			//Achtung bei Swop: MaxGen um 1 erhöhen wegen Auswertung bei Modelinitialisierung
			if (generationNo >= model.MaxGenerations) // kein neues Individuum bei zu hoher Generationenanzahl (wegen Speicherüberlauf!)
				return;

			model.IncIndividualNum();
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

			indiv.bioAge +=  indiv.model.GetDevRate(DevStage.Larva, di);
			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Larva])
			{
				indiv.stage = DevStage.Pupa;
				indiv.bioAge = 0.0;
				indiv.firstPupaDay = di;
			}
		}

		private static void PupaDay(IndividualDR indiv)
		{
			int di = indiv.dayIndex;

			double mort = indiv.model.GetMortality(DevStage.Pupa, di);

			if (indiv.model.GetRandom < mort)
			{
				indiv.isAlive = false;
				return;
			}

			if (indiv.isInDiapause)
				return;  // keine weiteren Berechnungen mehr nötig

			double bt = indiv.model.GetSoilTemp(di);	

			CalcDiapause(indiv, di, bt);

			CalcAestivation(indiv, di);
			if (indiv.isAestAsleep)
				return; // keine weiteren Berechnungen für diesen Tag nötig

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Pupa, di);

			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Pupa])
			{
				if (indiv.model.GetRandom > indiv.diapauseProb)
				{
					indiv.stage = DevStage.Fly;
					indiv.bioAge = 0.0;
					indiv.generation++;
				}
				else
					indiv.isInDiapause = true;
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
			indiv.repro += indiv.model.GetFertility(di, indiv.bioAge) *(1.0- indiv.model.GetFertInhib(di)); // * indiv.model.GetVpdRestr(di);
			while (indiv.repro >= indiv.fertCluster)
			{
				for (int i=0;i< indiv.fertCluster; i++)
				{
					indiv.model.ReportIndivStatus(DevStage.NewEgg, di, indiv.generation, 0.0, false);
					CreateAndLive(indiv.model, DevStage.Egg, 0.0, di, indiv.generation, (di >= indiv.model.DiapauseIndex));
				}
				indiv.repro -= indiv.fertCluster;
			}

			// aktive Fliegen berichten
			if (indiv.model.GetRandom < indiv.model.GetFlightAct(di))
         {
				indiv.model.ReportIndivStatus(DevStage.ActiveFly, di, indiv.generation, 0.0, false);
			}

			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Fly])
			{
				indiv.isAlive = false; // Tod durch Alter
				return;
			}
		}

		#endregion

		#region Diapause
		private static void CalcDiapause(IndividualDR indiv, int day, double soilTemp)
		{
			if (day < indiv.diapauseIndex)
				return;

			switch(indiv.diapauseMode)
			{
					case 1: CalcDiapauseCold(indiv, soilTemp); break;
					case 2: CalcDiapauseWarm(indiv, day, soilTemp); break;
					case 3: CalcDiapauseCold_T(indiv, soilTemp); break; 
					case 4: CalcDiapauseWarm_T(indiv, day, soilTemp); break;
					case 5: CalcDiapauseCold_Sum(indiv, soilTemp); break;
					case 6: CalcDiapause_Trig(indiv, day);break;
					default: break;
				}
		}


		//  Diapause-Fähigkeit unabhängig vom Eiablagetermin - durch Kältereiz ausgelöst
		private static void CalcDiapauseCold(IndividualDR indiv, double soilTemp)
		{

			if (indiv.diapauseProb >= 1.0) // vollendeter Diapausereiz soll nicht reversibel sein
				return;

			double threshold = indiv.diapauseTemp;
			if (soilTemp < threshold)
				indiv.diapauseProb += 1.0 / indiv.diapauseDur;

		}

		//  Diapause-Fähigkeit unabhängig vom Eiablagetermin - durch Wärme verhindert
		private static void CalcDiapauseWarm(IndividualDR indiv, int day, double soilTemp)
		{
			if (indiv.diapauseProb >= 1.0) // vollendeter Diapausereiz soll nicht reversibel sein
				return;

			double threshold = indiv.diapauseTemp + indiv.diapauseRise / indiv.diapauseDur * (day - indiv.diapauseIndex);
			if (soilTemp < threshold)
				indiv.diapauseProb += 1.0 / indiv.diapauseDur;
			else
			{
				//double v = Math.Min((soilTemp - threshold) / indiv.diapauseDur, 1.0 / indiv.diapauseDur);
				//indiv.diapauseProb -= v;
				indiv.diapauseProb -= 1.0 / indiv.diapauseDur;

				if (indiv.diapauseProb < 0.0)
					indiv.diapauseProb = 0.0;
			}
		}

		// Diapause  durch niedrige Temperaturen ausgelöst  -  Diapause-Sensivität abhängig vom Eiablagetermin
		private static void CalcDiapauseCold_T(IndividualDR indiv, double soilTemp)
		{
			if (indiv.isDiapauseGen)
			{
				if (indiv.diapauseProb >= 1.0) // vollendeter Diapausereiz soll nicht reversibel sein
					return;

				double threshold = indiv.model.DiapauseTemp;
				if (soilTemp < threshold)
					indiv.diapauseProb += 1.0 / indiv.model.DiapauseDur;
			}
		}

		//  Diapause-Fähigkeit abhängig vom Eiablagetermin - durch Wärme verhindert, mit ansteigender Temperaturschwelle

		private static void CalcDiapauseWarm_T(IndividualDR indiv, int day, double soilTemp)
		{
			if (indiv.isDiapauseGen)
			{
				if (indiv.diapauseProb >= 1.0) // vollendeter Diapausereiz soll nicht reversibel sein
					return;

				double threshold = indiv.model.DiapauseTemp + indiv.diapauseRise / indiv.model.DiapauseDur * (day - indiv.diapauseIndex); 
				if (soilTemp < threshold)
					indiv.diapauseProb += 1.0 / indiv.model.DiapauseDur;
				else
				{
					//double v = Math.Min((soilTemp - threshold) / indiv.model.DiapauseDur, 1.0 / indiv.model.DiapauseDur);
					//indiv.diapauseProb -= v;	
					//				
					indiv.diapauseProb -= 1.0 / indiv.model.DiapauseDur;
					if (indiv.diapauseProb < 0.0)
						indiv.diapauseProb = 0.0;
				}
			}
		}


		//Diapause über Kältesumme - unabh. vom Eiablagetermin
		private static void CalcDiapauseCold_Sum(IndividualDR indiv, double soilTemp)
		{
			if (indiv.diapauseProb >= 1.0) // vollendeter Diapausereiz soll nicht reversibel sein
				return;

			double threshold = indiv.diapauseTemp;
			if (soilTemp < threshold)
				indiv.diapauseProb += (threshold - soilTemp) / indiv.diapauseDur;
		}

		private static void CalcDiapause_Trig(IndividualDR indiv, int day)
		{
			indiv.diapauseProb = indiv.model.TableDiapause[day];
		}

		#endregion


		#region Ästivation

		private static void CalcAestivation(IndividualDR indiv, int day)
		{

			double aestTemp = indiv.model.TableAestTemp[day];
			if ((indiv.bioAge < indiv.aestMinAge) || (indiv.bioAge > indiv.aestMaxAge))
				indiv.isAestAsleep = false;
			else
			{
				if (aestTemp > indiv.aestTemp)
					indiv.isAestAsleep = true;
				else
				{
					if (aestTemp < (indiv.aestTemp - indiv.aestDropDiff))
						indiv.isAestAsleep = false;
				}
			}
		}
		#endregion

	}
}

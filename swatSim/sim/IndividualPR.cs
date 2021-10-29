using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	class IndividualPR
	{
		#region Variable

		ModelPR model;

		int generation;
		int dayIndex;

		double[] transitionLimits;
		int diapauseIndex;
		bool isDiapauseGen;
		double diapauseTemp;
		int diapauseDur;
		double diapauseProb;

		double aestTemp;
		double aestMinAge;
		double aestMaxAge;
		double aestDropDiff;
		bool isAestAsleep;
		//bool isWaitingForHatch;


		double mortLarvaMaxAge;

		bool monOnField;
		bool isPrecOvip;
		double fertCluster;

		DevStage stage;
		double bioAge;
		double repro;

		bool isAlive;

		#endregion


		#region Construction

		private IndividualPR(ModelPR model, DevStage startStage, double startAge, int startDay, int generation, bool isDiapauseGen)
		{
			this.model = model;
			this.stage = startStage;
			this.bioAge = startAge;
			this.dayIndex = startDay;
			this.generation = generation;

			this.mortLarvaMaxAge = model.MortLarvaMaxAge;


			this.diapauseIndex = model.DiapauseIndex;
			this.diapauseTemp = model.DiapauseTemp;
			this.diapauseDur = model.DiapauseDur;
			this.diapauseProb = 0.0;
			this.isDiapauseGen = isDiapauseGen;

			this.aestTemp = model.IndividualAestThreshold;
			this.aestMinAge = model.AestMinAge;
			this.aestMaxAge = model.AestMaxAge;

			this.isPrecOvip = model.IsPrecOvip;
			this.monOnField = model.MonOnField;
			this.fertCluster = model.FertCluster;
			//this.aestSensAge = model.AestSensAge;
			this.aestDropDiff = model.AestDropDiff;
			this.isAestAsleep = false;

			


			this.repro = 0.0;
			this.isAlive = true;
		}

		#endregion

		#region Simulationsschleife

		double[] GetTransitionLimits()
		{
			double[] tl = new double[5];

			for (int stage = 0; stage <= 4; stage++)
				tl[stage] = 1 + SimFunctions.Logit(model.GetRandom) * model.Transitions[stage];

			return tl;
		}

		public static void CreateAndLive(ModelPR model, DevStage startStage, double startAge, int startDay, int generationNo, bool isDiapauseGeneration)
		{
			IndividualPR indiv = new IndividualPR(model, startStage, startAge, startDay, generationNo, isDiapauseGeneration);
			indiv.transitionLimits = indiv.GetTransitionLimits();

			while (indiv.isAlive)
			{
				model.ReportIndivStatus(indiv.stage, indiv.dayIndex, indiv.generation, indiv.bioAge, indiv.isAestAsleep);
				switch (indiv.stage)
				{
					case DevStage.Egg: EggDay(indiv); break;
					case DevStage.Larva: LarvaDay(indiv); break;
					case DevStage.Pupa: PupaDay(indiv); break;
					case DevStage.WiPupa: WiPupaDay(indiv); break;
					case DevStage.Fly: FlyDay(indiv); break;
				}

				if (indiv.dayIndex == indiv.model.LastSimIndex)
					return;
				indiv.dayIndex++;
			}
		}

		#endregion

		#region Simulation Entwicklungsstadien

		private static void EggDay(IndividualPR indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Egg, di))
			{
				indiv.isAlive = false;
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Egg, di);
			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Egg])

			{
				indiv.stage = DevStage.Larva;
				indiv.bioAge = 0.0;
			}
		}

		private static void LarvaDay(IndividualPR indiv)
		{
			int di = indiv.dayIndex;
			double mort = (indiv.bioAge > indiv.mortLarvaMaxAge) ? // ohne Temp-abh. Mortalität sind Mortality[Larva] und [5] gleich!
				indiv.model.GetMortality(DevStage.Larva, di) :
				indiv.model.GetMortality((DevStage)5, di);

			if (indiv.model.GetRandom < mort)
			{
				indiv.isAlive = false;
				return;
			}

			if (di >= indiv.diapauseIndex)
			{
				double bt = indiv.model.GetSoilTemp(di);
				if (bt < indiv.diapauseTemp + 2.0)
				{
					if (bt < indiv.diapauseTemp - 2.0)
						indiv.diapauseProb += 1.0 / indiv.diapauseDur;
					else
						indiv.diapauseProb += (1.0 - (bt - (indiv.diapauseTemp - 2.0)) / 4.0) / indiv.diapauseDur;
				}
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Larva, di);
			if (indiv.bioAge > indiv.transitionLimits[(int)DevStage.Larva])

			{
				indiv.stage = DevStage.Pupa;
				indiv.bioAge = 0.0;
			}
		}

		private static void PupaDay(IndividualPR indiv)
		{
			int di = indiv.dayIndex;

			double mort = indiv.model.GetMortality(DevStage.Pupa, di);
			double hatchInhib = indiv.model.GetHatchInhib(di);

			if (indiv.model.GetRandom < mort)
			{
				indiv.isAlive = false;
				return;
			}

			double bt = indiv.model.GetSoilTemp(di);

			//Ästivation
			if ((indiv.bioAge < indiv.aestMinAge) || (indiv.bioAge > indiv.aestMaxAge))
				indiv.isAestAsleep = false;
			else
			{
				if (bt > indiv.aestTemp)
					indiv.isAestAsleep = true;
				else
				{
					if (bt < (indiv.aestTemp - indiv.aestDropDiff))
						indiv.isAestAsleep = false;
				}
			}

			//Diapause
			if (di >= indiv.diapauseIndex)
			{
				if (bt < indiv.diapauseTemp + 2.0)
				{
					if (bt < indiv.diapauseTemp - 2.0)
						indiv.diapauseProb += 1.0 / indiv.diapauseDur;
					else
						indiv.diapauseProb += (1.0 - (bt - (indiv.diapauseTemp - 2.0)) / 4.0) / indiv.diapauseDur;
				}
			}

			if (indiv.isAestAsleep) // inÄstivation: keine weiteren Berechnungen für diesen Tag nötig
				return; 

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Pupa, di);

			if ((indiv.bioAge) > indiv.transitionLimits[(int)DevStage.Pupa])
			{
				if (indiv.model.GetRandom > hatchInhib)
				{
					//indiv.isAlive = false;// dies aktivieren, wenn Bodentrockenheit Mortalität erzeugt
					return;// nur dies aktivieren, wenn Bodentrockenheit Schlupf verzögert
				}

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

		private static void WiPupaDay(IndividualPR indiv)
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

		private static void FlyDay(IndividualPR indiv)
		{
			int di = indiv.dayIndex;
			bool isIdle = indiv.monOnField; // Flug aufs Feld nur bei Eiablage - in Hecke immer Flug
			double flightInhib = indiv.model.GetFlightInhib(di);


			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Fly, di))
			{
				indiv.isAlive = false; // vorzeitiger Tod
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Fly, di);

			if (indiv.isPrecOvip)
				indiv.repro += indiv.model.GetFertility(di, indiv.bioAge) * flightInhib;
			else
				indiv.repro += indiv.model.GetFertility(di, indiv.bioAge);


			while (indiv.repro >= indiv.fertCluster)
			{
				isIdle = false; // Flug aufs Feld nur bei Eiablage
				for (int i = 0; i < indiv.fertCluster; i++)
				{
					indiv.model.ReportIndivStatus(DevStage.NewEgg, di, indiv.generation, 0.0, false);
					CreateAndLive(indiv.model, DevStage.Egg, 0.0, di, indiv.generation, (di >= indiv.diapauseIndex));
				}
				indiv.repro -= indiv.fertCluster;
			}

			// aktive Fliegen berichten
			
			double fa = indiv.model.GetFlightAct(di) * flightInhib;
			if ( !isIdle && (indiv.model.GetRandom < fa)) 
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

	}
}

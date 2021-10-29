using swat.defs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.sim
{
	class IndividualCRF
	{
		#region Variable

		ModelCRF model;

		int generation;
		int dayIndex;

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

		double fertCluster;

		DevStage stage;
      double bioAge;
		double repro;

		bool isAlive;

		#endregion


		#region Construction

		private IndividualCRF(ModelCRF modelCRF, DevStage startStage, double startAge, int startDay, int generation, bool isDiapauseGen)
		{
			this.model = modelCRF;
			this.stage = startStage;
			this.bioAge = startAge;
			this.dayIndex = startDay;
			this.generation = generation;

			this.diapauseIndex = modelCRF.DiapauseIndex;
			this.diapauseTemp = modelCRF.DiapauseTemp;
			this.diapauseDur = modelCRF.DiapauseDur;
			this.diapauseProb = 0.0;
			this.isDiapauseGen = isDiapauseGen;

			this.aestTemp = modelCRF.IndividualAestThreshold;
			this.aestMinAge = modelCRF.AestMinAge;
			this.aestMaxAge = modelCRF.AestMaxAge;

			this.fertCluster = modelCRF.FertCluster;
			//this.aestSensAge = modelCRF.AestSensAge;
			this.aestDropDiff = modelCRF.AestDropDiff;
			this.isAestAsleep = false;

			this.repro = 0.0;
			this.isAlive = true;
		}

		#endregion

		#region Simulationsschleife

		public static void CreateAndLive(ModelCRF model, DevStage startStage, double startAge, int startDay, int generationNo, bool isDiapauseGeneration)
		{
			IndividualCRF indiv = new IndividualCRF(model, startStage, startAge, startDay, generationNo, isDiapauseGeneration);

			
			while(indiv.isAlive)
			{
				model.Report(indiv.stage, indiv.dayIndex, indiv.generation, indiv.bioAge, indiv.isAestAsleep);
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

		private static void EggDay(IndividualCRF indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Egg, di))
			{
				indiv.isAlive = false;
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Egg, di);
			if(indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Egg, di, indiv.bioAge))
			{
				indiv.stage = DevStage.Larva;
				indiv.bioAge = 0.0;
			}
		}

		private static void LarvaDay(IndividualCRF indiv)
		{
			int di = indiv.dayIndex;
         if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Larva, di))
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
			indiv.bioAge +=  indiv.model.GetDevRate(DevStage.Larva, di);
			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Larva, di, indiv.bioAge))
			{
				indiv.stage = DevStage.Pupa;
				indiv.bioAge = 0.0;
			}
		}

		private static void PupaDay(IndividualCRF indiv)
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

			if (indiv.isAestAsleep)
				return; // keine weiteren Berechnungen für diesen Tag nötig

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Pupa, di);

			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Pupa, di, indiv.bioAge))
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

		private static void WiPupaDay(IndividualCRF indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.WiPupa, di))
			{
				indiv.isAlive = false; 
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.WiPupa, di);

			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.WiPupa, di, indiv.bioAge))
			{
				indiv.stage = DevStage.Fly;
				indiv.bioAge = 0.0;
				indiv.generation++;
			}

		}

		private static void FlyDay(IndividualCRF indiv)
		{
			int di = indiv.dayIndex;

			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Fly, di))
			{
				indiv.isAlive = false; // vorzeitiger Tod
				return;
			}

			indiv.bioAge += indiv.model.GetDevRate(DevStage.Fly, di);
			indiv.repro += indiv.model.GetFertility(di, indiv.bioAge) * indiv.model.GetWindRestr(di);
			while(indiv.repro >= indiv.fertCluster)
			{
				indiv.model.Report(DevStage.NewEgg, di, indiv.generation, 0.0, false);
				for (int i=0;i< indiv.fertCluster; i++)
					CreateAndLive(indiv.model, DevStage.Egg, 0.0, di, indiv.generation, (di >= indiv.diapauseIndex));
				indiv.repro -= indiv.fertCluster;
			}

			// aktive Fliegen berichten
			if (indiv.model.GetRandom < (indiv.model.GetFlightAct(di)* indiv.model.GetWindRestr(di)))
         {
				indiv.model.Report(DevStage.ActiveFly, di, indiv.generation, 0.0, false);
			}


			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Fly, di, indiv.bioAge))
			{
				indiv.isAlive = false; // Tod durch Alter
				return;
			}
		}

		#endregion

	}
}

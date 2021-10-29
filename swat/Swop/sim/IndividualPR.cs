//using Swop.defs;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Swop.sim
//{

//	class IndividualPR
//	{
//		#region Variable

//		ModelPR model;

//		int generation;
//		int dayIndex;

//		int diapauseIndex;
//		bool isDiapauseGen;
//		double diapauseTemp;
//		int diapauseDur;
//		double diapauseProb;

//		double aestTemp;
//		double aestMinAge;
//		double aestMaxAge;
//		double aestDropDiff;
//		bool isAestAsleep;

//		double mortLarvaMaxAge;

//		bool monOnField;
//		bool isVpdOvip;
//		double fertCluster;

//		DevStage stage;
//		double bioAge;
//		double repro;

//		bool isAlive;

//		#endregion


//		#region Construction

//		private IndividualPR(ModelPR model, DevStage startStage, double startAge, int startDay, int generation, bool isDiapauseGen)
//		{
//			this.model = model;
//			this.stage = startStage;
//			this.bioAge = startAge;
//			this.dayIndex = startDay;
//			this.generation = generation;

//			this.mortLarvaMaxAge = model.MortLarvaMaxAge;

//			this.diapauseIndex = model.DiapauseIndex;
//			this.diapauseTemp = model.DiapauseTemp;
//			this.diapauseDur = model.DiapauseDur;
//			this.diapauseProb = 0.0;
//			this.isDiapauseGen = isDiapauseGen;

//			this.aestTemp = model.IndividualAestThreshold;
//			this.aestMinAge = model.AestMinAge;
//			this.aestMaxAge = model.AestMaxAge;

//			this.isVpdOvip = model.IsVpdOvip;
//			this.monOnField = model.MonOnField;
//			this.fertCluster = model.FertCluster;

//			//this.aestSensAge = model.AestSensAge;
//			this.aestDropDiff = model.AestDropDiff;
//			this.isAestAsleep = false;

//			this.repro = 0.0;
//			this.isAlive = true;
//		}

//		#endregion

//		#region Simulationsschleife

//		public static void CreateAndLive(ModelPR model, DevStage startStage, double startAge, int startDay, int generationNo, bool isDiapauseGeneration)
//		{
//			IndividualPR indiv = new IndividualPR(model, startStage, startAge, startDay, generationNo, isDiapauseGeneration);


//			while (indiv.isAlive)
//			{
//				model.Report(indiv.stage, indiv.dayIndex, indiv.generation, indiv.bioAge, indiv.isAestAsleep);
//				switch (indiv.stage)
//				{
//					case DevStage.Egg: EggDay(indiv); break;
//					case DevStage.Larva: LarvaDay(indiv); break;
//					case DevStage.Pupa: PupaDay(indiv); break;
//					case DevStage.WiPupa: WiPupaDay(indiv); break;
//					case DevStage.Fly: FlyDay(indiv); break;
//				}

//				if (indiv.dayIndex == indiv.model.LastSimIndex)
//					return;
//				indiv.dayIndex++;
//			}
//		}

//		#endregion

//		#region Simulation Entwicklungsstadien

//		private static void EggDay(IndividualPR indiv)
//		{
//			int di = indiv.dayIndex;

//			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Egg, di))
//			{
//				indiv.isAlive = false;
//				return;
//			}

//			indiv.bioAge += indiv.model.GetDevRate(DevStage.Egg, di);
//			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Egg, di, indiv.bioAge))
//			{
//				indiv.stage = DevStage.Larva;
//				indiv.bioAge = 0.0;
//			}
//		}

//		private static void LarvaDay(IndividualPR indiv)
//		{
//			int di = indiv.dayIndex;

//			double mort = (indiv.bioAge > indiv.mortLarvaMaxAge) ?
//					indiv.model.GetMortality(DevStage.Larva, di) :
//					indiv.model.GetMortality((DevStage)5, di);

//			if (indiv.model.GetRandom < mort)
//			{
//				indiv.isAlive = false;
//				return;
//			}
//			//if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Larva, di))
//			//{
//			//	indiv.isAlive = false;
//			//	return;
//			//}

//			if (di >= indiv.diapauseIndex)
//			{
//				double bt = indiv.model.GetSoilTemp(di);
//				if (bt < indiv.diapauseTemp + 2.0)
//				{
//					if (bt < indiv.diapauseTemp - 2.0)
//						indiv.diapauseProb += 1.0 / indiv.diapauseDur;
//					else
//						indiv.diapauseProb += (1.0 - (bt - (indiv.diapauseTemp - 2.0)) / 4.0) / indiv.diapauseDur;

//				}
//			}
//			indiv.bioAge += indiv.model.GetDevRate(DevStage.Larva, di);
//			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Larva, di, indiv.bioAge))
//			{
//				indiv.stage = DevStage.Pupa;
//				indiv.bioAge = 0.0;
//			}
//		}

//		private static void PupaDay(IndividualPR indiv)
//		{
//			int di = indiv.dayIndex;

//			double mort = indiv.model.GetMortality(DevStage.Pupa, di);

//			if (indiv.model.GetRandom < mort)
//			{
//				indiv.isAlive = false;
//				return;
//			}

//			double bt = indiv.model.GetSoilTemp(di);

//			//Ästivation
//			if ((indiv.bioAge < indiv.aestMinAge) || (indiv.bioAge > indiv.aestMaxAge))
//				indiv.isAestAsleep = false;
//			else
//			{
//				if (bt > indiv.aestTemp)
//					indiv.isAestAsleep = true;
//				else
//				{
//					if (bt < (indiv.aestTemp - indiv.aestDropDiff))
//						indiv.isAestAsleep = false;
//				}
//			}

//			//Diapause
//			if (di >= indiv.diapauseIndex)
//			{
//				if (bt < indiv.diapauseTemp + 2.0)
//				{
//					if (bt < indiv.diapauseTemp - 2.0)
//						indiv.diapauseProb += 1.0 / indiv.diapauseDur;
//					else
//						indiv.diapauseProb += (1.0 - (bt - (indiv.diapauseTemp - 2.0)) / 4.0) / indiv.diapauseDur;
//				}
//			}

//			if (indiv.isAestAsleep)
//				return; // keine weiteren Berechnungen für diesen Tag nötig

//			indiv.bioAge += indiv.model.GetDevRate(DevStage.Pupa, di);

//			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Pupa, di, indiv.bioAge))
//			{
//				if (!indiv.isDiapauseGen && (indiv.model.GetRandom > indiv.diapauseProb))
//				{
//					indiv.stage = DevStage.Fly;
//					indiv.bioAge = 0.0;
//					indiv.generation++;
//				}
//				else
//					indiv.isDiapauseGen = true;
//			}
//		}

//		private static void WiPupaDay(IndividualPR indiv)
//		{
//			int di = indiv.dayIndex;

//			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.WiPupa, di))
//			{
//				indiv.isAlive = false;
//				return;
//			}

//			indiv.bioAge += indiv.model.GetDevRate(DevStage.WiPupa, di);

//			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.WiPupa, di, indiv.bioAge))
//			{
//				indiv.stage = DevStage.Fly;
//				indiv.bioAge = 0.0;
//				indiv.generation++;
//			}

//		}

//		private static void FlyDay(IndividualPR indiv)
//		{
//			int di = indiv.dayIndex;
//			bool isIdle = indiv.monOnField; // Flug aufs Feld nur bei Eiablage - in Hecke immer Flug

//			if (indiv.model.GetRandom < indiv.model.GetMortality(DevStage.Fly, di))
//			{
//				indiv.isAlive = false; // vorzeitiger Tod
//				return;
//			}

//			indiv.bioAge += indiv.model.GetDevRate(DevStage.Fly, di);

//			if (indiv.isVpdOvip)
//				indiv.repro += indiv.model.GetFertility(di, indiv.bioAge) * indiv.model.GetVpdRestr(di);
//			else
//				indiv.repro += indiv.model.GetFertility(di, indiv.bioAge);


//			while (indiv.repro >= indiv.fertCluster)
//			{
//				isIdle = false; // Flug aufs Feld nur bei Eiablage
//				for (int i = 0; i < indiv.fertCluster; i++)
//				{
//					indiv.model.Report(DevStage.NewEgg, di, indiv.generation, 0.0, false);
//					CreateAndLive(indiv.model, DevStage.Egg, 0.0, di, indiv.generation, (di >= indiv.diapauseIndex));
//				}
//				indiv.repro -= indiv.fertCluster;
//			}

//			// aktive Fliegen berichten
//			if (!isIdle && (indiv.model.GetRandom < (indiv.model.GetFlightAct(di) * indiv.model.GetVpdRestr(di))))
//			{
//				indiv.model.Report(DevStage.ActiveFly, di, indiv.generation, 0.0, false);
//			}


//			if (indiv.model.GetRandom < indiv.model.GetTransition(DevStage.Fly, di, indiv.bioAge))
//			{
//				indiv.isAlive = false; // Tod durch Alter
//				return;
//			}
//		}

//		#endregion

//	}
//}

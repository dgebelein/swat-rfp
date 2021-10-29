using swat.defs;
using swat.iodata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.sim
{
	class ModelCRF : ModelBase
	{
		#region Construction + Parameterdefinition

		public ModelCRF(WorkspaceData workspace,SimParamData explicitParams = null) : base(workspace, explicitParams)
		{
			//InitParamDefaults();
			//_workingParams = workspace.SimParameters; GetModelParameters(ModelType.DR);
		}

		public override string GetParamPrefix()
		{
			return "crf";
		}

		protected  override SimParamData GetDefaultParams()
		{
			// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

			SimParamData p = new SimParamData();

			// Grundlegendes

			p.InitItem("crf.StartPop",1000, typeof(int),500.0,10000.0, "Startpopulation (Anzahl Individuen)"); 

			p.InitItem("crf.SimStart", 59, typeof(int),1.0, 180.0, "Simulationsstart (Tag des Jahres)"); // (1. März)

			p.InitItem("crf.UseSummerSoil", false, typeof(bool),0.499, 0.501, "auch nach 1. Juni noch Bodentemp. verwenden? - oder Lufttemp.");
			p.InitItem("crf.AdjAir", 0.0, typeof(double),-5.0, 5.0, "Korrektur Lufttemp"); // Korrektur Lufttemp
			p.InitItem("crf.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp"); // Korrektur Bodentemp
			p.InitItem("crf.AdjWind", 1.0, typeof(double),0.0,10.0, "Korrektur Wind (Multiplikator)");// Korrektur Wind (Multiplikator)

			//Mortalitäten
			p.InitItem("crf.MortEgg", 0.05, typeof(double),0.01,0.25, "tägl. Mortalität Eier");
			p.InitItem("crf.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven");
			p.InitItem("crf.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen");
			p.InitItem("crf.MortFly", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen");
			p.InitItem("crf.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen");

			p.InitItem("crf.MortEggIsTDep", true, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?"); // Temperaturabh. Mortalität?
			p.InitItem("crf.MortEggThr", 21.0, typeof(double),15.0,35.0, "Temperaturschwelle für erhöhte Ei-Mortalität");    // Temperaturschwelle für erhöhte Mort.
			p.InitItem("crf.MortEggInc", 0.15, typeof(double),0.01,0.5, "Anstieg Ei-Mortalität pro Grad C");   // Anstieg Mort. pro Grad C

			p.InitItem("crf.MortLarvaIsTDep", true, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?"); // Temperaturabh. Mortalität?
			p.InitItem("crf.MortLarvaThr", 22.0, typeof(double),15.0,35.0, "Temperaturschwelle für erhöhte Larven-Mortalität");    // Temperaturschwelle für erhöhte Mort.
			p.InitItem("crf.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C");  // Anstieg Mort. pro Grad C

			//Transitionen - Übergänge ins nächste Stadium
			p.InitItem("crf.TransWiPupa", 4.0, typeof(double), 1.0, 10.0, "Faktor Transition Überwinterungsstadium->Adult");
			p.InitItem("crf.TransEgg", 4.0, typeof(double), 1.0,10.0, "Faktor Transition Ei->Larve");
			p.InitItem("crf.TransLarva", 4.0, typeof(double), 1.0, 10.0, "Faktor Transition Larve->Puppe");
			p.InitItem("crf.TransPupa", 4.0, typeof(double), 1.0, 10.0, "Faktor Transition Puppe->Adult");
			p.InitItem("crf.TransFly", 5.0, typeof(double), 1.0, 10.0, "Faktor Transition Adult->Tod");

			//Entwicklungsparameter
			p.InitItem("crf.DevEggTmax", 40.0, typeof(double),30.0,45.0, "Entwicklung Ei: Temperaturmaximum");
			p.InitItem("crf.DevEggTopt", 25.0, typeof(double),15.0,29.0, "Entwicklung Ei: Temperaturoptimum");
			p.InitItem("crf.DevEggQ", 1.88, typeof(double),1.0,2.0, "Entwicklung Ei: Abstieg rechte Flanke");
			p.InitItem("crf.DevEggL", 0.024, typeof(double),0.001,0.05, "Entwicklung Ei: Anstieg linke Flanke");
			p.InitItem("crf.DevEggKmax", 0.375, typeof(double),0.1,0.5, "Entwicklung Ei: max. tägl. Entwicklungsrate");


			p.InitItem("crf.DevLarvaTmax", 35.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum");
			p.InitItem("crf.DevLarvaTopt", 27.0, typeof(double),20.0,29.0, "Entwicklung Larve: Temperaturoptimum");
			p.InitItem("crf.DevLarvaQ", 1.73, typeof(double), 1.0, 2.0, "Entwicklung Larve: Abstieg rechte Flanke");
			p.InitItem("crf.DevLarvaL", 0.009, typeof(double),0.001,0.05, "Entwicklung Larve: Anstieg linke Flanke");
			p.InitItem("crf.DevLarvaKmax", 0.09, typeof(double), 0.05, 0.5, "Entwicklung Larve: max. tägl. Entwicklungsrate");


			p.InitItem("crf.DevPupaTmax", 30.0, typeof(double),28.0,40.0, "Entwicklung Puppe: Temperaturmaximum");
			p.InitItem("crf.DevPupaTopt", 22.5, typeof(double),20.0,27.0, "Entwicklung Puppe: Temperaturoptimum");
			p.InitItem("crf.DevPupaQ", 1.88, typeof(double), 1.0, 2.0, "Entwicklung Puppe: Abstieg rechte Flanke");
			p.InitItem("crf.DevPupaL", 0.008, typeof(double), 0.001, 0.05, "Entwicklung Puppe: Anstieg linke Flanke");
			p.InitItem("crf.DevPupaKmax", 0.073, typeof(double), 0.05, 0.5, "Entwicklung Puppe: max. tägl. Entwicklungsrate");


			p.InitItem("crf.DevFlyTmax", 35.0, typeof(double),31.0,45.0, "Entwicklung Fliege: Temperaturmaximum");
			p.InitItem("crf.DevFlyTopt", 30.0, typeof(double),20.0,30.0, "Entwicklung Fliege: Temperaturoptimum");
			p.InitItem("crf.DevFlyQ", 1.67, typeof(double), 1.0, 2.0, "Entwicklung Fliege: Abstieg rechte Flanke");
			p.InitItem("crf.DevFlyL", 0.006, typeof(double), 0.001, 0.05, "Entwicklung Fliege: Anstieg linke Flanke");
			p.InitItem("crf.DevFlyKmax", 0.072, typeof(double), 0.05, 0.15, "Entwicklung Fliege: max. tägl. Entwicklungsrate");


			p.InitItem("crf.DevWiPupaTmax", 30.0, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum");
			p.InitItem("crf.DevWiPupaTopt", 25.0, typeof(double),20.0,29.0, "Entwicklung Winterpuppe: Temperaturoptimum");
			p.InitItem("crf.DevWiPupaQ", 1.62, typeof(double), 1.0, 2.0, "Entwicklung Winterpuppe: Abstieg rechte Flanke");
			p.InitItem("crf.DevWiPupaL", 0.035, typeof(double),0.01, 0.05, "Entwicklung Winterpuppe: Anstieg linke Flanke");
			p.InitItem("crf.DevWiPupaKmax", 0.129, typeof(double),0.05,0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate");

			//Fertilität
			p.InitItem("crf.FertStart", 0.22, typeof(double),0.0,0.5, "Fertilität: Prä-Oviposition");
			p.InitItem("crf.FertStartExp", 8.0, typeof(double),2.0,10.0, "Fertilität: Faktor Anstieg Eiablage");
			p.InitItem("crf.FertEnd", 0.65, typeof(double),0.51,0.99, "Fertilität: Post-Oviposition");  //prüfen! könnte auch 35% sein
			p.InitItem("crf.FertEndExp", 1.0, typeof(double),1.0,10.0, "Fertilität: Faktor Abstieg Eiablage");
			p.InitItem("crf.FertSumEgg", 30.0, typeof(double),10.0,100.0, "Fertilität: Anzahl Eier/Fliege"); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
			p.InitItem("crf.FertCluster", 1.0, typeof(double), 1.0, 10.0, "Fertilität: Eiablage-Cluster");

			//Flugeinschränkung durch Wind
			p.InitItem("crf.IsWr", true, typeof(bool),0.0,1.0, "Flug durch Wind eingeschränkt?");
			p.InitItem("crf.WrThr", 3.0, typeof(double),1.0,10.0, "Grenzwert (m/s) für Flughemmung");
			p.InitItem("crf.WrInc", 0.20, typeof(double),0.1,0.9, "Anstieg Flughemmung pro zusätzl. m/s");

			//Diapause
			p.InitItem("crf.IsDia", true, typeof(bool),0.0,1.0, "Diapause (Winterruhe) berechnen?");
			p.InitItem("crf.DiaDate", 225, typeof(int),200.0,300.0, "frühester Eintritt in Diapause (Tag des Jahres)");
			p.InitItem("crf.DiaThr", 15.0, typeof(double),10.0,18.0, "Temperaturschwelle für Auslösen d. Diapause");
			p.InitItem("crf.DiaDur", 3, typeof(int),1.0,10.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause");

			//Ästivation
			p.InitItem("crf.IsAest", true, typeof(bool),0.0,1.0, "Ästivation(Sommerruhe) berechnen?");// true
			p.InitItem("crf.AestThr", 20.0, typeof(double),15.0,35.0, "Temperaturschwelle für Auslösen d. Ästivation");
			p.InitItem("crf.AestVar", 40.0, typeof(double),5.0,50.0, "Faktor Zunahme Wahrscheinlichkeit Ästivation");		// ?? klären!
			p.InitItem("crf.AestDropDiff", 2.0, typeof(double),0.0,5.0, "Differenz für Aufheben d. Ästivation");
			p.InitItem("crf.AestMinAge", 0.0, typeof(double), 0.0, 0.5, "Minimum biol. Alter für Ästivation");
			p.InitItem("crf.AestMaxAge", 1.0, typeof(double), 0.5, 1.0, "Maximum biol. Alter für Ästivation");


			return p;
		}

		protected override int GetStartPopulation()
		{
			return (int)_workingParams.GetValue("crf.StartPop");
		}

		public override int GetMaxGenerations()
		{
			return 10;
		}


		#endregion

		#region Wetterdaten vorbereiten

		// potentiellen Sim-Zeitraum
		// Bodentemp. ganzjährig?
		// Prognose von tatsächlichen Wetterdaten unterscheiden
		// Prognose ab?
		public override bool PrepareWeatherData()
		{
			Weather.AirTempAdjustment = (Double)_workingParams.GetValue("crf.AdjAir");

			Weather.SoilTempAdjustment = (Double)_workingParams.GetValue("crf.AdjSoil");
			Weather.WindSpeedAdjustment = (Double)_workingParams.GetValue("crf.AdjWind");
			Weather.UseSummerSoil= (bool)_workingParams.GetValue("crf.UseSummerSoil");

			_simAirTemps = Weather.GetSimAirTemp();
			_simSoilTemps = Weather.GetSimSoilTemp();
			_simWindSpeeds = Weather.GetSimWind();

			int startIndex = (int)_workingParams.GetValue("crf.SimStart");
			int endIndex = 365;

			//int endIndex = (int)_workingParams.GetValue("crf.SimEnd");

			int fpi = Weather.GetFirstPossibleSimIndex(startIndex);
			int lpi = Weather.GetLastPossibleSimIndex(startIndex);

			if ((startIndex < fpi) || (lpi <= fpi)) // keine lückenlosen Wetterdaten für Zeitraum
				return false;

			_firstSimIndex = startIndex;
			_lastSimIndex = Math.Min(endIndex, lpi);
			_prognStartIndex = Weather.GetPrognStartIndex(startIndex);

			return true;

		}


		#endregion


		#region lookup-tabels

		protected override void InitTableTransition()
		{
			double t = 0.0;
			for (int stage=0; stage <=4; stage++)
			{
				switch (stage)
				{
					case 0: t = (Double)_workingParams.GetValue("crf.TransEgg"); break;
					case 1: t = (Double)_workingParams.GetValue("crf.TransLarva"); break;
					case 2: t = (Double)_workingParams.GetValue("crf.TransPupa"); break;
					case 3: t = (Double)_workingParams.GetValue("crf.TransFly"); break;
					case 4: t = (Double)_workingParams.GetValue("crf.TransWiPupa"); break; 

				}
				Double BioAge = 0.0;
				for(int i = 0; i<= 1400; i++)
				{
					_tableTransition[stage, i] = SimFunctions.Weibul(BioAge, t);
					BioAge += 0.001;
            }
			}
		}

		protected override void InitTableDev()
		{
			
			double eggTmax = (Double)_workingParams.GetValue("crf.DevEggTmax");
			double eggTopt = (Double)_workingParams.GetValue("crf.DevEggTopt");
			double eggQ = (Double)_workingParams.GetValue("crf.DevEggQ");
			double eggL = (Double)_workingParams.GetValue("crf.DevEggL");
			double eggKmax = (Double)_workingParams.GetValue("crf.DevEggKmax");

			
			double larvaTmax = (Double)_workingParams.GetValue("crf.DevLarvaTmax");
			double larvaTopt = (Double)_workingParams.GetValue("crf.DevLarvaTopt");
			double larvaQ = (Double)_workingParams.GetValue("crf.DevLarvaQ");
			double larvaL = (Double)_workingParams.GetValue("crf.DevLarvaL");
			double larvaKmax = (Double)_workingParams.GetValue("crf.DevLarvaKmax");

			
			double pupaTmax = (Double)_workingParams.GetValue("crf.DevPupaTmax");
			double pupaTopt = (Double)_workingParams.GetValue("crf.DevPupaTopt");
			double pupaQ = (Double)_workingParams.GetValue("crf.DevPupaQ");
			double pupaL = (Double)_workingParams.GetValue("crf.DevPupaL");
			double pupaKmax = (Double)_workingParams.GetValue("crf.DevPupaKmax");

			
			double flyTmax = (Double)_workingParams.GetValue("crf.DevFlyTmax");
			double flyTopt = (Double)_workingParams.GetValue("crf.DevFlyTopt");
			double flyQ = (Double)_workingParams.GetValue("crf.DevFlyQ");
			double flyL = (Double)_workingParams.GetValue("crf.DevFlyL");
			double flyKmax = (Double)_workingParams.GetValue("crf.DevFlyKmax");

			
			double wipupaTmax = (Double)_workingParams.GetValue("crf.DevWiPupaTmax");
			double wipupaTopt = (Double)_workingParams.GetValue("crf.DevWiPupaTopt");
			double wipupaQ = (Double)_workingParams.GetValue("crf.DevWiPupaQ");
			double wipupaL = (Double)_workingParams.GetValue("crf.DevWiPupaL");
			double wipupaKmax = (Double)_workingParams.GetValue("crf.DevWiPupaKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				double airTemp = _simAirTemps[i];
				_tableDev[(int)DevStage.Fly, i] = SimFunctions.ONeal(airTemp,  flyTmax, flyTopt, flyQ,flyL, flyKmax);

				double soilTemp = _simSoilTemps[i];
				_tableDev[(int)DevStage.Egg, i] = SimFunctions.ONeal(airTemp,  eggTmax, eggTopt, eggQ, eggL, eggKmax);
				_tableDev[(int)DevStage.Larva, i] = SimFunctions.ONeal(airTemp,  larvaTmax, larvaTopt, larvaQ,larvaL, larvaKmax);
				_tableDev[(int)DevStage.Pupa, i] = SimFunctions.ONeal(airTemp,  pupaTmax, pupaTopt, pupaQ, pupaL, pupaKmax);
				_tableDev[(int)DevStage.WiPupa, i] = SimFunctions.ONeal(airTemp,  wipupaTmax, wipupaTopt, wipupaQ,wipupaL, wipupaKmax);
			}
		}

		protected override void InitTableDiapause()
		{
			// todo: Diapause implementieren
			//throw new NotImplementedException();
		}

		protected override void InitTableFert()
		{

			double startFert = (Double)_workingParams.GetValue("crf.FertStart");
			double startSkew = (Double)_workingParams.GetValue("crf.FertStartExp");
			double endFert = (Double)_workingParams.GetValue("crf.FertEnd");
			double endSkew = (Double)_workingParams.GetValue("crf.FertEndExp");
			double sumEgg = (Double)_workingParams.GetValue("crf.FertSumEgg");

			double calibFactor = CalcFertMult(startFert, startSkew, endFert, endSkew, sumEgg);

			double bioAge = 0.0;
			double fert = 0.0;
			for (int i=0; i<=1000; i++)
			{
				_tableFert[i] = fert;
				bioAge += 0.001;
				fert += SimFunctions.FertilityFkt(bioAge, startFert, startSkew, endFert, endSkew, calibFactor);
			}

		}

		protected override void InitTableFlightAct()
		{
			bool isWindRestr = (bool)_workingParams.GetValue("crf.IsWr");
			double windThreshold = (Double)_workingParams.GetValue("crf.WrThr");
			double windInc = (Double)_workingParams.GetValue("crf.WrInc");

			double flyMaxDev = (Double)_workingParams.GetValue("crf.DevFlyKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				_tableFlightAct[i] = _tableDev[(int)DevStage.Fly, i] / flyMaxDev; // Flugaktivität abh. von akt. Entwicklungsrate

				if (isWindRestr)// Einschränkung durch Wind
				{
					double windSpeed = _simWindSpeeds[i];
					double w = 1.0 - (windSpeed - windThreshold) * (windInc);
					_tableWindRestr[i] = Math.Min(1.0, Math.Max(0.0, w));
				}
				else
					_tableWindRestr[i] = 1.0;
			}
		}

		protected override void InitTableMortality()
		{
			double mortEgg = (Double)_workingParams.GetValue("crf.MortEgg");
			double mortLarva = (Double)_workingParams.GetValue("crf.MortLarva");
			double mortPupa = (Double)_workingParams.GetValue("crf.MortPupa");
			double mortFly = (Double)_workingParams.GetValue("crf.MortFly");
			double mortWiPupa = (Double)_workingParams.GetValue("crf.MortWiPupa");


			bool isVarEggMort = (bool)_workingParams.GetValue("crf.MortEggIsTDep");
			double mortEggThr = (Double)_workingParams.GetValue("crf.MortEggThr");
			double mortEggInc = (Double)_workingParams.GetValue("crf.MortEggInc");

			bool isVarLarvaMort = (bool)_workingParams.GetValue("crf.MortLarvaIsTDep");
			double mortLarvaThr = (Double)_workingParams.GetValue("crf.MortLarvaThr");
			double mortLarvaInc = (Double)_workingParams.GetValue("crf.MortLarvaInc");

			for (int i = _firstSimIndex; i <_lastSimIndex; i++)
			{
				double soilTemp = _simSoilTemps[i];//  Weather.SoilTemps[i];
				double mort;

				if (isVarEggMort)
				{
					mort = Math.Max(mortEgg, (soilTemp - mortEggThr) * mortEggInc);
					mort = Math.Min(mort, 1.0);
				}
				else
				{ 
					mort = mortEgg;
				}
				_tableMortality[(int)DevStage.Egg, i] = mort;

				if (isVarLarvaMort)
				{
					mort = Math.Max(mortLarva, (soilTemp - mortLarvaThr) * mortLarvaInc);
					mort = Math.Min(mort, 1.0);
				}
				else
				{
					mort = mortLarva;
				}
				_tableMortality[(int)DevStage.Larva, i] = mort;

				// feste Mortalitäten für andere Stadien
				_tableMortality[(int)DevStage.Pupa, i] = mortPupa;
				_tableMortality[(int)DevStage.Fly, i] = mortFly;
				_tableMortality[(int)DevStage.WiPupa, i] = mortWiPupa;

			}
		}

		private double CalcFertMult(double startFert, double startSkew, double endFert, double endSkew, double SumEgg)
		{
			double bioAge = 0.0;
			double sum = 0.0;

			for (int i = 0; i <= 1000; i++)
			{
				sum += SimFunctions.FertilityFkt(bioAge, startFert, startSkew, endFert, endSkew, 0.001);
			
			//	sum += ((1 - Math.Exp(-Math.Pow(bioAge/startFert, startSkew))) *		
			//				Math.Exp(-Math.Pow(bioAge / (1.0 - endFert), endSkew)) * 0.001);
				bioAge += 0.001;
			}

			return SumEgg/(sum*1000.0);
		}

		#endregion

		#region Simulationsschleife

		public double IndividualAestThreshold
		{
			get
			{ 
				bool isAest = (bool)_workingParams.GetValue("crf.IsAest");
				if (!isAest)
					return 100.0;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				double threshold = (double)_workingParams.GetValue("crf.AestThr");
				double variance = (double)_workingParams.GetValue("crf.AestVar");

				return SimFunctions.AestivTemp(_random, threshold, variance);
			}
		}

		public double AestMinAge
		{
			get { return (double)_workingParams.GetValue("crf.AestMinAge"); }
		}

		public double AestMaxAge
		{
			get { return (double)_workingParams.GetValue("crf.AestMaxAge"); }
		}

		public double AestDropDiff
		{
			get {return (double)_workingParams.GetValue("crf.AestDropDiff"); }
		}

		public double FertCluster
		{
			get { return (double)_workingParams.GetValue("crf.FertCluster"); }
		}

		public int DiapauseIndex
		{
			get
			{ 
				bool isDiapause = (bool)_workingParams.GetValue("crf.IsDia");
				if (!isDiapause)
					return 366;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				return (int)_workingParams.GetValue("crf.DiaDate");
			}
		}

		public double DiapauseTemp
		{
			get {  return (double)_workingParams.GetValue("crf.DiaThr"); }

		}

		public int DiapauseDur
		{
			get { return (int)_workingParams.GetValue("crf.DiaDur"); }

		}

		protected override void Individual(DevStage startStage, double startAge, int dayIndex, int generation, bool isDiapauseGen)
		{
			IndividualCRF.CreateAndLive(this, startStage, startAge, dayIndex, generation, isDiapauseGen);
		}
		
		#endregion

	}
}

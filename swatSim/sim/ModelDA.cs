//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace swatSim.sim
//{
//	class ModelDA
//	{
//	}
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	public class ModelDA : ModelBase
	{
		#region Construction + Parameterdefinition

		public ModelDA(string name, WeatherData weather, PopulationData population, SimParamData explicitParams = null, SimParamData locationParams = null) : base(name, weather, population, explicitParams, locationParams)
		{
			//InitParamDefaults();
			//_workingParams = workspace.SimParameters; GetModelParameters(ModelType.DR);
		}

		public override string GetParamPrefix()
		{
			return "da";
		}


		// Optimierte Parameter vom 6. 4. 20
		public override SimParamData GetDefaultParams()
		{
			// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

			SimParamData p = new SimParamData();

			// Grundlegendes

			p.InitItem("da.StartPop", 1000, typeof(int), 500.0, 10000.0, "Startpopulation (Anzahl Individuen)", 1);
			p.InitItem("da.MaxGen", 5, typeof(int), 4, 7, "maximale Generationenzahl", 3);

			p.InitItem("da.StartAge",0.20, typeof(double), 0.0, 0.9, "Biol. Alter der Startpopulation", 3);

			p.InitItem("da.SimStart", 59, typeof(int), 1.0, 180.0, "Simulationsstart (Tag des Jahres)", 1); // (1. März)

			p.InitItem("da.UseOnlyAir", false, typeof(bool), 0.0, 1.0, "nur Lufttemperatur nutzen?", 1);
			p.InitItem("da.SummerSoilRel", 0.5, typeof(double), 0.0, 1.0, "Mischungsverh. Boden-/Lufttemp ab 1.6.", 1); //0= 0%Boden, 100%Luft; 0.5= (Boden +Luft)/2; 1= 100% Boden
			p.InitItem("da.AdjAir", 0.0, typeof(double), -5.0, 5.0, "Korrektur Lufttemp", 1);
			p.InitItem("da.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp", 1);

			//Mortalitäten
			p.InitItem("da.MortEgg", 0.05, typeof(double), 0.01, 0.25, "tägl. Mortalität Eier", 3);
			p.InitItem("da.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven", 3);
			p.InitItem("da.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen", 3);
			p.InitItem("da.MortFly", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen", 3);
			p.InitItem("da.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen", 3);

			p.InitItem("da.MortEggIsTDep", false, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?", 3);
			p.InitItem("da.MortEggThr", 21.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Ei-Mortalität", 3);
			p.InitItem("da.MortEggInc", 0.15, typeof(double), 0.01, 0.5, "Anstieg Ei-Mortalität pro Grad C", 3);

			p.InitItem("da.MortLarvaIsTDep", false, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?", 3);
			p.InitItem("da.MortLarvaMaxAge", 0.25, typeof(double), 0.0, 1.0, "Maximalalter für erhöhte Larven-Mortalität", 3);
			p.InitItem("da.MortLarvaThr", 22.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Larven-Mortalität", 3);
			p.InitItem("da.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C", 3);

			//Transitionen - Übergänge ins nächste Stadium
			p.InitItem("da.TransWiPupa", 0.14, typeof(double), 0.0, 0.2, "Faktor Transition Überwinterungsstadium->Adult", 3);
			p.InitItem("da.TransEgg", 0.14, typeof(double), 0.0, 0.2, "Faktor Transition Ei->Larve", 3);
			p.InitItem("da.TransLarva", 0.14, typeof(double), 0.0, 0.02, "Faktor Transition Larve->Puppe", 3);
			p.InitItem("da.TransPupa", 0.14, typeof(double), 0.0, 0.02, "Faktor Transition Puppe->Adult", 3);
			p.InitItem("da.TransFly", 0.16, typeof(double), 0.0, 0.02, "Faktor Transition Adult->Tod", 3);

			//Entwicklungsparameter
			p.InitItem("da.DevEggTmax", 40.0, typeof(double), 30.0, 45.0, "Entwicklung Ei: Temperaturmaximum", 3);
			p.InitItem("da.DevEggTopt", 29.5, typeof(double), 15.0, 35.0, "Entwicklung Ei: Temperaturoptimum", 3);
			p.InitItem("da.DevEggQ", 1.83, typeof(double), 1.1, 4.0, "Entwicklung Ei: Temperatur-Spezifität", 3);
			p.InitItem("da.DevEggL", 0.025, typeof(double), 0.0, 0.1, "Entwicklung Ei: Konvexität linke Flanke", 3);
			p.InitItem("da.DevEggKmax", 0.62, typeof(double), 0.1, 0.9, "Entwicklung Ei: max. tägl. Entwicklungsrate", 3);


			p.InitItem("da.DevLarvaTmax", 36.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum", 3);
			p.InitItem("da.DevLarvaTopt", 26.5, typeof(double), 15.0, 34.0, "Entwicklung Larve: Temperaturoptimum", 3);
			p.InitItem("da.DevLarvaQ", 1.90, typeof(double), 1.1, 4.0, "Entwicklung Larve: Temperatur-Spezifität", 3);
			p.InitItem("da.DevLarvaL", 0.008, typeof(double), 0.0, 0.1, "Entwicklung Larve: Konvexität linke Flanke", 3);
			p.InitItem("da.DevLarvaKmax", 0.112, typeof(double), 0.05, 0.25, "Entwicklung Larve: max. tägl. Entwicklungsrate", 3);


			p.InitItem("da.DevPupaTmax", 40.0, typeof(double), 28.0, 42.0, "Entwicklung Puppe: Temperaturmaximum", 3);
			p.InitItem("da.DevPupaTopt", 30.0, typeof(double), 20.0, 35.0, "Entwicklung Puppe: Temperaturoptimum", 3);
			p.InitItem("da.DevPupaQ", 1.50, typeof(double), 1.1, 4.0, "Entwicklung Puppe: Temperatur-Spezifität", 3);
			p.InitItem("da.DevPupaL", 0.030, typeof(double), 0.0, 0.1, "Entwicklung Puppe: Konvexität linke Flanke", 3);
			p.InitItem("da.DevPupaKmax", 0.092, typeof(double), 0.05, 0.2, "Entwicklung Puppe: max. tägl. Entwicklungsrate", 3);


			p.InitItem("da.DevFlyTmax", 35.0, typeof(double), 32.0, 45.0, "Entwicklung Fliege: Temperaturmaximum", 3);
			p.InitItem("da.DevFlyTopt", 30.0, typeof(double), 20.0, 32.0, "Entwicklung Fliege: Temperaturoptimum", 3);
			p.InitItem("da.DevFlyQ", 1.65, typeof(double), 1.1, 4.0, "Entwicklung Fliege: Temperatur-Spezifität", 3);
			p.InitItem("da.DevFlyL", 0.008, typeof(double), 0.0, 0.1, "Entwicklung Fliege: Konvexität linke Flanke", 3);
			p.InitItem("da.DevFlyKmax", 0.072, typeof(double), 0.05, 0.2, "Entwicklung Fliege: max. tägl. Entwicklungsrate", 3);

			p.InitItem("da.DevWiPupaTmax", 40.00, typeof(double), 29.0, 42.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
			p.InitItem("da.DevWiPupaTopt", 30.00, typeof(double), 20.0, 35.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
			p.InitItem("da.DevWiPupaQ", 1.50, typeof(double), 1.1, 2.0, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
			p.InitItem("da.DevWiPupaL", 0.0030, typeof(double), 0.01, 0.05, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
			p.InitItem("da.DevWiPupaKmax", 0.092, typeof(double), 0.08, 0.17, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

			//Fertilität
			p.InitItem("da.FertPrae", 0.30, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition", 3);
			p.InitItem("da.FertStartExp", 8.0, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
			p.InitItem("da.FertPost", 0.72, typeof(double), 0.5, 1.0, "Fertilität: Post-Oviposition", 3);
			p.InitItem("da.FertEndExp", 1.0, typeof(double), 1.1, 10.0, "Fertilität: Faktor Abstieg Eiablage", 3);
			p.InitItem("da.FertSumEgg", 30.0, typeof(double), 10.0, 100.0, "Fertilität: Anzahl Eier/Fliege", 3); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
			p.InitItem("da.FertCluster", 1.0, typeof(double), 1.0, 10.0, "Fertilität: Eiablage-Cluster", 3);


			//Diapause
			p.InitItem("da.IsDia", true, typeof(bool), 0.0, 1.0, "Diapause (Winterruhe) berechnen?", 3);
			p.InitItem("da.DiaDate", 230, typeof(int), 200.0, 300.0, "frühester Eintritt in Diapause (Tag des Jahres)", 3);
			p.InitItem("da.DiaThr", 15.0, typeof(double), 5.0, 25.0, "Temperaturschwelle für Auslösen d. Diapause", 3);
			p.InitItem("da.DiaDur", 3, typeof(int), 1.0, 25.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause", 3);


			//Ästivation
			p.InitItem("da.IsAest", true, typeof(bool), 0.0, 1.0, "Ästivation(Sommerruhe) berechnen?", 3);// true
			p.InitItem("da.AestThr", 21.00, typeof(double), 15.0, 35.0, "Temperaturschwelle für Auslösen d. Ästivation", 3);
			p.InitItem("da.AestVar", 2.43, typeof(double), 0.0, 3.0, "Streuungsfaktor Ästivation", 3);     // ?? klären!
			p.InitItem("da.AestDropDiff", 2.0, typeof(double), 0.0, 5.0, "Differenz für Aufheben d. Ästivation", 3);
			p.InitItem("da.AestMinAge", 0.0, typeof(double), 0.0, 0.25, "Minimum biol. Alter für Ästivation", 3);
			p.InitItem("da.AestMaxAge", 1.0, typeof(double), 0.25, 1.4, "Maximum biol. Alter für Ästivation", 3);


			return p;
		}


		protected override int GetStartPopulation()
		{
			return (int)_workingParams.GetValue("da.StartPop");
		}

		protected override double GetStartAge()
		{
			return (double)_workingParams.GetValue("da.StartAge");
		}

		public override void SetMaxGenerations()
		{
			MaxGenerations= (int)_workingParams.GetValue("da.MaxGen");
		}


		#endregion

		#region Wetterdaten vorbereiten

		// potentiellen Sim-Zeitraum
		// Bodentemp. ganzjährig?
		// Prognose von tatsächlichen Wetterdaten unterscheiden
		// Prognose ab?
		public override bool PrepareWeatherData()
		{
			Weather.UseOnlyAir = (bool)_workingParams.GetValue("da.UseOnlyAir");
			Weather.AirTempAdjustment = (Double)_workingParams.GetValue("da.AdjAir");
			Weather.SoilTempAdjustment = (Double)_workingParams.GetValue("da.AdjSoil");
			Weather.SummerSoilRel = (Double)_workingParams.GetValue("da.SummerSoilRel");

			_simAirTemps = Weather.GetSimAirTemp();
			_simSoilTemps = Weather.GetSimSoilTemp();
			//_simVPD = Weather.GetSimVpd();

			int startIndex = (int)_workingParams.GetValue("da.SimStart");
			int endIndex = 365;

			//int endIndex = (int)_workingParams.GetValue("da.SimEnd");

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

		protected override void InitTransitions()
		{
			_transitions = new double[5];
			_transitions[0] = (Double)_workingParams.GetValue("da.TransEgg");
			_transitions[1] = (Double)_workingParams.GetValue("da.TransLarva");
			_transitions[2] = (Double)_workingParams.GetValue("da.TransPupa");
			_transitions[3] = (Double)_workingParams.GetValue("da.TransFly");
			_transitions[4] = (Double)_workingParams.GetValue("da.TransWiPupa");
		}

		//protected override void InitTableTransition()
		//{

		//	double t = 0.0;
		//	for (int stage = 0; stage <= 4; stage++)
		//	{
		//		switch (stage)
		//		{

		//			case 0:
		//				_maxDevRates[0] = (Double)_workingParams.GetValue("da.DevEggKmax");
		//				t = (Double)_workingParams.GetValue("da.TransEgg");
		//				break;
		//			case 1:
		//				_maxDevRates[1] = (Double)_workingParams.GetValue("da.DevLarvaKmax");
		//				t = (Double)_workingParams.GetValue("da.TransLarva");
		//				break;

		//			case 2:
		//				_maxDevRates[2] = (Double)_workingParams.GetValue("da.DevPupaKmax");
		//				t = (Double)_workingParams.GetValue("da.TransPupa");
		//				break;
		//			case 3:
		//				_maxDevRates[3] = (Double)_workingParams.GetValue("da.DevFlyKmax");
		//				t = (Double)_workingParams.GetValue("da.TransFly");
		//				break;
		//			case 4:
		//				_maxDevRates[4] = (Double)_workingParams.GetValue("da.DevWiPupaKmax");
		//				t = (Double)_workingParams.GetValue("da.TransWiPupa");
		//				break;

		//		}
		//		Double BioAge = 0.0;
		//		for (int i = 0; i <= 1400; i++)
		//		{
		//			_tableTransition[stage, i] = SimFunctions.Sigmoid(BioAge, t);
		//			BioAge += 0.001;
		//		}

		//	}
		//}

		protected override void InitTableDev()
		{

			double eggTmax = (Double)_workingParams.GetValue("da.DevEggTmax");
			double eggTopt = (Double)_workingParams.GetValue("da.DevEggTopt");
			double eggQ = (Double)_workingParams.GetValue("da.DevEggQ");
			double eggL = (Double)_workingParams.GetValue("da.DevEggL");
			double eggKmax = (Double)_workingParams.GetValue("da.DevEggKmax");


			double larvaTmax = (Double)_workingParams.GetValue("da.DevLarvaTmax");
			double larvaTopt = (Double)_workingParams.GetValue("da.DevLarvaTopt");
			double larvaQ = (Double)_workingParams.GetValue("da.DevLarvaQ");
			double larvaL = (Double)_workingParams.GetValue("da.DevLarvaL");
			double larvaKmax = (Double)_workingParams.GetValue("da.DevLarvaKmax");


			double pupaTmax = (Double)_workingParams.GetValue("da.DevPupaTmax");
			double pupaTopt = (Double)_workingParams.GetValue("da.DevPupaTopt");
			double pupaQ = (Double)_workingParams.GetValue("da.DevPupaQ");
			double pupaL = (Double)_workingParams.GetValue("da.DevPupaL");
			double pupaKmax = (Double)_workingParams.GetValue("da.DevPupaKmax");


			double flyTmax = (Double)_workingParams.GetValue("da.DevFlyTmax");
			double flyTopt = (Double)_workingParams.GetValue("da.DevFlyTopt");
			double flyQ = (Double)_workingParams.GetValue("da.DevFlyQ");
			double flyL = (Double)_workingParams.GetValue("da.DevFlyL");
			double flyKmax = (Double)_workingParams.GetValue("da.DevFlyKmax");


			double wipupaTmax = (Double)_workingParams.GetValue("da.DevWiPupaTmax");
			double wipupaTopt = (Double)_workingParams.GetValue("da.DevWiPupaTopt");
			double wipupaQ = (Double)_workingParams.GetValue("da.DevWiPupaQ");
			double wipupaL = (Double)_workingParams.GetValue("da.DevWiPupaL");
			double wipupaKmax = (Double)_workingParams.GetValue("da.DevWiPupaKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				double airTemp = _simAirTemps[i];
				_tableDev[(int)DevStage.Fly, i] = SimFunctions.ONeal(airTemp, flyTmax, flyTopt, flyQ, flyL, flyKmax);

				double soilTemp = _simSoilTemps[i];
				_tableDev[(int)DevStage.Egg, i] = SimFunctions.ONeal(soilTemp, eggTmax, eggTopt, eggQ, eggL, eggKmax);
				_tableDev[(int)DevStage.Larva, i] = SimFunctions.ONeal(soilTemp, larvaTmax, larvaTopt, larvaQ, larvaL, larvaKmax);
				_tableDev[(int)DevStage.Pupa, i] = SimFunctions.ONeal(soilTemp, pupaTmax, pupaTopt, pupaQ, pupaL, pupaKmax);
				_tableDev[(int)DevStage.WiPupa, i] = SimFunctions.ONeal(soilTemp, wipupaTmax, wipupaTopt, wipupaQ, wipupaL, wipupaKmax);
			}
		}

		//protected override void InitTableDiapause()
		//{
		//	// todo: Diapause implementieren
		//	//throw new NotImplementedException();
		//}

		protected override void InitTableFert()
		{

			double startFert = (Double)_workingParams.GetValue("da.FertPrae");
			double startSkew = (Double)_workingParams.GetValue("da.FertStartExp");
			double endFert = (Double)_workingParams.GetValue("da.FertPost");
			double endSkew = (Double)_workingParams.GetValue("da.FertEndExp");
			double sumEgg = (Double)_workingParams.GetValue("da.FertSumEgg");

			double calibFactor = CalcFertMult(startFert, startSkew, endFert, endSkew, sumEgg);

			double bioAge = 0.0;
			double fert = 0.0;
			for (int i = 0; i <= 1000; i++)
			{
				_tableFert[i] = fert;
				bioAge += 0.001;
				fert += SimFunctions.FertilityFkt(bioAge, startFert, startSkew, endFert, endSkew, calibFactor);
			}

		}

		protected override void InitTableFlightAct()
		{


			double flyMaxDev = (Double)_workingParams.GetValue("da.DevFlyKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				_tableFlightAct[i] = _tableDev[(int)DevStage.Fly, i] / flyMaxDev; // Flugaktivität abh. von akt. Entwicklungsrate
			}
		}

		protected override void InitTableMortality()
		{
			double mortEgg = (Double)_workingParams.GetValue("da.MortEgg");
			double mortLarva = (Double)_workingParams.GetValue("da.MortLarva");
			double mortPupa = (Double)_workingParams.GetValue("da.MortPupa");
			double mortFly = (Double)_workingParams.GetValue("da.MortFly");
			double mortWiPupa = (Double)_workingParams.GetValue("da.MortWiPupa");


			bool isVarEggMort = (bool)_workingParams.GetValue("da.MortEggIsTDep");
			double mortEggThr = (Double)_workingParams.GetValue("da.MortEggThr");
			double mortEggInc = (Double)_workingParams.GetValue("da.MortEggInc");

			bool isVarLarvaMort = (bool)_workingParams.GetValue("da.MortLarvaIsTDep");
			double mortLarvaThr = (Double)_workingParams.GetValue("da.MortLarvaThr");
			double mortLarvaInc = (Double)_workingParams.GetValue("da.MortLarvaInc");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
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
					_tableMortality[5, i] = mort; //erhöhte Mortalität im Junglarvenstadium
				}
				else
				{
					_tableMortality[5, i] = mortLarva;
				}

				// feste Mortalitäten für andere Stadien
				_tableMortality[(int)DevStage.Larva, i] = mortLarva;
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

			return SumEgg / (sum * 1000.0);
		}

		#endregion

		#region Simulationsschleife

		public double IndividualAestThreshold
		{
			get
			{
				bool isAest = (bool)_workingParams.GetValue("da.IsAest");
				if (!isAest)
					return 100.0;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				double threshold = (double)_workingParams.GetValue("da.AestThr");
				double variance = (double)_workingParams.GetValue("da.AestVar");

				return SimFunctions.AestivTemp(GetRandom, threshold, variance);
			}
		}

		public double AestMinAge
		{
			get { return (double)_workingParams.GetValue("da.AestMinAge"); }
		}

		public double AestMaxAge
		{
			get { return (double)_workingParams.GetValue("da.AestMaxAge"); }
		}

		public double AestDropDiff
		{
			get { return (double)_workingParams.GetValue("da.AestDropDiff"); }
		}

		public double MortLarvaMaxAge
		{
			get { return (double)_workingParams.GetValue("da.MortLarvaMaxAge"); }
		}

		public double FertCluster
		{
			get { return (double)_workingParams.GetValue("da.FertCluster"); }
		}

		public int DiapauseIndex
		{
			get
			{
				bool isDiapause = (bool)_workingParams.GetValue("da.IsDia");
				if (!isDiapause)
					return 366;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				return (int)_workingParams.GetValue("da.DiaDate");
			}
		}

		public double DiapauseTemp
		{
			get { return (double)_workingParams.GetValue("da.DiaThr"); }

		}

		public int DiapauseDur
		{
			get { return (int)_workingParams.GetValue("da.DiaDur"); }

		}

		protected override void Individual(DevStage startStage, double startAge, int dayIndex, int generation, bool isDiapauseGen)
		{
			IndividualDA.CreateAndLive(this, startStage, startAge, dayIndex, generation, isDiapauseGen);
		}

		#endregion

	}
}
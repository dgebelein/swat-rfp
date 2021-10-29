using swat.defs;
using swat.iodata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swat.sim
{
	class ModelPR : ModelBase
	{
		#region Construction + Parameterdefinition

		public ModelPR(WorkspaceData workspace, SimParamData explicitParams = null) : base(workspace, explicitParams)
		{
			//InitParamDefaults();
			//_workingParams = workspace.SimParameters; GetModelParameters(ModelType.DR);
		}

		public override string GetParamPrefix()
		{
			return "pr";
		}

		public override SimParamData GetDefaultParams()
		{
			// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

			SimParamData p = new SimParamData();

			// Grundlegendes

			p.InitItem("pr.StartPop", 3000, typeof(int), 500.0, 10000.0, "Startpopulation (Anzahl Individuen)");

			p.InitItem("pr.SimStart", 59, typeof(int), 1.0, 180.0, "Simulationsstart (Tag des Jahres)"); // (1. März)
			p.InitItem("pr.MonOnField", true, typeof(bool), 0.499, 0.501, "Monitoring auf dem Feld? (nein: Hecke)");

			p.InitItem("pr.UseSummerSoil", true, typeof(bool), 0.499, 0.501, "auch nach 1. Juni noch Bodentemp. verwenden?(nein: Lufttemp.)");
			p.InitItem("pr.AdjAir", 0.0, typeof(double), -5.0, 5.0, "Korrektur Lufttemp"); 
			p.InitItem("pr.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp"); 
			//p.InitItem("pr.AdjWind", 1.0, typeof(double), 0.0, 10.0, "Korrektur Wind (Multiplikator)");

			//Mortalitäten
			p.InitItem("pr.MortEgg", 0.05, typeof(double), 0.01, 0.25, "tägl. Mortalität Eier");
			p.InitItem("pr.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven");
			p.InitItem("pr.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen");
			p.InitItem("pr.MortFly", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen");
			p.InitItem("pr.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen");

			p.InitItem("pr.MortEggIsTDep", false, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?");
			p.InitItem("pr.MortEggThr", 20.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Ei-Mortalität");    
			p.InitItem("pr.MortEggInc", 0.15, typeof(double), 0.01, 0.5, "Anstieg Ei-Mortalität pro Grad C");   

			p.InitItem("pr.MortLarvaIsTDep", false, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?");
			p.InitItem("pr.MortLarvaMaxAge", 0.25, typeof(double), 0.0, 1.0, "Maximalalter für erhöhte Larven-Mortalität");
			p.InitItem("pr.MortLarvaThr", 21.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Larven-Mortalität");    
			p.InitItem("pr.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C");  

			//Transitionen - Übergänge ins nächste Stadium
			p.InitItem("pr.TransWiPupa", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Überwinterungsstadium->Adult");
			p.InitItem("pr.TransEgg", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Ei->Larve");
			p.InitItem("pr.TransLarva", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Larve->Puppe");
			p.InitItem("pr.TransPupa", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Puppe->Adult");
			p.InitItem("pr.TransFly", 5.0, typeof(double), 5.0, 15.0, "Faktor Transition Adult->Tod");

			//Entwicklungsparameter
			p.InitItem("pr.DevEggTmax", 40.0, typeof(double), 30.0, 45.0, "Entwicklung Ei: Temperaturmaximum");
			p.InitItem("pr.DevEggTopt", 25.0, typeof(double), 15.0, 29.0, "Entwicklung Ei: Temperaturoptimum");
			p.InitItem("pr.DevEggQ", 1.77, typeof(double), 1.1, 3.0, "Entwicklung Ei: Steilheit");
			p.InitItem("pr.DevEggL", 0.02, typeof(double), 0.001, 0.1, "Entwicklung Ei: Konvexität linke Flanke");
			p.InitItem("pr.DevEggKmax", 0.194, typeof(double), 0.1, 0.4, "Entwicklung Ei: max. tägl. Entwicklungsrate");


			p.InitItem("pr.DevLarvaTmax", 30.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum");
			p.InitItem("pr.DevLarvaTopt", 22.0, typeof(double), 15.0, 29.0, "Entwicklung Larve: Temperaturoptimum");
			p.InitItem("pr.DevLarvaQ", 1.49, typeof(double), 1.1, 2.5, "Entwicklung Larve: Steilheit");
			p.InitItem("pr.DevLarvaL", 0.015, typeof(double), 0.0, 0.1, "Entwicklung Larve: Konvexität linke Flanke");
			p.InitItem("pr.DevLarvaKmax", 0.038, typeof(double), 0.025, 0.2, "Entwicklung Larve: max. tägl. Entwicklungsrate");


			p.InitItem("pr.DevPupaTmax", 30.0, typeof(double), 28.0, 40.0, "Entwicklung Puppe: Temperaturmaximum");
			p.InitItem("pr.DevPupaTopt", 22.0, typeof(double), 15.0, 27.0, "Entwicklung Puppe: Temperaturoptimum");
			p.InitItem("pr.DevPupaQ", 1.62, typeof(double), 1.1, 2.5, "Entwicklung Puppe: Steilheit");
			p.InitItem("pr.DevPupaL", 0.015, typeof(double), 0.0, 0.1, "Entwicklung Puppe: Konvexität linke Flanke");
			p.InitItem("pr.DevPupaKmax", 0.056, typeof(double), 0.01, 0.2, "Entwicklung Puppe: max. tägl. Entwicklungsrate");


			p.InitItem("pr.DevFlyTmax", 35.0, typeof(double), 25.0, 45.0, "Entwicklung Fliege: Temperaturmaximum");
			p.InitItem("pr.DevFlyTopt", 30.0, typeof(double), 15.0, 25.0, "Entwicklung Fliege: Temperaturoptimum");
			p.InitItem("pr.DevFlyQ", 1.69, typeof(double), 1.1, 2.5, "Entwicklung Fliege: Steilheit");
			p.InitItem("pr.DevFlyL", 0.011, typeof(double), 0.0, 0.1, "Entwicklung Fliege: Konvexität linke Flanke");
			p.InitItem("pr.DevFlyKmax", 0.132, typeof(double), 0.025, 0.2, "Entwicklung Fliege: max. tägl. Entwicklungsrate");


			p.InitItem("pr.DevWiPupaTmax", 29.08,typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum");
			p.InitItem("pr.DevWiPupaTopt", 19.28, typeof(double), 15.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum");
			p.InitItem("pr.DevWiPupaQ", 1.934, typeof(double), 1.1, 2.5, "Entwicklung Winterpuppe: Steilheit");
			p.InitItem("pr.DevWiPupaL", 0.003, typeof(double), 0.0, 0.1, "Entwicklung Winterpuppe: Konvexität linke Flanke");
			p.InitItem("pr.DevWiPupaKmax", 0.036, typeof(double), 0.015, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate");
			
			//p.InitItem("pr.DevWiPupaTmax", 30.0, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum");
			//p.InitItem("pr.DevWiPupaTopt", 19.0, typeof(double), 15.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum");
			//p.InitItem("pr.DevWiPupaQ", 1.59, typeof(double), 1.1, 2.5, "Entwicklung Winterpuppe: Steilheit");
			//p.InitItem("pr.DevWiPupaL", 0.003, typeof(double), 0.0, 0.1, "Entwicklung Winterpuppe: Konvexität linke Flanke");
			//p.InitItem("pr.DevWiPupaKmax", 0.041, typeof(double), 0.015, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate");

			//Fertilität
			p.InitItem("pr.FertPrae", 0.18, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition");
			p.InitItem("pr.FertStartExp", 6.55, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage");
			//p.InitItem("pr.FertPrae", 0.15, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition");
			//p.InitItem("pr.FertStartExp", 6.0, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage");
			p.InitItem("pr.FertPost", 0.20, typeof(double), 0.0, 0.5, "Fertilität: Post-Oviposition");  
			p.InitItem("pr.FertEndExp", 3.0, typeof(double), 1.1, 10.0, "Fertilität: Faktor Abstieg Eiablage");
			p.InitItem("pr.FertSumEgg", 30.0, typeof(double), 10.0, 100.0, "Fertilität: Anzahl Eier/Fliege"); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
			p.InitItem("pr.FertCluster", 1.0, typeof(double), 1.0, 10.0, "Fertilität: Eiablage-Cluster");

			//Flugeinschränkung durch Sättigungsdefizit
			p.InitItem("pr.IsVpd", true, typeof(bool), 0.0, 1.0, "Flug durch hohes Sättigungsdefizit eingeschränkt?");
			p.InitItem("pr.VpdThr", 8.5, typeof(double), 1.0, 15.0, "Grenzwert (hPa) für Flughemmung");
			p.InitItem("pr.VpdInc", 0.07, typeof(double), 0.01, 0.9, "Zunahme Flughemmung pro zusätzl. hPa");
			p.InitItem("pr.IsVpdOvip", false, typeof(bool), 0.0, 1.0, "auch Eiablage durch hohes Sättigungsdefizit eingeschränkt?");

			////Flugeinschränkung durch hohe Temperaturen
			//p.InitItem("pr.IsTInhib", false, typeof(bool), 0.0, 1.0, "Flug durch hohe Temperaturen eingeschränkt?");
			//p.InitItem("pr.TInhibThr", 18.0, typeof(double), 15.0, 30.0, "Grenzwert (°C) für Flughemmung durch hohe Temp");
			//p.InitItem("pr.TInhibInc", 0.05, typeof(double), 0.025, 0.25, "Anstieg Flughemmung pro zusätzl. °C");

			//Diapause
			p.InitItem("pr.IsDia", false, typeof(bool), 0.0, 1.0, "Diapause (Winterruhe) berechnen?");
			p.InitItem("pr.DiaDate", 242, typeof(int), 200.0, 300.0, "frühester Eintritt in Diapause (Tag des Jahres)");
			p.InitItem("pr.DiaThr", 13.0, typeof(double), 10.0, 18.0, "Temperaturschwelle für Auslösen d. Diapause");
			p.InitItem("pr.DiaDur", 3, typeof(int), 1.0, 10.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause");

			//Ästivation
			p.InitItem("pr.IsAest", true, typeof(bool), 0.0, 1.0, "Ästivation(Sommerruhe) berechnen?");// true
			p.InitItem("pr.AestThr", 20.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für Auslösen d. Ästivation");
			p.InitItem("pr.AestVar", 3.0, typeof(double), 0.0, 10.0, "Streuungsfaktor Ästivation");     
			p.InitItem("pr.AestDropDiff", 2.0, typeof(double), 0.0, 5.0, "Differenz für Aufheben d. Ästivation");
			p.InitItem("pr.AestMinAge", 0.0, typeof(double), 0.0, 0.25, "Minimum biol. Alter für Ästivation");
			p.InitItem("pr.AestMaxAge", 1.0, typeof(double), 0.25, 1.4, "Maximum biol. Alter für Ästivation");


			return p;
		}

		protected override int GetStartPopulation()
		{
			return (int)_workingParams.GetValue("pr.StartPop");
		}

		public override int GetMaxGenerations()
		{
			return 10;
		}

		public bool MonOnField
		{
			get { return (bool)_workingParams.GetValue("pr.MonOnField"); }
		}

		#endregion

		#region Wetterdaten vorbereiten

		// potentiellen Sim-Zeitraum
		// Bodentemp. ganzjährig?
		// Prognose von tatsächlichen Wetterdaten unterscheiden
		// Prognose ab?
		public override bool PrepareWeatherData()
		{
			Weather.AirTempAdjustment = (Double)_workingParams.GetValue("pr.AdjAir");

			Weather.SoilTempAdjustment = (Double)_workingParams.GetValue("pr.AdjSoil");
			//Weather.WindSpeedAdjustment = (Double)_workingParams.GetValue("pr.AdjWind");
			Weather.UseSummerSoil = (bool)_workingParams.GetValue("pr.UseSummerSoil");

			_simAirTemps = Weather.GetSimAirTemp();
			_simSoilTemps = Weather.GetSimSoilTemp();
			_simVPD = Weather.GetSimVpd();

			//_simWindSpeeds = Weather.GetSimWind();

			int startIndex = (int)_workingParams.GetValue("pr.SimStart");
			int endIndex = 365;

			//int endIndex = (int)_workingParams.GetValue("pr.SimEnd");

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
			for (int stage = 0; stage <= 4; stage++)
			{
				switch (stage)
				{
					case 0: _maxDevRates[0] = (Double)_workingParams.GetValue("pr.DevEggKmax");
								t = (Double)_workingParams.GetValue("pr.TransEgg");
								break;
					case 1:	_maxDevRates[1] = (Double)_workingParams.GetValue("pr.DevLarvaKmax");
								t = (Double)_workingParams.GetValue("pr.TransLarva");
								break;

					case 2:	_maxDevRates[2] = (Double)_workingParams.GetValue("pr.DevPupaKmax");
								t = (Double)_workingParams.GetValue("pr.TransPupa");
								break;
					case 3:	_maxDevRates[3] = (Double)_workingParams.GetValue("pr.DevFlyKmax");
								t = (Double)_workingParams.GetValue("pr.TransFly");
								break;
					case 4:	_maxDevRates[4] = (Double)_workingParams.GetValue("pr.DevWiPupaKmax");
								t = (Double)_workingParams.GetValue("pr.TransWiPupa");
								break;

				}
				Double BioAge = 0.0;
				for (int i = 0; i <= 1400; i++)
				{
					_tableTransition[stage, i] = SimFunctions.Sigmoid(BioAge, t);
					BioAge += 0.001;
				}
			}
		}

		protected override void InitTableDev()
		{

			double eggTmax = (Double)_workingParams.GetValue("pr.DevEggTmax");
			double eggTopt = (Double)_workingParams.GetValue("pr.DevEggTopt");
			double eggQ = (Double)_workingParams.GetValue("pr.DevEggQ");
			double eggL = (Double)_workingParams.GetValue("pr.DevEggL");
			double eggKmax = (Double)_workingParams.GetValue("pr.DevEggKmax");


			double larvaTmax = (Double)_workingParams.GetValue("pr.DevLarvaTmax");
			double larvaTopt = (Double)_workingParams.GetValue("pr.DevLarvaTopt");
			double larvaQ = (Double)_workingParams.GetValue("pr.DevLarvaQ");
			double larvaL = (Double)_workingParams.GetValue("pr.DevLarvaL");
			double larvaKmax = (Double)_workingParams.GetValue("pr.DevLarvaKmax");


			double pupaTmax = (Double)_workingParams.GetValue("pr.DevPupaTmax");
			double pupaTopt = (Double)_workingParams.GetValue("pr.DevPupaTopt");
			double pupaQ = (Double)_workingParams.GetValue("pr.DevPupaQ");
			double pupaL = (Double)_workingParams.GetValue("pr.DevPupaL");
			double pupaKmax = (Double)_workingParams.GetValue("pr.DevPupaKmax");


			double flyTmax = (Double)_workingParams.GetValue("pr.DevFlyTmax");
			double flyTopt = (Double)_workingParams.GetValue("pr.DevFlyTopt");
			double flyQ = (Double)_workingParams.GetValue("pr.DevFlyQ");
			double flyL = (Double)_workingParams.GetValue("pr.DevFlyL");
			double flyKmax = (Double)_workingParams.GetValue("pr.DevFlyKmax");


			double wipupaTmax = (Double)_workingParams.GetValue("pr.DevWiPupaTmax");
			double wipupaTopt = (Double)_workingParams.GetValue("pr.DevWiPupaTopt");
			double wipupaQ = (Double)_workingParams.GetValue("pr.DevWiPupaQ");
			double wipupaL = (Double)_workingParams.GetValue("pr.DevWiPupaL");
			double wipupaKmax = (Double)_workingParams.GetValue("pr.DevWiPupaKmax");

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

			//double[] test = new double[40];
			//for ( int i=0;i<40;i++)
			//	test[i]= SimFunctions.ONeal((double) i, wipupaTmax, wipupaTopt, wipupaQ, wipupaL, wipupaKmax);
			//int n = 0;

		}

		//protected override void InitTableDiapause()
		//{
		//	// todo: Diapause implementieren
		//	//throw new NotImplementedException();
		//}

		protected override void InitTableFert()
		{

			double startFert = (Double)_workingParams.GetValue("pr.FertPrae");
			double startSkew = (Double)_workingParams.GetValue("pr.FertStartExp");
			double endFert = (Double)_workingParams.GetValue("pr.FertPost");
			double endSkew = (Double)_workingParams.GetValue("pr.FertEndExp");
			double sumEgg = (Double)_workingParams.GetValue("pr.FertSumEgg");

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
			bool isVpdRestr = (bool)_workingParams.GetValue("pr.IsVpd");
			double vpdThreshold = (Double)_workingParams.GetValue("pr.VpdThr");
			double vpdInc = (Double)_workingParams.GetValue("pr.VpdInc");
			double flyMaxDev = (Double)_workingParams.GetValue("pr.DevFlyKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				_tableFlightAct[i] = _tableDev[(int)DevStage.Fly, i] / flyMaxDev; // Flugaktivität abh. von akt. Entwicklungsrate

				if (isVpdRestr)// Flugeinschränkung durch hohes SättigungsdefizitWind
				{
					double vpd = _simVPD[i];
					double w = 1.0 - (vpd - vpdThreshold) * (vpdInc);
					_tableVpdRestr[i] = Math.Min(1.0, Math.Max(0.0, w));
				}
				else
					_tableVpdRestr[i] = 1.0;
			}
		}

		protected override void InitTableMortality()
		{
			double mortEgg = (Double)_workingParams.GetValue("pr.MortEgg");
			double mortLarva = (Double)_workingParams.GetValue("pr.MortLarva");
			double mortPupa = (Double)_workingParams.GetValue("pr.MortPupa");
			double mortFly = (Double)_workingParams.GetValue("pr.MortFly");
			double mortWiPupa = (Double)_workingParams.GetValue("pr.MortWiPupa");


			bool isVarEggMort = (bool)_workingParams.GetValue("pr.MortEggIsTDep");
			double mortEggThr = (Double)_workingParams.GetValue("pr.MortEggThr");
			double mortEggInc = (Double)_workingParams.GetValue("pr.MortEggInc");

			bool isVarLarvaMort = (bool)_workingParams.GetValue("pr.MortLarvaIsTDep");
			double mortLarvaThr = (Double)_workingParams.GetValue("pr.MortLarvaThr");
			double mortLarvaInc = (Double)_workingParams.GetValue("pr.MortLarvaInc");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				double soilTemp = _simSoilTemps[i];//  Weather.SoilTemps[i];
				double mort;

				//if (isVarEggMort)
				//{
				//	mort = Math.Max(mortEgg, (soilTemp - mortEggThr) * mortEggInc);
				//	mort = Math.Min(mort, 1.0);
				//}
				//else
				//{
				//	mort = mortEgg;
				//}
				//_tableMortality[(int)DevStage.Egg, i] = mort;

				//if (isVarLarvaMort)
				//{
				//	mort = Math.Max(mortLarva, (soilTemp - mortLarvaThr) * mortLarvaInc);
				//	mort = Math.Min(mort, 1.0);
				//}
				//else
				//{
				//	mort = mortLarva;
				//}
				//_tableMortality[(int)DevStage.Larva, i] = mort;

				//// feste Mortalitäten für andere Stadien
				//_tableMortality[(int)DevStage.Pupa, i] = mortPupa;
				//_tableMortality[(int)DevStage.Fly, i] = mortFly;
				//_tableMortality[(int)DevStage.WiPupa, i] = mortWiPupa;




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
				bool isAest = (bool)_workingParams.GetValue("pr.IsAest");
				if (!isAest)
					return 100.0;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				double threshold = (double)_workingParams.GetValue("pr.AestThr");
				double variance = (double)_workingParams.GetValue("pr.AestVar");

				return SimFunctions.AestivTemp(_random, threshold, variance);
			}
		}

		public double AestMinAge
		{
			get { return (double)_workingParams.GetValue("pr.AestMinAge"); }
		}

		public double AestMaxAge
		{
			get { return (double)_workingParams.GetValue("pr.AestMaxAge"); }
		}

		public double AestDropDiff
		{
			get { return (double)_workingParams.GetValue("pr.AestDropDiff"); }
		}

		public double MortLarvaMaxAge
		{
			get { return (double)_workingParams.GetValue("pr.MortLarvaMaxAge"); }
		}

		public double FertCluster
		{
			get { return (double)_workingParams.GetValue("pr.FertCluster"); }
		}

		public bool IsVpdOvip
		{
			get { return (bool)_workingParams.GetValue("pr.IsVpdOvip"); }
		}

		public int DiapauseIndex
		{
			get
			{
				bool isDiapause = (bool)_workingParams.GetValue("pr.IsDia");
				if (!isDiapause)
					return 366;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				return (int)_workingParams.GetValue("pr.DiaDate");
			}
		}

		public double DiapauseTemp
		{
			get { return (double)_workingParams.GetValue("pr.DiaThr"); }

		}

		public int DiapauseDur
		{
			get { return (int)_workingParams.GetValue("pr.DiaDur"); }

		}

		protected override void Individual(DevStage startStage, double startAge, int dayIndex, int generation, bool isDiapauseGen)
		{
			IndividualPR.CreateAndLive(this, startStage, startAge, dayIndex, generation, isDiapauseGen);
		}

		#endregion

	}
}

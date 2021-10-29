using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	public class ModelPR : ModelBase
	{
		#region Construction + Parameterdefinition

		public ModelPR(string name, WeatherData weather, PopulationData population, SimParamData explicitParams = null, SimParamData locationParams = null) : base(name, weather, population, explicitParams, locationParams)
		{
			//InitParamDefaults();
			//_workingParams = workspace.SimParameters; GetModelParameters(ModelType.DR);
		}

		public override string GetParamPrefix()
		{
			return "pr";
		}
		//Parameter mit optimierten Params für 1. Generation
		public override SimParamData GetDefaultParams()
		{
			// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

			SimParamData p = new SimParamData();

			// Grundlegendes

			p.InitItem("pr.StartPop", 3000, typeof(int), 500.0, 10000.0, "Startpopulation (Anzahl Individuen)", 1);
			p.InitItem("pr.StartAge", 0.00, typeof(double), 0.0, 0.9, "Biol. Alter der Startpopulation", 3);
			p.InitItem("pr.SimStart", 59, typeof(int), 1.0, 180.0, "Simulationsstart (Tag des Jahres)", 1); // (1. März)

			p.InitItem("pr.MonOnField", true, typeof(bool), 0.499, 0.501, "Monitoring auf dem Feld? (nein: Hecke)", 1);

			p.InitItem("pr.UseOnlyAir", false, typeof(bool), 0.0, 1.0, "nur Lufttemperatur nutzen?", 1);
			p.InitItem("pr.SummerSoilRel", 0.9, typeof(double), 0.0, 1.0, "Gewichtung Boden-/Lufttemp ab 1.6.", 1); //0= 0%Boden, 100%Luft; 0.5= (Boden +Luft)/2; 1= 100% Boden
			p.InitItem("pr.AdjAir", 0.0, typeof(double), -5.0, 5.0, "Korrektur Lufttemp", 1);
			p.InitItem("pr.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp", 1);

			// Niederschlag für Puppenschlupf
			p.InitItem("pr.PrecPeriod", 7, typeof(int), 0, 10, "Niederschlag gleitende Mittelungsperiode", 3);
			p.InitItem("pr.UsePrecHatch", false, typeof(bool), 0.0, 1.0, "fehlender Niederschlag hemmt Puppenschlupf?", 3);
			p.InitItem("pr.PrecHatchLimit", 4.0, typeof(double), 0.0, 5.0, "Puppenschlupf Niederschlag Grenzwert", 3);
			p.InitItem("pr.PrecHatchAsc", 0.25, typeof(double), 0.0, 1.0, "Puppenschlupf-Hemmung Niederschlagsdefizit (pro mm)", 3);

			p.InitItem("pr.IsPrecFlightAct", false, typeof(bool), 0.0, 1.0, "fehlender Niederschlag hemmt Flugaktivität?", 3);
			p.InitItem("pr.PrecFlightActLimit", 4.0, typeof(double), 0.0, 5.0, "Flugaktivität Niederschlag Grenzwert", 3);
			p.InitItem("pr.PrecFlightActAsc", 0.25, typeof(double), 0.0, 1.0, "Flugaktivität-Hemmung Niederschlagsdefizit (pro mm)", 3);
			p.InitItem("pr.IsPrecOvip", false, typeof(bool), 0.0, 1.0, "auch Eiablage durch fehlendem Niederschlag eingeschränkt?", 3);


			//Mortalitäten
			p.InitItem("pr.MortEgg", 0.05, typeof(double), 0.01, 0.25, "tägl. Mortalität Eier", 3);
			p.InitItem("pr.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven", 3);
			p.InitItem("pr.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen", 3);
			p.InitItem("pr.MortFly", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen", 3);
			p.InitItem("pr.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen", 3);

			p.InitItem("pr.MortEggIsTDep", false, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?", 3);
			p.InitItem("pr.MortEggThr", 20.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Ei-Mortalität", 3);
			p.InitItem("pr.MortEggInc", 0.15, typeof(double), 0.01, 0.5, "Anstieg Ei-Mortalität pro Grad C", 3);

			p.InitItem("pr.MortLarvaIsTDep", false, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?", 3);
			p.InitItem("pr.MortLarvaMaxAge", 0.25, typeof(double), 0.0, 1.0, "Maximalalter für erhöhte Larven-Mortalität", 3);
			p.InitItem("pr.MortLarvaThr", 21.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Larven-Mortalität", 3);
			p.InitItem("pr.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C", 3);

			//Transitionen - Übergänge ins nächste Stadium
			p.InitItem("pr.TransWiPupa", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Überwinterungsstadium->Adult", 3);
			p.InitItem("pr.TransEgg", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Ei->Larve", 3);
			p.InitItem("pr.TransLarva", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Larve->Puppe", 3);
			p.InitItem("pr.TransPupa", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Puppe->Adult", 3);
			p.InitItem("pr.TransFly", 5.0, typeof(double), 5.0, 15.0, "Faktor Transition Adult->Tod", 3);

			//Entwicklungsparameter
			p.InitItem("pr.DevEggTmax", 40.0, typeof(double), 30.0, 45.0, "Entwicklung Ei: Temperaturmaximum", 3);
			p.InitItem("pr.DevEggTopt", 25.0, typeof(double), 15.0, 29.0, "Entwicklung Ei: Temperaturoptimum", 3);
			p.InitItem("pr.DevEggQ", 1.77, typeof(double), 1.1, 3.0, "Entwicklung Ei: Temperatur-Spezifität", 3);
			p.InitItem("pr.DevEggL", 0.02, typeof(double), 0.001, 0.1, "Entwicklung Ei: Konvexität linke Flanke", 3);
			p.InitItem("pr.DevEggKmax", 0.194, typeof(double), 0.1, 0.4, "Entwicklung Ei: max. tägl. Entwicklungsrate", 3);


			p.InitItem("pr.DevLarvaTmax", 30.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum", 3);
			p.InitItem("pr.DevLarvaTopt", 22.0, typeof(double), 15.0, 29.0, "Entwicklung Larve: Temperaturoptimum", 3);
			p.InitItem("pr.DevLarvaQ", 1.49, typeof(double), 1.1, 2.5, "Entwicklung Larve: Temperatur-Spezifität", 3);
			p.InitItem("pr.DevLarvaL", 0.015, typeof(double), 0.0, 0.1, "Entwicklung Larve: Konvexität linke Flanke", 3);
			p.InitItem("pr.DevLarvaKmax", 0.038, typeof(double), 0.025, 0.2, "Entwicklung Larve: max. tägl. Entwicklungsrate", 3);


			p.InitItem("pr.DevPupaTmax", 30.0, typeof(double), 28.0, 40.0, "Entwicklung Puppe: Temperaturmaximum", 3);
			p.InitItem("pr.DevPupaTopt", 22.0, typeof(double), 15.0, 27.0, "Entwicklung Puppe: Temperaturoptimum", 3);
			p.InitItem("pr.DevPupaQ", 1.62, typeof(double), 1.1, 2.5, "Entwicklung Puppe: Temperatur-Spezifität", 3);
			p.InitItem("pr.DevPupaL", 0.015, typeof(double), 0.0, 0.1, "Entwicklung Puppe: Konvexität linke Flanke", 3);
			p.InitItem("pr.DevPupaKmax", 0.056, typeof(double), 0.01, 0.2, "Entwicklung Puppe: max. tägl. Entwicklungsrate", 3);


			p.InitItem("pr.DevFlyTmax", 35.0, typeof(double), 25.0, 45.0, "Entwicklung Fliege: Temperaturmaximum", 3);
			p.InitItem("pr.DevFlyTopt", 30.0, typeof(double), 15.0, 25.0, "Entwicklung Fliege: Temperaturoptimum", 3);
			p.InitItem("pr.DevFlyQ", 1.69, typeof(double), 1.1, 2.5, "Entwicklung Fliege: Temperatur-Spezifität", 3);
			p.InitItem("pr.DevFlyL", 0.011, typeof(double), 0.0, 0.1, "Entwicklung Fliege: Konvexität linke Flanke", 3);
			p.InitItem("pr.DevFlyKmax", 0.132, typeof(double), 0.025, 0.2, "Entwicklung Fliege: max. tägl. Entwicklungsrate", 3);

			// das sind die besseren
			//p.InitItem("pr.DevWiPupaTmax", 29.08,typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
			//p.InitItem("pr.DevWiPupaTopt", 19.28, typeof(double), 15.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
			//p.InitItem("pr.DevWiPupaQ", 1.934, typeof(double), 1.1, 2.5, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
			//p.InitItem("pr.DevWiPupaL", 0.003, typeof(double), 0.0, 0.1, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
			//p.InitItem("pr.DevWiPupaKmax", 0.036, typeof(double), 0.015, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

			p.InitItem("pr.DevWiPupaTmax", 29.08, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
			p.InitItem("pr.DevWiPupaTopt", 19.55, typeof(double), 15.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
			p.InitItem("pr.DevWiPupaQ", 1.935, typeof(double), 1.1, 2.5, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
			p.InitItem("pr.DevWiPupaL", 0.0122, typeof(double), 0.0, 0.1, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
			p.InitItem("pr.DevWiPupaKmax", 0.0516, typeof(double), 0.015, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

			//Fertilität
			//p.InitItem("pr.FertPrae", 0.18, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition", 3);
			//p.InitItem("pr.FertStartExp", 6.55, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
			p.InitItem("pr.FertPrae", 0.07, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition", 3);
			p.InitItem("pr.FertStartExp", 6.47, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
			p.InitItem("pr.FertPost", 0.2145, typeof(double), 0.0, 0.5, "Fertilität: Post-Oviposition", 3);
			p.InitItem("pr.FertEndExp", 3.2, typeof(double), 1.1, 10.0, "Fertilität: Faktor Abstieg Eiablage", 3);
			p.InitItem("pr.FertSumEgg", 30.0, typeof(double), 10.0, 100.0, "Fertilität: Anzahl Eier/Fliege", 3); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
			p.InitItem("pr.FertCluster", 1, typeof(int), 1.0, 10.0, "Fertilität: Eiablage-Cluster", 3);

			//Flugeinschränkung durch Sättigungsdefizit
			//p.InitItem("pr.IsVpd", false, typeof(bool), 0.0, 1.0, "Flug durch hohes Sättigungsdefizit eingeschränkt?", 3);
			//p.InitItem("pr.VpdThr", 8.5, typeof(double), 1.0, 15.0, "Grenzwert (hPa) für Flughemmung", 3);
			//p.InitItem("pr.VpdInc", 0.07, typeof(double), 0.01, 0.9, "Zunahme Flughemmung pro zusätzl. hPa", 3);
			//p.InitItem("pr.IsVpdOvip", false, typeof(bool), 0.0, 1.0, "auch Eiablage durch hohes Sättigungsdefizit eingeschränkt?", 3);

			//Verhinderung Puppenschlupf
			// Grenzwert Glm Regen
			// Anzahl tage Glm

			////Flugeinschränkung durch hohe Temperaturen
			//p.InitItem("pr.IsTInhib", false, typeof(bool), 0.0, 1.0, "Flug durch hohe Temperaturen eingeschränkt?");
			//p.InitItem("pr.TInhibThr", 18.0, typeof(double), 15.0, 30.0, "Grenzwert (°C) für Flughemmung durch hohe Temp");
			//p.InitItem("pr.TInhibInc", 0.05, typeof(double), 0.025, 0.25, "Anstieg Flughemmung pro zusätzl. °C");

			//Diapause
			p.InitItem("pr.IsDia", false, typeof(bool), 0.0, 1.0, "Diapause (Winterruhe) berechnen?", 3);
			p.InitItem("pr.DiaDate", 242, typeof(int), 200.0, 300.0, "frühester Eintritt in Diapause (Tag des Jahres)", 3);
			p.InitItem("pr.DiaThr", 13.0, typeof(double), 10.0, 18.0, "Temperaturschwelle für Auslösen d. Diapause", 3);
			p.InitItem("pr.DiaDur", 3, typeof(int), 1.0, 10.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause", 3);

			//Ästivation
			p.InitItem("pr.IsAest", false, typeof(bool), 0.0, 1.0, "Ästivation(Sommerruhe) berechnen?", 3);// true
			p.InitItem("pr.AestThr", 22.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für Auslösen d. Ästivation", 3);
			p.InitItem("pr.AestVar", 1.0, typeof(double), 0.0, 2.5, "Streuungsfaktor Ästivation", 3);
			p.InitItem("pr.AestDropDiff", 2.0, typeof(double), 0.0, 5.0, "Differenz für Aufheben d. Ästivation", 3);
			p.InitItem("pr.AestMinAge", 0.0, typeof(double), 0.0, 0.25, "Minimum biol. Alter für Ästivation", 3);
			p.InitItem("pr.AestMaxAge", 1.0, typeof(double), 0.25, 1.4, "Maximum biol. Alter für Ästivation", 3);


			return p;
		}


		// Original aus altem Swat

		//public override SimParamData GetDefaultParams()
		//{
		//	// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

		//	SimParamData p = new SimParamData();

		//	// Grundlegendes

		//	p.InitItem("pr.StartPop", 3000, typeof(int), 500.0, 10000.0, "Startpopulation (Anzahl Individuen)",1);

		//	p.InitItem("pr.SimStart", 59, typeof(int), 1.0, 180.0, "Simulationsstart (Tag des Jahres)",1); // (1. März)
		//	p.InitItem("pr.MonOnField", true, typeof(bool), 0.499, 0.501, "Monitoring auf dem Feld? (nein: Hecke)",1);


		//	p.InitItem("pr.SummerSoilRel", 0.5, typeof(double), 0.0, 1.0, "Mischungsverh. Boden-/Lufttemp ab 1.6.", 1); //0= 0%Boden, 100%Luft; 0.5= (Boden +Luft)/2; 1= 100% Boden

		//	p.InitItem("pr.AdjAir", 0.0, typeof(double), -5.0, 5.0, "Korrektur Lufttemp",1); 
		//	p.InitItem("pr.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp",1);

		//	// Niederschlag für Puppenschlupf
		//	p.InitItem("pr.UsePrec", false, typeof(bool), 0.0, 1.0, "fehlender Niederschlag hemmt Puppenschlupf?", 3);
		//	p.InitItem("pr.PrecPeriod", 7, typeof(int), 0, 10, "Niederschlag gleitende Mittelungsperiode", 3);
		//	p.InitItem("pr.PrecLimit", 4.0, typeof(double), 0.0, 5.0, "Niederschlag Grenzwert", 3);
		//	p.InitItem("pr.PrecAsc", 0.25, typeof(double), 0.0, 1.0, "Hemmung Niederschlagsdefizit (pro mm)", 3);


		//	//Mortalitäten
		//	p.InitItem("pr.MortEgg", 0.05, typeof(double), 0.01, 0.25, "tägl. Mortalität Eier",3);
		//	p.InitItem("pr.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven",3);
		//	p.InitItem("pr.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen",3);
		//	p.InitItem("pr.MortFly", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen",3);
		//	p.InitItem("pr.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen",3);

		//	p.InitItem("pr.MortEggIsTDep", false, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?",3);
		//	p.InitItem("pr.MortEggThr", 20.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Ei-Mortalität",3);    
		//	p.InitItem("pr.MortEggInc", 0.15, typeof(double), 0.01, 0.5, "Anstieg Ei-Mortalität pro Grad C",3);   

		//	p.InitItem("pr.MortLarvaIsTDep", false, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?",3);
		//	p.InitItem("pr.MortLarvaMaxAge", 0.25, typeof(double), 0.0, 1.0, "Maximalalter für erhöhte Larven-Mortalität",3);
		//	p.InitItem("pr.MortLarvaThr", 21.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Larven-Mortalität",3);    
		//	p.InitItem("pr.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C",3);  

		//	//Transitionen - Übergänge ins nächste Stadium
		//	p.InitItem("pr.TransWiPupa", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Überwinterungsstadium->Adult",3);
		//	p.InitItem("pr.TransEgg", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Ei->Larve",3);
		//	p.InitItem("pr.TransLarva", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Larve->Puppe",3);
		//	p.InitItem("pr.TransPupa", 8.0, typeof(double), 5.0, 15.0, "Faktor Transition Puppe->Adult",3);
		//	p.InitItem("pr.TransFly", 5.0, typeof(double), 5.0, 15.0, "Faktor Transition Adult->Tod",3);

		//	//Entwicklungsparameter
		//	p.InitItem("pr.DevEggTmax", 40.0, typeof(double), 30.0, 45.0, "Entwicklung Ei: Temperaturmaximum",3);
		//	p.InitItem("pr.DevEggTopt", 25.0, typeof(double), 15.0, 29.0, "Entwicklung Ei: Temperaturoptimum",3);
		//	p.InitItem("pr.DevEggQ", 1.77, typeof(double), 1.1, 3.0, "Entwicklung Ei: Temperatur-Spezifität",3);
		//	p.InitItem("pr.DevEggL", 0.02, typeof(double), 0.001, 0.1, "Entwicklung Ei: Konvexität linke Flanke",3);
		//	p.InitItem("pr.DevEggKmax", 0.194, typeof(double), 0.1, 0.4, "Entwicklung Ei: max. tägl. Entwicklungsrate", 3);


		//	p.InitItem("pr.DevLarvaTmax", 30.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum",3);
		//	p.InitItem("pr.DevLarvaTopt", 22.0, typeof(double), 15.0, 29.0, "Entwicklung Larve: Temperaturoptimum",3);
		//	p.InitItem("pr.DevLarvaQ", 1.49, typeof(double), 1.1, 2.5, "Entwicklung Larve: Temperatur-Spezifität",3);
		//	p.InitItem("pr.DevLarvaL", 0.015, typeof(double), 0.0, 0.1, "Entwicklung Larve: Konvexität linke Flanke", 3);
		//	p.InitItem("pr.DevLarvaKmax", 0.038, typeof(double), 0.025, 0.2, "Entwicklung Larve: max. tägl. Entwicklungsrate",3);


		//	p.InitItem("pr.DevPupaTmax", 30.0, typeof(double), 28.0, 40.0, "Entwicklung Puppe: Temperaturmaximum",3);
		//	p.InitItem("pr.DevPupaTopt", 22.0, typeof(double), 15.0, 27.0, "Entwicklung Puppe: Temperaturoptimum",3);
		//	p.InitItem("pr.DevPupaQ", 1.62, typeof(double), 1.1, 2.5, "Entwicklung Puppe: Temperatur-Spezifität",3);
		//	p.InitItem("pr.DevPupaL", 0.015, typeof(double), 0.0, 0.1, "Entwicklung Puppe: Konvexität linke Flanke", 3);
		//	p.InitItem("pr.DevPupaKmax", 0.056, typeof(double), 0.01, 0.2, "Entwicklung Puppe: max. tägl. Entwicklungsrate",3);


		//	p.InitItem("pr.DevFlyTmax", 35.0, typeof(double), 25.0, 45.0, "Entwicklung Fliege: Temperaturmaximum", 3);
		//	p.InitItem("pr.DevFlyTopt", 30.0, typeof(double), 15.0, 25.0, "Entwicklung Fliege: Temperaturoptimum", 3);
		//	p.InitItem("pr.DevFlyQ", 1.69, typeof(double), 1.1, 2.5, "Entwicklung Fliege: Temperatur-Spezifität", 3);
		//	p.InitItem("pr.DevFlyL", 0.011, typeof(double), 0.0, 0.1, "Entwicklung Fliege: Konvexität linke Flanke", 3);
		//	p.InitItem("pr.DevFlyKmax", 0.132, typeof(double), 0.025, 0.2, "Entwicklung Fliege: max. tägl. Entwicklungsrate", 3);

		//	// das sind die besseren
		//	//p.InitItem("pr.DevWiPupaTmax", 29.08,typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
		//	//p.InitItem("pr.DevWiPupaTopt", 19.28, typeof(double), 15.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
		//	//p.InitItem("pr.DevWiPupaQ", 1.934, typeof(double), 1.1, 2.5, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
		//	//p.InitItem("pr.DevWiPupaL", 0.003, typeof(double), 0.0, 0.1, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
		//	//p.InitItem("pr.DevWiPupaKmax", 0.036, typeof(double), 0.015, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

		//	p.InitItem("pr.DevWiPupaTmax", 30.0, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
		//	p.InitItem("pr.DevWiPupaTopt", 19.0, typeof(double), 15.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
		//	p.InitItem("pr.DevWiPupaQ", 1.59, typeof(double), 1.1, 2.5, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
		//	p.InitItem("pr.DevWiPupaL", 0.003, typeof(double), 0.0, 0.1, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
		//	p.InitItem("pr.DevWiPupaKmax", 0.041, typeof(double), 0.015, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

		//	//Fertilität
		//	//p.InitItem("pr.FertPrae", 0.18, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition", 3);
		//	//p.InitItem("pr.FertStartExp", 6.55, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
		//	p.InitItem("pr.FertPrae", 0.15, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition", 3);
		//	p.InitItem("pr.FertStartExp", 6.0, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
		//	p.InitItem("pr.FertPost", 0.20, typeof(double), 0.0, 0.5, "Fertilität: Post-Oviposition", 3);  
		//	p.InitItem("pr.FertEndExp", 3.0, typeof(double), 1.1, 10.0, "Fertilität: Faktor Abstieg Eiablage", 3);
		//	p.InitItem("pr.FertSumEgg", 30.0, typeof(double), 10.0, 100.0, "Fertilität: Anzahl Eier/Fliege", 3); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
		//	p.InitItem("pr.FertCluster", 1.0, typeof(double), 1.0, 10.0, "Fertilität: Eiablage-Cluster", 3);

		//	//Flugeinschränkung durch Sättigungsdefizit
		//	//p.InitItem("pr.IsVpd", false, typeof(bool), 0.0, 1.0, "Flug durch hohes Sättigungsdefizit eingeschränkt?", 3);
		//	//p.InitItem("pr.VpdThr", 8.5, typeof(double), 1.0, 15.0, "Grenzwert (hPa) für Flughemmung", 3);
		//	//p.InitItem("pr.VpdInc", 0.07, typeof(double), 0.01, 0.9, "Zunahme Flughemmung pro zusätzl. hPa", 3);
		//	//p.InitItem("pr.IsVpdOvip", false, typeof(bool), 0.0, 1.0, "auch Eiablage durch hohes Sättigungsdefizit eingeschränkt?", 3);

		//	//Verhinderung Puppenschlupf
		//	// Grenzwert Glm Regen
		//	// Anzahl tage Glm

		//	////Flugeinschränkung durch hohe Temperaturen
		//	//p.InitItem("pr.IsTInhib", false, typeof(bool), 0.0, 1.0, "Flug durch hohe Temperaturen eingeschränkt?");
		//	//p.InitItem("pr.TInhibThr", 18.0, typeof(double), 15.0, 30.0, "Grenzwert (°C) für Flughemmung durch hohe Temp");
		//	//p.InitItem("pr.TInhibInc", 0.05, typeof(double), 0.025, 0.25, "Anstieg Flughemmung pro zusätzl. °C");

		//	//Diapause
		//	p.InitItem("pr.IsDia", false, typeof(bool), 0.0, 1.0, "Diapause (Winterruhe) berechnen?", 3);
		//	p.InitItem("pr.DiaDate", 242, typeof(int), 200.0, 300.0, "frühester Eintritt in Diapause (Tag des Jahres)", 3);
		//	p.InitItem("pr.DiaThr", 13.0, typeof(double), 10.0, 18.0, "Temperaturschwelle für Auslösen d. Diapause", 3);
		//	p.InitItem("pr.DiaDur", 3, typeof(int), 1.0, 10.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause", 3);

		//	//Ästivation
		//	p.InitItem("pr.IsAest", false, typeof(bool), 0.0, 1.0, "Ästivation(Sommerruhe) berechnen?", 3);// true
		//	p.InitItem("pr.AestThr", 22.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für Auslösen d. Ästivation", 3);
		//	p.InitItem("pr.AestVar", 1.0, typeof(double), 0.0, 2.5, "Streuungsfaktor Ästivation", 3);     
		//	p.InitItem("pr.AestDropDiff", 2.0, typeof(double), 0.0, 5.0, "Differenz für Aufheben d. Ästivation", 3);
		//	p.InitItem("pr.AestMinAge", 0.0, typeof(double), 0.0, 0.25, "Minimum biol. Alter für Ästivation", 3);
		//	p.InitItem("pr.AestMaxAge", 1.0, typeof(double), 0.25, 1.4, "Maximum biol. Alter für Ästivation", 3);


		//	return p;
		//}

		protected override int GetStartPopulation()
		{
			return (int)_workingParams.GetValue("pr.StartPop");
		}

		protected override double GetStartAge()
		{
			return (double)_workingParams.GetValue("pr.StartAge");
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
			Weather.UseOnlyAir = (bool)_workingParams.GetValue("pr.UseOnlyAir");
			Weather.AirTempAdjustment = (Double)_workingParams.GetValue("pr.AdjAir");
			Weather.SoilTempAdjustment = (Double)_workingParams.GetValue("pr.AdjSoil");
			Weather.SummerSoilRel= (Double)_workingParams.GetValue("pr.SummerSoilRel");
			Weather.PrecPeriod= (int)_workingParams.GetValue("pr.PrecPeriod");
			_simAirTemps = Weather.GetSimAirTemp();
			_simSoilTemps = Weather.GetSimSoilTemp();
			_simPrec = Weather.GetSimPrec();

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

		protected override void InitTransitions()
		{
			_transitions = new double[5];
			_transitions[0] = (Double)_workingParams.GetValue("pr.TransEgg");
			_transitions[1] = (Double)_workingParams.GetValue("pr.TransLarva");
			_transitions[2] = (Double)_workingParams.GetValue("pr.TransPupa");
			_transitions[3] = (Double)_workingParams.GetValue("pr.TransFly");
			_transitions[4] = (Double)_workingParams.GetValue("pr.TransWiPupa");
		}

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
			double flyMaxDev = (Double)_workingParams.GetValue("pr.DevFlyKmax");
			bool isMonOnField = (bool)_workingParams.GetValue("pr.MonOnField");
			bool isFlightInhib = (bool)_workingParams.GetValue("pr.IsPrecFlightAct");
			double flightLimit = (Double)_workingParams.GetValue("pr.PrecFlightActLimit");
			double flightInc = (Double)_workingParams.GetValue("pr.PrecFlightActAsc");


			bool isHatchInhib= (bool)_workingParams.GetValue("pr.UsePrecHatch");
			double hatchLimit = (Double)_workingParams.GetValue("pr.PrecHatchLimit");
			double hatchInc = (Double)_workingParams.GetValue("pr.PrecHatchAsc");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				_tableFlightAct[i] = _tableDev[(int)DevStage.Fly, i] / flyMaxDev; // Flugaktivität abh. von akt. Entwicklungsrate

				if (isFlightInhib & isMonOnField)// Flugeinschränkung durch Bodentrockenheit nur bei Feldmonitoring
				{
					double fi = Math.Max(0.0, (flightLimit - _simPrec[i]) * flightInc);
					_tableFlightInhib[i] = Math.Max(0.0, 1.0 - fi);
				}
				else
				{
					_tableFlightInhib[i] = 1.0;
				}

				if ( isHatchInhib)// Hemmung Puppenschlupf durch Bodentrockenheit
				{
					double hi =Math.Max(0.0, (hatchLimit - _simPrec[i]) * hatchInc);
					_tableHatchInhib[i] = Math.Max(0.0, 1.0 - hi);
				}
				else
				{
					_tableHatchInhib[i] = 1.0;
				}

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
				bool isAest = (bool)_workingParams.GetValue("pr.IsAest");
				if (!isAest)
					return 100.0;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				double threshold = (double)_workingParams.GetValue("pr.AestThr");
				double variance = (double)_workingParams.GetValue("pr.AestVar");

				return SimFunctions.AestivTemp(GetRandom, threshold, variance);
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

		public int FertCluster
		{
			get { return (int)_workingParams.GetValue("pr.FertCluster"); }
		}

		public bool IsPrecOvip
		{
			get { return (bool)_workingParams.GetValue("pr.IsPrecOvip"); }
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

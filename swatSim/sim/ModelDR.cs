
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace swatSim
{
	public class ModelDR : ModelBase
	{
		#region Construction + Parameterdefinition

		public ModelDR(string name, WeatherData weather,PopulationData population, SimParamData explicitParams = null, SimParamData locationParams = null) : base(name, weather, population, explicitParams, locationParams)
		{
			//InitParamDefaults();
			//_workingParams = workspace.SimParameters; GetModelParameters(ModelType.DR);
		}

		public override string GetParamPrefix()
		{
			return "dr";
		}
		
		
		// Optimierte Parameter vom 6. 4. 20
		public override SimParamData GetDefaultParams()
		{
			// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

			SimParamData p = new SimParamData();

			// Grundlegendes

			p.InitItem("dr.StartPop", 1000, typeof(int), 500.0, 10000.0, "Startpopulation (Anzahl Individuen)", 1);

			p.InitItem("dr.SimStart", 59, typeof(int), 1.0, 180.0, "Simulationsstart (Tag des Jahres)", 1); // (1. März)

			p.InitItem("dr.UseOnlyAir", false, typeof(bool), 0.0, 1.0, "nur Lufttemperatur nutzen?", 1);
			p.InitItem("dr.SummerSoilRel", 0.5, typeof(double), 0.0, 1.0, "Mischungsverh. Boden-/Lufttemp ab 1.6.", 1); //0= 0%Boden, 100%Luft; 0.5= (Boden +Luft)/2; 1= 100% Boden
			p.InitItem("dr.AdjAir", 0.0, typeof(double), -5.0, 5.0, "Korrektur Lufttemp", 1);
			p.InitItem("dr.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp", 1);

			//Mortalitäten
			p.InitItem("dr.MortEgg", 0.05, typeof(double), 0.01, 0.25, "tägl. Mortalität Eier", 3);
			p.InitItem("dr.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven", 3);
			p.InitItem("dr.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen", 3);
			p.InitItem("dr.MortFly", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen", 3);
			p.InitItem("dr.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen", 3);

			p.InitItem("dr.MortEggIsTDep", true, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?", 3);
			p.InitItem("dr.MortEggThr", 21.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Ei-Mortalität", 3);
			p.InitItem("dr.MortEggInc", 0.15, typeof(double), 0.01, 0.5, "Anstieg Ei-Mortalität pro Grad C", 3);

			p.InitItem("dr.MortLarvaIsTDep", true, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?", 3);
			p.InitItem("dr.MortLarvaMaxAge", 0.25, typeof(double), 0.0, 1.0, "Maximalalter für erhöhte Larven-Mortalität", 3);
			p.InitItem("dr.MortLarvaThr", 22.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für erhöhte Larven-Mortalität", 3);
			p.InitItem("dr.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C", 3);

			//Transitionen - Übergänge ins nächste Stadium
			p.InitItem("dr.TransWiPupa", 0.14, typeof(double), 0.0, 0.2, "Faktor Transition Überwinterungsstadium->Adult", 3);
			p.InitItem("dr.TransEgg", 0.14, typeof(double), 0.0, 0.2, "Faktor Transition Ei->Larve", 3);
			p.InitItem("dr.TransLarva", 0.14, typeof(double), 0.0, 0.2,"Faktor Transition Larve->Puppe", 3);
			p.InitItem("dr.TransPupa", 0.14, typeof(double), 0.0, 0.2, "Faktor Transition Puppe->Adult", 3);
			p.InitItem("dr.TransFly", 0.14, typeof(double), 0.0, 0.2, "Faktor Transition Adult->Tod", 3);

			//Entwicklungsparameter
			p.InitItem("dr.DevEggTmax", 40.0, typeof(double), 30.0, 45.0, "Entwicklung Ei: Temperaturmaximum", 3);
			p.InitItem("dr.DevEggTopt", 25.0, typeof(double), 15.0, 29.0, "Entwicklung Ei: Temperaturoptimum", 3);
			p.InitItem("dr.DevEggQ", 1.88, typeof(double), 1.1, 4.0, "Entwicklung Ei: Temperatur-Spezifität", 3);
			p.InitItem("dr.DevEggL", 0.024, typeof(double), 0.0, 0.1, "Entwicklung Ei: Konvexität linke Flanke", 3);
			p.InitItem("dr.DevEggKmax", 0.375, typeof(double), 0.1, 0.5, "Entwicklung Ei: max. tägl. Entwicklungsrate", 3);


			p.InitItem("dr.DevLarvaTmax", 35.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum", 3);
			p.InitItem("dr.DevLarvaTopt", 27.0, typeof(double), 20.0, 29.9, "Entwicklung Larve: Temperaturoptimum", 3);
			p.InitItem("dr.DevLarvaQ", 1.73, typeof(double), 1.1, 4.0, "Entwicklung Larve: Temperatur-Spezifität", 3);
			p.InitItem("dr.DevLarvaL", 0.009, typeof(double), 0.0, 0.1, "Entwicklung Larve: Konvexität linke Flanke", 3);
			p.InitItem("dr.DevLarvaKmax", 0.09, typeof(double), 0.05, 0.25, "Entwicklung Larve: max. tägl. Entwicklungsrate", 3);


			p.InitItem("dr.DevPupaTmax", 30.0, typeof(double), 30.0, 40.0, "Entwicklung Puppe: Temperaturmaximum", 3);
			p.InitItem("dr.DevPupaTopt", 22.5, typeof(double), 20.0, 29.9, "Entwicklung Puppe: Temperaturoptimum", 3);
			p.InitItem("dr.DevPupaQ", 1.88, typeof(double), 1.1, 4.0, "Entwicklung Puppe: Temperatur-Spezifität", 3);
			p.InitItem("dr.DevPupaL", 0.008, typeof(double), 0.0, 0.1, "Entwicklung Puppe: Konvexität linke Flanke", 3);
			p.InitItem("dr.DevPupaKmax", 0.073, typeof(double), 0.05, 0.2, "Entwicklung Puppe: max. tägl. Entwicklungsrate", 3);


			p.InitItem("dr.DevFlyTmax", 35.0, typeof(double), 30.0, 45.0, "Entwicklung Fliege: Temperaturmaximum", 3);
			p.InitItem("dr.DevFlyTopt", 30.0, typeof(double), 20.0, 29.9, "Entwicklung Fliege: Temperaturoptimum", 3);
			p.InitItem("dr.DevFlyQ", 1.67, typeof(double), 1.1, 4.0, "Entwicklung Fliege: Temperatur-Spezifität", 3);
			p.InitItem("dr.DevFlyL", 0.006, typeof(double), 0.0, 0.1, "Entwicklung Fliege: Konvexität linke Flanke", 3);
			p.InitItem("dr.DevFlyKmax", 0.072, typeof(double), 0.05, 0.2, "Entwicklung Fliege: max. tägl. Entwicklungsrate", 3);

			//verbessert
			//p.InitItem("dr.DevWiPupaTmax", 30.87, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
			//p.InitItem("dr.DevWiPupaTopt", 24.44, typeof(double), 20.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
			//p.InitItem("dr.DevWiPupaQ", 1.68, typeof(double), 1.1, 2.0, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
			//p.InitItem("dr.DevWiPupaL", 0.039, typeof(double), 0.01, 0.05, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
			//p.InitItem("dr.DevWiPupaKmax", 0.154, typeof(double), 0.05, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

			p.InitItem("dr.DevWiPupaTmax", 30.52, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
			p.InitItem("dr.DevWiPupaTopt", 25.03, typeof(double), 20.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
			p.InitItem("dr.DevWiPupaQ", 1.56, typeof(double), 1.1, 2.0, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
			p.InitItem("dr.DevWiPupaL", 0.0313, typeof(double), 0.01, 0.05, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
			p.InitItem("dr.DevWiPupaKmax", 0.1029, typeof(double), 0.08, 0.17, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

			//Fertilität
			p.InitItem("dr.FertPrae", 0.22, typeof(double), 0.0, 0.5, "Fertilität: Prä-Oviposition", 3);
			p.InitItem("dr.FertStartExp", 8.0, typeof(double), 2.0, 10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
			p.InitItem("dr.FertPost", 0.65, typeof(double), 0.5, 1.0, "Fertilität: Post-Oviposition", 3);
			p.InitItem("dr.FertEndExp", 3.0, typeof(double), 1.1, 10.0, "Fertilität: Faktor Abstieg Eiablage", 3);
			p.InitItem("dr.FertSumEgg", 30.0, typeof(double), 10.0, 100.0, "Fertilität: Anzahl Eier/Fliege", 3); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
			p.InitItem("dr.FertCluster", 1.0, typeof(double), 1.0, 10.0, "Fertilität: Eiablage-Cluster", 3);

			//Flugeinschränkung durch Wind
			//p.InitItem("dr.IsWr", true, typeof(bool),0.0,1.0, "Flug durch Wind eingeschränkt?");
			//p.InitItem("dr.WrThr", 3.0, typeof(double),1.0,10.0, "Grenzwert (m/s) für Flughemmung");
			//p.InitItem("dr.WrInc", 0.20, typeof(double),0.1,0.9, "Anstieg Flughemmung pro zusätzl. m/s");

			//Diapause
			p.InitItem("dr.IsDia", true, typeof(bool), 0.0, 1.0, "Diapause (Winterruhe) berechnen?", 3);
			p.InitItem("dr.DiaDate", 243, typeof(int), 200.0, 300.0, "frühester Eintritt in Diapause (Tag des Jahres)", 3);
			p.InitItem("dr.DiaThr", 23.26, typeof(double), 5.0, 25.0, "Temperaturschwelle für Auslösen d. Diapause", 3);
			p.InitItem("dr.DiaDur", 14, typeof(int), 1.0, 25.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause", 3);


			//Ästivation
			p.InitItem("dr.IsAest", true, typeof(bool), 0.0, 1.0, "Ästivation(Sommerruhe) berechnen?", 3);// true
			p.InitItem("dr.AestThr", 23.97, typeof(double), 15.0, 35.0, "Temperaturschwelle für Auslösen d. Ästivation", 3);
			p.InitItem("dr.AestVar", 2.43, typeof(double), 0.0, 3.0, "Streuungsfaktor Ästivation", 3);     // ?? klären!
			p.InitItem("dr.AestDropDiff", 4.43, typeof(double), 0.0, 5.0, "Differenz für Aufheben d. Ästivation", 3);
			p.InitItem("dr.AestMinAge", 0.0, typeof(double), 0.0, 0.25, "Minimum biol. Alter für Ästivation", 3);
			p.InitItem("dr.AestMaxAge", 1.0, typeof(double), 0.25, 1.4, "Maximum biol. Alter für Ästivation", 3);


			return p;
		}

		// Original aus altem Swat


		//public  override SimParamData GetDefaultParams()
		//{
		//	// Achtung: Werte Typsicher eintragen d.h. Double immer mit Dezimalpunkt

		//	SimParamData p = new SimParamData();

		//	// Grundlegendes

		//	p.InitItem("dr.StartPop",1000, typeof(int),500.0,10000.0, "Startpopulation (Anzahl Individuen)", 1); 

		//	p.InitItem("dr.SimStart", 59, typeof(int),1.0, 180.0, "Simulationsstart (Tag des Jahres)", 1); // (1. März)

		//	p.InitItem("dr.SummerSoilRel", 1.0, typeof(double), 0.0, 1.0, "Mischungsverh. Boden-/Lufttemp ab 1.6.", 1); //0= 0%Boden, 100%Luft; 0.5= (Boden +Luft)/2; 1= 100% Boden
		//	p.InitItem("dr.AdjAir", 0.0, typeof(double),-5.0, 5.0, "Korrektur Lufttemp", 1); 
		//	p.InitItem("dr.AdjSoil", 0.0, typeof(double), -5.0, 5.0, "Korrektur Bodentemp", 1); 

		//	//Mortalitäten
		//	p.InitItem("dr.MortEgg", 0.05, typeof(double),0.01,0.25, "tägl. Mortalität Eier", 3);
		//	p.InitItem("dr.MortLarva", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Larven", 3);
		//	p.InitItem("dr.MortPupa", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Puppen", 3);
		//	p.InitItem("dr.MortFly", 0.03, typeof(double), 0.01, 0.25, "tägl. Mortalität Fliegen", 3);
		//	p.InitItem("dr.MortWiPupa", 0.02, typeof(double), 0.01, 0.25, "tägl. Mortalität Winterpuppen", 3);

		//	p.InitItem("dr.MortEggIsTDep", true, typeof(bool), 0.0, 1.0, "ist Ei-Mortalität temperaturabhängig?", 3); 
		//	p.InitItem("dr.MortEggThr", 21.0, typeof(double),15.0,35.0, "Temperaturschwelle für erhöhte Ei-Mortalität", 3);    
		//	p.InitItem("dr.MortEggInc", 0.15, typeof(double),0.01,0.5, "Anstieg Ei-Mortalität pro Grad C", 3);   

		//	p.InitItem("dr.MortLarvaIsTDep", true, typeof(bool), 0.0, 1.0, "ist Larven-Mortalität temperaturabhängig?", 3);
		//	p.InitItem("dr.MortLarvaMaxAge", 0.25, typeof(double), 0.0, 1.0, "Maximalalter für erhöhte Larven-Mortalität", 3);
		//	p.InitItem("dr.MortLarvaThr", 22.0, typeof(double),15.0,35.0, "Temperaturschwelle für erhöhte Larven-Mortalität", 3);    
		//	p.InitItem("dr.MortLarvaInc", 0.05, typeof(double), 0.01, 0.5, "Anstieg Larven-Mortalität pro Grad C", 3);  

		//	//Transitionen - Übergänge ins nächste Stadium
		//	p.InitItem("dr.TransWiPupa", 7.0, typeof(double), 5.0, 50.0, "Faktor Transition Überwinterungsstadium->Adult", 3);
		//	p.InitItem("dr.TransEgg", 7.0, typeof(double), 7.0,50.0, "Faktor Transition Ei->Larve", 3);
		//	p.InitItem("dr.TransLarva", 7.0, typeof(double), 7.0, 50.0, "Faktor Transition Larve->Puppe", 3);
		//	p.InitItem("dr.TransPupa", 7.0, typeof(double), 7.0, 50.0, "Faktor Transition Puppe->Adult", 3);
		//	p.InitItem("dr.TransFly", 8.0, typeof(double), 7.0, 50.0, "Faktor Transition Adult->Tod", 3);

		//	//Entwicklungsparameter
		//	p.InitItem("dr.DevEggTmax", 40.0, typeof(double),30.0,45.0, "Entwicklung Ei: Temperaturmaximum", 3);
		//	p.InitItem("dr.DevEggTopt", 25.0, typeof(double),15.0,29.0, "Entwicklung Ei: Temperaturoptimum", 3);
		//	p.InitItem("dr.DevEggQ", 1.88, typeof(double),1.1,4.0, "Entwicklung Ei: Temperatur-Spezifität", 3);
		//	p.InitItem("dr.DevEggL", 0.024, typeof(double),0.0,0.1, "Entwicklung Ei: Konvexität linke Flanke", 3);
		//	p.InitItem("dr.DevEggKmax", 0.375, typeof(double),0.1,0.5, "Entwicklung Ei: max. tägl. Entwicklungsrate", 3);


		//	p.InitItem("dr.DevLarvaTmax", 35.0, typeof(double), 30.0, 45.0, "Entwicklung Larve: Temperaturmaximum", 3);
		//	p.InitItem("dr.DevLarvaTopt", 27.0, typeof(double),20.0,34.0, "Entwicklung Larve: Temperaturoptimum", 3);
		//	p.InitItem("dr.DevLarvaQ", 1.73, typeof(double), 1.1, 4.0, "Entwicklung Larve: Temperatur-Spezifität", 3);
		//	p.InitItem("dr.DevLarvaL", 0.009, typeof(double),0.0,0.1, "Entwicklung Larve: Konvexität linke Flanke", 3);
		//	p.InitItem("dr.DevLarvaKmax", 0.09, typeof(double), 0.05, 0.25, "Entwicklung Larve: max. tägl. Entwicklungsrate", 3);


		//	p.InitItem("dr.DevPupaTmax", 30.0, typeof(double),28.0,40.0, "Entwicklung Puppe: Temperaturmaximum", 3);
		//	p.InitItem("dr.DevPupaTopt", 22.5, typeof(double),20.0,30.0, "Entwicklung Puppe: Temperaturoptimum", 3);
		//	p.InitItem("dr.DevPupaQ", 1.88, typeof(double), 1.1, 4.0, "Entwicklung Puppe: Temperatur-Spezifität", 3);
		//	p.InitItem("dr.DevPupaL", 0.008, typeof(double), 0.0, 0.1, "Entwicklung Puppe: Konvexität linke Flanke", 3);
		//	p.InitItem("dr.DevPupaKmax", 0.073, typeof(double), 0.05, 0.2, "Entwicklung Puppe: max. tägl. Entwicklungsrate", 3);


		//	p.InitItem("dr.DevFlyTmax", 35.0, typeof(double),32.0,45.0, "Entwicklung Fliege: Temperaturmaximum", 3);
		//	p.InitItem("dr.DevFlyTopt", 30.0, typeof(double),20.0,32.0, "Entwicklung Fliege: Temperaturoptimum", 3);
		//	p.InitItem("dr.DevFlyQ", 1.67, typeof(double), 1.1, 4.0, "Entwicklung Fliege: Temperatur-Spezifität", 3);
		//	p.InitItem("dr.DevFlyL", 0.006, typeof(double), 0.0, 0.1, "Entwicklung Fliege: Konvexität linke Flanke", 3);
		//	p.InitItem("dr.DevFlyKmax", 0.072, typeof(double), 0.05, 0.2, "Entwicklung Fliege: max. tägl. Entwicklungsrate", 3);

		//	//verbessert
		//	//p.InitItem("dr.DevWiPupaTmax", 30.87, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
		//	//p.InitItem("dr.DevWiPupaTopt", 24.44, typeof(double), 20.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
		//	//p.InitItem("dr.DevWiPupaQ", 1.68, typeof(double), 1.1, 2.0, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
		//	//p.InitItem("dr.DevWiPupaL", 0.039, typeof(double), 0.01, 0.05, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
		//	//p.InitItem("dr.DevWiPupaKmax", 0.154, typeof(double), 0.05, 0.2, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

		//	p.InitItem("dr.DevWiPupaTmax", 30.0, typeof(double), 29.0, 40.0, "Entwicklung Winterpuppe: Temperaturmaximum", 3);
		//	p.InitItem("dr.DevWiPupaTopt", 25.0, typeof(double), 20.0, 29.0, "Entwicklung Winterpuppe: Temperaturoptimum", 3);
		//	p.InitItem("dr.DevWiPupaQ", 1.62, typeof(double), 1.1, 2.0, "Entwicklung Winterpuppe: Temperatur-Spezifität", 3);
		//	p.InitItem("dr.DevWiPupaL", 0.035, typeof(double), 0.01, 0.05, "Entwicklung Winterpuppe: Konvexität linke Flanke", 3);
		//	p.InitItem("dr.DevWiPupaKmax", 0.129, typeof(double), 0.08, 0.17, "Entwicklung Winterpuppe: max. tägl. Entwicklungsrate", 3);

		//	//Fertilität
		//	p.InitItem("dr.FertPrae", 0.22, typeof(double),0.0,0.5, "Fertilität: Prä-Oviposition", 3);
		//	p.InitItem("dr.FertStartExp", 8.0, typeof(double),2.0,10.0, "Fertilität: Faktor Anstieg Eiablage", 3);
		//	p.InitItem("dr.FertPost", 0.35, typeof(double),0.0,0.5, "Fertilität: Post-Oviposition", 3);  
		//	p.InitItem("dr.FertEndExp", 3.0, typeof(double),1.1,10.0, "Fertilität: Faktor Abstieg Eiablage", 3);
		//	p.InitItem("dr.FertSumEgg", 30.0, typeof(double),10.0,100.0, "Fertilität: Anzahl Eier/Fliege", 3); // hier Gesamtsumme eintragen -Kalibrierung berechnen!
		//	p.InitItem("dr.FertCluster", 1.0, typeof(double), 1.0, 10.0, "Fertilität: Eiablage-Cluster", 3);

		//	//Flugeinschränkung durch Wind
		//	//p.InitItem("dr.IsWr", true, typeof(bool),0.0,1.0, "Flug durch Wind eingeschränkt?");
		//	//p.InitItem("dr.WrThr", 3.0, typeof(double),1.0,10.0, "Grenzwert (m/s) für Flughemmung");
		//	//p.InitItem("dr.WrInc", 0.20, typeof(double),0.1,0.9, "Anstieg Flughemmung pro zusätzl. m/s");

		//	//Diapause
		//	p.InitItem("dr.IsDia", false, typeof(bool), 0.0, 1.0, "Diapause (Winterruhe) berechnen?", 3);
		//	p.InitItem("dr.DiaDate", 225, typeof(int), 200.0, 300.0, "frühester Eintritt in Diapause (Tag des Jahres)", 3);
		//	p.InitItem("dr.DiaThr", 15.0, typeof(double), 5.0, 25.0, "Temperaturschwelle für Auslösen d. Diapause", 3);
		//	p.InitItem("dr.DiaDur", 3, typeof(int), 1.0, 25.0, "Anzahl Tage mit niedrigen Temp. für Auslösen d. Diapause", 3);


		//	//Ästivation
		//	p.InitItem("dr.IsAest", false, typeof(bool), 0.0, 1.0, "Ästivation(Sommerruhe) berechnen?", 3);// true
		//	p.InitItem("dr.AestThr", 23.0, typeof(double), 15.0, 35.0, "Temperaturschwelle für Auslösen d. Ästivation", 3);
		//	p.InitItem("dr.AestVar", 1.0, typeof(double), 0.0, 2.5, "Streuungsfaktor Ästivation", 3);     // ?? klären!
		//	p.InitItem("dr.AestDropDiff", 2.0, typeof(double), 0.0, 5.0, "Differenz für Aufheben d. Ästivation", 3);
		//	p.InitItem("dr.AestMinAge", 0.0, typeof(double), 0.0, 0.25, "Minimum biol. Alter für Ästivation", 3);
		//	p.InitItem("dr.AestMaxAge", 1.0, typeof(double), 0.25, 1.4, "Maximum biol. Alter für Ästivation", 3);


		//	return p;
		//}

		protected override int GetStartPopulation()
		{
			return (int)_workingParams.GetValue("dr.StartPop");
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
			Weather.UseOnlyAir = (bool)_workingParams.GetValue("dr.UseOnlyAir");
			Weather.AirTempAdjustment = (Double)_workingParams.GetValue("dr.AdjAir");
			Weather.SoilTempAdjustment = (Double)_workingParams.GetValue("dr.AdjSoil");
			Weather.SummerSoilRel = (Double)_workingParams.GetValue("dr.SummerSoilRel");

			_simAirTemps = Weather.GetSimAirTemp();
			_simSoilTemps = Weather.GetSimSoilTemp();
			//_simVPD = Weather.GetSimVpd();

			int startIndex = (int)_workingParams.GetValue("dr.SimStart");
			int endIndex = 365;

			//int endIndex = (int)_workingParams.GetValue("dr.SimEnd");

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
			_transitions[0]=(Double)_workingParams.GetValue("dr.TransEgg");
			_transitions[1] = (Double)_workingParams.GetValue("dr.TransLarva");
			_transitions[2] = (Double)_workingParams.GetValue("dr.TransPupa");
			_transitions[3] = (Double)_workingParams.GetValue("dr.TransFly"); 
			_transitions[4] = (Double)_workingParams.GetValue("dr.TransWiPupa");
		}

		protected override void InitTableTransition()
		{
			double t = 0.0;
			for (int stage=0; stage <=4; stage++)
			{
				switch (stage)
				{
 
					case 0:
						_maxDevRates[0] = (Double)_workingParams.GetValue("dr.DevEggKmax");
						t = (Double)_workingParams.GetValue("dr.TransEgg");
						break;
					case 1:
						_maxDevRates[1] = (Double)_workingParams.GetValue("dr.DevLarvaKmax");
						t = (Double)_workingParams.GetValue("dr.TransLarva");
						break;

					case 2:
						_maxDevRates[2] = (Double)_workingParams.GetValue("dr.DevPupaKmax");
						t = (Double)_workingParams.GetValue("dr.TransPupa");
						break;
					case 3:
						_maxDevRates[3] = (Double)_workingParams.GetValue("dr.DevFlyKmax");
						t = (Double)_workingParams.GetValue("dr.TransFly");
						break;
					case 4:
						_maxDevRates[4] = (Double)_workingParams.GetValue("dr.DevWiPupaKmax");
						t = (Double)_workingParams.GetValue("dr.TransWiPupa");
						break;

				}
				Double BioAge = 0.0;
				for(int i = 0; i<= 1400; i++)
				{
					_tableTransition[stage, i] = SimFunctions.Sigmoid(BioAge, t);
					BioAge += 0.001;
            }
			}
		}

		protected override void InitTableDev()
		{
			
			double eggTmax = (Double)_workingParams.GetValue("dr.DevEggTmax");
			double eggTopt = (Double)_workingParams.GetValue("dr.DevEggTopt");
			double eggQ = (Double)_workingParams.GetValue("dr.DevEggQ");
			double eggL = (Double)_workingParams.GetValue("dr.DevEggL");
			double eggKmax = (Double)_workingParams.GetValue("dr.DevEggKmax");

			
			double larvaTmax = (Double)_workingParams.GetValue("dr.DevLarvaTmax");
			double larvaTopt = (Double)_workingParams.GetValue("dr.DevLarvaTopt");
			double larvaQ = (Double)_workingParams.GetValue("dr.DevLarvaQ");
			double larvaL = (Double)_workingParams.GetValue("dr.DevLarvaL");
			double larvaKmax = (Double)_workingParams.GetValue("dr.DevLarvaKmax");

			
			double pupaTmax = (Double)_workingParams.GetValue("dr.DevPupaTmax");
			double pupaTopt = (Double)_workingParams.GetValue("dr.DevPupaTopt");
			double pupaQ = (Double)_workingParams.GetValue("dr.DevPupaQ");
			double pupaL = (Double)_workingParams.GetValue("dr.DevPupaL");
			double pupaKmax = (Double)_workingParams.GetValue("dr.DevPupaKmax");

			
			double flyTmax = (Double)_workingParams.GetValue("dr.DevFlyTmax");
			double flyTopt = (Double)_workingParams.GetValue("dr.DevFlyTopt");
			double flyQ = (Double)_workingParams.GetValue("dr.DevFlyQ");
			double flyL = (Double)_workingParams.GetValue("dr.DevFlyL");
			double flyKmax = (Double)_workingParams.GetValue("dr.DevFlyKmax");

			
			double wipupaTmax = (Double)_workingParams.GetValue("dr.DevWiPupaTmax");
			double wipupaTopt = (Double)_workingParams.GetValue("dr.DevWiPupaTopt");
			double wipupaQ = (Double)_workingParams.GetValue("dr.DevWiPupaQ");
			double wipupaL = (Double)_workingParams.GetValue("dr.DevWiPupaL");
			double wipupaKmax = (Double)_workingParams.GetValue("dr.DevWiPupaKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				double airTemp = _simAirTemps[i];
				_tableDev[(int)DevStage.Fly, i] = SimFunctions.ONeal(airTemp,  flyTmax, flyTopt, flyQ,flyL, flyKmax);

				double soilTemp = _simSoilTemps[i];
				_tableDev[(int)DevStage.Egg, i] = SimFunctions.ONeal(soilTemp,  eggTmax, eggTopt, eggQ, eggL, eggKmax);
				_tableDev[(int)DevStage.Larva, i] = SimFunctions.ONeal(soilTemp,  larvaTmax, larvaTopt, larvaQ,larvaL, larvaKmax);
				_tableDev[(int)DevStage.Pupa, i] = SimFunctions.ONeal(soilTemp,  pupaTmax, pupaTopt, pupaQ, pupaL, pupaKmax);
				_tableDev[(int)DevStage.WiPupa, i] = SimFunctions.ONeal(soilTemp,  wipupaTmax, wipupaTopt, wipupaQ,wipupaL, wipupaKmax);
			}
		}

		//protected override void InitTableDiapause()
		//{
		//	// todo: Diapause implementieren
		//	//throw new NotImplementedException();
		//}

		protected override void InitTableFert()
		{

			double startFert = (Double)_workingParams.GetValue("dr.FertPrae");
			double startSkew = (Double)_workingParams.GetValue("dr.FertStartExp");
			double endFert = (Double)_workingParams.GetValue("dr.FertPost");
			double endSkew = (Double)_workingParams.GetValue("dr.FertEndExp");
			double sumEgg = (Double)_workingParams.GetValue("dr.FertSumEgg");

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
			//bool isWindRestr = (bool)_workingParams.GetValue("dr.IsWr");
			//double windThreshold = (Double)_workingParams.GetValue("dr.WrThr");
			//double windInc = (Double)_workingParams.GetValue("dr.WrInc");

			double flyMaxDev = (Double)_workingParams.GetValue("dr.DevFlyKmax");

			for (int i = _firstSimIndex; i < _lastSimIndex; i++)
			{
				_tableFlightAct[i] = _tableDev[(int)DevStage.Fly, i] / flyMaxDev; // Flugaktivität abh. von akt. Entwicklungsrate

				//if (isWindRestr)// Einschränkung durch Wind
				//{
				//	double windSpeed = _simWindSpeeds[i];
				//	double w = 1.0 - (windSpeed - windThreshold) * (windInc);
				//	_tableWindRestr[i] = Math.Min(1.0, Math.Max(0.0, w));
				//}
				//else
				//	_tableWindRestr[i] = 1.0;
			}
		}

		protected override void InitTableMortality()
		{
			double mortEgg = (Double)_workingParams.GetValue("dr.MortEgg");
			double mortLarva = (Double)_workingParams.GetValue("dr.MortLarva");
			double mortPupa = (Double)_workingParams.GetValue("dr.MortPupa");
			double mortFly = (Double)_workingParams.GetValue("dr.MortFly");
			double mortWiPupa = (Double)_workingParams.GetValue("dr.MortWiPupa");


			bool isVarEggMort = (bool)_workingParams.GetValue("dr.MortEggIsTDep");
			double mortEggThr = (Double)_workingParams.GetValue("dr.MortEggThr");
			double mortEggInc = (Double)_workingParams.GetValue("dr.MortEggInc");

			bool isVarLarvaMort = (bool)_workingParams.GetValue("dr.MortLarvaIsTDep");
			double mortLarvaThr = (Double)_workingParams.GetValue("dr.MortLarvaThr");
			double mortLarvaInc = (Double)_workingParams.GetValue("dr.MortLarvaInc");

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

			return SumEgg/(sum*1000.0);
		}

		#endregion

		#region Simulationsschleife

		public double IndividualAestThreshold
		{
			get
			{ 
				bool isAest = (bool)_workingParams.GetValue("dr.IsAest");
				if (!isAest)
					return 100.0;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				double threshold = (double)_workingParams.GetValue("dr.AestThr");
				double variance = (double)_workingParams.GetValue("dr.AestVar");

				return SimFunctions.AestivTemp(GetRandom, threshold, variance);
			}
		}

		public double AestMinAge
		{
			get { return (double)_workingParams.GetValue("dr.AestMinAge"); }
		}

		public double AestMaxAge
		{
			get { return (double)_workingParams.GetValue("dr.AestMaxAge"); }
		}

		public double AestDropDiff
		{
			get {return (double)_workingParams.GetValue("dr.AestDropDiff"); }
		}

		public double MortLarvaMaxAge
		{
			get { return (double)_workingParams.GetValue("dr.MortLarvaMaxAge"); }
		}

		public double FertCluster
		{
			get { return (double)_workingParams.GetValue("dr.FertCluster"); }
		}

		public int DiapauseIndex
		{
			get
			{ 
				bool isDiapause = (bool)_workingParams.GetValue("dr.IsDia");
				if (!isDiapause)
					return 366;  // auf unmöglichen Wert setzen, wenn ausgeschaltet

				return (int)_workingParams.GetValue("dr.DiaDate");
			}
		}

		public double DiapauseTemp
		{
			get {  return (double)_workingParams.GetValue("dr.DiaThr"); }

		}

		public int DiapauseDur
		{
			get { return (int)_workingParams.GetValue("dr.DiaDur"); }

		}

		protected override void Individual(DevStage startStage, double startAge, int dayIndex, int generation, bool isDiapauseGen)
		{
			IndividualDR.CreateAndLive(this, startStage, startAge, dayIndex, generation, isDiapauseGen);
		}
		
		#endregion

	}
}

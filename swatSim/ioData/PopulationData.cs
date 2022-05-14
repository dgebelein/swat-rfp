
using SwatPresentations;
using System;
//using System.Collections.Generic;
//using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace swatSim
{


	public class PopulationData
	{
		#region Variable


		const int maxAc = 10;   // Aufteilung in 10 Altersklassen
										//todo:14 Altersklassen?

		int _maxGen;
		Int64[,] eggs;
		Int64[,] larvals;
		Int64[,] pupas;
		Int64[,] flies;
		Int64[,] wipupas;
		Int64[,] newEggs;
		Int64[,] activeFlies;
		Int64[,,] eggAc;
		Int64[,,] larvalAc;
		Int64[,,] pupaAc;
		Int64[,,] flyAc;
		Int64[,,] pupaAestAc;

		int[] _maxEggPeriods; //für Monitoring: max Anzahl von Tagen, die Eiablage zurückliegen kann

		double[] _normalisationFactors;
		int _numGenerations;

		public string Title { get; set; }
		public int Year { get; set; }
		public Int64 NumIndividuals { get; set; }
		public bool HasValidData { get; set; }

		#endregion

		#region Construction

		public PopulationData(int maxGen)
		{
			_maxGen = maxGen;
			Initialize();
		}

		public void Initialize()
		{
			eggs = new Int64[_maxGen, 366];
			larvals = new Int64[_maxGen, 366];
			pupas = new Int64[_maxGen, 366];
			flies = new Int64[_maxGen, 366];
			wipupas = new Int64[_maxGen, 366];
			newEggs = new Int64[_maxGen, 366];
			activeFlies = new Int64[_maxGen, 366];

			eggAc = new Int64[_maxGen, maxAc, 366];
			larvalAc = new Int64[_maxGen, maxAc, 366];
			pupaAc = new Int64[_maxGen, maxAc, 366];
			flyAc = new Int64[_maxGen, maxAc, 366];
			pupaAestAc = new Int64[_maxGen, maxAc, 366];
			NumIndividuals = 0;

			_normalisationFactors = null;
			_numGenerations = -1;
			HasValidData = false;
		}

		#endregion

		#region Datenzugriff

		public void Add(DevStage stage, int dayIndex, int generation, double bioAge, bool isAest)
		{
			if (generation >= _maxGen)
				return;

			int bioAc = Math.Min(9, (int)(bioAge * 10));

			switch (stage)
			{
				case DevStage.Egg:
					eggs[generation, dayIndex]++;
					eggAc[generation, bioAc, dayIndex]++;
					break;
				case DevStage.Larva:
					larvals[generation, dayIndex]++;
					larvalAc[generation, bioAc, dayIndex]++;
					break;
				case DevStage.Pupa:
					pupas[generation, dayIndex]++;
					pupaAc[generation, bioAc, dayIndex]++;
					if (isAest)
						pupaAestAc[generation, bioAc, dayIndex]++;
					break;
				case DevStage.WiPupa:
					wipupas[generation, dayIndex]++;
					pupaAc[generation, bioAc, dayIndex]++;
					break;
				case DevStage.Fly:
					flies[generation, dayIndex]++;
					flyAc[generation, bioAc, dayIndex]++;
					break;
				case DevStage.NewEgg:
					newEggs[generation, dayIndex]++;
					break;
				case DevStage.ActiveFly:
					activeFlies[generation, dayIndex]++;
					break;
			}
		}


		public void SetEggPeriods(int[] periods)
		{
			_maxEggPeriods = new int[366];

			for (int i = 0; i < 366; i++)
				_maxEggPeriods[i] = periods[i];
		}

		public int[] MaxEggPeriods
		{
			get { return _maxEggPeriods; }
		}

		public double[] NormalisationFactors
		{
			get
			{
				CalcNormalisation();
				return _normalisationFactors;
			}
		}

		public Int64 GetVal(DevStage stage, int generation, int dayIndex)
		{
			if (generation >= _maxGen)
				return 0;

			switch (stage)
			{
				case DevStage.Egg:
					return eggs[generation, dayIndex];
				case DevStage.Larva:
					return larvals[generation, dayIndex];
				case DevStage.Pupa:
					return pupas[generation, dayIndex];
				case DevStage.Fly:
					return flies[generation, dayIndex];
				case DevStage.WiPupa:
					return wipupas[generation, dayIndex];
				case DevStage.NewEgg:
					return newEggs[generation, dayIndex];
				case DevStage.ActiveFly:
					return activeFlies[generation, dayIndex];
			}
			return 0;
		}

		public double[] GetValRow(DevStage stage, int generation)
		{
			double[] row = new double[366];

			int mg = GetNumGenerations();
			if (generation > mg)
				return row;

			if (generation >= 0)
			{
				for (int d = 0; d < 366; d++)
					row[d] = GetVal(stage, generation, d);
			}
			else
			{
				for (int g = 0; g <= mg; g++)
				{
					for (int d = 0; d < 366; d++)
						row[d] += GetVal(stage, g, d);
				}
			}

			return row;
		}

		public double[] GetNormalizedRow(DevStage stage, int generation, double[] normFactors, int maxIndex = 366)
		{
			if (generation >= 0) // Generationen einzeln
			{
				double[] normRow = GetValRow(stage, generation);

				for (int d = 0; d < maxIndex; d++)
				{
					normRow[d] *= normFactors[generation];
				}
				return normRow;
			}
			else // alle Generationen zusammen
			{
				double[] normRow = new double[maxIndex];
				int mg = Math.Min(_maxGen, normFactors.Length);
				for (int g = 0; g < mg; g++)
				{
					double[] row = GetValRow(stage, g);
					for (int d = 0; d < maxIndex; d++)
					{
						normRow[d] += row[d] * normFactors[g];
					}
				}
				return normRow;
			}
		}

		public double[] GetNormalizedRow(DevStage stage, int generation)
		{
			if (_normalisationFactors == null)
			{
				CalcNormalisation(); // gleiche Individuenzahlen für alle Generationen
			}
			return GetNormalizedRow(stage, generation, _normalisationFactors);
		}

		public double[] GetAgeClasses(DevStage stage, int day)
		{
			if (_normalisationFactors == null)
			{
				CalcNormalisation();
			}

			double[] ac = new double[maxAc];
			switch (stage)
			{
				case DevStage.Egg:
					for (int g = 0; g < _maxGen; g++)
					{
						for (int c = 0; c < maxAc / 2; c++)
						{
							ac[c] += eggAc[g, c * 2, day] * _normalisationFactors[g];   // immer zwei Altersklassen zu einem Balken zusammenfassen
							ac[c] += eggAc[g, c * 2 + 1, day] * _normalisationFactors[g]; // wegen sonst zu kleiner Belegungszahlen
						}

					}
					break;

				case DevStage.Larva:
					for (int g = 0; g < _maxGen; g++)
					{
						for (int c = 0; c < maxAc; c++)
							ac[c] += larvalAc[g, c, day] * _normalisationFactors[g];
					}
					break;

				case DevStage.Pupa:
					for (int g = 0; g < _maxGen; g++)
					{
						for (int c = 0; c < maxAc; c++)
							ac[c] += pupaAc[g, c, day] * _normalisationFactors[g];
					}
					break;

				case DevStage.Fly:
					for (int g = 0; g < _maxGen; g++)
					{
						for (int c = 0; c < maxAc; c++)
							ac[c] += flyAc[g, c, day] * _normalisationFactors[g];
					}
					break;

				case DevStage.AestPup:
					for (int g = 0; g < _maxGen; g++)
					{
						for (int c = 0; c < maxAc; c++)
							ac[c] += pupaAestAc[g, c, day] * _normalisationFactors[g];
					}
					break;

				default: break;

			}
			return ac;

		}

		private void CalcNormalisation()
		{
			_normalisationFactors = new double[_maxGen];

			double num = GetValRow(DevStage.WiPupa, 0).Max();
			if (num == 0)
				num = 1.0;
			_normalisationFactors[0] = 1.0;

			for (int g = 1; g < _maxGen; g++)
			{
				double s = GetValRow(DevStage.NewEgg, g).Sum();
				_normalisationFactors[g] = (s > 0.0) ? num / s : 0.0;
			}

			//eventuelle letzte "rein rechnerische" Generation mit verschwindend geringen Individuenzahlen ignorieren
			for (int numGen = GetNumGenerations(); numGen > 3; numGen--)
			{
				if ((_normalisationFactors[numGen] / _normalisationFactors[numGen - 1]) > 5)
					_normalisationFactors[numGen] = 0.0;
			}
		}
		#endregion

		#region Berechnungen

		public int GetNumGenerations()
		{
			if (_numGenerations >= 0) // schon berechnet?
				return _numGenerations;

			// ansonsten aus Array-Belegung ermitteln
			_numGenerations = 0;
			for (int g = 0; g < _maxGen; g++)
			{
				for (int s = (int)DevStage.Egg; s <= (int)DevStage.ActiveFly; s++)
				{
					for (int d = 0; d < 365; d++)
					{
						if (GetVal((DevStage)s, g, d) > 0)
						{
							_numGenerations = g;
							goto nextGen;
						}
					}
				}

			nextGen: continue;
			}

			return _numGenerations;
		}


		public int GenerationStartIndex(DevStage stage, int generation)
		{
			if ((generation < 1) || (generation > GetNumGenerations()))
				return -1;

			double[] row = GetValRow(stage, generation);
			double sumStart = row.Sum() * 0.05; // Generationstart bei 5% der Eiablagesumme/aktiven Fliegen (Fliegen ungenauer!)
			double sumIndiv = 0.0;

			for (int i = 0; i < 365; i++)
			{
				sumIndiv += row[i];
				if (sumIndiv > sumStart)
					return i;
			}

			return -1;

		}

		#endregion

		#region IO

		private void WriteElem(StreamWriter w, Int64 elem)
		{
			string delim = ";";
			w.Write($"{elem}{delim}");
		}


		public void WritePopToFile(string fileName)
		{
			int numGenerations = GetNumGenerations();

			try
			{
				using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					//Kopfzeile
					sw.Write("day;");
					for (int s = (int)DevStage.Egg; s <= (int)DevStage.ActiveFly; s++)
					{
						for (int g = 0; g <= numGenerations; g++)
						{
							DevStage st = (DevStage)s;
							sw.Write($"{st}{g};");

						}
					}
					sw.WriteLine();

					// Populationdaten
					for (int d = 0; d < 365; d++)
					{
						WriteElem(sw, d);
						for (int s = (int)DevStage.Egg; s <= (int)DevStage.ActiveFly; s++)
						{
							for (int g = 0; g <= numGenerations; g++)
							{
								WriteElem(sw, GetVal((DevStage)s, g, d));
							}

						}
						sw.WriteLine();
					}
				}
			}
			catch (Exception ex)
			{
				DlgMessage.Show("Daten können nicht geschrieben werden", ex.Message, MessageLevel.Error);
			}

		}

		public void WriteAKToFile(string fileName)
		{
			//Int64[,,] eggAc;
			//Int64[,,] larvalAc;
			//Int64[,,] pupaAc;
			//Int64[,,] flyAc;
			//Int64[,,] pupaAestAc;

			try
			{
				using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
				{
					//Kopfzeile
					sw.Write("day;");
					for (int s = (int)DevStage.Egg; s <= (int)DevStage.Fly; s++)
					{
						int mc = (s == (int)DevStage.Egg) ? 5 : 10;
						for (int g = 0; g < mc; g++)
						{
							DevStage st = (DevStage)s;
							sw.Write($"{st}{g};");
						}
					}
					for (int g = 0; g < 10; g++)
					{
						DevStage st = DevStage.AestPup;
						sw.Write($"{st}{g};");
					}

					sw.WriteLine();

					// Age-Class-Data
					for (int d = 0; d < 365; d++)
					{
						WriteElem(sw, d+1);

						double[] eggs = GetAgeClasses(DevStage.Egg, d);
						for(int i=0; i<5; i++) 
						{ 
							WriteElem(sw,(Int64) eggs[i]);
						}
						double[] larvals = GetAgeClasses(DevStage.Larva, d);
						for (int i = 0; i < 10; i++)
						{
							WriteElem(sw, (Int64)larvals[i]);
						}

						double[] pupals = GetAgeClasses(DevStage.Pupa, d);
						for (int i = 0; i < 10; i++)
						{
							WriteElem(sw, (Int64)pupals[i]);
						}

						double[] flies = GetAgeClasses(DevStage.Fly, d);
						for (int i = 0; i < 10; i++)
						{
							WriteElem(sw, (Int64)flies[i]);
						}

						double[] aestPupals = GetAgeClasses(DevStage.AestPup, d);
						for (int i = 0; i < 10; i++)
						{
							WriteElem(sw, (Int64)aestPupals[i]);
						}


						sw.WriteLine();
					}
				}
			}
			catch (Exception ex)
			{
				DlgMessage.Show("Daten können nicht geschrieben werden", ex.Message, MessageLevel.Error);
			}


		}
		#endregion
	}
}

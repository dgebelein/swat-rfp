using swatSim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwopCompare
{
	class GraphData 
	{

		#region Variables
		double[] _randomNumbers;
		CmpData _data;
		bool _hasEggs;
		//SimParamData _localSetParams;
		//SimParamData _commonBestParams;
		//SimParamData _setBestParams;
		int _setIndex;
		Int64 _lastSimIndivCalc; // Hilfsvariable zum Übertragen der Individuenanzahl;

		//public WeatherData Weather { get; private set; }
		//public MonitoringData Monitoring { get; private set; }


		ModelBase _model;
		//SimParamData _localSetParams;

		#endregion

		#region Construction
		public GraphData(CmpData sd, int setId)
		{
			_data = sd;
			_setIndex = setId;
			_randomNumbers = new double[10000000]; // Erzeugung von Zufallszahlen-Folge
			Random rand = new Random(96);
			for (int i = 0; i < _randomNumbers.Length; i++)
			{
				_randomNumbers[i] = rand.NextDouble();
			}
			
		}


		//private ModelBase CreateSimulationModel(WeatherData wd, SimParamData simParam)
		//{
		//	ModelBase model;
		//	switch (_data.ModelType)
		//	{
		//		case FlyType.DR: model = new ModelDR("", wd, null, simParam); break;
		//		case FlyType.PR: model = new ModelPR("", wd, null, simParam); break;
		//		case FlyType.DA: model = new ModelDA("", wd, null, simParam); break;
		//		default: throw new Exception("Modelloptimierung für dieses Modell nicht implementiert");
		//	}

		//	model.SetRandomNumbers(_randomNumbers);
		//	return model;
		//}

		#endregion
	}
}

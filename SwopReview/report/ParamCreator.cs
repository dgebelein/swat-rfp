using SwatPresentations;
using swatSim;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwopReview
{
	class ParamCreator
	{
		readonly SwopData _data;
		//string _paramPath;
		SimParamData _setParams;
		//int _setId;

		public ParamCreator(SwopData sd)
		{
			_data = sd;
		}

		public bool CreateParamFile(int setIndex,string filePath)
		{
			CreateSetParameters(setIndex);
			return WriteParameterFile(setIndex, filePath);
		}

		void CreateSetParameters(int setId)
		{
			ModelBase model;
			switch (_data.ModelType)
			{
				case FlyType.DR: model = new ModelDR(null, null, null); break;
				case FlyType.PR: model = new ModelPR(null, null, null); break;
				case FlyType.DA: model = new ModelDA(null, null, null); break;

				default: model = null; break;
			}
			_setParams = model.CodedParams;
			
			//Lokale Parameter
			foreach(string s in _data.OptSets[setId].LocalParams)
			{
				_setParams.ReadFromString(s);
			}
			
			// neu optimierte Parameter
			int p = 0;
			foreach (string param in _data.OptParameters)
			{
				string val = GetParameterValueString(_setParams, param, _data.BestParamValues[p++]);
				string st=$"{param} = {val}";
				_setParams.ReadFromString(st);
			}
		}


		public static string GetParameterValueString(SimParamData paramData, string paramKey, double paramValue)
		{
			Type elemType = null; // wegen Szenarien, in denen paramKeys geändert wurden und die deshalb ungültige Keys enthalten
			SimParamElem tt = paramData.GetParamElem(paramKey);
			if (tt!= null)
				elemType = tt.ObjType;


			try { 
				switch (Type.GetTypeCode(elemType))
				{
					case TypeCode.Boolean:
						return (paramValue >= 0.5) ? "true" : "false"; 
					case TypeCode.Int32:
						Int32 n = (Int32)paramValue; return n.ToString();
					default:// TypeCode.Double:
						return paramValue.ToString("0.0###", CultureInfo.InvariantCulture);
				}
			}
			catch
			{
				return "";
			}

		}

		bool WriteParameterFile(int setIndex, string filePath)
		{
			string fileName = Path.Combine(filePath, _data.OptSets[setIndex].Monitoring + ".swat-par");
			if(!_setParams.WriteToFile(fileName))
			{
				DlgMessage.Show("Error writing File", _setParams.ErrorMsg, MessageLevel.Error);
				return false;
			}
			return true;
		}

		public SimParamData GetBestCommonParameters(int setId)
		{
			return null;
		}

		public SimParamData GetBestSetParameters(int setId)
		{
			return null;
		}

	}
}

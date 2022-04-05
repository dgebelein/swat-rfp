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
	static class ParamCreator
	{
		public static bool CreateParamFile(SwopData swopData,int setIndex,string filePath)
		{
			SimData sd = new SimData(swopData, setIndex);
			SimParamData para = sd.GetCommonBestParams();
			string prefix = swopData.ParaPrefix;

			string fileName = Path.Combine(filePath, prefix + swopData.OptSets[setIndex].Monitoring + ".swat-par");
			if (!para.WriteToFile(fileName))
			{
				DlgMessage.Show("Error writing File", para.ErrorMsg, MessageLevel.Error);
				return false;
			}
			return true;
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
	}
}

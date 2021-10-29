using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.Engine3;

namespace SwatImporter
{
	internal class MesWriter
	{
		private readonly DwdColumnDeclarator _declarator;
		private readonly List<ConvertedLine> _dataList;
		private readonly int _dataIndex;
		private readonly string _outputPath;
		private readonly string _mesGroup;
		private readonly TtpEnPattern _pattern = TtpEnPattern.Pattern1Day;

		public string ErrorMessage { get; set; }

		public MesWriter(string outputPath, string mesGroup, DwdColumnDeclarator declarator, List<ConvertedLine> dataList, int dataIndex)
		{
			_outputPath = outputPath;
			_mesGroup = mesGroup;
			_declarator = declarator;
			_dataList = dataList;
			_dataIndex = dataIndex;


		}

		public bool Write()
		{
			TtpMes mes = new TtpMes();
			string fn = Path.Combine(_outputPath, _mesGroup + "-" + _declarator.MesIndex + ".mes");

			if (!mes.Attach(fn))
			{
				if (mes.Error == TtpEnMesError.FILE_NOT_FOUND)
				{
					if (!mes.CreateNewAnalogFile(fn, _declarator.MesText, "", _declarator.MesDim,
						_pattern, _declarator.RecType, 1, true, _declarator.DefCalc))
					{
						ErrorMessage = fn + " " + mes.ErrorMsg;
						return false;
					}
				}
				else
				{
					ErrorMessage = fn + " " + mes.ErrorMsg;
					return false;
				}
			}

			if (!mes.SetValues(GetDataRow(), TtpEnWriteMode.UPDATE))
			{
				ErrorMessage = fn + " " + mes.ErrorMsg;
				return false;
			}

			return true;

		}

		private TtpAnalogData GetDataRow()
		{
			int numVal = _dataList.Count;
			long start = _dataList[0].Time;
			long end = _dataList[numVal - 1].Time;
			TtpTimeRange tr = new TtpTimeRange(start, end, _pattern);
			double mult = _declarator.Multiplicator;

			TtpAnalogData mesData = new TtpAnalogData();
			mesData.SetTimeRange(tr);

			if (mult == 1.0)
			{
				for (int i = 0; i < numVal; i++)
					mesData.SetData(_dataList[i].Values[_dataIndex], new TtpTime(_dataList[i].Time));
			}
			else
			{
				for (int i = 0; i < numVal; i++)
					mesData.SetData(_dataList[i].Values[_dataIndex] * mult, new TtpTime(_dataList[i].Time));
			}



			return mesData;
		}


	}
}

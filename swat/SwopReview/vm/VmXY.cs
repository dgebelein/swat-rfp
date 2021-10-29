using SwatPresentations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TTP.TtpCommand3;
using TTP.UiUtils;

namespace SwopReview
{
	class VmXY : VmBase
	{
		#region variable
		PresentationsData _graphData;
		RelayCommand _printCommand;
		readonly Brush[] _colors = { Brushes.CornflowerBlue, Brushes.Crimson, Brushes.DarkOrange, Brushes.SpringGreen, Brushes.Gold, Brushes.Fuchsia, Brushes.HotPink, Brushes.Indigo, Brushes.Cyan };


		#endregion

		#region construction

		public VmXY(SwopData sd, bool absoluteErrors, int paramId, int[]setIds, List<string> optLaps ):base(sd, null) 
		{
			_printCommand = new RelayCommand(param => this.Print());

			_graphData = GeneratePresentationsData(absoluteErrors, paramId, setIds, optLaps);
			ViewVisual = PresentationCreator.Create(PresentationType.ScatterPlot, _graphData, false);
		}


		#endregion

		#region properties

		public ICommand PrintCommand { get { return _printCommand; } }


		#endregion

		#region  generierung daten 
		private PresentationsData GeneratePresentationsData(bool absoluteErrors, int paramId, int[] setIds, List<string> optLaps)
		{
			PresentationsData data = new PresentationsData
			{
				Title = _swopData.SwopLogName,
				YLegend= (absoluteErrors)?"Error / absolute": "Error / relative",
				ZoomFactor = 0
			};

			AddSets(data, absoluteErrors, paramId, setIds, optLaps);  
			return data;
		}


		private void AddSets(PresentationsData data, bool absoluteErrors, int paramId, int[] setIds, List<string> optLaps)
		{
			int groupId = 0;

			data.AddRow(CreateXRow(data, paramId, optLaps));// Reihe 0 ist X-Wert

			foreach(int id in setIds)
			{
				if(groupId < 8) // max 8 Reihen
					data.AddRow(CreateYRow(data, absoluteErrors, id, groupId++, optLaps));
			}
		}

		private PresentationRow CreateXRow(PresentationsData data, int paramId, List<string> optLaps)
		{
			List<double> valList = new List<double>();

			int num = _swopData.CommonErrors.Length;
			for (int i = 0; i < num; i++)
			{
				if (optLaps.Contains(_swopData.StepLaps[i]))
					valList.Add( (paramId == 0) ? i : _swopData.GetOptParamValue(i, paramId - 1)); // Indices??
			}

			PresentationRow xRow = new PresentationRow
			{
				Values = valList.ToArray(),
				IsVisible = true,
				LegendIndex = -1,
				Color = Brushes.Black,
				LineType = TtpEnLineType.Point,
				Legend = (paramId == 0) ? "Step" : _swopData.OptParameters[paramId-1]
			};
			return xRow;

		}


		private PresentationRow CreateYRow(PresentationsData data,bool absoluteErrors, int setId, int groupId, List<string> optLaps)
		{
			List<double> valList = new List<double>();
			double[] tot = (absoluteErrors) ? _swopData.CommonErrorsAbsolute : _swopData.CommonErrors;

			int num = _swopData.CommonErrors.Length;
			for (int i = 0; i < num; i++)
			{
				if (optLaps.Contains(_swopData.StepLaps[i]))
					valList.Add( (setId == 0) ? tot[i] : _swopData.GetSetError(setId - 1, i, absoluteErrors));
			}

			PresentationRow yRow = new PresentationRow			{
				Values = valList.ToArray(),
				IsVisible = true,
				LegendIndex= groupId,
				LineType = TtpEnLineType.Point,
				Color = _colors[groupId],
				Legend = (setId == 0) ? "Common" : _swopData.OptSets[setId - 1].Monitoring
			};
			return yRow;

		}

		#endregion

		#region methoden

		void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.ScatterPlot, _graphData, true);
			printPres.PrintView();
		}

		#endregion

	}
}

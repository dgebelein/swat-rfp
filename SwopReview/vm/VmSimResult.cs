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
using TTP.Engine3;
using TTP.TtpCommand3;
using TTP.UiUtils;

namespace SwopReview
{
	class VmSimResult : VmBase
	{
		#region variable
		SimData _simData;
		//PresentationsData _graphData;
		RelayCommand _printCommand;

		#endregion

		#region construction

		public VmSimResult(SwopData sd, int setId) : base(sd, null)
		{
			//_printCommand = new RelayCommand(param => this.Print());
			_simData = new SimData(_swopData, setId);
			PresentationsData graphData = GeneratePresentationsData(setId);
			ViewVisual = (graphData != null ) ? PresentationCreator.Create(PresentationType.Optimization, graphData, false) : null;
		}


		#endregion

		#region  generierung daten 
		private PresentationsData GeneratePresentationsData(int setId)
		{
			PresentationsData presData = new PresentationsData
			{
				Title = _swopData.SwopLogName + "  "+ _swopData.OptSets[setId].Monitoring,
				ZoomFactor = 0,
				ZoomFactorRight = 0,
			};

			if (!_simData.AddSimTrends(presData))
				return null;

			presData.TimeRange = new TtpTimeRange(new TtpTime("1.1." + _simData.GetSimYear()), TtpEnPattern.Pattern1Year, 1);
			presData.HighlightTimeRange = _simData.GetEvalTimeSpan();
			return presData;

		}

		//private bool AddSimTrends(PresentationsData presData, int setId)
		//{ 
		//	if(!_simData.BuildSimSources())
		//		return false;

		//	_simData.AddTrends(presData);
		//	return true;
		//}

		//private void AddMonitoringRow(PresentationsData pd)
		//{
		//	pd.AddRow(new PresentationRow
		//	{
		//		Legend = (hasEggs) ? "Oviposion - Monitoring" : "Flight - Monitoring",				
		//		Values = (hasEggs) ?_simData.Monitoring.Eggs: _simData.Monitoring.Adults,
		//		LegendIndex = 0,
		//		IsVisible = true,
		//		Thicknes = 1.0,
		//		Color = Brushes.CornflowerBlue,
		//		Axis = TtpEnAxis.Left,
		//		LineType = TtpEnLineType.LinePoint
		//	});

		//}

		private void AddPrognStartRow()
		{

		}
		private void AddPrognBestCommonRow()
		{

		}
		private void AddPrognBestForSetRow()
		{

		}

		private void AddWeatherRows()
		{

		}

		#endregion
	}
}

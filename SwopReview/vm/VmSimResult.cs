﻿using SwatPresentations;
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
		PresentationsData _graphData;
		RelayCommand _printCommand;
		RelayCommand _exportCommand;


		#endregion

		#region construction

		public VmSimResult(SwopData sd, int setId) : base(sd, null)
		{
			_printCommand = new RelayCommand(param => this.Print());
			_exportCommand = new RelayCommand(param => this.ExportToFile());


			_simData = new SimData(_swopData, setId);
			_graphData = GeneratePresentationsData(setId);

			ViewVisual = (_graphData != null ) ? PresentationCreator.Create(PresentationType.Optimization, _graphData, false) : null;
		}


		#endregion

		#region  generierung daten 
		private PresentationsData GeneratePresentationsData(int setId)
		{
			_graphData = new PresentationsData
			{
				Title = _swopData.SwopLogName + "  "+ _swopData.OptSets[setId].Monitoring,
				ZoomFactor = 0,
				ZoomFactorRight = 0,
			};

			if (!_simData.AddSimTrends(_graphData))
				return null;

			_graphData.TimeRange = new TtpTimeRange(new TtpTime("1.1." + _simData.GetSimYear()), TtpEnPattern.Pattern1Year, 1);
			_graphData.HighlightTimeRange = _simData.GetEvalTimeSpan();
			_graphData.TitleToolTip = _simData.Notes;
			_graphData.AddMarkers(_simData.Notes, _simData.GetSimYear());


			return _graphData;

		}

		#endregion

		#region Properties für Binding
		public ICommand PrintCommand { get { return _printCommand; } }
		public ICommand ExportCommand { get { return _exportCommand; } }

		public Visibility VisQuantCommands { get { return Visibility.Collapsed; } } // entfernt die quantifizierungsmethoden aus contextmenü
		public Visibility VisExportCommand { get { return Visibility.Collapsed; } } // entfernt den Quantifizierungsexport im contextmenü
		#endregion

		#region Methoden  aus Contextmenü

		private void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.Optimization, _graphData, true);
			printPres.PrintView();
		}

		private void ExportToFile()
		{
			_simData.ExportToCsv();
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using SwatPresentations;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TTP.TtpCommand3;
using TTP.UiUtils;
using System.Collections.Concurrent;

namespace SwopReview
{
	class VmPanelMesh : VmBase
	{
		#region variable
		PresentationsMeshPanelData _graphPanelData;
		RelayCommand _printCommand;
		#endregion

		#region construction

		public VmPanelMesh(SwopData sd, bool absoluteErrors, List<ParamCombi> pcl, int setId, List<string> optLaps) : base(sd, null)
		{
			_printCommand = new RelayCommand(param => this.Print());

			_graphPanelData = GeneratePresentationsData(absoluteErrors, pcl, setId, optLaps);
			ViewVisual = PresentationCreator.Create(PresentationType.MeshPanel, _graphPanelData, false);
		}

		#endregion

		#region properties

		public ICommand PrintCommand { get { return _printCommand; } }


		#endregion

		#region  generierung daten 
		private PresentationsMeshPanelData GeneratePresentationsData(bool absoluteErrors, List<ParamCombi> pcl, int setId, List<string> optLaps)
		{
			string title = _swopData.SwopLogName;
			if(setId != 0)
			{
				string set = _swopData.OptSets[setId-1].Monitoring;
				title = title + " / " + set;
			}

			PresentationsMeshPanelData panelData = new PresentationsMeshPanelData
			{
				Title = title,
			};


			AddMeshPanelData(panelData, absoluteErrors, pcl, setId, optLaps);
			return panelData;
		}

		private void AddMeshPanelData(PresentationsMeshPanelData panelData, bool absoluteErrors, List<ParamCombi> pcl, int setId, List<string> optLaps)
		{
			SolidColorBrush[] colorBrushes = MeshRects.CreateBrushes();
			foreach (ParamCombi pc in pcl)
			{
				PresentationsMeshData presData = new PresentationsMeshData
				{
					HasPanelData = true,
					Title = pc.YPara,
					XLegend = pc.XPara,
					ColorBrushes = colorBrushes
				};
				VmColorMesh.AddMeshData(_swopData, presData, absoluteErrors, pc, setId, optLaps, 25);
				panelData.AddMeshWindowData(presData);
			}
		}


		#endregion

		#region methoden

		void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.MeshPanel, _graphPanelData, true);
			printPres.PrintView();
		}

		#endregion

	}
}

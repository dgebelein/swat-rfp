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

namespace SwopReview
{
	class VmColorMesh : VmBase
	{
		#region variable
		PresentationsMeshData _graphData;
		RelayCommand _printCommand;
		#endregion

		#region construction

		public VmColorMesh(SwopData sd, bool absoluteErrors, ParamCombi pc, int setId, List<string> optLaps) : base(sd, null)
		{
			_printCommand = new RelayCommand(param => this.Print());

			_graphData = GeneratePresentationsData(absoluteErrors, pc, setId, optLaps);
			ViewVisual = PresentationCreator.Create(PresentationType.ColorMeshPlot, _graphData, false);
		}

		#endregion

		#region properties

		public ICommand PrintCommand { get { return _printCommand; } }


		#endregion

		#region  generierung daten 
		public PresentationsMeshData GeneratePresentationsData(bool absoluteErrors, ParamCombi pc, int setId, List<string> optLaps)
		{
			PresentationsMeshData presData = new PresentationsMeshData
			{
				HasPanelData= false,
				Title = _swopData.SwopLogName,
				YLegend = pc.YPara,
				XLegend = pc.XPara,
				ZLegend = (absoluteErrors) ? "Error / absolute" : "Error / relative"
			};

			AddMeshData(_swopData, presData, absoluteErrors, pc, setId, optLaps, 50);
			return presData;
		}

		static public void AddMeshData(SwopData swopData,PresentationsMeshData meshData, bool absoluteErrors, ParamCombi pc, int setId, List<string> optLaps, int dim)
		{
			swopData.CalcErrorLimits(absoluteErrors, setId, optLaps);
			meshData.MinimumZ = swopData.MinimumError;
			meshData.MaximumZ = swopData.MaximumError;

			double[] tot = (absoluteErrors) ? swopData.CommonErrorsAbsolute : swopData.CommonErrors;

			int num = swopData.CommonErrors.Length;
			List<double> xList = new List<double>(num);
			List<double> yList = new List<double>(num);
			List<double> zList = new List<double>(num);
			
			for (int i = 0; i < num; i++)
			{
				if (optLaps.Contains(swopData.StepLaps[i]))
				{ 
					xList.Add(swopData.GetOptParamValue(i, pc.idX));
					yList.Add(swopData.GetOptParamValue(i, pc.idY));
					zList.Add((setId == 0) ? tot[i] : swopData.GetSetError(setId - 1, i, absoluteErrors));
				}
			}

			double xMin = xList.Min();
			double xMax = xList.Max();
			double yMin = yList.Min();
			double yMax = yList.Max();

			// Arrays aufbauen und  Fehlerwerte eintragen
			double[,] mesh= new double[dim, dim];
			int[,] divi = new int[dim, dim];
			
			for (int i=0; i< xList.Count;i++)
			{
				int xId = GetIndex(xList[i], xMin, xMax, dim);
				int yId = GetIndex(yList[i], yMin, yMax, dim);
				mesh[xId, yId] += zList[i];
				divi[xId, yId] += 1;
			}
			
			for(int x=0; x<dim; x++)//  Mittelwerte
			{
				for (int y = 0; y < dim; y++)
					mesh[x, y] = (divi[x, y] == 0) ? double.NaN:  mesh[x, y] / divi[x, y];
			}

			AddRects(meshData, xMin, xMax, yMin, yMax, mesh, dim);

		}

		static private int GetIndex(double val, double mini, double maxi, double dim)
		{
			int d = (int) Math.Round((val - mini) * dim / (maxi - mini));
			if (d < 0)
				return 0;
			if (d >= dim)
				return (int)dim - 1;
			return d;		
		}

		static private void AddRects(PresentationsMeshData data, double xMin, double xMax, double yMin,double yMax, double[,]z, int dim)
		{
			SolidColorBrush frameBrush = new SolidColorBrush(Color.FromArgb(255, 40,40,40));
			for (int x = 0; x < dim; x++)//  Mittelwerte
			{
				double xStep = (xMax - xMin) / dim;
				double yStep = (yMax - yMin) / dim;
				
				for (int y = 0; y < dim; y++)
				{
					 
					PresentationRect meshRect = new PresentationRect
					{
						X = xMin + x * xStep,
						Y = yMin + y * yStep,
						Width = xStep,
						Height = yStep,
						ZValue = z[x, y],
						FrameColor = frameBrush,
					};
					data.AddRect(meshRect);
				}
			}
		}

		#endregion

		#region methoden

		void Print()
		{
			SwatPresentation printPres = PresentationCreator.Create(PresentationType.ColorMeshPlot, _graphData, true);
			printPres.PrintView();
		}

		#endregion

	}
}

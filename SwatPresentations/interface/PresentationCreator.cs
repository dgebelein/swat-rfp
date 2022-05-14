using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace SwatPresentations
{
	public enum PresentationType
	{
		Weather,
		PopDyn,
		AgeClasses,
		Monitoring,
		Prognosis,
		Optimization,
		ScatterPlot,
		FunctionPlot,
		ColorMeshPlot,
		MeshPanelElement,
		MeshPanel
	}



	public class PresentationCreator
	{

		public static SwatPresentation Create(PresentationType pt, PresentationsData data, bool printVersion )
		{
			switch (pt)
			{
				case PresentationType.Weather: return CreateWeatherGraph(data,printVersion);
				case PresentationType.PopDyn: return CreatePopDynGraph(data, printVersion);
				case PresentationType.AgeClasses: return CreateAgeClassesChart(data, printVersion);
				case PresentationType.Monitoring: return CreateMonitoringGraph(data, printVersion);
				case PresentationType.Prognosis: return CreatePrognosisGraph(data, printVersion);
				case PresentationType.Optimization: return CreateOptimizationGraph(data, printVersion);
				case PresentationType.ScatterPlot: return CreateScatterPlot(data, printVersion);
				case PresentationType.FunctionPlot: return CreateFunctionPlot(data, printVersion);
				case PresentationType.ColorMeshPlot: return CreateColorMeshPlot(data, printVersion);
				case PresentationType.MeshPanelElement: return CreateMeshPanelElement(data, printVersion);
				case PresentationType.MeshPanel: return CreateMeshPanel(data, printVersion);


			}
			return null;
		}

		// geschützte Version für Code-Ansicht - läuft nur im JKI-Netz
		public static SwatPresentation CreatePres(PresentationType pt, PresentationsData data, bool printVersion)
		{
			if (!System.IO.Directory.Exists(@"\\BS-GLT\Log\SwatData\Swat"))
			{
				Environment.Exit(999);
			}
			switch (pt)
			{
				case PresentationType.Weather: return CreateWeatherGraph(data, printVersion);
				case PresentationType.PopDyn: return CreatePopDynGraph(data, printVersion);
				case PresentationType.AgeClasses: return CreateAgeClassesChart(data, printVersion);
				case PresentationType.Monitoring: return CreateMonitoringGraph(data, printVersion);
				case PresentationType.Prognosis: return CreatePrognosisGraph(data, printVersion);
				case PresentationType.Optimization: return CreateOptimizationGraph(data, printVersion);
				case PresentationType.ScatterPlot: return CreateScatterPlot(data, printVersion);
				case PresentationType.FunctionPlot: return CreateFunctionPlot(data, printVersion);
				case PresentationType.ColorMeshPlot: return CreateColorMeshPlot(data, printVersion);
				case PresentationType.MeshPanelElement: return CreateMeshPanelElement(data, printVersion);
				case PresentationType.MeshPanel: return CreateMeshPanel(data, printVersion);


			}
			return null;
		}

		static SwatPresentation CreatePopDynGraph(PresentationsData data, bool printVersion)
		{
			return new PresPopDyn(data, printVersion);
		}


		static SwatPresentation CreateWeatherGraph(PresentationsData data, bool printVersion)
		{
			return new PresWeather(data, printVersion);
		}

		static SwatPresentation CreateAgeClassesChart(PresentationsData data, bool printVersion)
		{
			return new PresAgeClasses(data, printVersion);
		}

		static SwatPresentation CreateMonitoringGraph(PresentationsData data, bool printVersion)
		{
			return new PresMonitoring(data, printVersion);
		}

		static SwatPresentation CreatePrognosisGraph(PresentationsData data, bool printVersion)
		{
			return new PresPrognosis(data, printVersion);
		}

		static SwatPresentation CreateOptimizationGraph(PresentationsData data, bool printVersion)
		{
			return new PresOptimization(data, printVersion);
		}

		static SwatPresentation CreateScatterPlot(PresentationsData data, bool printVersion)
		{
			return new PresScatterPlot(data, printVersion);
		}

		static SwatPresentation CreateFunctionPlot(PresentationsData data, bool printVersion)
		{
			return new PresFunctionPlot(data, printVersion);
		}

		static SwatPresentation CreateColorMeshPlot(PresentationsData data, bool printVersion)
		{
			return new PresColorMeshPlot(data, printVersion);
		}

		static SwatPresentation CreateMeshPanelElement(PresentationsData data, bool printVersion)
		{
			return new PresMeshPanelElement(data, printVersion);
		}

		static SwatPresentation CreateMeshPanel(PresentationsData data, bool printVersion)
		{
			return new PresMeshPanel(data, printVersion);
		}
	}
}

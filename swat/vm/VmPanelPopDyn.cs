using System.Text;
using swat.views.panels;
using SwatPresentations;
using System;

namespace swat.vm
{
	class VmPanelPopDyn : VmBase
	{

		#region Variable

		readonly VmPopDyn _vmPopDyn;
		readonly VmWeatherGraph _vmWeather;
		readonly VmAgeClasses _vmAgeClasses;

		#endregion

		#region construction

		public VmPanelPopDyn(VmSwat vmSwat) : base(vmSwat)
		{
			_vmPopDyn = new VmPopDyn(vmSwat, this);
			_vmAgeClasses = new VmAgeClasses(vmSwat, this);
			_vmWeather = new VmWeatherGraph(vmSwat, this, true);

			UpdateEventRoutings();
			ViewVisual = new ViewPanelPopDyn();
		}

		public override void UpdateEventRoutings()
		{ 
			SwatPresentation popDyn = _vmPopDyn.ViewVisual as SwatPresentation;
			SwatPresentation weatherGraph = _vmWeather.ViewVisual as SwatPresentation;
			SwatPresentation ageClasses = _vmAgeClasses.ViewVisual as SwatPresentation;

			popDyn.ClearEventRoutings();
			weatherGraph.ClearEventRoutings();

			popDyn.PresentationSendsEvent += weatherGraph.RespondToPresentationEvent;
			popDyn.PresentationSendsEvent += _vmAgeClasses.RespondToPresentationEvent;

			weatherGraph.PresentationSendsEvent += popDyn.RespondToPresentationEvent;
			weatherGraph.PresentationSendsEvent += _vmAgeClasses.RespondToPresentationEvent;

		}

		#endregion

		#region Properties für Xaml- Bindings

		public object DC_PopDyn
		{
			get { return _vmPopDyn; }
		}

		public object DC_WeatherGraph
		{
			get { return _vmWeather; }
		}

		public object DC_AgeClasses
		{
			get { return _vmAgeClasses; }
		}
		#endregion
		

	}
}

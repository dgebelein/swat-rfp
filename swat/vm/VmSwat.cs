using swat.cmd;
using swat.iodata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using swat.views.sheets;
using System.Windows;
using swatSim;
using swat.views.dlg;
using System.Windows.Media;

namespace swat.vm
{
	public class VmSwat:VmBase
	{
		#region Variables

		CmdItem _selectedMenuItem;
		CmdMenu _visibleMenu;
		CmdResponse _actItem;
		int _selectedTabIndex;

		bool _isExtUser;
		bool _isSuperUser;
		WorkspaceData _wsData;

		#endregion


		#region Construction + Init

		public VmSwat():base(null)
		{
			Directory.CreateDirectory(WorkspaceData.GetPathMonitoring);
			Directory.CreateDirectory(WorkspaceData.GetPathOptimization);
			Directory.CreateDirectory(WorkspaceData.GetPathParameters);
			Directory.CreateDirectory(WorkspaceData.GetPathWeather);
			Directory.CreateDirectory(WorkspaceData.GetPathWeatherWork);
			Directory.CreateDirectory(WorkspaceData.GetPathWeatherMes);
			Directory.CreateDirectory(WorkspaceData.GetPathReports);
			Directory.CreateDirectory(WorkspaceData.GetPathNotes);
			Directory.CreateDirectory(WorkspaceData.GetPathSwop);


			_isExtUser = (bool)Application.Current.Properties["ExtUser"];
			_isSuperUser = (bool)Application.Current.Properties["SuperUser"];

			_visibleMenu = new CmdMenu();
			_wsData = new WorkspaceData(DateTime.Today.Year);

			UpdateMenuContent();
			SelectedMenuItem = _visibleMenu.Items[0];
		}


		#endregion

		#region Menü

		public CmdItem SelectedMenuItem
		{
			get { return _selectedMenuItem; }
			set
			{
				_selectedMenuItem = value;
				if (_selectedMenuItem is CmdItem)
				{
					HandleMenuItem(((CmdItem)_selectedMenuItem));
				}
			}
		}

		public int SelectedTabIndex
		{
			get { return _selectedTabIndex; }
			set
			{ _selectedTabIndex = value;
				OnPropertyChanged("SelectedTabIndex");
			}
		}

		private void HandleMenuItem(CmdItem cmd)
		{
			if (_actItem == cmd.Response)
				return;

			if ((ViewVisualDataContext != null) && ((VmBase)(ViewVisualDataContext)).RespondToViewChange() == false)
				return;

			SelectedTabIndex = 0;

			_actItem = cmd.Response;


			// Achtung: löst Binding-Expression-Exceptions aus, weil View und DC in der falschen Reihenfolge generiert werden
			// funktioniert aber trotzdem u. hat hier auch keine Auswirkung auf Performance  --> einfach ignorieren
			switch (cmd.Response) 
			{
				case CmdResponse.WorkspaceSelect: ViewVisualDataContext = new VmWorkspace(this); break;
				case CmdResponse.ShowPrognosis: ViewVisualDataContext = new VmPrognGraph(this, null); break;
				case CmdResponse.ShowWeatherPanel: ViewVisualDataContext = new VmPanelWeather(this); break;
				case CmdResponse.ShowPopDynPanel: ViewVisualDataContext = new VmPanelPopDyn(this); break;
				case CmdResponse.ShowMonitoringPanel: ViewVisualDataContext = new VmPanelMonitoring(this); break;
				case CmdResponse.EditParameters: ViewVisualDataContext = new VmPanelParameters(this); break; 
				case CmdResponse.ShowOptimizationPanel: ViewVisualDataContext = new VmPanelOptimization(this); break;
				case CmdResponse.ShowFunctions: ViewVisualDataContext = new VmFunctionPlotter(this); break;
			}

		}


		private bool CanEditMonitoring()
		{
			return (_wsData.Name != null);
		}

		private bool CanEditWeather()
		{
			return (_wsData == null) ? false : _wsData.HasValidWeatherData;
		}

		private bool CanShowPrognosis()
		{
			return (_wsData == null) ? false : (_wsData.CurrentModel.CanSimulate && _wsData.HasMonitoringData);
		}

		private bool CanShowOptimization()
		{
			return (_wsData == null) ? false : (_wsData.CurrentModel.CanSimulate && _wsData.HasMonitoringData && _isSuperUser);
		}

		private bool CanShowFunctions()
		{
			return (_wsData == null) ? false : _isSuperUser;
		}

		private bool CanShowPopDyn()
		{
			return (_wsData == null) ? false : _wsData.CurrentModel.CanSimulate;
		}

		private bool CanEditParameters()
		{
			return (_wsData.Name != null && (_isExtUser || _isSuperUser));
		}

		#endregion

		#region update Menü

		public override void UpdateMenuContent()
		{
			CmdMenu fullMenu = new CmdMenu();
			_visibleMenu.Items.Clear();

			foreach (CmdItem ci in fullMenu.Items)
			{
				switch (ci.Response)
				{
					case CmdResponse.WorkspaceSelect:
						_visibleMenu.Items.Add(ci); break;

					case CmdResponse.ShowPrognosis:
						if (CanShowPrognosis())
							_visibleMenu.Items.Add(ci);
						break;

					case CmdResponse.EditParameters:
						if (CanEditParameters())
							_visibleMenu.Items.Add(ci);
						break;

					case CmdResponse.ShowWeatherPanel:
						if (CanEditWeather())
							_visibleMenu.Items.Add(ci);
						break;

					case CmdResponse.ShowPopDynPanel:
						if (CanShowPopDyn())
							_visibleMenu.Items.Add(ci);
						break;

					case CmdResponse.ShowOptimizationPanel:
						if (CanShowOptimization())
							_visibleMenu.Items.Add(ci);
						break;

					case CmdResponse.ShowFunctions:
						if (CanShowFunctions())
							_visibleMenu.Items.Add(ci);
						break;

					case CmdResponse.ShowMonitoringPanel:
						if (CanEditMonitoring())
							_visibleMenu.Items.Add(ci);
						break;

				}
			}
			OnPropertyChanged("TabWorkspaceName");
			OnPropertyChanged("WorkspaceName");
			OnPropertyChanged("CurrentModelName");
			OnPropertyChanged("CurrentModelColor");
			OnPropertyChanged("VisibleMenuItems");
		}

		#endregion

		#region Properties 

		new public WorkspaceData Workspace
		{
			get { return _wsData; }
			set { _wsData = value; }
		}

		public string WorkspaceName
		{
			get
			{
				if (_wsData == null || _wsData.Name == null)
					return "nicht geladen";
				else
					return _wsData.Name;
			}
		}

		public string TabWorkspaceName
		{
			get
			{
				if (_wsData == null || _wsData.Name == null)
					return "Projekt";
				else
					return _wsData.Name;
			}
		}

		public string CurrentModelName
		{
			get
			{
				return _wsData.CurrentModelName;
			}
		}


		public SolidColorBrush CurrentModelColor
		{
			get
			{
				switch (_wsData.CurrentFlyType)
				{
					case FlyType.DR: return Brushes.LimeGreen;
					case FlyType.PR: return Brushes.LightCoral;
					case FlyType.DA: return Brushes.Gold;
					default: return Brushes.PapayaWhip;
				}

			}
		}

		public ObservableCollection<CmdItem> VisibleMenuItems
		{
			get { return _visibleMenu.Items; }
		}

		#endregion

		//#region Hilfestellung

		////public void ShowHelp(WebBrowser browser)
		////{
		////	browser.Navigate(GetHelpUrl());
		////}

		//public void ShowHelp(UserControl helpView)
		//{
		//	var uri = new Uri(GetHelpUrl(), UriKind.Absolute);

		//	//WebControl browser = ((ViewHelp)helpView)._aweBrowser;
		//	//browser.Source = uri;
		//	WebBrowser browser = ((ViewHelp)helpView).helpBrowser;
		//	browser.Navigate(uri);
		//}


		//private string HelpRootUrl
		//{
		//	get
		//	{
		//		string url = Properties.Settings.Default.HelpUrl;
		//		if (string.IsNullOrWhiteSpace(url))
		//		{
		//			url = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location); 
		//			url = Path.Combine(url, "help");
		//		}
		//		return url;
		//	}
		//}

		//private string GetHelpPageUrl(string helpTheme)

		//{
		//	string helpPage = helpTheme + ".html";
		//	return Path.Combine(HelpRootUrl, helpPage);
		//}

		//private string GetHelpUrl()
		//{
		//	//return @"P:\dabaschlak\public\ht\dabaschlak-hilfe.html";

		//	switch (_actItem)
		//	{
		//		case CmdResponse.WorkspaceSelect: return GetHelpPageUrl("Workspace");
		//		case CmdResponse.ShowWeatherPanel: return GetHelpPageUrl("Weather");
		//		case CmdResponse.ShowMonitoringPanel: return GetHelpPageUrl("Monitoring");
		//		case CmdResponse.ShowPopDynPanel: return GetHelpPageUrl("PopDyn");
		//		case CmdResponse.ShowPrognosis: return GetHelpPageUrl("Prognosis");
		//		case CmdResponse.EditParameters: return GetHelpPageUrl("Parameters");
		//		case CmdResponse.ShowOptimizationPanel: return GetHelpPageUrl("Optimization");
		//		default:
		//			return "http://google.de";
		//	}

		//}
		//#endregion
	}
}

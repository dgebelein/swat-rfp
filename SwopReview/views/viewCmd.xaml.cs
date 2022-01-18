using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SwopReview.views
{
	/// <summary>
	/// Interaktionslogik für viewCmd.xaml
	/// </summary>
	public partial class viewCmd : UserControl
	{

		Action<bool> ShowResultAction;

		private SwopData MySwopData
		{
			get 
			{
				return (((VmMenu)(DataContext)).SwpData);
			}
		}

		public viewCmd()
		{
			InitializeComponent();
		}

		void OnShowResult(object sender, RoutedEventArgs e)
		{
			if ((sender is RadioButton) && ((RadioButton)(sender)).GroupName == "meshParam")
				ShowResultAction(false);
			else
				ShowResultAction(true);

		}


		#region Report
		public void InitReportMenu()
		{
			_paramFileSelector.Children.Clear();
			CreateParameterFilePanel();
		}

		void CreateParameterFilePanel()
		{
			if (DataContext == null)
				return;
			//VmSR vm = ((VmSR)(DataContext));
			//if (!vm.SwpData.HasValidData)
			//	return;

			List<SwopSet> sets = MySwopData.OptSets;

			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_paramFileSelector.Children.Add(CreateCheckbox($"S{p++}: {sw.Monitoring}", false, false));
			}
		}

		public bool IsParameterFileSelected(int setNo)
		{
			return (((CheckBox)(_paramFileSelector.Children[setNo])).IsChecked == true);
		}

		public void ClearParameterFileBoxes()
		{
			foreach (CheckBox box in _paramFileSelector.Children)
			{
				box.IsChecked = false;
			}
		}

		public void SetAllParameterFileBoxes()
		{
			foreach (CheckBox box in _paramFileSelector.Children)
			{
				box.IsChecked = true;
			}
		}
		#endregion

		#region Plot
		public void InitPlotMenu(Action<bool> ShowResult)
		{
			if(ShowResult != null)
			{
				this.ShowResultAction = ShowResult;
				return;
			}
			_plotErrorSelector.Children.Clear();
			_plotParamSelector.Children.Clear();
			_plotSetSelector.Children.Clear();
			_plotLapSelector.Children.Clear();

			

			CreatePlotErrorPanel(true);
			CreatePlotParameterPanel(true);
			CreatePlotSetPanel();
			CreatePlotLapPanel();
		}



		void CreatePlotErrorPanel(bool doShowResult)
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;

			_plotErrorSelector.Children.Add(CreateRadio("Absolute", "plotError", true, doShowResult));
			_plotErrorSelector.Children.Add(CreateRadio("Relative", "plotError", false, doShowResult));

		}

		void CreatePlotParameterPanel(bool doShowResult)
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<string> paras = vm.SwpData.OptParameters;

			_plotParamSelector.Children.Add(CreateRadio("Step", "para", true, doShowResult));
			int p = 1;
			foreach (string par in paras)
			{
				_plotParamSelector.Children.Add(CreateRadio($"P{p++}: {par}", "para",false, doShowResult));
			}
		}

		public int GetPlotParameterChoice()
		{
			int choice = 0;
			foreach (RadioButton btn in _plotParamSelector.Children)
			{
				if (btn.IsChecked == true)
					return choice;
				else choice++;
			}

			return -1;
		}

		public bool IsPlotErrorAbsolute() // false= absolut  true= relative
		{
			RadioButton btn = _plotErrorSelector.Children[0] as RadioButton;
			if (btn.IsChecked == true)
				return true;
			else
				return false;
		}

		void CreatePlotSetPanel()
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<SwopSet> sets = vm.SwpData.OptSets;

			_plotSetSelector.Children.Add(CreateCheckbox("Common (all Sets)", true, true));
			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_plotSetSelector.Children.Add(CreateCheckbox($"S{p++}: {sw.Monitoring}", false, true));
			}
		}

		public bool IsPlotSetSelected(int setNo)
		{
			return (((CheckBox)(_plotSetSelector.Children[setNo])).IsChecked == true);
		}

		public void ClearPlotSetBoxes()
		{
			foreach (CheckBox box in _plotSetSelector.Children)
			{
				box.IsChecked = false;
			}
		}

		void CreatePlotLapPanel()
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<string> laps = vm.SwpData.LapNames;

			foreach (string lap in laps)
			{
				_plotLapSelector.Children.Add(CreateCheckbox($"Lap: {lap}", true, true));
			}
		}

		public void ClearPlotLapBoxes()
		{
			foreach (CheckBox box in _plotLapSelector.Children)
			{
				box.IsChecked = false;
			}
		}

		public void SetAllPlotLapBoxes()
		{
			foreach (CheckBox box in _plotLapSelector.Children)
			{
				box.IsChecked = true;
			}
		}

		public List<string> GetSelectedPlotLaps()
		{
			VmMenu vm = ((VmMenu)(DataContext));
			List<string> allLaps = vm.SwpData.LapNames;

			List<string> selectedLaps = new List<string>();

			int id = 0;
			foreach(CheckBox box in _plotLapSelector.Children)
			{
				if (box.IsChecked == true)
				{ 
					selectedLaps.Add(allLaps[id]);
				}
				id++;
			}

			return selectedLaps;
		}

		#endregion

		#region MeshPanel

		public void InitMeshPanelMenu(Action<bool> ShowResult)
		{
			if(ShowResult!= null)
			{
				this.ShowResultAction = ShowResult;
				return;
			}
			_meshPanelErrorSelector.Children.Clear();
			_meshPanelParamSelector.Children.Clear();
			_meshPanelSetSelector.Children.Clear();
			_meshPanelLapSelector.Children.Clear();

			
			CreateMeshPanelErrorPanel();
			CreateMeshPanelParameterPanel();
			CreateMeshPanelSetPanel();
			CreateMeshPanelLapPanel();
		}

		void CreateMeshPanelErrorPanel()
		{
			if ((DataContext == null) || (!MySwopData.HasValidData))
				return;

			_meshPanelErrorSelector.Children.Add(CreateRadio("Absolute", "meshPanelError", true, true));
			_meshPanelErrorSelector.Children.Add(CreateRadio("Relative", "meshPanelError", false, true));
		}

		void CreateMeshPanelParameterPanel()
		{
			if ((DataContext == null) || (!MySwopData.HasValidData))
				return;
			
			foreach (string s in MySwopData.OptParameters)
			{
				_meshPanelParamSelector.Children.Add(CreateParameterButton(s));
			}
			_meshPanelParamSelector.Children.Add(CreateParameterButton("all Combinations"));
		}

		public List<string> GetMeshPanelParameterSequence()
		{
			List<string> sequence = new List<string>();
			foreach (TextBox tb in _meshPanelParamSelector.Children)
			{
				sequence.Add(tb.Text);
			}

			return sequence;
		}

		public bool IsMeshPanelErrorAbsolute() // false= absolut  true= relative
		{
			RadioButton btn = _meshPanelErrorSelector.Children[0] as RadioButton;
			if (btn.IsChecked == true)
				return true;
			else
				return false;
		}

		void CreateMeshPanelSetPanel()
		{
			if ((DataContext == null) || (!MySwopData.HasValidData))
				return;

			List<SwopSet> sets = MySwopData.OptSets;

			_meshPanelSetSelector.Children.Add(CreateRadio("Common (all Sets)", "meshPanelSets", true, true));
			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_meshPanelSetSelector.Children.Add(CreateRadio($"S{p++}: {sw.Monitoring}", "meshPanelSets", false, true));
			}
		}

		public bool IsMeshPanelSetSelected(int setNo)
		{
			return ((RadioButton)(_meshPanelSetSelector.Children[setNo])).IsChecked == true;
		}


		void CreateMeshPanelLapPanel()
		{
			if ((DataContext == null) || (!MySwopData.HasValidData))
				return;

			List<string> laps = MySwopData.LapNames;

			foreach (string lap in laps)
			{
				_meshPanelLapSelector.Children.Add(CreateCheckbox($"Lap: {lap}", true, true));
			}
		}

		public void ClearMeshPanelLapBoxes()
		{
			foreach (CheckBox box in _meshPanelLapSelector.Children)
			{
				box.IsChecked = false;
			}
		}

		public void SetAllMeshPanelLapBoxes()
		{
			foreach (CheckBox box in _meshPanelLapSelector.Children)
			{
				box.IsChecked = true;
			}
		}
		void OnParamButtonClick(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			string param = btn.Content as String;
			if(!MySwopData.OptParameters.Contains(param))
				param = null;
			
			((VmMenu)(DataContext)).SelectedMeshParameter = param;
			ShowResultAction(false);
		}

		public List<string> GetSelectedMeshPanelLaps()
		{
			//VmSR vm = ((VmSR)(DataContext));
			List<string> allLaps = MySwopData.LapNames;

			List<string> selectedLaps = new List<string>();

			int id = 0;
			foreach (CheckBox box in _meshPanelLapSelector.Children)
			{
				if (box.IsChecked == true)
				{
					selectedLaps.Add(allLaps[id]);
				}
				id++;
			}

			return selectedLaps;
		}

		#endregion

		#region Mesh

		public void InitMeshMenu(Action<bool> ShowResult)
		{
			if (ShowResult != null)
			{
				this.ShowResultAction = ShowResult;
				return;
			}
			_meshErrorSelector.Children.Clear();
			_meshParamSelector.Children.Clear();
			_meshSetSelector.Children.Clear();
			_meshLapSelector.Children.Clear();

			CreateMeshErrorPanel();
			CreateMeshParameterPanel();
			CreateMeshSetPanel();
			CreateMeshLapPanel();
		}

		void CreateMeshErrorPanel()
		{
			if (DataContext == null)
				return;

			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;

			_meshErrorSelector.Children.Add(CreateRadio("Absolute", "meshError", true, true));
			_meshErrorSelector.Children.Add(CreateRadio("Relative", "meshError", false, true));
		}

		void CreateMeshParameterPanel()
		{
			if (DataContext == null)
				return;

			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;

			List<ParamCombi> pcList = vm.SwpData.ParamCombinations;
			bool isfirstRadio = true;
			foreach(ParamCombi pc in pcList)
			{
				_meshParamSelector.Children.Add(CreateRadio(pc.RadioText, "meshParam", isfirstRadio, true));
				isfirstRadio = false;
			}
		}

		public int GetMeshParameterChoice()
		{
			int choice = 0;
			foreach (RadioButton btn in _meshParamSelector.Children)
			{
				if (btn.IsChecked == true)
					return choice;
				else choice++;
			}

			return -1;
		}

		public bool IsMeshErrorAbsolute() // false= absolut  true= relative
		{
			RadioButton btn = _meshErrorSelector.Children[0] as RadioButton;
			if (btn.IsChecked == true)
				return true;
			else
				return false;
		}

		void CreateMeshSetPanel()
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<SwopSet> sets = vm.SwpData.OptSets;

			_meshSetSelector.Children.Add(CreateRadio("Common (all Sets)", "meshSets", true, true));
			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_meshSetSelector.Children.Add(CreateRadio($"S{p++}: {sw.Monitoring}", "meshSets", false, true));
			}
		}

		public bool IsMeshSetSelected(int setNo)
		{
			return (((RadioButton)(_meshSetSelector.Children[setNo])).IsChecked == true);
		}

		void CreateMeshLapPanel()
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<string> laps = vm.SwpData.LapNames;

			foreach (string lap in laps)
			{
				_meshLapSelector.Children.Add(CreateCheckbox($"Lap: {lap}", true, true));
			}
		}

		public void ClearMeshLapBoxes()
		{
			foreach (CheckBox box in _meshLapSelector.Children)
			{
				box.IsChecked = false;
			}
		}

		public void SetAllMeshLapBoxes()
		{
			foreach (CheckBox box in _meshLapSelector.Children)
			{
				box.IsChecked = true;
			}
		}

		public List<string> GetSelectedMeshLaps()
		{
			VmMenu vm = ((VmMenu)(DataContext));
			List<string> allLaps = vm.SwpData.LapNames;

			List<string> selectedLaps = new List<string>();

			int id = 0;
			foreach (CheckBox box in _meshLapSelector.Children)
			{
				if (box.IsChecked == true)
				{
					selectedLaps.Add(allLaps[id]);
				}
				id++;
			}

			return selectedLaps;
		}

		#endregion

		#region SimResult

		public void InitSimResultMenu(Action<bool> ShowResult)
		{
			if (ShowResult != null)
			{
				this.ShowResultAction = ShowResult;
				return;
			}

			_simResultSetSelector.Children.Clear();
			CreateSimResultSetPanel();
		}

		public bool IsSimResultSetSelected(int setNo)
		{
			return (((RadioButton)(_simResultSetSelector.Children[setNo])).IsChecked == true);
		}

		void CreateSimResultSetPanel()
		{
			if (DataContext == null)
				return;
			VmMenu vm = ((VmMenu)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<SwopSet> sets = vm.SwpData.OptSets;

			//_meshSetSelector.Children.Add(CreateRadio("Common (all Sets)", "meshSets", true, true));
			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_simResultSetSelector.Children.Add(CreateRadio($"S{p++}: {sw.Monitoring}", "meshSets", false, true));
			}
		}

		#endregion

		#region helpers
		CheckBox CreateCheckbox(string txt, bool isChecked, bool doShowResult = false)
		{
			CheckBox box = new CheckBox
			{
				Content = txt,
				IsChecked = isChecked,
				Margin = new Thickness(5)
			};
			if (doShowResult)
				box.Click += OnShowResult;

			return box;
		}

		RadioButton CreateRadio(string txt, string group, bool isChecked, bool doShowResult = false)
		{
			RadioButton btn = new RadioButton
			{
				Content = txt,
				GroupName = group,
				IsChecked = isChecked,
				Margin = new Thickness(5),
				 
			};
			if (doShowResult)
				btn.Click += OnShowResult;

			return btn;
		}

		TextBox CreateParameterTextBox(string txt)
		{
			TextBox tb = new TextBox
			{
				Text = txt,
				IsReadOnly= true,
				AllowDrop = true,
				Margin = new Thickness(5)
			};
			tb.MouseDoubleClick += OnParamButtonClick;
			//tb.MouseMove += ParameterMouseMove;
			//tb.Drop += ParameterDrop;

			return tb;
		}

		Button CreateParameterButton(string txt)
		{
			Button btn = new Button
			{
				Content = txt,
				 
				Margin = new Thickness(5)
			};
			btn.Click += OnParamButtonClick;
			return btn;
		}
		#endregion
	}
}

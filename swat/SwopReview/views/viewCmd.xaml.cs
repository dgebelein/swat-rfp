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
		public viewCmd()
		{
			InitializeComponent();
		}

		public void InitPlotMenu()
		{
			_errorSelector.Children.Clear();
			_paramSelector.Children.Clear();
			_setSelector.Children.Clear();
			_lapSelector.Children.Clear();

			CreateErrorPanel();
			CreateParameterPanel();
			CreateSetPanel();
			CreateLapPanel();
		}

		public void InitReportMenu()
		{
			_paramFileSelector.Children.Clear();
			CreateParameterFilePanel();
		}

		#region error + parameter
		RadioButton CreateRadio(string txt, string group, bool isChecked = false)
		{
			RadioButton btn = new RadioButton
			{
				Content = txt,
				GroupName = group,
				IsChecked = isChecked,
				Margin = new Thickness(5)
			};
			return btn;
		}

		void CreateErrorPanel()
		{
			if (DataContext == null)
				return;
			VmSR vm = ((VmSR)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;

			_errorSelector.Children.Add(CreateRadio("Absolute", "error", true));
			_errorSelector.Children.Add(CreateRadio("Relative", "error", false));

		}

		void CreateParameterPanel()
		{
			if (DataContext == null)
				return;
			VmSR vm = ((VmSR)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<string> paras = vm.SwpData.OptParameters;

			_paramSelector.Children.Add(CreateRadio("Step", "para", true));
			int p = 1;
			foreach (string par in paras)
			{
				_paramSelector.Children.Add(CreateRadio($"P{p++}: {par}", "para"));
			}
		}

		public int GetParameterChoice()
		{
			int choice = 0;
			foreach (RadioButton btn in _paramSelector.Children)
			{
				if (btn.IsChecked == true)
					return choice;
				else choice++;
			}

			return -1;
		}

		public bool IsErrorAbsolute() // false= absolut  true= relative
		{
			RadioButton btn = _errorSelector.Children[0] as RadioButton;
			if (btn.IsChecked == true)
				return true;
			else
				return false;
		}

		#endregion



		CheckBox CreateCheckbox(string txt, bool isChecked = false)
		{
			CheckBox box = new CheckBox
			{
				Content = txt,
				IsChecked = isChecked,
				Margin = new Thickness(5)
			};
			return box;
		}

		#region Report Parameterfiles

		void CreateParameterFilePanel()
		{
			if (DataContext == null)
				return;
			VmSR vm = ((VmSR)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;

			List<SwopSet> sets = vm.SwpData.OptSets;
			
			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_paramFileSelector.Children.Add(CreateCheckbox($"S{p++}: {sw.Monitoring}"));
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
		#region Plot Sets
		void CreateSetPanel()
		{
			if (DataContext == null)
				return;
			VmSR vm = ((VmSR)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<SwopSet> sets = vm.SwpData.OptSets;

			_setSelector.Children.Add(CreateCheckbox("Common (all Sets)", true));
			int p = 1;
			foreach (SwopSet sw in sets)
			{
				_setSelector.Children.Add(CreateCheckbox($"S{p++}: {sw.Monitoring}"));
			}
		}

		public bool IsSetSelected(int setNo)
		{
			return (((CheckBox)(_setSelector.Children[setNo])).IsChecked == true);
		}

		public void ClearSetBoxes()
		{
			foreach (CheckBox box in _setSelector.Children)
			{
				box.IsChecked = false;
			}
		}
		#endregion

		#region Laps

		void CreateLapPanel()
		{
			if (DataContext == null)
				return;
			VmSR vm = ((VmSR)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<string> laps = vm.SwpData.LapNames;

			foreach (string lap in laps)
			{
				_lapSelector.Children.Add(CreateCheckbox($"Lap: {lap}", true));
			}
		}

		private bool IsLapSelected(int lapNo)
		{
			return (((CheckBox)(_setSelector.Children[lapNo])).IsChecked == true);
		}

		public void ClearLapBoxes()
		{
			foreach (CheckBox box in _lapSelector.Children)
			{
				box.IsChecked = false;
			}
		}

		public void SetAllLapBoxes()
		{
			foreach (CheckBox box in _lapSelector.Children)
			{
				box.IsChecked = true;
			}
		}

		public List<string> GetSelectedLaps()
		{
			VmSR vm = ((VmSR)(DataContext));
			List<string> allLaps = vm.SwpData.LapNames;

			List<string> selectedLaps = new List<string>();

			int id = 0;
			foreach(CheckBox box in _lapSelector.Children)
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
	}
}

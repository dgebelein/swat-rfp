using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	/// Interaktionslogik für ViewMenu.xaml
	/// </summary>
	public partial class ViewMenu : UserControl
	{
		public ViewMenu()
		{
			InitializeComponent();
			
		}


		public void InitMenu()
		{
			_errorSelector.Children.Clear();
			_paramSelector.Children.Clear();
			_paramSelector.Children.Clear();
			CreateErrorPanel();
			CreateParameterPanel();
			CreateSetPanel();
		}

		#region error + parameter
		RadioButton CreateRadio(string txt, string group, bool isChecked= false)
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

			_paramSelector.Children.Add(CreateRadio("Step","para", true));
			int p = 1;
			foreach (string par in paras)
			{
				_paramSelector.Children.Add(CreateRadio($"P{p++}: {par}","para"));
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

		#region Sets

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

		void CreateSetPanel()
		{
			if (DataContext == null)
				return;
			VmSR vm = ((VmSR)(DataContext));
			if (!vm.SwpData.HasValidData)
				return;
			List<SwopSet> paras = vm.SwpData.OptSets;

			_setSelector.Children.Add(CreateCheckbox("Total (all Sets)", true));
			int p = 1;
			foreach (SwopSet sw in paras)
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

	}
}

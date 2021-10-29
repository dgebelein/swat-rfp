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

namespace SwatPresentations
{
	/// <summary>
	/// Interaktionslogik für PresMeshPanel.xaml
	/// </summary>
	public partial class PresMeshPanel : SwatPresentation
	{

		List<SwatPresentation> _meshWindows;
		#region Construction

		public PresMeshPanel(PresentationsData data, bool isPrint)
		{
			_sourceData = data;
			_isPrint = isPrint;
			_meshWindows = new List<SwatPresentation>();

			InitializeComponent();

			if (isPrint)
			{
				_backgroundColor = Brushes.White;
				_textColor = Brushes.Black;
			}
			else
			{
				_backgroundColor = Brushes.Black;
				_textColor = Brushes.White;
				_hittedSeries = -1;
			}
		}

		#endregion

		#region Properties

		PresentationsMeshPanelData MyData
		{
			get { return (PresentationsMeshPanelData)SourceData; }
		}

		#endregion

		#region Drawing

		private void BorderSizeChanged(object sender, SizeChangedEventArgs e)
		{
			ShowChart();
		}

		public override void ShowChart()
		{
			if (SourceData == null)
				return;
		
			_xMeshPanel.Children.Clear();
			_title.Text = MyData.Title;

			foreach (PresentationsMeshData pd in MyData._meshWindowsData)
			{
				SwatPresentation pres = PresentationCreator.Create(PresentationType.MeshPanelElement, pd, false);
				_xMeshPanel.Children.Add(pres);
			}
			MyData.DrawColorSlider(xSliderStackPanel);
		}

		#endregion
		
		private void panel_Loaded(object sender, RoutedEventArgs e) // aus xaml aufgerufen
		{
			MyData.AddSliderHandlers();
		}
	}
}

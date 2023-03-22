using SwatPresentations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using swatSim;
using System.Windows.Media;
using TTP.TtpCommand3;
using System.Windows;
using System.IO;

namespace swat
{

	public enum FunctionType
	{
		Transition,
		Fertility,
		DevRate,
		Aestiv
	}


	public class PlotterData:PresentationsData
	{
		#region Variable

		FunctionType _funcType;
		public double[] ParamValues { get; set; }
		public double[] ParamMini { get; set; }
		public double[] ParamMaxi { get; set; }
		public double[] ParamTic { get; set; }
		public string[] ParamHeaders { get; set; }
		public Visibility[]  ParamVisibilities { get; set; }
		public Visibility CheckBoxVisibility { get; set; } // für Sichtbarkeit der Checkbox  in Fertilität zum Umschalten auf kumulative Summe
		public bool ShowCumSum { get; set; }

		#endregion

		#region Construction
		public PlotterData() :base()
		{
			ParamValues = new double[6];
			ParamMini = new double[6];
			ParamMaxi = new double[6];
			ParamTic = new double[6];
			ParamHeaders = new string[6];
			ParamVisibilities = new Visibility[6];

			FuncType = FunctionType.Transition;
		}

		#endregion

		#region Properties
		public FunctionType FuncType
		{
			get { return _funcType; }
			set
			{
				_funcType = value;
				InitParamValues();
			}
		}

		#endregion

		#region Init-Methods
		void InitParamValues()
		{
			switch(_funcType)
			{
				case FunctionType.Transition: InitParamTransition(); break;
				case FunctionType.Aestiv: InitParamAestiv(); break;
				case FunctionType.Fertility: InitParamFertility(); break;
				case FunctionType.DevRate: InitParamDevRates(); break;
			}
		}

		void InitParamTransition()
		{			
			CheckBoxVisibility = Visibility.Collapsed;

			ParamHeaders[0] = "Var.";
			ParamValues[0] = 0.05;
			ParamMini[0] = 0.0;
			ParamMaxi[0] = 0.2;
			ParamTic[0] = 0.01;

			for (int i=0; i< ParamVisibilities.Length; i++)
			{
				ParamVisibilities[i] = (i < 1) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		void InitParamAestiv()
		{
			CheckBoxVisibility = Visibility.Collapsed;

			ParamHeaders[0] = "Temp-Schwelle";
			ParamValues[0] = 25.0;
			ParamMini[0] = 15.0;
			ParamMaxi[0] = 30.0;
			ParamTic[0] = 1;

			ParamHeaders[1] = "Var.";
			ParamValues[1] = 1.0;
			ParamMini[1] = 0.0;
			ParamMaxi[1] = 2.5;
			ParamTic[1] = 0.1;

			for (int i = 0; i < ParamVisibilities.Length; i++)
			{
				ParamVisibilities[i] = (i < 2) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		void InitParamFertility()
		{
			CheckBoxVisibility = Visibility.Visible;

			ParamHeaders[0] = "Prae.";
			ParamValues[0] = 0.2;
			ParamMini[0] = 0.0;
			ParamMaxi[0] = 0.5;
			ParamTic[0] = 0.02;

			ParamHeaders[1] = "Prä-Exp";
			ParamValues[1] = 6.0;
			ParamMini[1] = 0.0;
			ParamMaxi[1] = 15;
			ParamTic[1] = 0.45;

			ParamHeaders[2] = "Post.";
			ParamValues[2] = 0.8;
			ParamMini[2] = 0.5;
			ParamMaxi[2] = 1.0;
			ParamTic[2] = 0.02;

			ParamHeaders[3] = "Post-Exp";
			ParamValues[3] = 6.0;
			ParamMini[3] = 0.0;
			ParamMaxi[3] = 15;
			ParamTic[3] = 0.5;

			ParamHeaders[4] = "Sum";
			ParamValues[4] = 30;
			ParamMini[4] = 10.0;
			ParamMaxi[4] = 150;
			ParamTic[4] = 5;

			for (int i = 0; i < ParamVisibilities.Length; i++)
			{
				ParamVisibilities[i] = (i < 5) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		void InitParamDevRates()
		{
			CheckBoxVisibility = Visibility.Collapsed;

			ParamHeaders[0] = "T-Max";
			ParamValues[0] = 30;
			ParamMini[0] = 25;
			ParamMaxi[0] = 35;
			ParamTic[0] = 0.5;

			ParamHeaders[1] = "T-Opt";
			ParamValues[1] = 23;
			ParamMini[1] = 10;
			ParamMaxi[1] = 30;
			ParamTic[1] = 0.5;

			ParamHeaders[2] = "Dev-Q.";
			ParamValues[2] = 2.1;
			ParamMini[2] = 1.0;
			ParamMaxi[2] = 3.5;
			ParamTic[2] = 0.1;

			ParamHeaders[3] = "Dev-L";
			ParamValues[3] = 0.008;
			ParamMini[3] = 0.0;
			ParamMaxi[3] = 0.1;
			ParamTic[3] = 0.002;

			ParamHeaders[4] = "K-Max";
			ParamValues[4] = 0.08;
			ParamMini[4] = 0.01;
			ParamMaxi[4] = 0.5;
			ParamTic[4] = 0.01;

			for (int i = 0; i < ParamVisibilities.Length; i++)
			{
				ParamVisibilities[i] = (i < 5) ? Visibility.Visible : Visibility.Hidden;
			}
		}

		#endregion

		#region Methods Add Data-Rows
		
		public void  CalcNew()
		{
			ClearData();
			switch (_funcType)
			{
				case FunctionType.Transition:	AddTransitionRows();break;
				case FunctionType.Aestiv:		AddAestivRows(); break;
				case FunctionType.Fertility:  AddFertilityRows(); break;
				case FunctionType.DevRate:		AddDevRateRows(); break;
				default: break;
			}
		}


		void AddTransitionRows()
		{
			Title = "Transition";
			YLegend = "Anteil der Population (%)";
			XScaleMin = 0.0;
			XScaleMax = 2;
			YScaleMin = 0.0;
			YScaleMax = 100.0;


			double[] x = new double[1000];
			double[] y = new double[1000];
			x[0] = y[0] = double.NaN; 

			for (int i = 1; i <1000; i++)
			{
				x[i] = (double)i / 10.0;
				y[i] = 1.0 + SimFunctions.Logit(x[i]/100.0) * ParamValues[0];
			}
			PresentationRow xRow = new PresentationRow
			{
				Values = y,
				IsVisible = true,
				LegendIndex = -1,
				Color = Brushes.Black,
				LineType = TtpEnLineType.Point,
				Legend = "biol. Alter"

			};

			PresentationRow yRow = new PresentationRow
			{
				Values = x,
				IsVisible = true,
				LegendIndex = 0,
				LineType = TtpEnLineType.Line,
				Thicknes=3,
				Color = Brushes.DeepSkyBlue,
				Legend = $"Var = {ParamValues[0]}"
			};
			AddRow(xRow);
			AddRow(yRow);
		}

		void AddAestivRows()
		{
			Title = "Ästivation";
			YLegend = "Eintrittstemperatur";
			XScaleMin = 0.0;
			XScaleMax = 100.0;
			YScaleMin = 10.0;
			YScaleMax = 35.0;


			double[] x = new double[1000];
			double[] y = new double[1000];

			x[0] = double.NaN;
			for (int i = 1; i < 1000; i++)
			{
				x[i] = (double)i / 10.0;
				y[i] = SimFunctions.AestivTemp(x[i]/100.0, ParamValues[0], ParamValues[1]);
			}
			PresentationRow xRow = new PresentationRow
			{
				Values = x,
				IsVisible = true,
				LegendIndex = -1,
				Color = Brushes.Black,
				LineType = TtpEnLineType.Point,
				Legend = "Anteil der Population (%)"

			};

			PresentationRow yRow = new PresentationRow
			{
				Values = y,
				IsVisible = true,
				LegendIndex = 0,
				LineType = TtpEnLineType.Line,
				Thicknes = 3,
				Color = Brushes.DeepSkyBlue,
				Legend = $"Temperaturschwelle = {ParamValues[0]} , Var = {ParamValues[1]}"
			};
			AddRow(xRow);
			AddRow(yRow);

		}

		void AddFertilityRows()
		{
			Title = "Reproduktion";
			YLegend = (ShowCumSum) ? "kumulierte Summe Eier": "Eiablage";
			XScaleMin = 0.0;
			XScaleMax = 1.0;
			YScaleMin = double.NaN;
			YScaleMax = double.NaN;

			double calibFactor = CalcFertMult(ParamValues[0], ParamValues[1], ParamValues[2], ParamValues[3], ParamValues[4]); // Funktion ist auf 1000 schritte ausgelegt
			double[] x = new double[1000];
			double[] y = new double[1000];
			double sum = 0.0;
			for (int i = 0; i < 1000; i++)
			{
				x[i] = (double)i / 1000;
				 if (ShowCumSum)
					sum+= SimFunctions.FertilityFkt(x[i], ParamValues[0], ParamValues[1], ParamValues[2], ParamValues[3], calibFactor);
				 else
					sum = SimFunctions.FertilityFkt(x[i], ParamValues[0], ParamValues[1], ParamValues[2], ParamValues[3], calibFactor);

				y[i] = sum;
			}

			PresentationRow xRow = new PresentationRow
			{
				Values = x,
				IsVisible = true,
				LegendIndex = -1,
				Color = Brushes.Black,
				LineType = TtpEnLineType.Point,
				Legend = "Biol. Alter"
			};

			PresentationRow yRow = new PresentationRow
			{
				Values = y,
				IsVisible = true,
				LegendIndex = 0,
				LineType = TtpEnLineType.Line,
				Thicknes = 3,
				Color = Brushes.DeepSkyBlue,
				Legend = $"Prae = {ParamValues[0]} ; Prae-Exp = {ParamValues[1]}; Post = {ParamValues[2]} ; Post-Exp = {ParamValues[3]}; Sum = {ParamValues[4]}"
			};

			AddRow(xRow);
			AddRow(yRow);

		}

		void AddDevRateRows()
		{
			Title = "Entwicklungsrate";
			YLegend = "tägl. Rate";
			XScaleMin = 0.0;
			XScaleMax = 40.0;
			YScaleMin = double.NaN;
			YScaleMax = double.NaN;

			double[] x = new double[1000];
			double[] y = new double[1000];
			for (int i = 0; i < 800; i++)
			{
				x[i] = (double)i / 20;
				y[i] = SimFunctions.ONeal(x[i], ParamValues[0], ParamValues[1], ParamValues[2], ParamValues[3], ParamValues[4]);
			}

			PresentationRow xRow = new PresentationRow
			{
				Values = x,
				IsVisible = true,
				LegendIndex = -1,
				Color = Brushes.Black,
				LineType = TtpEnLineType.Point,
				Legend = "Temperatur(°C)"
			};

			PresentationRow yRow = new PresentationRow
			{
				Values = y,
				IsVisible = true,
				LegendIndex = 0,
				LineType = TtpEnLineType.Line,
				Thicknes = 3,
				Color = Brushes.DeepSkyBlue,
				Legend = $"T-Max = {ParamValues[0]} ; T-Opt = {ParamValues[1]}; Dev-Q = {ParamValues[2]} ; Dev-L = {ParamValues[3]}; K-Max = {ParamValues[4]}"
			};

			AddRow(xRow);
			AddRow(yRow);

		}

		private double CalcFertMult(double startFert, double startSkew, double endFert, double endSkew, double SumEgg)
		{
			double bioAge = 0.0;
			double sum = 0.0;

			for (int i = 0; i <= 1000; i++)
			{
				sum += SimFunctions.FertilityFkt(bioAge, startFert, startSkew, endFert, endSkew, 0.001);
				bioAge += 0.001;
			}

			return SumEgg / (sum * 1000.0);
		}

		#endregion
	}
}

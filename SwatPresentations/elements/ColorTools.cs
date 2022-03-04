using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SwatPresentations
{
	public class ColorTools
	{
		static public Color GetShortRainbow(double f) // blau (0) bis rot(1)
		{
			if (f < 0.0)
				f = 0.0;
			if (f > 1.0)
				f = 1.0;
			byte r = 0, g = 0, b = 0;
			double a = (1 - f) / 0.25; //invert and group
			byte X = (byte)Math.Floor(a);  //this is the integer part
			byte Y = (byte)Math.Floor(255 * (a - X)); //fractional part from 0 to 255
			switch (X)
			{
				case 0: r = 255; g = Y; b = 0; break;
				case 1: r = (byte)(255 - Y); g = 255; b = 0; break;
				case 2: r = 0; g = 255; b = (byte)Y; break;
				case 3: r = 0; g = (byte)(255 - Y); b = 255; break;
				case 4: r = 0; g = 0; b = 255; break;
			}
			return Color.FromRgb(r, g, b);
		}

		static public Color GetLongRainbow(double f) // violett(0) bis rot (1)
		{
			if (f < 0.0)
				f = 0.0;
			if (f > 1.0)
				f = 1.0;
			byte r = 0, g = 0, b = 0;
			double a = (1 - f) / 0.2;
			byte X = (byte)(Math.Floor(a));
			byte Y = (byte)Math.Floor(255 * (a - X));
			switch (X)
			{
				case 0: r = 255; g = Y; b = 0; break;
				case 1: r = (byte)(255 - Y); g = 255; b = 0; break;
				case 2: r = 0; g = 255; b = Y; break;
				case 3: r = 0; g = (byte)(255 - Y); b = 255; break;
				case 4: r = Y; g = 0; b = 255; break;
				case 5: r = 255; g = 0; b = 255; break;
			}
			return Color.FromRgb(r, g, b);
		}

		static public Color GetDifferentColor(int id) 
		{
			//double[] ff = { 0.0, 1.0, 0.5, 0.75, 0.125, 0.875, 0.375, 0.625 };
			double[] ff = { 0.0, 1.0, 0.5, 0.75, 0.25, 0.875, 0.125, 0.625 };

			//byte[] cc = { 255, 128, 64, 196, 32, 228, 96, 160 };
			byte[] cc = { 255, 64, 128, 32, 196, 96, 228, 160 };

			byte[] tt = { 255, 64, 196, 128 };
			int maxId = ff.Length * cc.Length * tt.Length;
			id %= maxId;
			double f = ff[id % ff.Length];
			byte c = cc[id / ff.Length];
			byte t = tt[id / (ff.Length * cc.Length)];

			byte r = 0, g = 0, b = 0;
			double a = (1 - f) / 0.2;
			byte X = (byte)(Math.Floor(a));
			byte Y = (byte)Math.Floor(255 * (a - X));
			switch (X)
			{
				case 0: r = c; g = Y; b = 0; break;
				case 1: r = (byte)(255 - Y); g = c; b = 0; break;
				case 2: r = 0; g = 255; b = Y; break;
				case 3: r = 0; g = (byte)(255 - Y); b = c; break;
				case 4: r = Y; g = 0; b = c; break;
				case 5: r = c; g = 0; b = c; break;
			}
			return Color.FromArgb(t, r, g, b);
		}

		static public Color GetColorRedYellowGreen(double f) // 0= rot... 1= grün
		{
			if (f < 0.0)
				f = 0.0;
			if (f > 1.0)
				f = 1.0;

			var green = (f > 0.5 ? 1 - 2 * (f - 0.5) : 1.0) * 255;
			var red = (f > 0.50 ? 1.0 : 2 * f) * 255;
			var blue = 0.0;
			Color result = Color.FromRgb((byte)red, (byte)green, (byte)blue);
			return result;
		}
	}
}

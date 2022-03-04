using swat.iodata;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace swat
{
	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			Properties["ExtUser"] = false;
			Properties["SuperUser"] = false;
			Properties["Developer"] = false;



			for (int i=0; i< e.Args.Length;i++)
			{ 
				if (string.Compare("-e", e.Args[i].Trim(), true) == 0)
					Properties["ExtUser"] = true;
				if (string.Compare("-x", e.Args[i].Trim(), true) == 0)
					Properties["SuperUser"] = true;
				if (string.Compare("-dev", e.Args[i].Trim(), true) == 0)
					Properties["Developer"] = true;
			}

			base.OnStartup(e);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			try
			{
				DeleteTemporaryFiles();
			}
			catch { }

			base.OnExit(e);
		}

		private void DeleteTemporaryFiles()
		{
			try
			{
				string[] files = Directory.GetFiles(WorkspaceData.GetPathWeatherWork, "*.zip");
				foreach (string f in files)
				{
					File.Delete(f);
				}
			}
			catch { }
		}

	}
}

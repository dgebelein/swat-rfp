using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;

namespace SwopReview
{
	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application
	{


		protected override void OnExit(ExitEventArgs e)
		{
			try
			{
				//Settings.Default.Save();
				DeleteTemporaryFiles();
			}
			catch { }

			base.OnExit(e);
		}

		// löscht alle html-Dateien im temporärenVerzeichnis
		private void DeleteTemporaryFiles()
		{
			String tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "swopReview");

			try
			{
				DirectoryInfo di = new DirectoryInfo(tempPath);
				FileInfo[] files = di.GetFiles();
				foreach (FileInfo file in files)
				{
					if (String.Compare(file.Extension, ".html", true) == 0)
						System.IO.File.Delete(file.FullName);
				}
			}
			catch (Exception)
			{ }
		}
	}
}

using Microsoft.Win32;
using SwatPresentations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SwopReview
{
	class VmReport: VmBase
	{
		string _code;

		public VmReport(SwopData sd):base(sd, null)
		{
			ViewVisual = new ViewReport();
		}

		public void SetTheCode(string code)
		{
			_code = code;
			WebBrowser browser = ((ViewReport)ViewVisual).reportBrowser;


			string url = CreateTemporaryFile(code);
			if (!string.IsNullOrEmpty(url))
				browser.Navigate(url);
				//browser.NavigateToString(_code);
		}

		public void PrintReport()
		{
			WebBrowser browser = ((ViewReport)ViewVisual).reportBrowser;

			mshtml.IHTMLDocument2 doc = browser.Document as mshtml.IHTMLDocument2;
			doc.execCommand("Print", true, null);
		}

		public void SaveReport()
		{
			SaveFileDialog sfd = new SaveFileDialog
			{
				Filter = "Html Datei|*.html",
				Title = "Save Report as File",
				FileName = _swopData.SwopLogName
			};
			sfd.ShowDialog();

			if (sfd.FileName != "")
			{
				try
				{
					File.WriteAllText(sfd.FileName, _code, Encoding.UTF8);
				}				
				catch (Exception e)
				{
				DlgMessage.Show("Error writing File", e.Message, MessageLevel.Error);
				}
			}

		}

		string CreateTemporaryFile(String sourceCode)
		{
			try
			{
				String tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "swopReview");
				if (!Directory.Exists(tempPath))
					Directory.CreateDirectory(tempPath);

				String fn = Guid.NewGuid().ToString() + ".html";
				string tempFilename = System.IO.Path.Combine(tempPath, fn);

				using (StreamWriter sw = new StreamWriter(File.Open(tempFilename, FileMode.Create), Encoding.UTF8))
				{
					sw.Write(sourceCode);
				}
				return tempFilename;
			}
			catch (Exception)
			{
				return null;
			}
		}

	}
}

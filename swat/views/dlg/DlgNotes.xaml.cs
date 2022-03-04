using swat.iodata;
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
using System.IO;
using SwatPresentations;

namespace swat.views.dlg
{
	/// <summary>
	/// Interaktionslogik für DlgNotes.xaml
	/// </summary>
	public partial class DlgNotes : Window
	{
		WorkspaceData _ws;

		bool _isChanged;

		private DlgNotes(WorkspaceData ws)
		{
			InitializeComponent();
			_ws = ws;
			txtNotes.Text = GetNotes();
			_isChanged = false;
		}

		public  static void Show(WorkspaceData ws)
		{
			DlgNotes dlg = new DlgNotes(ws);
			dlg.ShowDialog();
		}

		string GetNotesName()
		{
			 return System.IO.Path.Combine(WorkspaceData.GetPathNotes, _ws.Name + " - Notes.txt"); 
		}

		void SaveNotes()
		{
			string fn = GetNotesName();
			try { 
				if (string.IsNullOrEmpty(txtNotes.Text)) // Datei löschen, wenn Text gelöscht
				{ 
					if(File.Exists(fn))
						File.Delete(fn);
				}
				else
					File.WriteAllText(fn, txtNotes.Text, Encoding.UTF8);
			}
			catch { }
			_isChanged = false;
			_ws.Notes = txtNotes.Text.Trim();

		}

		string GetNotes()
		{
			string fn = GetNotesName();

			if (!File.Exists(fn))
				return "";

			try
			{
				return File.ReadAllText(fn.Trim());
			}
			catch { }

			return "";
		}

		void CanCreateNew(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = _isChanged;
		}

		void CreateNew(object sender, ExecutedRoutedEventArgs e)
		{
			SaveNotes();
			Close();
		}

		void CanCloseDlg(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		void CloseDlg(object sender, ExecutedRoutedEventArgs e)
		{
			if(_isChanged)
			{
				bool? response = DlgRememberSave.Show(_ws.Name, "Notizen sind noch nicht gespeichert!", "Jetzt speichern?", MessageLevel.Warning);
				if (response == null)
					return;

				if (response == true)
					SaveNotes();
			}
			Close();
		}

		void txtNotes_TextChanged(object sender, TextChangedEventArgs e)
		{
			_isChanged = true;
		}
	}
}

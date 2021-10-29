using swat.iodata;
using SwatPresentations;
//using swat.views.dlg;
using swat.views.sheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TTP.UiUtils;

namespace swat.vm
{
	class VmNotes:VmBase
	{

			
		#region Variable

		RelayCommand _saveNotesCommand;

		string _notesText;
		bool _notesChanged;


		#endregion

		#region Construction


		public VmNotes(VmSwat vm) : base(vm)
		{
			ViewVisual = new ViewNotes();

			_saveNotesCommand = new RelayCommand(param => this.SaveNotes(), param => this.MustSaveNotes);
			ReadNotes();

		}

		public override bool RespondToViewChange()
		{
			if (_notesChanged)
			{
				bool? response = DlgRememberSave.Show(Workspace.Name, "Notizen sind noch nicht gespeichert!", "Jetzt speichern?", MessageLevel.Warning);
				if (response == null)
					return false;

				if (response == true)
					SaveNotes();
			}
			return true;
		}

		#endregion

		#region Commands

		public ICommand SaveNotesCommand { get { return _saveNotesCommand; } }

		bool MustSaveNotes
		{
			get { return _notesChanged; }
		}

		#endregion

		#region Properties

		public string NotesText
		{
			get { return _notesText; }
			set
			{
				_notesText = value;
				_notesChanged = true;

				OnPropertyChanged("NotesText");
			}
		}

		private string NotesName
		{
			get { return Path.Combine(WorkspaceData.GetPathNotes, Workspace.Name + " - Notes.txt"); }
		}

		#endregion

		#region FileIO

		private void SaveNotes()
		{
			try
			{
				File.WriteAllText(NotesName, _notesText, Encoding.UTF8);
			}
			catch{ }
			
			_notesChanged = false;
		}

		private void ReadNotes()
		{
			string fn = NotesName;

			if (!File.Exists(fn))
				return;

			try
			{
				_notesText = File.ReadAllText(fn, Encoding.UTF8);
			}
			catch { }

			_notesChanged = false;
		}

		#endregion

	}
}

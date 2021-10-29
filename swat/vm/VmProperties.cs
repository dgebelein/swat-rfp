using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TTP.UiUtils;
using swat.views.dlg;
using swat.views.sheets;
using System.Windows.Input;
using System.Windows;
using swat.iodata;
using System.IO;

namespace swat.vm
{
	//class VmProperties : VmBase
	//{


	//	#region Variable

	//	RelayCommand _updateCommand;
	//	RelayCommand _selectFolderCommand;

	//	string _dataFolder;


	//	bool _changed;


	//	#endregion

	//	#region Construction


	//	public VmProperties(VmSwat vm) : base(vm)
	//	{
	//		ViewVisual = new ViewProperties();
	//		_dataFolder = (string)Application.Current.Properties["DataFolder"];
	//		if (string.IsNullOrEmpty(_dataFolder))
	//			_dataFolder =  Path.GetDirectoryName(WorkspaceData.GetPathWorkspace);

	//		_updateCommand = new RelayCommand(param => this.UpdateProperties(), param => this.MustUpdateProperties);
	//		_selectFolderCommand = new RelayCommand(param => SelectFolder());

	//		//ReadNotes();

	//	}



	//	#endregion

	//	#region Commands

	//	public ICommand UpdateCommand { get { return _updateCommand; } }
	//	public ICommand SelectFolderCommand { get { return _selectFolderCommand; } }


	//	private void SelectFolder()
	//	{
	//		System.Windows.Forms.FolderBrowserDialog fbDlg = new System.Windows.Forms.FolderBrowserDialog
	//		{
	//			Description = @"bitte wählen Sie das Verzeichnis zur Speicherung der importierten Wetterdaten aus",
	//			SelectedPath = DataFolder
	//		};

	//		if (fbDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
	//		{
	//			if(WorkspaceData.HasWriteAccessToFolder(fbDlg.SelectedPath))
	//				DataFolder = fbDlg.SelectedPath;
	//			else
	//			{
	//				DlgMessage.Show("falsche Auswahl des Arbeitsordners", "Für den Arbeitsordner muss Lese- und Schreibberechtigung bestehen", MessageLevel.Error);
	//			}
	//		}
	//	}

	//	bool MustUpdateProperties
	//	{
	//		get { return _changed && CanApply; }
	//	}

	//	private void UpdateProperties()
	//	{
	//		try
	//		{
	//			Properties.Settings.Default["DataFolder"] = _dataFolder;
	//			//Settings.Default.DataFolder
	//			//Application.Current.Properties["DataFolder"] = _dataFolder;
	//			Properties.Settings.Default.Save();
	//		}
	//		catch { }

	//		_changed = false;
	//	}
	//	#endregion

	//	#region Properties

	//	public string DataFolder
	//	{
	//		get { return _dataFolder; }
	//		set
	//		{
	//			_dataFolder = value;
	//			_changed = true;

	//			Validate();
	//		}
	//	}

	//	#endregion

	//	#region Validation

	//	//private bool HasWriteAccessToFolder(string folderPath)
	//	//{
	//	//	try
	//	//	{
	//	//		// Attempt to get a list of security permissions from the folder. 
	//	//		// This will raise an exception if the path is read only or do not have access to view the permissions. 
	//	//		System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
	//	//		return true;
	//	//	}
	//	//	catch (UnauthorizedAccessException)
	//	//	{
	//	//		return false;
	//	//	}
	//	//}

	//	void Validate()
	//	{
	//		ResetErrorList();

	//		if (string.IsNullOrWhiteSpace(DataFolder) || ! WorkspaceData.HasWriteAccessToFolder(DataFolder))
	//			AddErrorMessage("DataFolder", "Hier muss ein bereits vorhandener Ordner angegeben werden. Es muss Lese- und Schreibberechtigung bestehen.");

	//		OnPropertyChanged("DataFolder");
	//	}
	//	#endregion
	//}
}


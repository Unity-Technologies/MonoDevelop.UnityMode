using System;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public class FileSystemTreeBuilder
	{
		AssetDatabaseDTO _assetDatabaseDTO;

		Dictionary<string,Folder> _folders;

		public FileSystemTreeBuilder (AssetDatabaseDTO assetDatabaseDTO)
		{
			_assetDatabaseDTO = assetDatabaseDTO;
			_folders = new Dictionary<string, Folder> ();
		}

		public Folder Create ()
		{
			BuildFoldersDictionary ();
			PopulateFoldersWithFolders ();
			PopulateFoldersWithFiles ();
			return _folders [""];
		}

		void PopulateFoldersWithFolders ()
		{
			foreach (var folder in _folders.Values)
			{
				if (folder.IsRoot)
					continue;
				FindParent (folder.RelativePath).Add (folder);
			}
		}

		Folder FindParent (string fileSystemEntry)
		{
			var parentStr = FolderPathOf (fileSystemEntry);
			var parent = _folders [parentStr];
			if (parent == null)
				throw new InvalidProgramException ();
			return parent;
		}

		void PopulateFoldersWithFiles()
		{
			foreach (var f in _assetDatabaseDTO.Files)
				FindParent (f).Add (new File(f));
		}

		void BuildFoldersDictionary()
		{
			RegisterFolder ("");

			foreach (var f in _assetDatabaseDTO.Files)
				RegisterAllFoldersIn (f);
		}

		void RegisterAllFoldersIn (string f)
		{
			var folderPath = FolderPathOf (f);
			if (folderPath.Length == 0)
				return;

			RegisterFolder (folderPath);
			RegisterAllFoldersIn (folderPath);
		}

		void RegisterFolder(string folder)
		{
			if (_folders.ContainsKey (folder))
				return;
			_folders.Add (folder, new Folder (folder));
		}
		/*

			SetupFolder (result, _assetDatabaseDTO);
			return result;
		}

		void SetupFolder (Folder result,AssetDatabaseDTO assetDatabaseDTO)
		{
			var subfolders = AllSubfoldersIn (result, assetDatabaseDTO).ToList ();
			foreach (var subfolder in subfolders)
			{
				result.Add (subfolder);
				SetupFolder (subfolder, assetDatabaseDTO);
			}

			foreach (var f in assetDatabaseDTO.Files)
			{
				if (FolderPathOf (f) == result.Path.ToString ())
					result.Add (new File (f));
			}
		}

		IEnumerable<Folder> AllSubfoldersIn (Folder folder, AssetDatabaseDTO assetDatabaseDTO)
		{
			var returned = new List<string> ();

			foreach (var f in assetDatabaseDTO.Files)
			{
				var folderpath = FolderPathOf(f);
				if (folderpath.Length == 0)
					continue;

				var folderparentpath = FolderPathOf (folderpath);

				if (folderparentpath == folder.Path.ToString ())
				{
					if (returned.Contains (folderparentpath))
						continue;
					returned.Add (folderparentpath);

					yield return new Folder (folderpath);
				}
			}
		}
*/
		string FolderPathOf (string f)
		{
			if (f.Length == 0)
				return "";

			if (!f.Contains ("/"))
				return "";
			return f.Substring (0, f.LastIndexOf ('/'));
		}
	}
}


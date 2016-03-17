using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	/// <summary>
	/// Maintains a hierarchical folder structure used by AssetsFolderPad,
	/// updated from the UnityAssetDatabase received from Unity.
	/// </summary>
	public class FolderUpdater
	{
		UnityAssetDatabase assetDatabase;
		Dictionary<string, Folder> folders;

		public Folder RootFolder { get; private set; }

		public bool Update(UnityAssetDatabase database)
		{
			if (assetDatabase == null || assetDatabase.Empty)
			{
				Create(database);
				return false;
			}

			var oldAssetDatabase = assetDatabase;
			var newAssetDatabase = database;

			var hint = database.Hint;
			database.Hint = null;

			assetDatabase = database;

			if (hint is RenameHint)
			{
				RenameHint renameHint = hint as RenameHint;
				RenameFileOrDirectory(renameHint.OldPath, renameHint.NewPath);
				return true;
			}

			// Perform delta update of folders by comparing existing asset database with new one.
			var addFiles = newAssetDatabase.Files.Where(f => !oldAssetDatabase.Files.Contains(f)).ToArray();
			var addDirectories = newAssetDatabase.Directories.Where(f => !oldAssetDatabase.Directories.Contains(f)).ToArray();

			var removeFiles = oldAssetDatabase.Files.Where(f => !newAssetDatabase.Files.Contains(f)).ToArray();
			var removeDirectories = oldAssetDatabase.Directories.Where(f => !newAssetDatabase.Directories.Contains(f)).ToArray();

			var numChanges = addFiles.Length + addDirectories.Length + removeFiles.Length + removeDirectories.Length;

			if (numChanges == 0)
				return true;

			// Add all new folders
			foreach (var directory in addDirectories)
				AddDirectory(directory);

			// Add all new files
			foreach (var file in addFiles)
				AddFile(file);

			// Remove all removed files
			foreach (var file in removeFiles)
				RemoveFile(file);

			// Remove all removed folders
			foreach (var directory in removeDirectories)
				RemoveDirectory(directory);

			return true;
		}

		public void Create(UnityAssetDatabase database)
		{
			assetDatabase = database;

			RootFolder = new Folder("");
			folders = new Dictionary<string, Folder> {{RootFolder.RelativePath, RootFolder}};

			// Build folder structure from folders
			foreach (var directory in assetDatabase.Directories)
				AddDirectory(directory);

			// Add files to folders
			foreach (var file in assetDatabase.Files)
				AddFile(file);
		}

		void AddFile(string file)
		{
			var fileFolder = folders[GetParentDirectoryPath(file)];
			fileFolder.Add(new File(file));
		}

		void RemoveFile(string file)
		{
			var fileFolder = folders[GetParentDirectoryPath(file)];
			var fileEntry = fileFolder.GetFiles().Single(f => f.RelativePath == file);
			fileFolder.Remove(fileEntry);
		}

		void AddDirectory(string directory)
		{
			Folder childFolder = null;

			while (!folders.ContainsKey(directory))
			{
				var newFolder = new Folder(directory);

				if (childFolder != null)
					newFolder.Add(childFolder);

				folders.Add(directory, newFolder);

				childFolder = newFolder;
				directory = GetParentDirectoryPath(directory);
			}

			if (childFolder != null)
				folders[directory].Add(childFolder);
		}

		void RemoveDirectory(string directory)
		{
			var folder = folders[directory];
			var parentFolder = folders[GetParentDirectoryPath(folder.RelativePath)];
			parentFolder.Remove(folder);
		}

		void RenameFileOrDirectory(string oldPath, string newPath)
		{
			if (folders.ContainsKey(oldPath)) // Folder
			{
				var folder = folders[oldPath];

				foreach (var entry in folder.Children)
					RenameFileOrDirectory(entry.RelativePath, entry.RelativePath.Replace(oldPath, newPath));

				folder.RelativePath = newPath;

				folders.Remove(oldPath);
				folders.Add(newPath, folder);
			}
			else // File
			{
				var folder = folders[GetParentDirectoryPath(oldPath)];
				var file = folder.GetFiles().Single(f => f.RelativePath == oldPath);
				file.RelativePath = newPath;
			}	
		}

		string GetParentDirectoryPath(string path)
		{
			if (path.Length == 0)
				return RootFolder.RelativePath;

			if (!path.Contains("/"))
				return RootFolder.RelativePath;

			return path.Substring(0, path.LastIndexOf('/'));
		}

	}
}

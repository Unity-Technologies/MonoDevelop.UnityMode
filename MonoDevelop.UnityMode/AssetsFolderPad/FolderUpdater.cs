using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public class FolderUpdater
	{
		UnityAssetDatabase assetDatabase;
		Dictionary<string, Folder> folders;

		public Folder RootFolder { get; private set; }

		public bool Update(UnityAssetDatabase database)
		{
			if (assetDatabase == null || assetDatabase == database)
			{
				Create(database);
				return false;
			}

			var oldAssetDatabase = assetDatabase;
			var newAssetDatabase = database;

			assetDatabase = database;

			if (database.RenameHint != null)
			{
				RenameFileOrDirectory(database.RenameHint.OldPath, database.RenameHint.NewPath);
				return true;
			}

			var addFiles = newAssetDatabase.Files.Where(f => !oldAssetDatabase.Files.Contains(f)).ToArray();
			var addEmptyDirectories = newAssetDatabase.Directories.Where(f => !oldAssetDatabase.Directories.Contains(f)).ToArray();

			var removeFiles = oldAssetDatabase.Files.Where(f => !newAssetDatabase.Files.Contains(f)).ToArray();
			var removeEmptyDirectories = oldAssetDatabase.Directories.Where(f => !newAssetDatabase.Directories.Contains(f)).ToArray();

			var numChanges = addFiles.Length + addEmptyDirectories.Length + removeFiles.Length + removeEmptyDirectories.Length;

			if (numChanges == 0)
				return true;

			foreach (var directory in addEmptyDirectories)
				AddEmptyDirectory(directory);

			foreach (var directory in removeEmptyDirectories)
				RemoveEmptyDirectory(directory, addEmptyDirectories);

			foreach (var file in addFiles)
				AddFile(file);

			foreach (var file in removeFiles)
				RemoveFile(file, addEmptyDirectories);

			return true;
		}

		public void Create(UnityAssetDatabase database)
		{
			assetDatabase = database;

			RootFolder = new Folder("");
			folders = new Dictionary<string, Folder> {{RootFolder.RelativePath, RootFolder}};

			// Build folder structure from files
			foreach (var file in assetDatabase.Files)
				AddFile(file);

			// Build folder structure from empty folders
			foreach (var emptyDirectory in assetDatabase.Directories)
				AddEmptyDirectory(emptyDirectory);

		}

		void AddFile(string file)
		{
			var parentPath = GetParentDirectoryPath(file);
			Folder childFolder = null;

			while (!folders.ContainsKey(parentPath))
			{
				var newFolder = new Folder(parentPath);

				if (childFolder != null)
					newFolder.Add(childFolder);

				folders.Add(parentPath, newFolder);

				childFolder = newFolder;
				parentPath = GetParentDirectoryPath(parentPath);
			}

			if (childFolder != null)
				folders[parentPath].Add(childFolder);

			var fileFolder = folders[GetParentDirectoryPath(file)];
			fileFolder.Add(new File(file));
		}

		void RemoveFile(string file, string[] addEmptyDirectories)
		{
			var parentPath = GetParentDirectoryPath(file);
			var parentFolder = folders[parentPath];

			var fileEntry = parentFolder.GetFiles().Single(f => f.RelativePath == file);

			parentFolder.Remove(fileEntry);

			while (parentFolder != RootFolder && parentFolder.Empty() && !addEmptyDirectories.Contains(parentFolder.RelativePath))
			{
				var folder = parentFolder;

				parentPath = GetParentDirectoryPath(parentPath);
				parentFolder = folders[parentPath];

				if (parentFolder != null)
				{
					parentFolder.Remove(folder);
				}
			}
		}

		void AddEmptyDirectory(string directory)
		{
			Folder childFolder = null;

			// Note: If there is a hierarchy of folders that only contain
			// folders, then the folders will not be added as part of the
			// files, therefore we need to add the entire hierarchy.
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

		void RemoveEmptyDirectory(string directory, string[] addEmptyDirectories)
		{
			var folder = folders[directory];

			while (folder != RootFolder && folder.Empty() && !addEmptyDirectories.Contains(folder.RelativePath))
			{
				var parentPath = GetParentDirectoryPath(folder.RelativePath);
				var parentFolder = folders[parentPath];

				parentFolder.Remove(folder);

				folder = parentFolder;
			}
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

		private string GetParentDirectoryPath(string path)
		{
			if (path.Length == 0)
				return RootFolder.RelativePath;

			if (!path.Contains("/"))
				return RootFolder.RelativePath;

			return path.Substring(0, path.LastIndexOf('/'));
		}

	}
}

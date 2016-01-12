using System;
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Core.FileSystem;
using MonoDevelop.Core;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	class UnityModeFileSystemExtension : FileSystemExtension
	{
		public static void CreateAsset(string path, string type)
		{
			UnityRestClient.RestClient.CreateAsset(path, type);
			UnityModeAddin.UnityProjectRefresh (new NewFileHint{ Path = path });
		}

		public static bool FileExists(string path)
		{
			return UnityModeAddin.UnityAssetDatabase.FileExists (path);
		}

		public static bool DirectoryExists(string path)
		{
			return UnityModeAddin.UnityAssetDatabase.DirectoryExists (path);
		}

		public static string FindAvailableFilename(string filename)
		{
			if (!FileExists(filename))
				return filename;

			string basename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension (filename));
			string extension = Path.GetExtension (filename);

			int suffix = 1;

			while (FileExists (basename + suffix + extension))
				suffix++;

			return basename + suffix + extension;
		}

		public static string FindAvailableDirectoryName(string directoryName)
		{
			if (!DirectoryExists (directoryName))
				return directoryName;

			int suffix = 1;

			while (UnityModeFileSystemExtension.DirectoryExists (directoryName + suffix))
				suffix++;

			return directoryName + suffix;
		}

		public override bool CanHandlePath (FilePath path, bool isDirectory)
		{
			return !path.IsAbsolute;
		}

		public override void CopyFile (FilePath source, FilePath dest, bool overwrite)
		{
			if (source == dest)
				return;

			UnityRestClient.RestClient.CopyAsset(source, dest);
			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void RenameFile (FilePath path, string newName)
		{
			RenameAsset (path, path.ParentDirectory + "/" + newName);
		}

		public override void MoveFile (FilePath source, FilePath dest)
		{
			MoveAsset (source, dest);
		}

		public override void DeleteFile (FilePath file)
		{
			UnityRestClient.RestClient.DeleteAsset(file);
			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void DeleteDirectory(FilePath path)
		{
			DeleteFile (path);
		}

		public override void CreateDirectory (FilePath path)
		{
			UnityRestClient.RestClient.CreateDirectory(path);
			UnityModeAddin.UnityProjectRefresh (new NewFolderHint{ Path = path });
		}

		public override void CopyDirectory (FilePath sourcePath, FilePath destPath)
		{
			CopyDirectory (sourcePath, destPath, "");
		}

		void CopyDirectory (FilePath src, FilePath dest, FilePath subdir)
		{
			CopyFile (src, dest, true);
		}

		public override void RenameDirectory (FilePath path, string newName)
		{
			RenameFile (path, newName);
		}

		public override void MoveDirectory (FilePath source, FilePath dest)
		{
			// FileService calls this on RenameDirectory, so we check for rename.
			if (source.ParentDirectory.FullPath == dest.ParentDirectory.FullPath)
				RenameAsset(source, dest);
			else
				MoveAsset(source, dest);
		}

		public override FilePath ResolveFullPath (FilePath path)
		{
			throw new NotImplementedException ();
		}

		public override void RequestFileEdit (IEnumerable<FilePath> files)
		{
			throw new NotSupportedException ();
		}

		public override void NotifyFilesChanged (IEnumerable<FilePath> file)
		{
		}

		static void MoveAsset(string oldPath, string newPath)
		{
			if (oldPath == newPath)
				return;
			UnityRestClient.RestClient.MoveAsset(oldPath, newPath);
			UnityModeAddin.UnityProjectRefresh ();
		}

		static void RenameAsset(string oldPath, string newPath)
		{
			if (oldPath == newPath)
				return;

			UnityRestClient.RestClient.MoveAsset(oldPath, newPath);
			UnityModeAddin.UnityProjectRefresh (new RenameHint{ OldPath = oldPath, NewPath = newPath});
		}

	}
}


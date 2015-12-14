using System;
using MonoDevelop.Core.FileSystem;
using MonoDevelop.Core;
using System.IO;
using File = System.IO.File;
using Directory = System.IO.Directory;
using System.Collections.Generic;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	internal class UnityModeFileSystemExtension : FileSystemExtension
	{
		public override bool CanHandlePath (FilePath path, bool isDirectory)
		{
			return true;
		}

		public override void CopyFile (FilePath source, FilePath dest, bool overwrite)
		{
			System.IO.File.Copy (source, dest, overwrite);
			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void RenameFile (FilePath path, string newName)
		{
			var oldPath = MakeRelative(path.FullPath);
			var relativeParent = MakeRelative(path.ParentDirectory);

			var newPath = relativeParent == "." ? newName : relativeParent + "/" + newName;

			RenameFileOrDirectory(oldPath, newPath);
		}

		public override void MoveFile (FilePath source, FilePath dest)
		{
			MoveFileOrDirectory(MakeRelative(source), MakeRelative(dest));
		}

		public override void DeleteFile (FilePath file)
		{
			System.IO.File.Delete (file);
			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void DeleteDirectory(FilePath path)
		{
			Directory.Delete(path, true);
			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void CreateDirectory (FilePath path)
		{
			Directory.CreateDirectory (path);
			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void CopyDirectory (FilePath sourcePath, FilePath destPath)
		{
			CopyDirectory (sourcePath, destPath, "");
			UnityModeAddin.UnityProjectRefresh ();
		}

		void CopyDirectory (FilePath src, FilePath dest, FilePath subdir)
		{
			string destDir = Path.Combine (dest, subdir);

			if (!Directory.Exists (destDir))
				FileService.CreateDirectory (destDir);

			foreach (string file in Directory.GetFiles (src))
				FileService.CopyFile (file, Path.Combine (destDir, Path.GetFileName (file)));

			foreach (string dir in Directory.GetDirectories (src))
				CopyDirectory (dir, dest, Path.Combine (subdir, Path.GetFileName (dir)));

			UnityModeAddin.UnityProjectRefresh ();
		}

		public override void RenameDirectory (FilePath path, string newName)
		{
			var oldPath = MakeRelative(path.FullPath);
			var relativeParent = MakeRelative(path.ParentDirectory);

			var newPath = relativeParent == "." ? newName : relativeParent + "/" + newName;

			RenameFileOrDirectory(oldPath, newPath);
		}

		public override FilePath ResolveFullPath (FilePath path)
		{
			return Path.GetFullPath (path);
		}

		public override void MoveDirectory (FilePath source, FilePath dest)
		{
			// FileService calls this on RenameDirectory, so we check for rename.
			if (source.ParentDirectory.FullPath == dest.ParentDirectory.FullPath)
				RenameFileOrDirectory(MakeRelative(source), MakeRelative(dest));
			else
				MoveFileOrDirectory(MakeRelative(source), MakeRelative(dest));
		}

		public override void RequestFileEdit (IEnumerable<FilePath> files)
		{
		}

		public override void NotifyFilesChanged (IEnumerable<FilePath> file)
		{
		}

		static string MakeRelative(FilePath abs)
		{
			return abs.ToRelative(UnityModeAddin.UnityProjectState.BaseDirectory).ToString().Replace('\\', '/');
		}

		private static void MoveFileOrDirectory(string oldPath, string newPath)
		{
			try
			{
				UnityRestClient.RestClient.MoveAssetRequest(oldPath, newPath);
				UnityModeAddin.UnityProjectRefresh ();
			}
			catch (Exception)
			{
				LoggingService.LogInfo("Unity move failed: " + oldPath + " -> " + newPath);
			}
		}

		private static void RenameFileOrDirectory(string oldPath, string newPath)
		{
			try
			{
				UnityRestClient.RestClient.MoveAssetRequest(oldPath, newPath);
				UnityModeAddin.UnityProjectRefresh (new RenameHint{ OldPath = oldPath, NewPath = newPath});
			}
			catch (Exception)
			{
				LoggingService.LogInfo("Unity rename failed: " + oldPath + " -> " + newPath);
			}
		}
	}
}


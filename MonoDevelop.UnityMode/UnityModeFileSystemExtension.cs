using System;
using System.Collections.Generic;
using System.IO;
using MonoDevelop.Core.FileSystem;
using MonoDevelop.Core;
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
			throw new NotImplementedException ();
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
			try
			{
				UnityRestClient.RestClient.DeleteAsset(MakeRelative(file.FullPath));
				UnityModeAddin.UnityProjectRefresh ();
			}
			catch (Exception e)
			{
				LoggingService.LogError("Unity delete asset failed: " + file, e);
			}			
		}

		public override void DeleteDirectory(FilePath path)
		{
			throw new NotImplementedException ();
		}

		public override void CreateDirectory (FilePath path)
		{
			try
			{
				UnityRestClient.RestClient.CreateDirectory(path);
				UnityModeAddin.UnityProjectRefresh ();
			}
			catch (Exception e)
			{
				LoggingService.LogError("Unity create directory failed: " + path, e);
			}
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
			throw new NotImplementedException ();
		}

		public override void NotifyFilesChanged (IEnumerable<FilePath> file)
		{
		}

		public static void CreateAsset(string path, string type)
		{
			try
			{
				UnityRestClient.RestClient.CreateAsset(path, type); 
				UnityModeAddin.UnityProjectRefresh ();
			}
			catch (Exception e)
			{
				LoggingService.LogError("Unity create asset from template failed: " + path + " type " + type, e);
			}
		}

		public static bool FileExists(string path)
		{
			return UnityModeAddin.UnityAssetDatabase.FileExists (path);
		}

		public static bool DirectoryExists(string path)
		{
			return UnityModeAddin.UnityAssetDatabase.DirectoryExists (path);
		}

		static string MakeRelative(FilePath abs)
		{
			return abs.ToRelative(UnityModeAddin.UnityProjectState.AssetsDirectory).ToString().Replace('\\', '/');
		}

		private static void MoveFileOrDirectory(string oldPath, string newPath)
		{
			try
			{
				UnityRestClient.RestClient.MoveAsset(oldPath, newPath);
				UnityModeAddin.UnityProjectRefresh ();
			}
			catch (Exception e)
			{
				LoggingService.LogError("Unity move failed: " + oldPath + " -> " + newPath, e);
			}
		}

		private static void RenameFileOrDirectory(string oldPath, string newPath)
		{
			try
			{
				UnityRestClient.RestClient.MoveAsset(oldPath, newPath);
				UnityModeAddin.UnityProjectRefresh (new RenameHint{ OldPath = oldPath, NewPath = newPath});
			}
			catch (Exception e)
			{
				LoggingService.LogError("Unity rename failed: " + oldPath + " -> " + newPath, e);
			}
		}
	}
}


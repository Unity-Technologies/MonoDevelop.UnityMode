//
// UnityModeFileSystemExtension.cs
//
// Author:
//       lucas <>
//
// Copyright (c) 2014 lucas
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Linq;
using MonoDevelop.Core.FileSystem;
using MonoDevelop.Core;
using System.IO;
using File = System.IO.File;
using Directory = System.IO.Directory;
using System.Collections.Generic;

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
			UnityModeAddin.UnityProjectStateRefresh ();
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
			UnityModeAddin.UnityProjectStateRefresh ();
		}

		public override void DeleteDirectory(FilePath path)
		{
			Directory.Delete(path, true);
			UnityModeAddin.UnityProjectStateRefresh ();
		}

		public override void CreateDirectory (FilePath path)
		{
			Directory.CreateDirectory (path);
			UnityModeAddin.UnityProjectStateRefresh ();
		}

		public override void CopyDirectory (FilePath sourcePath, FilePath destPath)
		{
			CopyDirectory (sourcePath, destPath, "");
			UnityModeAddin.UnityProjectStateRefresh ();
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

			UnityModeAddin.UnityProjectStateRefresh ();
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
				UnityModeAddin.UnityProjectStateRefresh ();
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
				UnityModeAddin.UnityProjectStateRefreshRename (oldPath, newPath);
			}
			catch (Exception)
			{
				LoggingService.LogInfo("Unity rename failed: " + oldPath + " -> " + newPath);
			}
		}
	}
}


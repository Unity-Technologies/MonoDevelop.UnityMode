using System;

namespace MonoDevelop.UnityMode
{
	public abstract class FileSystemEntry 
	{
		public string RelativePath { get; set; }
		public string Name { get { return System.IO.Path.GetFileName (RelativePath); } }
		public string AbsolutePath { get { return UnityModeAddin.UnityProjectState.AssetsDirectory+ "/" + RelativePath; } }
		public FileSystemEntry Parent { get; set; }
	}
}


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	public class UnityAssetDatabase
	{
		public List<string> Files { get; set; }
		public List<string> Directories { get; set; }
		public Hint Hint { get; set; }

		public UnityAssetDatabase()
		{
			Files = new List<string> ();
			Directories = new List<string>();
		}

		public bool FileExists(string path)
		{
			return Files.Contains (path);
		}

		public bool DirectoryExists(string path)
		{
			return Directories.Contains (path);
		}
	}

	public abstract class Hint
	{
	}

	public class RenameHint : Hint
	{
		public String OldPath { get; set; }
		public String NewPath { get; set; }
	}
}


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
		public RenameHint RenameHint { get; set; }

		public UnityAssetDatabase()
		{
			Files = new List<string> ();
			Directories = new List<string>();
		}
	}

	public class RenameHint
	{
		public String OldPath { get; set; }
		public String NewPath { get; set; }
	}
}


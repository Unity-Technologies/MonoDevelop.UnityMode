using System;
using MonoDevelop.Core;

namespace MonoDevelop.UnityMode
{
	public class Folder
	{
		//Folder parent;

		public Folder (string path, Folder parent)
		{
			//this.parent = parent;
			Path = path;
		}

		public FilePath Path {
			get;
			set;
		}

		public string Name {
			get { return System.IO.Path.GetDirectoryName (Path.FileName); }
		}
	}
}


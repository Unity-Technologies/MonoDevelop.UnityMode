using System;
using MonoDevelop.Ide.Gui.Components;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.UnityMode;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.UnityMode
{
	public class File : FileSystemEntry
	{
		public File()
		{
		}

		public File(string path)
		{
			RelativePath = path;
		}
	}
}


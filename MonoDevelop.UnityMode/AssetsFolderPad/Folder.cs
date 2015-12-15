using System;
using System.IO;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Ide.Gui.Pads;

namespace MonoDevelop.UnityMode
{
	public class Folder : FileSystemEntry
	{
		public List<FileSystemEntry> Children { get; set; }

		public Folder()
		{
			Children = new List<FileSystemEntry>();
		}

		public Folder(string path) : this()
		{
			RelativePath = path;
		}

		public File GetFile(string path)
		{
			if (path.StartsWith (UnityModeAddin.UnityProjectState.AssetsDirectory))
				path = path.Substring (UnityModeAddin.UnityProjectState.AssetsDirectory.Length+1);

			Folder currentFolder = this;
			string currentPath = path;

			while (currentFolder != null) 
			{
				var index = currentPath.IndexOf ('/');

				if (index == -1)
					return currentFolder.GetChild (currentPath) as File;

				var folderName = currentPath.Substring (0, index);
				currentFolder = currentFolder.GetChild (folderName) as Folder;

				currentPath = currentPath.Substring (index+1);
			}

			return null;
		}

		public FileSystemEntry GetChild(string name)
		{
			foreach (var child in Children)
				if (child.Name == name)
					return child;
			return null;
		}

		public void Add(FileSystemEntry child)
		{
			Children.Add (child);
			child.Parent = this;
		}

		public void Remove(FileSystemEntry child)
		{
			Children.Remove(child);
			child.Parent = null;
		}

		public bool Empty()
		{
			return Children.Count == 0;
		}

		public IEnumerable<File> GetFiles()
		{
			return Children.OfType<File> ();
		}

		public IEnumerable<Folder> GetFolders()
		{
			return Children.OfType<Folder> ();
		}

		public bool IsRoot {
			get { return RelativePath.Length == 0; }
		}

		public bool IsAssetsFolder()
		{
			return RelativePath == "Assets";
		}
	}


}


using System;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Core;
using Gdk;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.UnityMode
{
	public abstract class FileSystemEntry {

		public string RelativePath { get; set; }
		public string Name { get { return System.IO.Path.GetFileName (RelativePath); } }
	}

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

		public void Add(FileSystemEntry child)
		{
			Children.Add (child);
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
	}

	class FolderNodeBuilder: TypeNodeBuilder
	{
		public override Type NodeDataType {
			get { return typeof(Folder); }
		}

		public override Type CommandHandlerType {
			get { return typeof(FolderNodeCommandHandler); }
		}

		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			var file = (Folder) dataObject;
			return file.Name;
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			base.BuildNode (treeBuilder, dataObject, ref label, ref icon, ref closedIcon);

			var folder = (Folder) dataObject;
			label = folder.Name;
			icon = Context.GetIcon (Stock.ClosedFolder);
		}

		public override void BuildChildNodes (ITreeBuilder treeBuilder, object dataObject)
		{
			base.BuildChildNodes (treeBuilder, dataObject);

			var folder = (Folder)dataObject;

			foreach (var file in folder.GetFiles())
				treeBuilder.AddChild (file);
			foreach (var subfolder in folder.GetFolders())
				treeBuilder.AddChild (subfolder);
		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			return true;
		}

		public override void GetNodeAttributes (ITreeNavigator parentNode, object dataObject, ref NodeAttributes attributes)
		{
			attributes |= NodeAttributes.AllowRename;
		}
	}

	class FolderNodeCommandHandler: NodeCommandHandler
	{
		public override bool CanDropNode (object dataObject, DragOperation operation)
		{
			if (dataObject is File)
				return true;
			return false;
		}

		public override void OnNodeDrop (object dataObjects, DragOperation operation)
		{
			var file = dataObjects as File;
			if (file == null)
				return;

			//file.Path = new FilePath (file.Path.FileName + "Q");
		}

	}
}


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
	public abstract class FileSystemEntry {

		public string RelativePath { get; set; }
		public string Name { get { return System.IO.Path.GetFileName (RelativePath); } }
		public string AbsolutePath { get { return UnityModeAddin.UnityProjectState.BaseDirectory + "/" + RelativePath; } }
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

		public bool IsAssetsFolder()
		{
			return RelativePath == "Assets";
		}
	}

	class FolderNodeBuilder: TypeNodeBuilder
	{
		public override string ContextMenuAddinPath
		{
			get { return "/UnityMode/ContextMenu/AssetsFolderPad"; }
		}

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

		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, NodeInfo nodeInfo)
		{
			base.BuildNode(treeBuilder, dataObject, nodeInfo);

			var folder = (Folder)dataObject;
			nodeInfo.Label = folder.Name;
			nodeInfo.Icon = Context.GetIcon(Stock.OpenFolder);
			nodeInfo.ClosedIcon = Context.GetIcon(Stock.ClosedFolder);
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
			var folder = (Folder)dataObject;
			return folder.Children.Count > 0;
		}

		public override void GetNodeAttributes (ITreeNavigator parentNode, object dataObject, ref NodeAttributes attributes)
		{
			var folder = (Folder)dataObject;

			if(!folder.IsAssetsFolder())
				attributes |= NodeAttributes.AllowRename;
		}
	}

	class FolderNodeCommandHandler: NodeCommandHandler
	{
		public override void RenameItem(string newName)
		{
			base.RenameItem(newName);
			var folder = (Folder)CurrentNode.DataItem;

			FileService.RenameDirectory(new FilePath(folder.AbsolutePath), newName);
		}

		public override bool CanDropNode (object dataObject, DragOperation operation)
		{
			if (dataObject is File)
				return true;

			var folder = dataObject as Folder;

			if (folder != null && !folder.IsAssetsFolder())
				return true;

			return false;
		}

		public override bool CanDeleteItem()
		{
			return !IsAssetsFolder();
		}

		public override DragOperation CanDragNode()
		{
			return IsAssetsFolder() ? DragOperation.None : DragOperation.Copy | DragOperation.Move;
		}

		public override void DeleteItem()
		{
			var folder = (Folder)CurrentNode.DataItem;
			FileService.DeleteDirectory(folder.AbsolutePath);
		}

		public override void OnNodeDrop(object dataObjects, DragOperation operation)
		{
			var file = dataObjects as File;
			var folder = (Folder)CurrentNode.DataItem;

			if (file != null)
			{
				var src = new FilePath(file.AbsolutePath);
				var dst = new FilePath(folder.AbsolutePath + "/" + file.Name);

				if(operation == DragOperation.Move)
					FileService.MoveFile(src, dst);
				else if(operation == DragOperation.Copy)
					FileService.CopyFile(src, dst);

				return;
			}

			var dropFolder = dataObjects as Folder;

			if (dropFolder != null)
			{
				var src = new FilePath(dropFolder.AbsolutePath);
				var dst = new FilePath(folder.AbsolutePath + "/" + dropFolder.Name);

				if(operation == DragOperation.Move)
					FileService.MoveDirectory(src, dst);
				else if(operation == DragOperation.Copy)
					FileService.CopyDirectory(src, dst);
			}
		}

		[CommandHandler(ProjectCommands.NewFolder)]
		public void AddNewFolder()
		{
			var folder = CurrentNode.GetParentDataItem(typeof(Folder), true) as Folder;
			string directoryName = folder.AbsolutePath + "/" +  GettextCatalog.GetString("New Folder");
			int index = -1;

			if (Directory.Exists(directoryName))
			{
				while (Directory.Exists(directoryName + (++index + 1))) ;
			}

			if (index >= 0)
			{
				directoryName += index + 1;
			}

			Directory.CreateDirectory(directoryName);

			UnityModeAddin.UpdateUnityProjectState();
		}

		private bool IsAssetsFolder()
		{
			var folder = CurrentNode.DataItem as Folder;

			if (folder == null)
				return false;

			return folder.IsAssetsFolder();
		}
	}
}


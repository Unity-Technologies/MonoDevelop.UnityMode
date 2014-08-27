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

	class FileNodeBuilder: TypeNodeBuilder
	{
		public override string ContextMenuAddinPath
		{
			get { return "/UnityMode/ContextMenu/AssetsFolderPad"; }
		}

		public override Type NodeDataType {
			get { return typeof(File); }
		}

		public override Type CommandHandlerType {
			get { return typeof(FileNodeCommandHandler); }
		}

		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			var file = (File) dataObject;
			return file.Name;
		}

		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, NodeInfo nodeInfo)
		{
			base.BuildNode(treeBuilder, dataObject, nodeInfo);

			var file = (File)dataObject;

			nodeInfo.Label = file.Name;
			nodeInfo.Icon = DesktopService.GetIconForFile(file.RelativePath);
		}

		public override void GetNodeAttributes (ITreeNavigator parentNode, object dataObject, ref NodeAttributes attributes)
		{
			attributes |= NodeAttributes.AllowRename;
		}
	}

	class FileNodeCommandHandler: NodeCommandHandler
	{
		public override void RenameItem (string newName)
		{
			base.RenameItem (newName);
			var file = (File) CurrentNode.DataItem;

			FileService.RenameFile (new FilePath(file.AbsolutePath), newName);
		}

		public override void ActivateItem ()
		{
			var file = (File) CurrentNode.DataItem;
			IdeApp.Workbench.OpenDocument (new FileOpenInformation (file.AbsolutePath, null));
		}

		public override bool CanDeleteItem()
		{
			return true;
		}

		public override void DeleteItem()
		{
			var file = (File)CurrentNode.DataItem;
			FileService.DeleteFile(file.AbsolutePath);
		}

		public override DragOperation CanDragNode()
		{
			return DragOperation.Copy | DragOperation.Move;
		}
	}
}


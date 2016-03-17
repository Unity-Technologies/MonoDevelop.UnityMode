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
			var folder = dataObject as Folder;
			return folder.Name;
		}

		public override void BuildNode(ITreeBuilder treeBuilder, object dataObject, NodeInfo nodeInfo)
		{
			base.BuildNode(treeBuilder, dataObject, nodeInfo);

			var folder = dataObject as Folder;
			nodeInfo.Label = folder.Name;
			nodeInfo.Icon = Context.GetIcon(Stock.OpenFolder);
			nodeInfo.ClosedIcon = Context.GetIcon(Stock.ClosedFolder);
		}

		public override void BuildChildNodes (ITreeBuilder treeBuilder, object dataObject)
		{
			base.BuildChildNodes (treeBuilder, dataObject);

			var folder = dataObject as Folder;

			foreach (var file in folder.GetFiles())
				treeBuilder.AddChild (file);
			foreach (var subfolder in folder.GetFolders())
				treeBuilder.AddChild (subfolder);
		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			var folder = dataObject as Folder;
			return folder.Children.Count > 0;
		}

		public override void GetNodeAttributes (ITreeNavigator parentNode, object dataObject, ref NodeAttributes attributes)
		{
			attributes |= NodeAttributes.AllowRename;
		}

		public override object GetParentObject (object dataObject)
		{
			var fileSystemEntry = (FileSystemEntry)dataObject;
			return fileSystemEntry.Parent;
		}
	}

}

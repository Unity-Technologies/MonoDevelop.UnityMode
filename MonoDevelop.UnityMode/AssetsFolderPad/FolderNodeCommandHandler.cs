using System;
using System.IO;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide.Gui.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide;

namespace MonoDevelop.UnityMode
{

	class FolderNodeCommandHandler: NodeCommandHandler
	{
		public override void RenameItem(string newName)
		{
			base.RenameItem(newName);
			var folder = CurrentNode.DataItem as Folder;

			FileService.RenameDirectory(new FilePath(folder.RelativePath), newName);
		}

		public override bool CanDropNode (object dataObject, DragOperation operation)
		{
			return dataObject is File || dataObject is Folder;
		}

		public override DragOperation CanDragNode()
		{
			var folder = CurrentNode.DataItem as Folder;
			return folder.IsRoot ? DragOperation.None : DragOperation.Copy | DragOperation.Move;
		}

		public override bool CanDeleteMultipleItems ()
		{
			var folder = CurrentNode.DataItem as Folder;
			return !folder.IsRoot;
		}

		public override void DeleteMultipleItems ()
		{
			var folders = CurrentNodes.Select (nn => nn.DataItem as Folder).ToArray ();

			var message = folders.Length == 1 ? "Delete selected directory?" : "Delete selected directories?";
			var result = MessageService.AskQuestion (message, string.Join("\n", folders.Select(f => f.RelativePath )) + "\n\nYou cannot undo this action", AlertButton.Delete, AlertButton.Cancel);

			if(result == AlertButton.Delete)
				foreach(var folder in folders)
					FileService.DeleteDirectory(folder.RelativePath);
		}

		public override void OnNodeDrop(object dataObjects, DragOperation operation)
		{
			var folder = CurrentNode.DataItem as Folder;
			var file = dataObjects as FileSystemEntry;

			var src = new FilePath(file.RelativePath);
			var dst = new FilePath(folder.RelativePathCombine(file.Name));

			if(operation == DragOperation.Move)
				FileService.MoveFile(src, dst);
			else if(operation == DragOperation.Copy)
				FileService.CopyFile(src, dst);
		}

		[CommandHandler(ProjectCommands.NewFolder)]
		public void AddNewFolder()
		{
			var folder = CurrentNode.GetParentDataItem(typeof(Folder), true) as Folder;
			string directoryName = UnityModeFileSystemExtension.FindAvailableDirectoryName(folder.RelativePathCombine("New Folder"));
			FileService.CreateDirectory (directoryName);
		}

		[CommandHandler(ProjectCommands.NewCSharpScript)]
		public void AddNewCSharpScript ()
		{
			var folder = CurrentNode.GetParentDataItem(typeof(Folder), true) as Folder;
			string filename =  UnityModeFileSystemExtension.FindAvailableFilename(folder.RelativePathCombine("NewBehaviourScript.cs"));
			UnityModeFileSystemExtension.CreateAsset (filename, "C#Script");
		}
	}
}

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

		public override bool CanDeleteItem()
		{
			return true;
		}

		public override DragOperation CanDragNode()
		{
			return DragOperation.Copy | DragOperation.Move;
		}

		public override void DeleteItem()
		{
			var folder = CurrentNode.DataItem as Folder;
			FileService.DeleteDirectory(folder.RelativePath);
		}

		public override void OnNodeDrop(object dataObjects, DragOperation operation)
		{
			var folder = CurrentNode.DataItem as Folder;
			var file = dataObjects as FileSystemEntry;

			var src = new FilePath(file.RelativePath);
			var dst = new FilePath(folder.RelativePath + "/" + file.Name);

			if(operation == DragOperation.Move)
				FileService.MoveFile(src, dst);
			else if(operation == DragOperation.Copy)
				FileService.CopyFile(src, dst);
		}

		[CommandHandler(ProjectCommands.NewFolder)]
		public void AddNewFolder()
		{
			var folder = CurrentNode.GetParentDataItem(typeof(Folder), true) as Folder;
			string directoryName = UnityModeFileSystemExtension.FindAvailableDirectoryName(folder.RelativePath + "/" +  "New Folder");
			FileService.CreateDirectory (directoryName);
		}

		[CommandHandler(ProjectCommands.NewCSharpScript)]
		public void AddNewCSharpScript ()
		{
			var folder = CurrentNode.GetParentDataItem(typeof(Folder), true) as Folder;
			string filename =  UnityModeFileSystemExtension.FindAvailableFilename(folder.RelativePath + "/" + "NewBehaviourScript.cs");
			UnityModeFileSystemExtension.CreateAsset (filename, "C#Script");
		}
	}
}

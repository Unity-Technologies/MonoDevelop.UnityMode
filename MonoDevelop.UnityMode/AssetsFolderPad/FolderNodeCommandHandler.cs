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
			string directoryName = folder.RelativePath + "/" +  GettextCatalog.GetString("New Folder");
			int index = -1;

			if (UnityModeFileSystemExtension.DirectoryExists (directoryName))
				while (UnityModeFileSystemExtension.DirectoryExists (directoryName + (++index + 1))) {}

			if (index >= 0)
				directoryName += index + 1;

			FileService.CreateDirectory (directoryName);
		}

		[CommandHandler(ProjectCommands.NewCSharpScript)]
		public void AddNewCSharpScript ()
		{
			var folder = CurrentNode.GetParentDataItem(typeof(Folder), true) as Folder;
			string filename = folder.RelativePath + "/" + "NewBehaviourScript";

			int index = -1;

			if (UnityModeFileSystemExtension.FileExists(filename + ".cs"))
				while (UnityModeFileSystemExtension.FileExists (filename + (++index + 1) + ".cs")) {}

			if (index >= 0)
				filename += index + 1;

			UnityModeFileSystemExtension.CreateAsset (filename + ".cs", "C#Script");
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

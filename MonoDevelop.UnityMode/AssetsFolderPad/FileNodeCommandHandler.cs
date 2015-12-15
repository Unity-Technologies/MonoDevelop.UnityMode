using System;
using MonoDevelop.Ide.Gui.Components;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.UnityMode;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.UnityMode
{

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

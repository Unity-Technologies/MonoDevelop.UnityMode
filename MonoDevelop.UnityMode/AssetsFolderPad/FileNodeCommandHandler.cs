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
			var file = CurrentNode.DataItem as File;

			FileService.RenameFile (new FilePath(file.RelativePath), newName);
		}

		public override void ActivateItem ()
		{
			var file = CurrentNode.DataItem as File;
			IdeApp.Workbench.OpenDocument (new FileOpenInformation (file.AbsolutePath, null));
		}

		public override bool CanDeleteItem()
		{
			return true;
		}

		public override void DeleteItem()
		{
			var file = CurrentNode.DataItem as File;
			FileService.DeleteFile(file.RelativePath);
		}

		public override DragOperation CanDragNode()
		{
			return DragOperation.Copy | DragOperation.Move;
		}
	}
}

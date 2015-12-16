using System;
using MonoDevelop.Ide.Gui.Components;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.UnityMode;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using System.Linq;

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

		public override bool CanDeleteMultipleItems ()
		{
			return true;
		}

		public override void DeleteMultipleItems ()
		{
			var folders = CurrentNodes.Select (nn => nn.DataItem as File).ToArray ();

			var message = folders.Length == 1 ? "Delete selected asset?" : "Delete selected assets?";
			var result = MessageService.AskQuestion (message, string.Join("\n", folders.Select(f => f.RelativePath )) + "\n\nYou cannot undo this action", AlertButton.Delete, AlertButton.Cancel);

			if(result == AlertButton.Delete)
				foreach(var folder in folders)
					FileService.DeleteDirectory(folder.RelativePath);
		}

		public override DragOperation CanDragNode()
		{
			return DragOperation.Copy | DragOperation.Move;
		}
	}
}

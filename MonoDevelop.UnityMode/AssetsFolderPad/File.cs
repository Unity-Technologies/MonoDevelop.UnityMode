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

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			base.BuildNode (treeBuilder, dataObject, ref label, ref icon, ref closedIcon);

			var file = (File) dataObject;
			label = file.Name;
			icon = DesktopService.GetPixbufForFile (file.RelativePath, Gtk.IconSize.Menu);
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

			FileService.RenameFile (new FilePath(UnityModeAddin.UnityProjectState.BaseDirectory+"/"+file.RelativePath), newName);
		}

		public override void ActivateItem ()
		{
			var file = (File) CurrentNode.DataItem;
			IdeApp.Workbench.OpenDocument (new FileOpenInformation (UnityModeAddin.UnityProjectState.BaseDirectory+"/"+file.RelativePath, null));
		}
	}
}


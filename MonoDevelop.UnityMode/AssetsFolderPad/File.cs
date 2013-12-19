using System;
using MonoDevelop.Ide.Gui.Components;
using Gdk;
using MonoDevelop.Core;
using MonoDevelop.UnityMode;

namespace MonoDevelop.UnityMode
{
	public class File
	{
		public FilePath Path { get; set; }
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
			return file.Path.FileName + "LUCAS";
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			base.BuildNode (treeBuilder, dataObject, ref label, ref icon, ref closedIcon);

			var file = (File) dataObject;
			label = file.Path.FileName;
		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			return true;
		}
	}

	class FileNodeCommandHandler: NodeCommandHandler
	{
	}
}


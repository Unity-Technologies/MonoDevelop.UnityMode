using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui.Dialogs;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;

namespace MonoDevelop.UnityMode
{
	public class OpenUnityProjectCommand : CommandHandler
	{
		protected override void Run ()
		{
			var dlg = new OpenFileDialog (GettextCatalog.GetString ("Select Unity project folder"), Gtk.FileChooserAction.SelectFolder) {
				TransientFor = IdeApp.Workbench.RootWindow
			};
			if (!dlg.Run ())
				return;

			var folder = dlg.SelectedFile;

			UnityModeAddin.OpenUnityProject (folder);
		}
	}
}


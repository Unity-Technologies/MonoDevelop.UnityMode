using System;
using MonoDevelop.Core;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.UnityMode.ServiceModel;

namespace MonoDevelop.UnityMode
{
	public class UnityModeArgs
	{
		public string UnityProjectPath { get; set; }
		public string OpenFile { get; set; }
	}

	public class StartupHandler : CommandHandler
	{
		protected override void Run ()
		{
			Workbench workbench = IdeApp.Workbench;
			WorkbenchWindow workbenchWindow = workbench.RootWindow;

			var assetsFolderPad = workbench.GetPad<AssetsFolderPad>();

			if (assetsFolderPad != null && !assetsFolderPad.Visible)
				assetsFolderPad.Visible = true;

			var solutionPad = workbench.Pads.SolutionPad;

			if (solutionPad != null && solutionPad.Visible)
				solutionPad.Visible = false;

			workbenchWindow.FocusInEvent += WorkbenchFocusInEvent;
			workbench.DocumentOpened += DocumentOpenedOrClosed;
			workbench.DocumentClosed += DocumentOpenedOrClosed;

			var args = ParseArgs (Environment.GetCommandLineArgs ());

			if (args.OpenFile != null)
				OpenFileFromArgument (args.OpenFile);

			if (args.UnityProjectPath != null)
				UnityModeAddin.OpenUnityProject (args.UnityProjectPath);
			else 
			{
				// For development
				#if DEBUG
				UnityModeAddin.InitializeAndPair("http://localhost:38000");
				#endif
			}
		}

		~StartupHandler()
		{
			Workbench workbench = IdeApp.Workbench;
			WorkbenchWindow workbenchWindow = workbench.RootWindow;

			workbenchWindow.FocusInEvent -= WorkbenchFocusInEvent;
			workbench.DocumentOpened -= DocumentOpenedOrClosed;
			workbench.DocumentClosed -= DocumentOpenedOrClosed;
		}

		static void WorkbenchFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			UnityModeAddin.UnityProjectStateRefresh ();
		}

		static void DocumentOpenedOrClosed (object sender, EventArgs e)
		{
			UnityRestHelpers.SaveProjectSettings ();
		}

		static UnityModeArgs ParseArgs(string[] args)
		{
			LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "UnityMode Args " + String.Join("!",args));

			UnityModeArgs unityModeArgs = new UnityModeArgs ();

			var p = new Mono.Options.OptionSet ();
			p.Add ("unityOpenFile=", "Unity Open File", f => unityModeArgs.OpenFile = f);
			p.Add ("unityProjectPath=", "Unity Project Path", path => unityModeArgs.UnityProjectPath = path);

			try 
			{
				p.Parse (args);
			} 
			catch(Mono.Options.OptionException e)
			{
				LoggingService.LogInfo("OptionException: " + e);
				return null;
			}

			return unityModeArgs;
		}

		static void OpenFileFromArgument(string openFileArg)
		{
			string[] fileLine = openFileArg.Split (';');

			if (fileLine.Length == 2)
				UnityRestHelpers.OpenFile (fileLine [0], int.Parse (fileLine [1]), OpenDocumentOptions.BringToFront);
			else
				UnityRestHelpers.OpenFile (openFileArg, 0, OpenDocumentOptions.BringToFront);
		}
	}
}

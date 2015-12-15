using System;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Commands;
using MonoDevelop.Debugger;
using Mono.Debugging.Client;

namespace MonoDevelop.UnityMode
{
	public class UnityModeArgs
	{
		public string UnityProjectPath { get; set; }
		public string OpenFile { get; set; }
	}

	public class StartupHandler : CommandHandler
	{
		EventHandler<BreakpointEventArgs> breakpointUpdatedHandler;
		EventHandler<BreakpointEventArgs> breakpointRemovedHandler;
		EventHandler<BreakpointEventArgs> breakpointAddedHandler;
		EventHandler breakpointChangedHandler;

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
			workbench.DocumentOpened += UpdateAndSaveProjectSettings;
			workbench.DocumentClosed += UpdateAndSaveProjectSettings;


			breakpointUpdatedHandler = DispatchService.GuiDispatch<EventHandler<BreakpointEventArgs>> (UpdateAndSaveProjectSettings);
			breakpointRemovedHandler = DispatchService.GuiDispatch<EventHandler<BreakpointEventArgs>> (UpdateAndSaveProjectSettings);
			breakpointAddedHandler = DispatchService.GuiDispatch<EventHandler<BreakpointEventArgs>> (UpdateAndSaveProjectSettings);
			breakpointChangedHandler = DispatchService.GuiDispatch<EventHandler> (UpdateAndSaveProjectSettings);

			var breakpoints = DebuggingService.Breakpoints;
			breakpoints.BreakpointAdded += breakpointAddedHandler;
			breakpoints.BreakpointRemoved += breakpointRemovedHandler;
			breakpoints.Changed += breakpointChangedHandler;
			breakpoints.BreakpointUpdated += breakpointUpdatedHandler;

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
			workbench.DocumentOpened -= UpdateAndSaveProjectSettings;
			workbench.DocumentClosed -= UpdateAndSaveProjectSettings;
		}

		static void WorkbenchFocusInEvent(object o, Gtk.FocusInEventArgs args)
		{
			UnityModeAddin.UnityProjectRefresh ();
		}

		static void UpdateAndSaveProjectSettings (object sender, EventArgs e)
		{
			UnityRestHelpers.UpdateAndSaveProjectSettings (UnityModeAddin.UnityProjectSettings);
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

using System;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Components.Commands;
using MonoDevelop.Debugger;
using Mono.Debugging.Client;

namespace MonoDevelop.UnityMode
{
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

			IdeApp.Exiting += (object sender, ExitEventArgs args) => Exiting();
			IdeApp.FocusIn += FocusInEvent;

			workbench.ActiveDocumentChanged += UpdateAndSaveProjectSettings;
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

			var unityProjectPath = ParseUnityProjectPathFromArgs (Environment.GetCommandLineArgs ());

			if (unityProjectPath != null)
				UnityModeAddin.OpenUnityProject (unityProjectPath);
			else 
			{
				// For development
				#if DEBUG
				UnityModeAddin.InitializeAndPair("http://localhost:38000");
				#endif
			}
		}

		static void Exiting()
		{
			Workbench workbench = IdeApp.Workbench;
			WorkbenchWindow workbenchWindow = workbench.RootWindow;

			workbenchWindow.FocusInEvent -= FocusInEvent;

			workbench.ActiveDocumentChanged -= UpdateAndSaveProjectSettings;
			workbench.DocumentOpened -= UpdateAndSaveProjectSettings;
			workbench.DocumentClosed -= UpdateAndSaveProjectSettings;
		}

		static void FocusInEvent(object o, EventArgs args)
		{
			UnityModeAddin.UnityProjectRefresh ();
		}

		static void UpdateAndSaveProjectSettings (object sender, EventArgs e)
		{
			UnityRestHelpers.UpdateAndSaveProjectSettings (UnityModeAddin.UnityProjectSettings);
		}

		static string ParseUnityProjectPathFromArgs(string[] args)
		{
			LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "UnityMode Args " + String.Join("!",args));

			string unityProjectPath = null;

			var p = new Mono.Options.OptionSet ();
			p.Add ("unityProjectPath=", "Unity Project Path", path => unityProjectPath = path);

			try 
			{
				p.Parse (args);
			} 
			catch(Mono.Options.OptionException e)
			{
				LoggingService.LogInfo("OptionException: " + e);
				return null;
			}

			return unityProjectPath;
		}
	}
}

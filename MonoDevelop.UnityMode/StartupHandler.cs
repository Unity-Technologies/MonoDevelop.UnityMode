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
		static EventHandler<BreakpointEventArgs> breakpointUpdatedHandler;
		static EventHandler<BreakpointEventArgs> breakpointRemovedHandler;
		static EventHandler<BreakpointEventArgs> breakpointAddedHandler;
		static EventHandler breakpointChangedHandler;

		protected override void Run ()
		{
			UnityModeAddin.UnityPaired += delegate { ShowAssetsPad(true); };
			UnityModeAddin.UnityUnpaired += delegate { ShowAssetsPad(false); };
			Init();

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

		static void Init()
		{
			Workbench workbench = IdeApp.Workbench;

			IdeApp.Exiting += (object sender, ExitEventArgs args) => Exit();
			IdeApp.FocusIn += FocusInEvent;

			// Save changes to open documents in Unity's CodeEditorProjectSettings.json
			workbench.ActiveDocumentChanged += UpdateAndSaveProjectSettings;
			workbench.DocumentOpened += UpdateAndSaveProjectSettings;
			workbench.DocumentClosed += UpdateAndSaveProjectSettings;

			// Save changes to breakpoints in Unity's CodeEditorProjectSettings.json
			breakpointUpdatedHandler = DispatchService.GuiDispatch<EventHandler<BreakpointEventArgs>> (UpdateAndSaveProjectSettings);
			breakpointRemovedHandler = DispatchService.GuiDispatch<EventHandler<BreakpointEventArgs>> (UpdateAndSaveProjectSettings);
			breakpointAddedHandler = DispatchService.GuiDispatch<EventHandler<BreakpointEventArgs>> (UpdateAndSaveProjectSettings);
			breakpointChangedHandler = DispatchService.GuiDispatch<EventHandler> (UpdateAndSaveProjectSettings);

			var breakpoints = DebuggingService.Breakpoints;
			breakpoints.BreakpointAdded += breakpointAddedHandler;
			breakpoints.BreakpointRemoved += breakpointRemovedHandler;
			breakpoints.Changed += breakpointChangedHandler;
			breakpoints.BreakpointUpdated += breakpointUpdatedHandler;

		}

		static void Exit()
		{
			IdeApp.FocusIn -= FocusInEvent;

			Workbench workbench = IdeApp.Workbench;
			workbench.ActiveDocumentChanged -= UpdateAndSaveProjectSettings;
			workbench.DocumentOpened -= UpdateAndSaveProjectSettings;
			workbench.DocumentClosed -= UpdateAndSaveProjectSettings;

			var breakpoints = DebuggingService.Breakpoints;
			breakpoints.BreakpointAdded -= breakpointAddedHandler;
			breakpoints.BreakpointRemoved -= breakpointRemovedHandler;
			breakpoints.Changed -= breakpointChangedHandler;
			breakpoints.BreakpointUpdated -= breakpointUpdatedHandler;
		}

		static void ShowAssetsPad(bool assetPadVisible)
		{
			Workbench workbench = IdeApp.Workbench;

			// Show the Unity Assets Folder Pad
			var assetsFolderPad = workbench.GetPad<AssetsFolderPad>();

			if (assetsFolderPad != null && assetsFolderPad.Visible != assetPadVisible)
				assetsFolderPad.Visible = assetPadVisible;

			// Hide the MonoDevelop Solution Pad.
			// TODO: See if we can disable showing the solution pad by a setting.
			var solutionPad = workbench.Pads.SolutionPad;

			if (solutionPad != null && solutionPad.Visible == assetPadVisible)
				solutionPad.Visible = !assetPadVisible;
		}

		// Refresh the project each time MonoDevelop gets focus, 
		// in case the Unity project
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

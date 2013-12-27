using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Diagnostics;
using MonoDevelop.Ide.Gui;
using MonoDevelop.UnityMode.UnityRestClient;
using System.Linq;
using System;
using System.Collections.Generic;

namespace MonoDevelop.UnityMode
{
	public class StartupHandler : CommandHandler
	{
		protected override void Run ()
		{
			SetupStartupOptions ();

			//Mono.Addins.AddinManager.AddinEngine.Registry.DisableAddin ("MonoDevelop.VersionControl");

			IdeApp.CommandService.CommandEntrySetPostProcessor += MyPostProcessor;

			((DefaultWorkbench)IdeApp.Workbench.RootWindow).RecreateMenu ();

			InitializeUnitySolution ();
			IdeApp.Workbench.ShowCommandBar ("UnityDebugging");

			Workbench wb = IdeApp.Workbench;
			WorkbenchWindow ww = wb.RootWindow;
			var dw = (DefaultWorkbench)ww;

			/*
			Gtk.HBox contentBox = dw.Toolbar.ContentBox;
			//contentBox.PackStart(new Gtk.Label ("LUCASLUCAS"), false, false, 0);
			//contentBox.PackStart(new Gtk.Button ("LUCASLUCAS"), false, false, 0);

			var statusAreaAlign = new Gtk.Alignment (100, 0, 1, 1);
			var button = new Gtk.Button ("AISHDASDADAS");
			button.Visible = true;
			statusAreaAlign.Add (button);
			contentBox.PackStart (statusAreaAlign, true, true, 0);
			contentBox.PackStart (button, false, false, 10);
			//MonoDevelop.Debugger.DebuggerService;

			dw.Toolbar.unityDebugButton.Toggled += (sender, e) => {if (true) {
					DebugEditorHandler.Doit();
					}
				};*/
		}


		CommandEntrySet MyPostProcessor(CommandEntrySet input)
		{
			var toRemove = new List<CommandEntry> ();
			foreach(CommandEntry ce in input)
			{
				var theSet = ce as CommandEntrySet;
				if (theSet != null)
					MyPostProcessor (theSet);

				var id = ce.CommandId as string;
				if (isBlackListed (id))
					toRemove.Add (ce);
			}
			foreach (var r in toRemove)
				input.Remove (r);

			return input;
		}

		bool isBlackListed(string command)
		{
			switch (command)
			{
			/*			case "MonoDevelop.Ide.Commands.ProjectCommands.CleanSolution":
			case "MonoDevelop.Ide.Commands.ProjectCommands.RebuildSolution":
			case "MonoDevelop.Ide.Commands.ProjectCommands.Rebuild":
			case "MonoDevelop.Ide.Commands.ProjectCommands.Clean":*/
			case "Project":
			case "Build":
			case "Tools":
			case "RecentProjects":
			case "MonoDevelop.Ide.Commands.FileCommands.NewProject":
			case "MonoDevelop.Ide.Commands.FileCommands.NewWorkspace":
			case "MonoDevelop.Ide.Commands.FileCommands.CloseWorkspace":
			case "MonoDevelop.Ide.Commands.ViewCommands.ShowWelcomePage":
			case "MonoDevelop.Ide.Commands.HelpCommands.About":
			case "MonoDevelop.Ide.Updater.UpdateCommands.CheckForUpdates":
				return true;
			default:
				return false;
			}
		}

		void SetupStartupOptions ()
		{
			var args = Environment.GetCommandLineArgs ();

			var p = new Mono.Options.OptionSet ();
			p.Add ("unityProcessId=", "unityProcessId", (int i) => StartupOptions.UnityProcessId = i);

			LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "ARGS: " + String.Join("!",args));

			try {
				p.Parse (args);
			} catch(Mono.Options.OptionException e)
			{
				LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "OptionException: " + e.ToString());
			}

			LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "ProcessID: " + StartupOptions.UnityProcessId);
		}

		//if (Environment.GetCommandLineArgs ().Contains ("--unityMode"))
		static void InitializeUnitySolution()
		{
			UnityModeAddin.Initialize ();

			var state = new UnityProjectState ();
			state.MonoIslands.Add (new MonoIsland () { Files = new List<string>(){"test.cs"}, Language = "C#", Name = "BLA"});

			var assetDatabaseDTO = new AssetDatabaseDTO () { Files = new List<string> () {
					"test.cs",
					"sub1/test2.cs",
					"sub1/lucas.cs",
					"sub1/sub2/bla.cs"
				} };

			state.AssetDatabase = assetDatabaseDTO;
			UnityModeAddin.UnityProjectState = state;

			new RestService (unityProjectState => {
				UnityModeAddin.UnityProjectState = unityProjectState;
			}, fileOpenRequest => {
				var fileOpenInformation = new FileOpenInformation (fileOpenRequest.File, null, fileOpenRequest.Line, 0, OpenDocumentOptions.BringToFront);
				IdeApp.Workbench.OpenDocument (fileOpenInformation);
				DispatchService.GuiDispatch (IdeApp.Workbench.GrabDesktopFocus);
			}
			);

			DispatchService.BackgroundDispatch (() => {
				LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "SendingInfoRequest");
				RestClient2.SendSolutionInformationRequest ();
			});
		}
	}
}

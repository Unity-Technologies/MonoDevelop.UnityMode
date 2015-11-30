using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Codons;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Projects;
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

			SetupUnityInstanceFromArgs ();
			UnityModeAddin.InitializeRestServiceAndPair ();
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
			UnityRestHelpers.SendOpenDocumentsToUnity ();
		}

		static void SetupUnityInstanceFromArgs ()
		{
			var args = Environment.GetCommandLineArgs ();

			string openFileArg = null;

			var p = new Mono.Options.OptionSet ();
			p.Add ("unityProcessId=", "Unity Process Id", (int i) => UnityInstance.ProcessId = i);
			p.Add ("unityRestServerUrl=", "Unity REST Server URL", s => UnityInstance.RestServerUrl = s);
			p.Add ("unityOpenFile=", "Unity Open File", f => openFileArg = f);

			RestClient.SetServerUrl(UnityInstance.RestServerUrl);

			LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "UnityMode Args " + String.Join("!",args));

			try 
			{
				p.Parse (args);
			} 
			catch(Mono.Options.OptionException e)
			{
				LoggingService.LogInfo("OptionException: " + e.ToString());
			}

			if (!string.IsNullOrEmpty (openFileArg)) {
				string[] fileLine = openFileArg.Split (';');

				if (fileLine.Length == 2)
					UnityRestHelpers.OpenFile (fileLine [0], int.Parse (fileLine [1]), OpenDocumentOptions.BringToFront);
				else
					UnityRestHelpers.OpenFile (openFileArg, 0, OpenDocumentOptions.BringToFront);
			}

			UnityInstance.Log();
		}
	}
}

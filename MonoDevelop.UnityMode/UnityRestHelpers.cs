using System;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Ide;
using MonoDevelop.UnityMode.UnityRestClient;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Debugger;
using Mono.Debugging.Client;

namespace MonoDevelop.UnityMode
{
	static class UnityRestHelpers
	{
		internal static void OpenFile(string filename, int line, OpenDocumentOptions options = OpenDocumentOptions.None)
		{
			if (filename == null)
				return;

			LoggingService.LogInfo ("OpenFile: " + filename + " Line " + line);

			var workbench = IdeApp.Workbench;

			if (workbench.ActiveDocument != null && workbench.ActiveDocument.FileName == filename)
				return;

			var fileOpenInformation = new FileOpenInformation (filename, null, line, 0, options);

			try
			{
				DispatchService.GuiSyncDispatch(() =>
					{
						if (workbench.Documents.Any(d => d.FileName == fileOpenInformation.FileName))
						{
							var doc = workbench.Documents.Single(d => d.FileName == fileOpenInformation.FileName);
							doc.Select();

							IEditableTextBuffer ipos = (IEditableTextBuffer) doc.Window.ViewContent.GetContent (typeof(IEditableTextBuffer));
							if (line >= 1 && ipos != null) 
							{
								doc.DisableAutoScroll ();
								doc.RunWhenLoaded (() => ipos.SetCaretTo (line, 1, false, false));
							}
						}
						else
						{
							workbench.OpenDocument(fileOpenInformation);
							IdeApp.Workbench.GrabDesktopFocus();
						}
					});
			}
			catch (Exception e)
			{
				LoggingService.LogError(e.ToString());
			}
		}

		internal static void QuitApplication(string unityProject)
		{
			if (unityProject == UnityModeAddin.UnityProjectSettings.ProjectPath)
				DispatchService.GuiDispatch (() => IdeApp.Exit ());
			else
				LoggingService.LogInfo ("UnityMode: Could not Exit application because requested unityProject '"
					+ unityProject + "' does not match '" + UnityModeAddin.UnityProjectSettings.ProjectPath + "'");
		}

		internal static void UpdateAndSaveProjectSettings(UnityProjectSettings projectSettings)
		{
			var openDocuments = IdeApp.Workbench.Documents.Select (d => d.FileName.ToString ().Replace ('\\', '/')).ToList ();
			var breakpoints = DebuggingService.Breakpoints.GetBreakevents ().OfType<Breakpoint>().Select (bp => string.Format("{0};{1};{2}", bp.FileName, bp.Line, bp.Column)).ToList ();

			if (!openDocuments.SequenceEqual(projectSettings.OpenDocuments) || ! breakpoints.SequenceEqual(projectSettings.Breakpoints))
			{
				projectSettings.OpenDocuments = openDocuments;
				projectSettings.Breakpoints = breakpoints;
				DispatchService.BackgroundDispatch (() => RestClient.SaveProjectSettings(projectSettings.OpenDocuments, projectSettings.Breakpoints));
			}
		}

		internal static UnityProjectSettings LoadAndApplyProjectSettings()
		{
			var projectSettings =  RestClient.GetProjectSettings();

			var breakpointStore = DebuggingService.Breakpoints;

			breakpointStore.Clear ();

			foreach (var breakpoint in projectSettings.breakpoints) {
				var pathLineColumn = breakpoint.Split (';');

				if (pathLineColumn.Length == 2)
					breakpointStore.Add (pathLineColumn [0], int.Parse (pathLineColumn [1]));
				else if (pathLineColumn.Length == 3)
					breakpointStore.Add (pathLineColumn [0], int.Parse (pathLineColumn [1]), int.Parse (pathLineColumn [2]));
				else
					LoggingService.LogWarning (string.Format ("UnityMode: Unable to add breakpoint: {0}", breakpoint));
			}

			foreach(var document in projectSettings.documents)
				OpenFile(document, 0);

			OpenFile (projectSettings.documents.FirstOrDefault(), 0, OpenDocumentOptions.BringToFront);

			var result = new UnityProjectSettings ();

			result.OpenDocuments = projectSettings.documents;
			result.Breakpoints = projectSettings.breakpoints;

			return result;
		}
	}
}


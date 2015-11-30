﻿using System;
using System.Linq;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.Ide;
using MonoDevelop.UnityMode.UnityRestClient;

namespace MonoDevelop.UnityMode
{
	static class UnityRestHelpers
	{
		internal static void OpenFile(string filename, int line, OpenDocumentOptions options = OpenDocumentOptions.None)
		{
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

		internal static void Paired(int unityProcessId, string unityRestServerUrl, string unityProject)
		{
			UnityInstance.ProcessId = unityProcessId;
			UnityInstance.RestServerUrl = unityRestServerUrl;
			UnityInstance.Project = unityProject;

			RestClient.SetServerUrl (UnityInstance.RestServerUrl);

			LoggingService.LogInfo("Received Pair request from Unity");
			UnityInstance.Log();

			UnityModeAddin.UnityProjectStateRefresh  ();
		}

		internal static void QuitApplication(string unityProject)
		{
			if (unityProject == UnityInstance.Project)
				DispatchService.GuiDispatch (() => IdeApp.Exit ());
			else
				LoggingService.LogInfo ("UnityMode: Could not Exit application because requested unityProject '"
					+ unityProject + "' does not match '" + UnityInstance.Project + "'");
		}

		internal static void SendOpenDocumentsToUnity()
		{
			var openDocuments = IdeApp.Workbench.Documents.Select (d => d.FileName.ToString ().Replace ('\\', '/')).ToList ();

			if (!openDocuments.SequenceEqual(UnityInstance.OpenDocuments))
			{
				UnityInstance.OpenDocuments = openDocuments;
				DispatchService.BackgroundDispatch (() => RestClient.SaveOpenDocuments (UnityInstance.OpenDocuments));
			}
		}
	}
}

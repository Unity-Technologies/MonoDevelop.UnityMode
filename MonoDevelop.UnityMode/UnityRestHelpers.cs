﻿using System;
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
		/// <summary>
		/// Handle Unity open file request.
		/// </summary>
		/// <param name="filename">Filename.</param>
		/// <param name="line">Line.</param>
		/// <param name="options">Options.</param>
		internal static void OpenFile(string filename, int line = 0, OpenDocumentOptions options = OpenDocumentOptions.None)
		{
			if (filename == null || !System.IO.File.Exists(filename))
				return;

			LoggingService.LogInfo ("OpenFile: " + filename + " Line " + line);

			var workbench = IdeApp.Workbench;

			if (workbench.ActiveDocument != null && workbench.ActiveDocument.FileName == filename)
				return;

			var fileOpenInformation = new FileOpenInformation (filename, null, line, 0, options);

			try
			{
				DispatchService.GuiDispatch(() =>
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

		/// <summary>
		/// Save changed settings in the Unity project's CodeEditorProjectSettings.json settings
		/// </summary>
		/// <param name="projectSettings">Project settings.</param>
		internal static void UpdateAndSaveProjectSettings(UnityProjectSettings projectSettings)
		{
			if (!RestClient.Available)
				return;

			DispatchService.GuiDispatch (() => {
				var documents = IdeApp.Workbench.Documents.Where(d => d.FileName != null).Select (d => d.FileName.ToString ().Replace ('\\', '/')).ToList ();
				var activeDocument = IdeApp.Workbench.ActiveDocument != null && IdeApp.Workbench.ActiveDocument.FileName != null ? IdeApp.Workbench.ActiveDocument.FileName.ToString().Replace('\\', '/') : null;
				var breakEvents = DebuggingService.Breakpoints.GetBreakevents ();
				var breakpoints = breakEvents.OfType<Breakpoint> ().Select (bp => new UnityProjectSettings.Breakpoint (bp.FileName, bp.Line, bp.Column, bp.Enabled)).ToList ();
				var functionBreakpoints = breakEvents.OfType<FunctionBreakpoint> ().Select (bp => new UnityProjectSettings.FunctionBreakpoint (bp.FunctionName, bp.Language, bp.Enabled)).ToList ();
				var exceptionBreaks = breakEvents.OfType<Catchpoint> ().Select (cp => new UnityProjectSettings.ExceptionBreak (cp.ExceptionName, cp.IncludeSubclasses, cp.Enabled)).ToList ();

				if (activeDocument != projectSettings.ActiveDocument ||
					!documents.SequenceEqual (projectSettings.Documents) ||
					!breakpoints.SequenceEqual (projectSettings.Breakpoints) ||
					functionBreakpoints.SequenceEqual (projectSettings.FunctionBreakpoints) ||
					!exceptionBreaks.SequenceEqual (projectSettings.ExceptionBreaks)) 
				{
					projectSettings.ActiveDocument = activeDocument;
					projectSettings.Documents = documents;
					projectSettings.Breakpoints = breakpoints;
					projectSettings.FunctionBreakpoints = functionBreakpoints;
					projectSettings.ExceptionBreaks = exceptionBreaks;
					DispatchService.ThreadDispatch (() => RestClient.SaveUnityProjectSettings (projectSettings));
				}
			});
		}

		/// <summary>
		/// Loads the and apply Unity project's CodeEditorProjectSettings.json settings
		/// </summary>
		/// <returns>The and apply project settings.</returns>
		internal static UnityProjectSettings LoadAndApplyProjectSettings()
		{
			var projectSettings =  RestClient.GetProjectSettings();
			var breakpointStore = DebuggingService.Breakpoints;

			DispatchService.GuiDispatch (() => {
				breakpointStore.Clear ();

				foreach (var breakpoint in projectSettings.Breakpoints) {
					var bp = new Breakpoint (breakpoint.Filename, breakpoint.Line, breakpoint.Column);
					bp.Enabled = breakpoint.Enabled;
					breakpointStore.Add (bp);
				}

				foreach (var functionbreakpoint in projectSettings.FunctionBreakpoints) {
					var bp = new FunctionBreakpoint (functionbreakpoint.Function, functionbreakpoint.Language);
					bp.Enabled = functionbreakpoint.Enabled;
					breakpointStore.Add (bp);
				}

				foreach (var exceptionBreak in projectSettings.ExceptionBreaks) {
					var bp = new Catchpoint (exceptionBreak.Exception, exceptionBreak.IncludeSubclasses);
					bp.Enabled = exceptionBreak.Enabled;
					breakpointStore.Add (bp);
				}
			});

			foreach(var document in projectSettings.Documents)
				OpenFile(document);

			OpenFile (projectSettings.ActiveDocument != null ? projectSettings.ActiveDocument : projectSettings.Documents.FirstOrDefault(), 0, OpenDocumentOptions.BringToFront);

			return projectSettings;
		}
	}
}


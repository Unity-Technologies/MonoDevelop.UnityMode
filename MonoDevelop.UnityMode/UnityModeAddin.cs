using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Codons;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Projects;
using MonoDevelop.Ide.Gui.Content;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.UnityMode.UnityRestClient;

namespace MonoDevelop.UnityMode
{
	public static class UnityModeAddin
	{
		public static UnitySolution UnitySolution { get; private set; }
		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;

		static RestService restService;
		static UnityProjectState unityProjectState = new UnityProjectState ();

		public static UnityProjectState UnityProjectState 
		{
			get { return unityProjectState; }
			set 
			{
				unityProjectState = value;

				if (UnityProjectStateChanged != null)
					UnityProjectStateChanged(null, new UnityProjectStateChangedEventArgs() { State = unityProjectState });
			}
		}

		internal static void InitializeRestServiceAndPair()
		{
			InitializeSolution ();

			restService = new RestService ( 
				fileOpenRequest => UnityRestHelpers.OpenFile(fileOpenRequest.File, fileOpenRequest.Line, OpenDocumentOptions.BringToFront),
				pairRequest => UnityRestHelpers.Paired(pairRequest.UnityProcessId, pairRequest.UnityRestServerUrl, pairRequest.UnityProject),
				quitRequest => UnityRestHelpers.QuitApplication(quitRequest.UnityProject)
			);

			Pair ();

			UnityProjectStateRefresh ();
		}

		static void InitializeSolution()
		{
			UnitySolution = new UnitySolution { Name = "UnitySolution" };

			// TODO: Should we close all other open solutions?
			UnityProjectStateChanged += (sender, e) =>
			{
				SolutionUpdater.Update(UnitySolution, e.State);

				if(!IdeApp.Workspace.Items.Contains(UnitySolution))
					IdeApp.Workspace.Items.Insert(0, UnitySolution);
			};
		}

		static void Pair()
		{
			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Pair request to Unity");
				var result = RestClient.Pair(restService.Url, BrandingService.ApplicationName + " " + BuildInfo.VersionLabel);
				LoggingService.LogInfo("Unity Pair Request Result: " + result.result);

				UnityInstance.ProcessId = result.unityprocessid;
				UnityInstance.Project = result.unityproject;

				UnityInstance.OpenDocuments = RestClient.GetOpenDocuments().documents;

				foreach(var document in UnityInstance.OpenDocuments)
					UnityRestHelpers.OpenFile(document, 0);

				UnityInstance.Log();
			});
		}

		public static void UnityProjectStateRefresh ()
		{
			if(UnityInstance.ProcessId > 0)
			{
				try
				{
					Process.GetProcessById(UnityInstance.ProcessId);
				}
				catch(Exception)
				{
					UnityInstance.ProcessId = -1;
					UnityInstance.RestServerUrl = null;

					RestClient.SetServerUrl (null);

					UnityProjectState = new UnityProjectState ();
					return;
				}
			}

			if (!RestClient.Available)
				return;

			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Unity Project request");
				UnityProjectState = RestClient.GetUnityProjectState();
			});
		}

		public static void UnityProjectStateRefreshRename (string oldPath, string newPath)
		{
			if (!RestClient.Available)
				return;

			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Unity Project request (rename)");
				var projectState = RestClient.GetUnityProjectState();

				projectState.RenameHint = new RenameHint {OldPath = oldPath, NewPath = newPath};

				UnityProjectState = projectState;
			});
		}
	}
}


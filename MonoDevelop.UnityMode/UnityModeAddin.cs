using System;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.UnityMode.UnityRestClient;
using MonoDevelop.UnityMode.ServiceModel;
using System.IO;

namespace MonoDevelop.UnityMode
{
	public static class UnityModeAddin
	{
		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;

		static RestService restService;
		static UnityProjectState unityProjectState;
		static UnitySolution unitySolution;

		static UnityModeAddin ()
		{
			restService = new RestService 
			(
				fileOpenRequest => UnityRestHelpers.OpenFile(fileOpenRequest.File, fileOpenRequest.Line, OpenDocumentOptions.BringToFront),
				quitRequest => UnityRestHelpers.QuitApplication(quitRequest.UnityProject)
			);

			UnityProjectStateChanged += (sender, e) => SolutionUpdater.Update (UnitySolution, e.State);
		}

		static UnitySolution UnitySolution 
		{
			get { return unitySolution; }
			set 
			{
				unitySolution = value;
				IdeApp.Workspace.Items.Clear ();
				IdeApp.Workspace.Items.Add (unitySolution);
			}
		}

		public static UnityInstance UnityInstance { get; private set; }

		public static UnityProjectState UnityProjectState 
		{
			get { return unityProjectState; }

			private set 
			{
				unityProjectState = value;

				if (UnityProjectStateChanged != null)
					UnityProjectStateChanged(null, new UnityProjectStateChangedEventArgs() { State = unityProjectState });
			}
		}

		internal static void OpenUnityProject(string projectPath)
		{
			var editorSettings = UnityRestServiceSettings.Load (projectPath);
			InitializeAndPair (editorSettings.EditorRestServiceUrl);
		}

		internal static void InitializeAndPair(string unityRestServiceUrl)
		{
			UnityInstance = new UnityInstance ();
			UnitySolution = new UnitySolution { Name = "UnitySolution" };
			UnityProjectState = new UnityProjectState ();

			// FIXME: Unable to connect to own IP, might be blocked by Mongoose in Unity.
			var editorRestServiecUri = new Uri(unityRestServiceUrl);
			Pair ("http://localhost:"+editorRestServiecUri.Port, restService.Url);
		}

		static void Pair(string unityRestServiceUrl, string monoDevelopRestServiceUrl)
		{
			UnityInstance.RestServiceUrl = unityRestServiceUrl;
			RestClient.SetServerUrl(unityRestServiceUrl);

			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Pair request to Unity");
				
				PairResult pairResult = null;

				try
				{
					pairResult = RestClient.Pair(monoDevelopRestServiceUrl, BrandingService.ApplicationName + " " + BuildInfo.VersionLabel);
					LoggingService.LogInfo("Unity Pair Request Result: " + pairResult.result);
				}
				catch(Exception e)
				{
					LoggingService.LogInfo("Unity Pair Request Exception: " + e);
					return;
				}

				UnityInstance.ProcessID = pairResult.unityprocessid;
				UnityInstance.ProjectPath = pairResult.unityproject;
				UnityInstance.OpenDocuments = RestClient.GetOpenDocuments().documents;

				foreach(var document in UnityInstance.OpenDocuments)
					UnityRestHelpers.OpenFile(document, 0);
				
				UnityProjectStateRefresh ();
			});
		}

		static void ShutdownAndUnpair()
		{
			UnityInstance = new UnityInstance ();
			UnitySolution = new UnitySolution { Name = "UnitySolution" };
			UnityProjectState = new UnityProjectState ();
			RestClient.SetServerUrl (null);
		}

		public static void UnityProjectStateRefresh ()
		{
			if (!UnityInstance.Paired)
				return;

			if (!UnityInstance.Running) 
			{
				ShutdownAndUnpair ();
				return;
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
			if (!UnityInstance.Paired)
				return;

			if (!UnityInstance.Running) 
			{
				ShutdownAndUnpair ();
				return;
			}

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


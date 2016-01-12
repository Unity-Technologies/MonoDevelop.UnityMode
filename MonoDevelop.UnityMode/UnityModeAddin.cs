using System;
using System.Diagnostics;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.UnityMode.UnityRestClient;
using MonoDevelop.UnityMode.ServiceModel;
using MonoDevelop.Projects;
using System.Linq;

namespace MonoDevelop.UnityMode
{
	public static class UnityModeAddin
	{
		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;
		public static event UnityAssetDatabaseChangedHandler UnityAssetDatabaseChanged;

		public delegate void UnityPairedHandler();
		public static event UnityPairedHandler UnityPaired;
		public static event UnityPairedHandler UnityUnpaired;

		static RestService restService;
		static UnityProjectState unityProjectState;
		static UnityAssetDatabase unityAssetDatabase;
		static UnitySolution unitySolution;
		static string savedUnityProjectPath;

		static UnityModeAddin ()
		{
			restService = new RestService 
			(
				fileOpenRequest => UnityRestHelpers.OpenFile(fileOpenRequest.File, fileOpenRequest.Line, OpenDocumentOptions.BringToFront)
			);

			UnityProjectStateChanged += (sender, e) => {
				if (UnitySolution != null && e.State != null)
					SolutionUpdater.Update (UnitySolution, e.State);
			};
	
			UnityProjectSettings = new UnityProjectSettings ();
			UnitySolution = new UnitySolution ();
			UnityProjectState = new UnityProjectState ();

			IdeApp.Workspace.WorkspaceItemUnloaded += OnWorkspaceItemUnloaded;
		}

		static void OnWorkspaceItemUnloaded (object s, WorkspaceItemEventArgs args)
		{
			if(args.Item == UnitySolution)
				ShutdownAndUnpair(true);
		}

		static UnityRestServiceSettings UnityRestServiceSettings { get; set; }

		static UnitySolution UnitySolution 
		{
			get { return unitySolution; }
			set 
			{
				if(unitySolution == value)
					return;

				if(unitySolution != null)
					IdeApp.Workspace.Items.Remove(unitySolution);
				else
					IdeApp.Workspace.Items.Clear ();

				unitySolution = value;

				unitySolution.SolutionItemAdded += (object sender, SolutionItemChangeEventArgs e) => 
				{
					var solution = e.Solution;
					var firstProject = solution.Items.FirstOrDefault();

					if(IdeApp.ProjectOperations.CurrentSelectedWorkspaceItem != solution || 
						(firstProject != null && IdeApp.ProjectOperations.CurrentSelectedSolutionItem != firstProject))
					{
						DispatchService.GuiDispatch(() => {
							if(!IdeApp.Workspace.Items.Contains(solution))
								IdeApp.Workspace.Items.Add (solution);
							
							IdeApp.ProjectOperations.CurrentSelectedWorkspaceItem = solution;
							IdeApp.ProjectOperations.CurrentSelectedSolutionItem = firstProject;
						});
					}
						
				};
			}
		}

		public static UnityProjectSettings UnityProjectSettings { get; private set; }

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

		public static UnityAssetDatabase UnityAssetDatabase
		{
			get { return unityAssetDatabase; }

			private set 
			{
				unityAssetDatabase = value;

				if (UnityAssetDatabaseChanged != null)
					UnityAssetDatabaseChanged(null, new UnityAssetDatabaseChangedEventArgs() { Database = unityAssetDatabase });
			}
		}

		public static void OpenUnityProject(string projectPath)
		{
			if(!System.IO.Directory.Exists(System.IO.Path.Combine(projectPath, "Assets")))
			{
				MessageService.GenericAlert(new GenericMessage("Not a Unity project directory. Assets folder not found.", projectPath));
				ShutdownAndUnpair();
				return;
			}

			var restServiceSettings = UnityRestServiceSettings.Load (projectPath);

			if(restServiceSettings == null)
			{
				MessageService.GenericAlert(new GenericMessage("Unable to load Unity project. The project must be open in Unity", projectPath));
				ShutdownAndUnpair();
			}
			else
				InitializeAndPair (restServiceSettings.EditorRestServiceUrl);
		}

		internal static void InitializeAndPair(string unityRestServiceUrl)
		{
			UnityAssetDatabase = new UnityAssetDatabase ();

			// FIXME: Unable to connect to own IP, might be blocked by Mongoose in Unity.
			var editorRestServiecUri = new Uri(unityRestServiceUrl);
			Pair ("http://localhost:"+editorRestServiecUri.Port, restService.Url);
		}

		static void Pair(string unityRestServiceUrl, string monoDevelopRestServiceUrl)
		{
			RestClient.SetServerUrl(unityRestServiceUrl);

			DispatchService.ThreadDispatch(() =>
			{
				LoggingService.LogInfo("Sending Pair request to Unity");

				PairResult pairResult = null;

				try
				{
					pairResult = RestClient.Pair(monoDevelopRestServiceUrl, BrandingService.ApplicationName + " " + BuildInfo.VersionLabel);
					LoggingService.LogInfo("Unity Pair response. Project:" + pairResult.unityproject + " Process ID: " + pairResult.unityprocessid);
				}
				catch(Exception e)
				{
					MessageService.GenericAlert(new GenericMessage("Unable to connect to Unity instance. Is Unity running?"));
					LoggingService.LogWarning("Unity Pair Request (" + unityRestServiceUrl + ")", e);
					ShutdownAndUnpair();
					return;
				}
				
				UnityRestServiceSettings = new UnityRestServiceSettings(unityRestServiceUrl, pairResult.unityprocessid);
				
				UnityProjectSettings = UnityRestHelpers.LoadAndApplyProjectSettings();
				UnityProjectSettings.ProjectPath = pairResult.unityproject;

				UnityProjectRefreshImmediate ();

				if(UnityPaired != null)
					UnityPaired();
			});
		}

		static void ShutdownAndUnpair(bool closeProject = false)
		{
			if(!RestClient.Available)
				return;

			LoggingService.LogInfo("Unpairing (" + UnityProjectSettings.ProjectPath + ")");

			savedUnityProjectPath = closeProject ? null : UnityProjectSettings.ProjectPath;

			UnityAssetDatabase = new UnityAssetDatabase ();
			UnityProjectSettings = new UnityProjectSettings ();

			if(closeProject)
			{
				UnitySolution = new UnitySolution ();
				UnityProjectState = new UnityProjectState ();

				if(UnityUnpaired != null)
					UnityUnpaired();
			}

			RestClient.SetServerUrl (null);
		}

		public static void UnityProjectRefresh (Hint hint = null)
		{
			DispatchService.ThreadDispatch( () => UnityProjectRefreshImmediate (hint));
		}

		static void UnityProjectRefreshImmediate (Hint hint = null)
		{
			if (!Paired && savedUnityProjectPath != null)
			{
				// Try to repair
				var restServiceSettings = UnityRestServiceSettings.Load (savedUnityProjectPath);

				if(restServiceSettings != null)
				{
					savedUnityProjectPath = null;
					InitializeAndPair(restServiceSettings.EditorRestServiceUrl);
					return;
				}
			}

			if(!Paired || !IsUnityRunning())
			{
				ShutdownAndUnpair();
				return;
			}

			LoggingService.LogInfo("Starting Unity Project refresh");

			LoggingService.LogInfo("Sending Unity AssetDatabase request");
			var assetDatabase = RestClient.GetUnityAssetDatabase();
			assetDatabase.Hint = hint;
			UnityAssetDatabase = assetDatabase;

			LoggingService.LogInfo("Sending Unity Project request");
			UnityProjectState = RestClient.GetUnityProjectState();
			LoggingService.LogInfo("Unity Project refresh done");
		}

		static bool Paired
		{
			get { return UnityRestServiceSettings != null && UnityRestServiceSettings.EditorProcessID > 0 && RestClient.Available; }
		}

		static bool IsUnityRunning()
		{
			try
			{
				var process = Process.GetProcessById(UnityRestServiceSettings.EditorProcessID);
				if(!process.HasExited)
					return true;
			}
			catch(Exception) {}

			return false;
		}
	}
}


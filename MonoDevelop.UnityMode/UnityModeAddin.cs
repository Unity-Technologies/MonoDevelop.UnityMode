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

		public static UnitySolution UnitySolution 
		{
			get { return unitySolution; }
			private set 
			{
				if(unitySolution == value)
					return;

				// If the existing Solution is in the MonoDevelop workspace, remove it.
				// Otherwise clear all solutions.
				if(unitySolution != null)
					IdeApp.Workspace.Items.Remove(unitySolution);
				else
					IdeApp.Workspace.Items.Clear ();

				unitySolution = value;

				// When a project is added to the UnitySolution via the SolutionUpdater,
				// make sure that a project is set as the current project in MonoDevelop.
				// A project must be the current project (e.g. selected) in MonoDevelop 
				// in order for project operations such as building and debugging to work.
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
					UnityAssetDatabaseChanged(null, new UnityAssetDatabaseChangedEventArgs() { Database = unityAssetDatabase, ProjectBaseDirectory = unityProjectState.BaseDirectory });
			}
		}

		/// <summary>
		/// Open Unity project from a file path.
		/// </summary>
		/// <param name="projectPath">Project path.</param>
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

		/// <summary>
		/// Connect to Unity Rest service and pair the MonoDevelop and Unity instance.
		/// By pairing Unity registers which MonoDevelop instance to open files in and
		/// MonoDevelop knows which Unity instance to talk to.
		/// </summary>
		/// <param name="unityRestServiceUrl">Unity rest service URL.</param>
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
					// Pair with Unity, send our own REST service URL for opening files in MonoDevelop.
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
				
				// Load the project specific settings stored in CodeEditorProjectSettings.json,
				// such breakpoints and open documents.
				UnityProjectSettings = UnityRestHelpers.LoadAndApplyProjectSettings();
				UnityProjectSettings.ProjectPath = pairResult.unityproject;

				UnityProjectRefreshImmediate ();

				if(UnityPaired != null)
					UnityPaired();
			});
		}

		static void ShutdownAndUnpair(bool closeProject = false)
		{
			if(!Paired)
				return;

			LoggingService.LogInfo("Unpairing (" + UnityProjectSettings.ProjectPath + ")");

			// Save the Unity project path in case Unity is shut down and MonoDevleop is still
			// running. If Unity is restarted with the same project again, we can automatically
			// repair with Unity and the asset folder pad will be updated.
			savedUnityProjectPath = closeProject ? null : UnityProjectSettings.ProjectPath;

			// Clear the assets database and thereby also the Unity Asset Folder pad.
			UnityAssetDatabase = new UnityAssetDatabase ();
			UnityProjectSettings = new UnityProjectSettings ();

			// We keep the MonoDevelop represtation of the solution open if we get disconnected
			// from Unity, this is required for code completion etc. to work on the open source files.
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

		/// <summary>
		/// Refresh MonoDevelop state from Unity REST service state.
		/// </summary>
		/// <param name="hint">
		/// Used for providing a hint about a change in the assetDatase. 
		/// For instance, instead of updating the entire state of the 
		/// Asset Folder pad when renaming a file and having to find
		/// the renamed file in the Asset Folder pad hierarhcy so it can
		/// be selected. We just send the rename to Unity and on success
		/// we just rename one file in our own locol representation of
		/// the Unity AssetDatabase. See FolderUpdater.Update.
		/// </param>
		static void UnityProjectRefreshImmediate (Hint hint = null)
		{
			// If we are no longer paired with Unity, check if Unity
			// has been restarted and the same project is open when MonoDevelop
			// gets window focus and if so, pair again with Unity.
			if (!Paired && savedUnityProjectPath != null)
			{
				// Try to pair again.
				var restServiceSettings = UnityRestServiceSettings.Load (savedUnityProjectPath);

				if(restServiceSettings != null)
				{
					savedUnityProjectPath = null;
					InitializeAndPair(restServiceSettings.EditorRestServiceUrl);
					return;
				}
			}

			// Unpair (clear the asset folder pad) if the connection to Unity is lost.
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


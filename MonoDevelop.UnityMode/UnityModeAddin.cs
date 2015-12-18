using System;
using System.Diagnostics;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.UnityMode.UnityRestClient;
using MonoDevelop.UnityMode.ServiceModel;

namespace MonoDevelop.UnityMode
{
	public static class UnityModeAddin
	{
		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;
		public static event UnityAssetDatabaseChangedHandler UnityAssetDatabaseChanged;

		static RestService restService;
		static UnityProjectState unityProjectState;
		static UnityAssetDatabase unityAssetDatabase;
		static UnitySolution unitySolution;
		static string savedUnityProjectPath;

		static UnityModeAddin ()
		{
			restService = new RestService 
			(
				fileOpenRequest => UnityRestHelpers.OpenFile(fileOpenRequest.File, fileOpenRequest.Line, OpenDocumentOptions.BringToFront),
				quitRequest => UnityRestHelpers.QuitApplication(quitRequest.UnityProject)
			);

			UnityProjectStateChanged += (sender, e) => {
				if (UnitySolution != null && e.State != null)
					SolutionUpdater.Update (UnitySolution, e.State);
			};
		}

		static UnityRestServiceSettings UnityRestServiceSettings { get; set; }

		static UnitySolution UnitySolution 
		{
			get { return unitySolution; }
			set 
			{
				unitySolution = value;
				IdeApp.Workspace.Items.Clear ();
				if(unitySolution != null)
					IdeApp.Workspace.Items.Add (unitySolution);
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
			var restServiceSettings = UnityRestServiceSettings.Load (projectPath);

			if(restServiceSettings == null)
			{
				MessageService.GenericAlert(new GenericMessage("Unable to load Unity project. The project must be open in Unity", projectPath));
				ShutdownAndUnpair();
			}
			else
				InitializeAndPair (restServiceSettings.EditorRestServiceUrl);
		}

		static void Reset()
		{
			UnityProjectSettings = new UnityProjectSettings ();
			UnitySolution = new UnitySolution { Name = "UnitySolution" };
			UnityProjectState = new UnityProjectState ();
			UnityAssetDatabase = new UnityAssetDatabase ();
		}

		internal static void InitializeAndPair(string unityRestServiceUrl)
		{
			Reset();
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
					LoggingService.LogInfo("Unity Pair Request Result: " + pairResult.result);
				}
				catch(Exception e)
				{
					MessageService.GenericAlert(new GenericMessage("Unable to connect to Unity instance. Is Unity running?"));
					LoggingService.LogInfo("Unity Pair Request (" + unityRestServiceUrl + ") Exception: " + e);
					ShutdownAndUnpair();
					return;
				}
				
				UnityRestServiceSettings = new UnityRestServiceSettings(unityRestServiceUrl, pairResult.unityprocessid);
				
				UnityProjectSettings = UnityRestHelpers.LoadAndApplyProjectSettings();
				UnityProjectSettings.ProjectPath = pairResult.unityproject;

				UnityProjectRefreshImmediate ();
			});
		}

		static void ShutdownAndUnpair()
		{
			if(!RestClient.Available)
				return;

			LoggingService.LogInfo("Unpairing (" + UnityProjectSettings.ProjectPath + ")");

			savedUnityProjectPath = UnityProjectSettings.ProjectPath;

			Reset();
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
			get { return UnityRestServiceSettings.EditorProcessID > 0 && RestClient.Available; }
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


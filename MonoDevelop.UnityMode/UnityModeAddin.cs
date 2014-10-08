using System;
using System.Diagnostics;
using MonoDevelop.Core;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Ide;
using MonoDevelop.UnityMode.UnityRestClient;

namespace MonoDevelop.UnityMode
{
	public static class UnityModeAddin
	{
		static UnityProjectState unityProjectState;
		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;

		static UnityModeAddin()
		{
			UnityProjectState = new UnityProjectState ();
		}

		public static void Initialize()
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

		public static void ClearUnityProjectState()
		{
			UnityProjectState = new UnityProjectState ();
			NotifyUnityProjectStateChanged ();
		}

		public static UnitySolution UnitySolution { get; private set; }

		public static void NotifyUnityProjectStateChanged()
		{
			if (UnityProjectStateChanged != null)
				UnityProjectStateChanged(null, new UnityProjectStateChangedEventArgs() { State = unityProjectState });
		}

		public static void UpdateUnityProjectState()
		{
			if(UnityModeSettings.UnityProcessId > 0)
			{
				try
				{
					Process.GetProcessById(UnityModeSettings.UnityProcessId);
				}
				catch(Exception)
				{
					UnityModeSettings.UnityProcessId = -1;
					UnityModeSettings.UnityRestServerUrl = null;

					RestClient.SetServerUrl (null);

					ClearUnityProjectState ();
					return;
				}
			}

			if (!RestClient.Available)
				return;

			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Unity Project request");
				UnityModeAddin.UnityProjectState = RestClient.GetUnityProjectState();
			});
		}

		public static void UpdateUnityProjectStateRename(string oldPath, string newPath)
		{
			if (!RestClient.Available)
				return;

			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Unity Project request (rename)");
				var projectState = RestClient.GetUnityProjectState();

				projectState.RenameHint = new RenameHint {OldPath = oldPath, newPath = newPath};

				UnityProjectState = projectState;
			});
		}

		public static UnityProjectState UnityProjectState 
		{
			get { return unityProjectState; }
			set {
				unityProjectState = value;
				NotifyUnityProjectStateChanged();
			}
		 }
	}

	public delegate void UnityProjectStateChangedHandler(object sender, UnityProjectStateChangedEventArgs e);

	public class UnityProjectStateChangedEventArgs : EventArgs
	{
		public UnityProjectState State;
	}

}


using System;
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


			UnityProjectStateChanged += (sender, e) =>
			{
				SolutionUpdater.Update(UnitySolution, e.State);

				if(!IdeApp.Workspace.Items.Contains(UnitySolution))
					IdeApp.Workspace.Items.Insert(0, UnitySolution);
			};
		}

		public static UnitySolution UnitySolution { get; private set; }

		public static void NotifyUnityProjectStateChanged()
		{
			if (UnityProjectStateChanged != null)
				UnityProjectStateChanged(null, new UnityProjectStateChangedEventArgs() { State = unityProjectState });
		}

		public static void UpdateUnityProjectState()
		{
			DispatchService.BackgroundDispatch(() =>
			{
				LoggingService.LogInfo("Sending Unity Project request");
				UnityModeAddin.UnityProjectState = RestClient.GetUnityProjectState();
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


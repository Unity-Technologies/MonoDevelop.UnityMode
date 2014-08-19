using System;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Ide;

namespace MonoDevelop.UnityMode
{
	public static class UnityModeAddin
	{
		static UnityModeAddin()
		{
			UnityProjectState = new UnityProjectState ();
		}

		public static void Initialize()
		{
			UnitySolution = new UnitySolution { Name = "UnitySolution" };
			IdeApp.Workspace.Items.Insert(0, UnitySolution);

			UnityProjectStateChanged += (sender, e) => SolutionUpdater.Update(UnitySolution, e.State);
		}

		public static UnitySolution UnitySolution { get; private set; }

		static UnityProjectState unityProjectState;

		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;

	
		public static UnityProjectState UnityProjectState 
		{
			get { return unityProjectState; }
			set {
				unityProjectState = value;
				if (UnityProjectStateChanged != null)
					UnityProjectStateChanged (null, new UnityProjectStateChangedEventArgs() { State = value });
			}
		 }
	}

	public delegate void UnityProjectStateChangedHandler(object sender, UnityProjectStateChangedEventArgs e);

	public class UnityProjectStateChangedEventArgs : EventArgs
	{
		public UnityProjectState State;
	}

}


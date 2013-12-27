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
			UnityProjectStateChanged += (sender, e) => new SolutionUpdater ().Update (UnitySolution, e.State);
		
			UnitySolution = new UnitySolution ();
			UnitySolution.Name = "UnitySolution";
			IdeApp.Workspace.Items.Insert (0, UnitySolution);
		}

		public static UnitySolution UnitySolution { get; private set; }

		static UnityProjectState _unityProjectState;

		public static event UnityProjectStateChangedHandler UnityProjectStateChanged;

	
		public static UnityProjectState UnityProjectState 
		{
			get { return _unityProjectState; }
			set {
				_unityProjectState = value;
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


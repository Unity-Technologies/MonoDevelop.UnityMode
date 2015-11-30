using System;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public delegate void UnityProjectStateChangedHandler(object sender, UnityProjectStateChangedEventArgs e);

	public class UnityProjectStateChangedEventArgs : EventArgs
	{
		public UnityProjectState State;
	}

}


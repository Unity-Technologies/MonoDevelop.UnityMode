using System;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public delegate void UnityAssetDatabaseChangedHandler(object sender, UnityAssetDatabaseChangedEventArgs e);

	public class UnityAssetDatabaseChangedEventArgs : EventArgs
	{
		public UnityAssetDatabase Database;
	}
}

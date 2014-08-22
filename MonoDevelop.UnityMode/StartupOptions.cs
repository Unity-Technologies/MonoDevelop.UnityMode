using System;

namespace MonoDevelop.UnityMode
{
	public static class StartupOptions
	{
		private static int unityProcessId;

		static StartupOptions()
		{
			UnityRestServerUrl = "http://localhost:38000";
		}

		public static int UnityProcessId
		{
			get { return unityProcessId; }
			set 
			{ 
				unityProcessId = value;
				Debugger.Soft.Unity.Util.UnityProcessId = value;
			}
		}

		public static string UnityRestServerUrl { get; set; }
	}
}


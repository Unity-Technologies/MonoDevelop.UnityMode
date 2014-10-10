using MonoDevelop.Core;
using System.Collections.Generic;

namespace MonoDevelop.UnityMode
{
	public static class UnityInstance
	{
		static UnityInstance()
		{
			ProcessId = -1;
			RestServerUrl = "http://localhost:38000";
			OpenDocuments = new List<string> ();
		}

		public static int ProcessId { get; set; }
		public static string RestServerUrl { get; set; }
		public static string Project { get; set; }
		public static List<string> OpenDocuments { get; set; }

		public static void Log()
		{
			LoggingService.LogInfo("Unity Process ID: " + ProcessId);
			LoggingService.LogInfo("Unity Server Url: " + RestServerUrl);
			LoggingService.LogInfo("Unity Project: " +  Project);
		}
	}
}


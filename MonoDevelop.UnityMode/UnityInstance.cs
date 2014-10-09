using MonoDevelop.Core;

namespace MonoDevelop.UnityMode
{
	public static class UnityInstance
	{
		static UnityInstance()
		{
			ProcessId = -1;
			RestServerUrl = "http://localhost:38000";
		}

		public static int ProcessId { get; set; }
		public static string RestServerUrl { get; set; }
		public static string Project { get; set; }

		public static void Log()
		{
			LoggingService.LogInfo("Unity Process ID: " + ProcessId);
			LoggingService.LogInfo("Unity Server Url: " + RestServerUrl);
			LoggingService.LogInfo("Unity Project: " +  Project);
		}
	}
}


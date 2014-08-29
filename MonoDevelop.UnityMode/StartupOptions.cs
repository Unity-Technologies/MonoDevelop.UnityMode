
namespace MonoDevelop.UnityMode
{
	public static class StartupOptions
	{
		static StartupOptions()
		{
			UnityRestServerUrl = "http://localhost:38000";
		}

		public static int UnityProcessId { get; set; }
		public static string UnityRestServerUrl { get; set; }
	}
}


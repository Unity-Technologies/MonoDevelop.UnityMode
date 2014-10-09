
namespace MonoDevelop.UnityMode
{
	public static class UnityModeSettings
	{
		static UnityModeSettings()
		{
			UnityProcessId = -1;
			UnityRestServerUrl = "http://localhost:38000";
		}

		public static int UnityProcessId { get; set; }
		public static string UnityRestServerUrl { get; set; }
		public static string UnityProject { get; set; }
	}
}


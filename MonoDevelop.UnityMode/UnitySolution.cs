using MonoDevelop.Projects;
using MonoDevelop.UnityMode.UnityRestClient;

namespace MonoDevelop.UnityMode
{
	//this class relays all build commands on projects, to the main build command on the solution.
	public class UnitySolution : Solution
	{
		public UnitySolution Singleton;

		public UnitySolution()
		{
			Singleton = this;
			var config = new UnitySolutionConfiguration ();
			config.Id = "UnitySolutionConfiguration";
			Configurations.Add (config);
			DefaultConfiguration = config;
		}

		public override SolutionConfiguration GetConfiguration (ConfigurationSelector configuration)
		{
			return DefaultConfiguration;
		}

		protected override BuildResult OnBuild (MonoDevelop.Core.IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			var restResult = RestClient2.CompileScripts ();
			var result = new BuildResult ();

			foreach (var message in restResult.Messages)
				result.AddError(message.File, message.Line, message.Column, "", message.Message);
			return result;
		}
	}

	public class UnitySolutionConfiguration : SolutionConfiguration
	{

	}
}

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
			var rest_result = RestClient2.CompileScripts ();
			var result = new BuildResult ();

			foreach (var item in rest_result.Output)
				result.AddError (item.File, item.Line, 0, "", item.LogString);
			return result;
		}
	}

	public class UnitySolutionConfiguration : SolutionConfiguration
	{

	}
}

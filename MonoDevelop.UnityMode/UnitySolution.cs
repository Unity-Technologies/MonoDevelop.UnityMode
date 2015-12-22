using MonoDevelop.Projects;
using MonoDevelop.UnityMode.UnityRestClient;
using System.IO;

namespace MonoDevelop.UnityMode
{
	//this class relays all build commands on projects, to the main build command on the solution.
	public class UnitySolution : Solution
	{
		public UnitySolution Singleton;

		public UnitySolution()
		{
			Singleton = this;

			string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(tempDirectory);
			Name = "UnitySolution";
			FileName = Path.Combine(tempDirectory, "UnitySolution.sln");

			var config = new UnitySolutionConfiguration {Id = "Unity"};
			Configurations.Add (config);
			DefaultConfiguration = config;
		}

		public override SolutionConfiguration GetConfiguration (ConfigurationSelector configuration)
		{
			return DefaultConfiguration;
		}

		protected override BuildResult OnBuild (MonoDevelop.Core.IProgressMonitor monitor, ConfigurationSelector configuration)
		{
			var result = new BuildResult ();

			if (!RestClient.Available) 
			{
				result.AddError ("Not connected to Unity instance");
				return result;
			}

			var restResult = RestClient.CompileScripts ();

			foreach (var message in restResult.Messages)
			{
				var file = BaseDirectory + "/" + message.File;
				var msg = message.Message;
				var errorNum = "";
				
				var messageStrings = message.Message.Split(':');

				if (messageStrings.Length == 3)
				{
					var errorNumStrings = messageStrings[1].Split(' ');

					if (errorNumStrings.Length > 1)
						errorNum = errorNumStrings[errorNumStrings.Length - 1];

					msg = messageStrings[2];
				}

				if(message.Type == "warning")
					result.AddWarning(file, message.Line, message.Column, errorNum, msg);
				else
					result.AddError(file, message.Line, message.Column, errorNum, msg);
			}
			
			return result;
		}
	}

	public class UnitySolutionConfiguration : SolutionConfiguration
	{

	}
}

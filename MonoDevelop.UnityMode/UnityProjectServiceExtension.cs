using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.UnityMode
{
	//this class relays all build commands on projects, to the main build command on the solution.
	public class UnityProjectServiceExtension : ProjectServiceExtension
	{
		public override BuildResult RunTarget (IProgressMonitor monitor, IBuildTarget item, string target, ConfigurationSelector configuration)
		{
			var solutionItem = item as SolutionItem;
			if (solutionItem == null)
				return base.RunTarget (monitor, item, target, configuration);

			return solutionItem.ParentSolution.Build (monitor, configuration);
		}
	}
}


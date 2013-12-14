using System;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Core;
using System.Linq;

namespace MonoDevelop.UnityMode
{
	public class ProjectUpdater
	{
		public void Update (DotNetAssemblyProject project, ProjectUpdate update)
		{
			var toRemove = project.Files.Where (f => !update.Files.Any (f2 => f.FilePath.ToString() == f2)).ToArray();
			var toAdd = update.Files.Where(f => !project.Files.Any(f2=>f2.FilePath.ToString() == f)).ToArray();

			project.Files.RemoveRange (toRemove);
			project.AddFiles (toAdd.Select(f => new FilePath(f)));
		}
	}
}


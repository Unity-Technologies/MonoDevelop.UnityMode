using System;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Core;
using System.Linq;

namespace MonoDevelop.UnityMode
{
	public class ProjectUpdater
	{
		public void Update (DotNetAssemblyProject oldProject, ProjectUpdate update)
		{
			oldProject.AddFiles (update.Files.Select(s => new FilePath(s)));
		}
	}
}


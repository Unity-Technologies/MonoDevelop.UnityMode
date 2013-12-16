using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Linq;
using MonoDevelop.CSharp.Project;

namespace MonoDevelop.UnityMode
{
	public class SolutionUpdater
	{
		public void Update (UnitySolution s, SolutionUpdate update)
		{
			var existingProjects = s.GetAllProjects ();

			var toRemoves = existingProjects.Where (p => !update.Projects.Any (p2 => p.Name == p2.Name)).ToArray ();
			foreach (var toRemove in toRemoves)
				s.RootFolder.Items.Remove (toRemove);

			foreach (var projectUpdate in update.Projects)
			{
				var existing = existingProjects.OfType<DotNetAssemblyProject>().SingleOrDefault (p => p.Name == projectUpdate.Name);
				if (existing == null)
					existing = CreateMonoDevelopProjectFromProjectUpdate (s, projectUpdate);

				new ProjectUpdater ().Update (existing, projectUpdate);
			}
		}

		DotNetAssemblyProject CreateMonoDevelopProjectFromProjectUpdate (UnitySolution solution, ProjectUpdate projectUpdate)
		{
			var p = new DotNetAssemblyProject (projectUpdate.Language);

			switch (projectUpdate.Language)
			{
				case "C#":
					var dotNetProjectConfig = (DotNetProjectConfiguration)p.AddNewConfiguration ("Debug");
					p.DefaultConfiguration = dotNetProjectConfig;
					break;
			}
		
			solution.RootFolder.AddItem (p);
			solution.DefaultConfiguration.AddItem (p).Build = true;
			return p;
		}
	}
}

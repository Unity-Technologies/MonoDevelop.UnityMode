using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Linq;
using MonoDevelop.CSharp.Project;

namespace MonoDevelop.UnityMode
{
	public class SolutionUpdater
	{
		public static void Update (UnitySolution s, UnityProjectState update)
		{
			var existingProjects = s.GetAllProjects ();

			var toRemoves = existingProjects.Where (p => update.Islands.All(p2 => p.Name != p2.Name)).ToArray ();
			foreach (var toRemove in toRemoves)
				s.RootFolder.Items.Remove (toRemove);

			foreach (var projectUpdate in update.Islands.Where(i => i.Language == "C#"))
			{
				var existing = existingProjects.OfType<DotNetAssemblyProject>().SingleOrDefault (p => p.Name == projectUpdate.Name);
				if (existing == null)
					existing = CreateMonoDevelopProjectFromProjectUpdate (s, projectUpdate);

				ProjectUpdater.Update (existing, projectUpdate);
			}

			s.BaseDirectory = update.BaseDirectory;
		}

		static DotNetAssemblyProject CreateMonoDevelopProjectFromProjectUpdate (UnitySolution solution, MonoIsland projectUpdate)
		{
			var p = new DotNetAssemblyProject (projectUpdate.Language);

			switch (projectUpdate.Language)
			{
				case "C#":
					var dotNetProjectConfig = (DotNetProjectConfiguration)p.AddNewConfiguration ("Debug");
					dotNetProjectConfig.CompilationParameters = new CSharpCompilerParameters();
					p.DefaultConfiguration = dotNetProjectConfig;
					break;
			}
		
			var rootFolder = solution.RootFolder;
			rootFolder.AddItem (p);
			solution.DefaultConfiguration.AddItem (p).Build = true;
			return p;
		}
	}
}

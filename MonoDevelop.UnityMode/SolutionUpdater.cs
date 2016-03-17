using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Linq;
using MonoDevelop.CSharp.Project;
using MonoDevelop.Core;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.UnityMode
{
	/// <summary>
	/// Solution updater.
	/// Updates the Unity specific MonoDevelop representation of a Solution (UnitySolution)
	/// from the UnityProjectState received from the Unity REST service.
	/// </summary>
	public class SolutionUpdater
	{
		public static void Update (UnitySolution s, UnityProjectState update)
		{
			var existingProjects = s.GetAllProjects ();

			var toRemoves = existingProjects.Where (p => update.Islands.All(p2 => p.Name != p2.Name)).ToArray ();
			foreach (var toRemove in toRemoves)
				s.RootFolder.Items.Remove (toRemove);

			foreach (var projectUpdate in update.Islands.Where(i => i.Files.Count > 0))
			{
				var existing = existingProjects.OfType<DotNetAssemblyProject>().SingleOrDefault (p => p.Name == projectUpdate.Name);
				if (existing == null)
					existing = CreateMonoDevelopProjectFromProjectUpdate (s, projectUpdate, FrameworkVersionToMoniker(update.Framework));

				ProjectUpdater.Update (existing, projectUpdate);
			}

			s.BaseDirectory = update.BaseDirectory;
		}

		static DotNetAssemblyProject CreateMonoDevelopProjectFromProjectUpdate (UnitySolution solution, MonoIsland projectUpdate, TargetFrameworkMoniker moniker)
		{
			var p = new DotNetAssemblyProject (projectUpdate.Language);

			p.TargetFramework = Runtime.SystemAssemblyService.GetTargetFramework (moniker);

			// FIXME
			switch (projectUpdate.Language)
			{
				default: 
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

		static TargetFrameworkMoniker FrameworkVersionToMoniker(string version)
		{
			if (version == "3.5")
				return TargetFrameworkMoniker.NET_3_5;

			throw new System.ArgumentException ("Unsupported framework version " + version);
		}
	}
}

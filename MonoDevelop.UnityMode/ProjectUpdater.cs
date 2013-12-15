using System;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Core;
using System.Linq;
using MonoDevelop.CSharp.Project;

namespace MonoDevelop.UnityMode
{
	public class ProjectUpdater
	{
		public void Update (DotNetAssemblyProject project, ProjectUpdate update)
		{
			ProcessFiles (project, update);

			var compilationParameters = (CSharpCompilerParameters)((DotNetProjectConfiguration)project.DefaultConfiguration).CompilationParameters;
			var toAdd = update.Defines.Where (d => !compilationParameters.HasDefineSymbol (d)).ToArray ();
			var toRemove = compilationParameters.AllDefineSymbols.Where (d => !update.Defines.Contains (d)).ToArray();

			foreach (var define in toAdd)
				compilationParameters.AddDefineSymbol (define);
			foreach (var define in toRemove)
				compilationParameters.RemoveDefineSymbol (define);
		}

		static void ProcessFiles (DotNetAssemblyProject project, ProjectUpdate update)
		{
			var toRemove = project.Files.Where (f => !update.Files.Any (f2 => f.FilePath.ToString () == f2)).ToArray ();
			var toAdd = update.Files.Where (f => !project.Files.Any (f2 => f2.FilePath.ToString () == f)).ToArray ();
			project.Files.RemoveRange (toRemove);
			project.AddFiles (toAdd.Select (f => new FilePath (f)));
		}
	}
}


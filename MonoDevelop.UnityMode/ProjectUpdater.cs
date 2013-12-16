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
			if (update.BaseDirectory != project.BaseDirectory)
				project.BaseDirectory = update.BaseDirectory;

			ProcessFiles (project, update);
			ProcessDefines (project, update);
			ProcessReferences (project, update);
			project.Name = update.Name;

			var dotNetProjectConfiguration = ((DotNetProjectConfiguration)project.DefaultConfiguration);
			dotNetProjectConfiguration.OutputAssembly = project.Name + ".dll";
		}

		static void ProcessReferences (DotNetAssemblyProject project, ProjectUpdate update)
		{
			var referencesToAdd = update.References.Where (r => !project.References.Any (r2 => r2.Reference == r)).ToArray ();
			foreach (var reference in referencesToAdd)
				project.References.Add (ProjectReferenceFor (reference));

			var referencesToRemove = project.References.Where (r => !update.References.Any (r2 => r.Reference == r2)).ToArray ();
			project.References.RemoveRange (referencesToRemove);
		}

		static ProjectReference ProjectReferenceFor (string reference)
		{
			return new ProjectReference (IsAssemblyReference(reference) ? ReferenceType.Assembly : ReferenceType.Project, reference);
		}

		static bool IsAssemblyReference(string reference)
		{
			return System.IO.Path.GetExtension (reference).ToLower () == ".dll";
		}

		static void ProcessDefines (DotNetAssemblyProject project, ProjectUpdate update)
		{
			var compilationParameters = (CSharpCompilerParameters)((DotNetProjectConfiguration)project.DefaultConfiguration).CompilationParameters;
			var toAdd = update.Defines.Where (d => !compilationParameters.HasDefineSymbol (d)).ToArray ();
			var toRemove = compilationParameters.AllDefineSymbols.Where (d => !update.Defines.Contains (d)).ToArray ();
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


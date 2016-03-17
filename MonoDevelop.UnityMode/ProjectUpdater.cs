using System;
using System.IO;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using MonoDevelop.Core;
using System.Linq;
using MonoDevelop.CSharp.Project;

namespace MonoDevelop.UnityMode
{
	/// <summary>
	/// Project updater.
	/// Updates the MonoDevelop representations of a project (DotNetAssemblyProject) from 
	/// the MonoIsland received from the Unity REST service.
	/// </summary>
	public class ProjectUpdater
	{
		public static void Update (DotNetAssemblyProject project, MonoIsland update)
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

		static void ProcessReferences (DotNetAssemblyProject project, MonoIsland update)
		{
			var updateReferences = update.References;

			updateReferences.Add ("System");
			updateReferences.Add ("System.Core");
			updateReferences.Add ("System.Runtime.Serialization");
			updateReferences.Add ("System.Xml.Linq");
			updateReferences.Sort ();

			var referencesToAdd = updateReferences.Where (r => project.References.All(r2 => r2.Reference != r)).ToArray ();
			foreach (var reference in referencesToAdd)
				project.References.Add (ProjectReferenceFor (reference));

			var referencesToRemove = project.References.Where (r => updateReferences.All(r2 => r.Reference != r2)).ToArray ();
			project.References.RemoveRange (referencesToRemove);
		}

		static ProjectReference ProjectReferenceFor (string reference)
		{
			return new ProjectReference (IsAssemblyReference(reference) ? ReferenceType.Assembly : ReferenceType.Project, reference, reference);
		}

		static bool IsAssemblyReference(string reference)
		{
			return System.IO.Path.GetExtension (reference).ToLower () == ".dll";
		}

		static void ProcessDefines (DotNetAssemblyProject project, MonoIsland update)
		{
			var compilationParameters = (CSharpCompilerParameters)((DotNetProjectConfiguration)project.DefaultConfiguration).CompilationParameters;
			var toAdd = update.Defines.Where (d => !compilationParameters.HasDefineSymbol (d)).ToArray ();
			var toRemove = compilationParameters.GetDefineSymbols().Where (d => !update.Defines.Contains (d)).ToArray ();
			foreach (var define in toAdd)
				compilationParameters.AddDefineSymbol (define);
			foreach (var define in toRemove)
				compilationParameters.RemoveDefineSymbol (define);
		}

		static void ProcessFiles (DotNetAssemblyProject project, MonoIsland update)
		{
			var updateFiles = update.Files.Select (f => Path.GetFullPath(project.BaseDirectory + "/" + f)).ToArray();

			var toRemove = project.Files.Where (f => updateFiles.All(f2 => f.FilePath.FullPath != f2)).ToArray ();
			var toAdd = updateFiles.Where(f => project.Files.All(f2 => f2.FilePath.FullPath != f)).ToArray();

			if(toRemove.Length > 0)
				project.Files.RemoveRange (toRemove);

			if(toAdd.Length > 0)
				project.AddFiles (toAdd.Select (f => new FilePath (f)));
		}

	}
}


using NUnit.Framework;
using System;
using MonoDevelop.UnityMode;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Collections.Generic;

namespace MonoDevelop.UnityMode.Tests
{
	[TestFixture ()]
	public class ProjectUpdaterTests
	{
		[Test ()]
		public void UpdateWithNewFileGetsAdded ()
		{
			var oldProject = new DotNetAssemblyProject("C#");

			var update = new ProjectUpdate () { Files = new List<string> { "file.cs" } };
			var projectUpdater = new ProjectUpdater ();
			projectUpdater.Update (oldProject, update);

			CollectionAssert.AreEquivalent (new[] { "file.cs" }, oldProject.Files);
		}
	}
}


using NUnit.Framework;
using System;
using MonoDevelop.UnityMode;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Collections.Generic;
using System.Linq;

namespace MonoDevelop.UnityMode.Tests
{
	[TestFixture ()]
	public class ProjectUpdaterTests : UnitTests.TestBase
	{
		DotNetAssemblyProject _project;
		ProjectUpdater _projectUpdater;

		[SetUp]
		public void TestSetup()
		{
			_project = new DotNetAssemblyProject ("C#");
			_projectUpdater = new ProjectUpdater ();
		}

		[Test ()]
		public void UpdateWithNewFileGetsAdded ()
		{
			var update = new ProjectUpdate () { Files = new List<string> { "/file.cs" } };
			_projectUpdater.Update (_project, update);
			AssertProjectFilesEquals (new[] {"/file.cs"});
		}

		[Test ()]
		public void UpdateWithoutFileGetsRemoved ()
		{
			var update = new ProjectUpdate () { Files = new List<string> {} };
			_project.AddFile ("somefile");
			_projectUpdater.Update (_project, update);
			AssertProjectFilesEquals (new string[0] );
		}

		void AssertProjectFilesEquals (string[] expected)
		{
			CollectionAssert.AreEqual (expected, _project.Files.Select (p => p.FilePath.ToString ()).ToArray ());
		}
	}
}


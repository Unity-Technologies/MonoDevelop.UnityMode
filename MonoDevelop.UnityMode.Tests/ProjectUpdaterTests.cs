using NUnit.Framework;
using System;
using MonoDevelop.UnityMode;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.CSharp.Project;

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
			var p = new CSharpCompilerParameters ();
			var config = new DotNetProjectConfiguration () { CompilationParameters = p };
			_project.DefaultConfiguration = config;

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

		[Test ()]
		public void UpdateWithNewDefinesGetsAdopted ()
		{
			var update = new ProjectUpdate () { Files = new List<string> {}, Defines = new List<string>() { "UNITY_EDITOR" }};
			_projectUpdater.Update (_project, update);

			var config = (DotNetProjectConfiguration)_project.DefaultConfiguration;
			Assert.IsTrue (config.CompilationParameters.HasDefineSymbol ("UNITY_EDITOR"));
		}

		void AssertProjectFilesEquals (string[] expected)
		{
			CollectionAssert.AreEqual (expected, _project.Files.Select (p => p.FilePath.ToString ()).ToArray ());
		}
	}
}


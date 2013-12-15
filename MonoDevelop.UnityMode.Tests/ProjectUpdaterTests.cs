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
	[TestFixture]
	public class ProjectUpdaterTests : UnitTests.TestBase
	{
		DotNetAssemblyProject _project;
		ProjectUpdater _projectUpdater;
		ProjectUpdate _update;

		[SetUp]
		public void TestSetup()
		{
			_project = new DotNetAssemblyProject ("C#");
			var p = new CSharpCompilerParameters ();
			var config = new DotNetProjectConfiguration () { CompilationParameters = p };
			_project.DefaultConfiguration = config;

			_projectUpdater = new ProjectUpdater ();
			_update = new ProjectUpdate ();
		}

		[Test]
		public void UpdateWithNewFileGetsAdded ()
		{
			_update.Files.Add ("/file.cs");
			DoUpdate ();
			AssertProjectFilesEquals (new[] {"/file.cs"});
		}

		[Test]
		public void UpdateWithoutFileGetsRemoved ()
		{
			_project.AddFile ("somefile");
			DoUpdate ();
			AssertProjectFilesEquals (new string[0] );
		}

		[Test]
		public void UpdateWithNewDefinesGetsAdopted ()
		{
			_update.Defines.Add ("UNITY_EDITOR");
			DoUpdate ();
			Assert.IsTrue (CompilationParameters.HasDefineSymbol ("UNITY_EDITOR"));
		}

		[Test]
		public void UpdateWithoutDefinesRemovesExistingOnes ()
		{
			CompilationParameters.AddDefineSymbol ("UNITY_EDITOR");
			DoUpdate ();
			Assert.IsFalse (CompilationParameters.HasDefineSymbol ("UNITY_EDITOR"));
		}

		void AssertProjectFilesEquals (string[] expected)
		{
			CollectionAssert.AreEqual (expected, _project.Files.Select (p => p.FilePath.ToString ()).ToArray ());
		}

		ConfigurationParameters CompilationParameters {
			get {
				var config = (DotNetProjectConfiguration)_project.DefaultConfiguration;
				return config.CompilationParameters;
			}
		}

		void DoUpdate ()
		{
			_projectUpdater.Update (_project, _update);
		}
	}
}


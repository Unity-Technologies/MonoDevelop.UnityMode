using System.IO;
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
		MonoIsland _update;

		[SetUp]
		public void TestSetup()
		{
			_project = new DotNetAssemblyProject ("C#");
			var p = new CSharpCompilerParameters ();
			var config = new DotNetProjectConfiguration () { CompilationParameters = p };
			_project.DefaultConfiguration = config;

			_update = new MonoIsland ();
			_update.BaseDirectory = "/mybase";
		}

		[Test]
		public void UpdateWithNewFileGetsAdded ()
		{
			_update.Files.Add ("file.cs");
			DoUpdate ();
			AssertProjectFilesEquals (new[] {"/mybase/file.cs"});
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

		[Test]
		public void UpdateWithNewReferenceGetsAdopted ()
		{
			var reference = "someassembly.dll";
			_update.References.Add (reference);
			DoUpdate ();
			Assert.IsNotNull (_project.References.SingleOrDefault(r => r.Reference == reference && r.ReferenceType == ReferenceType.Assembly));
		}

		[Test]
		public void UpdateWithoutReferenceRemovesExisting()
		{
			_project.References.Add (new ProjectReference (ReferenceType.Assembly, "myassembly.dll"));
			DoUpdate ();
			Assert.IsEmpty (_project.References);
		}

		[Test]
		public void UpdateWithReferenceThatAlreadyExistDoesNotCreateDuplicate()
		{
			var reference = "myassembly.dll";
			_project.References.Add (new ProjectReference (ReferenceType.Assembly, reference));
			_update.References.Add (reference);
			DoUpdate ();
			Assert.IsNotNull (_project.References.SingleOrDefault(r => r.Reference == reference));
		}

		void AssertProjectFilesEquals (string[] expected)
		{
			CollectionAssert.AreEqual (expected.Select(Path.GetFullPath).ToArray(), _project.Files.Select (p => p.FilePath.ToString ()).ToArray ());
		}

		ConfigurationParameters CompilationParameters {
			get {
				var config = (DotNetProjectConfiguration)_project.DefaultConfiguration;
				return config.CompilationParameters;
			}
		}

		void DoUpdate ()
		{
			ProjectUpdater.Update (_project, _update);
		}
	}
}


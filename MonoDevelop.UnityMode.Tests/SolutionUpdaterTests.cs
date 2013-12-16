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
	public class SolutionUpdaterTests : UnitTests.TestBase
	{
		UnitySolution _solution;
		SolutionUpdater _solutionUpdater;
		SolutionUpdate _update;

		[SetUp]
		public void TestSetup()
		{
			_solution = new UnitySolution ();
			_solutionUpdater = new SolutionUpdater ();
			_update = new SolutionUpdate ();
		}

		[Test]
		public void UpdateWithNewProjectGetsAdded ()
		{
			_update.Projects.Add (new ProjectUpdate () { Name = "p1", Language = "C#" });
			DoUpdate ();

			var projects = _solution.GetAllProjects ();
			Assert.AreEqual (1, projects.Count);
			Assert.AreEqual ("p1", projects [0].Name);
		}

		[Test]
		public void UpdateWithoutExistingProjectGetsRemoved ()
		{
			_solution.RootFolder.AddItem (new DotNetAssemblyProject ("C#") { Name = "hello" });
			DoUpdate ();

			Assert.IsEmpty (_solution.GetAllProjects ());
		}

		[Test]
		public void UpdateWithProjectThatReferencesOtherProject()
		{
			var project1 = new ProjectUpdate () { Name = "p1", Language = "C#" };
			_update.Projects.Add (project1);

			var project2 = new ProjectUpdate () { Name = "p2", Language = "C#", References = new List<string> { "p1" } };
			_update.Projects.Add (project2);

			DoUpdate ();

			Assert.AreEqual (2, MDProjects.Length);

			var p1 = MDProjects.Single (p => p.Name == "p1");
			var p2 = MDProjects.Single (p => p.Name == "p2");

			//assertion in form of .Single();
			p2.References.Single (pr => pr.ReferenceType == ReferenceType.Project && pr.Reference == p1.Name);
		}

		DotNetAssemblyProject[] MDProjects
		{
			get { return _solution.GetAllProjects ().OfType<DotNetAssemblyProject> ().ToArray (); }
		}

		void DoUpdate ()
		{
			_solutionUpdater.Update (_solution, _update);
		}
	}
}


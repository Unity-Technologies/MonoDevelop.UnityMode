using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public class StartupHandler : CommandHandler
	{
		protected override void Run ()
		{
			InitializeUnitySolution ();
		}

		//if (Environment.GetCommandLineArgs ().Contains ("--unityMode"))
		static void InitializeUnitySolution()
		{
			var project1 = new DotNetAssemblyProject ("C#");
			project1.Name = "FirstPassC#";

			var project2 = new DotNetAssemblyProject ("C#");
			project2.Name = "SecondPassC#";

			project2.References.Add (new ProjectReference (project1));

			project1.AddReference ("/Users/lucas/unity/build/MacEditor/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll");
			project2.AddReference ("/Users/lucas/unity/build/MacEditor/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll");

			var s = new UnitySolution ();
			s.Name = "UnitySolution";
			s.BaseDirectory = "/Users/lucas/Projects/md1/Assets";
			s.RootFolder.AddItem (project1);
			s.RootFolder.AddItem (project2);
			s.AddConfiguration ("Debug", true);
			IdeApp.Workspace.Items.Insert (0, s);

			var updater = new SolutionUpdater ();

			new RestService (updater.ProcessIncomingUpdate);
		}
	}

	public class SolutionUpdater
	{
		public void ProcessIncomingUpdate(SolutionUpdate update)
		{
		}
	}
}

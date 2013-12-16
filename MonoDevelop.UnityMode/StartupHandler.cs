using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;
using System.Diagnostics;
using MonoDevelop.Ide.Gui;
using MonoDevelop.UnityMode.UnityRestClient;
using System.Linq;

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
			//var project1 = new DotNetAssemblyProject ("C#");
			//project1.Name = "FirstPassC#";
			/*
			var project2 = new DotNetAssemblyProject ("C#");
			project2.Name = "SecondPassC#";

			project2.References.Add (new ProjectReference (project1));

			project1.AddReference ("/Users/lucas/unity/build/MacEditor/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll");
			project2.AddReference ("/Users/lucas/unity/build/MacEditor/Unity.app/Contents/Frameworks/Managed/UnityEngine.dll");
*/
			var solution = new UnitySolution ();
			solution.Name = "UnitySolution";


			//solution.RootFolder.AddItem (project1);
			//			s.RootFolder.AddItem (project2);

			IdeApp.Workspace.Items.Insert (0, solution);

			var updater = new SolutionUpdater ();
			var update = new SolutionUpdate ();
			updater.Update (solution, update);

			new RestService (solutionUpdate => {
				updater.Update (solution, solutionUpdate);
				solution.BaseDirectory = solutionUpdate.BaseDirectory;
			}, fileOpenRequest => {
				var fileOpenInformation = new FileOpenInformation (fileOpenRequest.File, solution.GetAllProjects()[0], fileOpenRequest.Line, 0, OpenDocumentOptions.BringToFront);
				IdeApp.Workbench.OpenDocument (fileOpenInformation);
				DispatchService.GuiDispatch (IdeApp.Workbench.GrabDesktopFocus);
			}
			);

			DispatchService.BackgroundDispatch (() => {
				LoggingService.Log (MonoDevelop.Core.Logging.LogLevel.Info, "SendingInfoRequest");
				RestClient2.SendSolutionInformationRequest ();
			});
		}
	}
}

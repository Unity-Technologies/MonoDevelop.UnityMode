using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Debugger;
using Mono.Debugging.Client;
using MonoDevelop.UnityMode;
using MonoDevelop.Debugger.Soft.Unity;
using System.Linq;

namespace MonoDevelop.UnityMode
{

	public enum DebugCommands
	{
		DebugEditor
	}

	class DebugEditorHandler : CommandHandler
	{
		protected override void Run ()
		{
			Doit ();
		}

		public static void Doit()
		{
			var processInfo = new ProcessInfo (UnityInstance.ProcessId, "Unity");
			var engines = DebuggingService.GetDebuggerEngines ();
			var engine = engines.Where (e => e.Id == "MonoDevelop.Debugger.Soft.Unity").SingleOrDefault ();
			if (engine == null)
				return;
			IdeApp.ProjectOperations.AttachToProcess (engine, processInfo );
		}

		protected override void Update (CommandInfo info)
		{
			info.Visible = true;
		}
	}
}


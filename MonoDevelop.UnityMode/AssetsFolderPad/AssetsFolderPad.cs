using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using Mono.TextEditor;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Pads;
using MonoDevelop.Ide.Gui.Components;
using System.Linq;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Ide.Gui.Pads.ProjectPad;
using MonoDevelop.Projects;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public class AssetsFolderPad : TreeViewPad
	{
		public static AssetsFolderPad Singleton;

		Folder _rootFolder;

		public AssetsFolderPad ()
		{
			Singleton = this;
		}

		public override void Initialize (NodeBuilder[] builders, TreePadOption[] options, string contextMenuPath)
		{
			base.Initialize (builders, options, contextMenuPath);

			/*
			IdeApp.Workspace.ItemAddedToSolution += Refresh;
			IdeApp.Workspace.FileAddedToProject += Refresh;
			IdeApp.Workspace.FileRemovedFromProject += Refresh;
			IdeApp.Workspace.FileRenamedInProject += Refresh;
			IdeApp.Workspace.WorkspaceItemOpened += Refresh;
			//IdeApp.Workbench.ActiveDocumentChanged += OnWindowChanged;*/
			Refresh (UnityModeAddin.UnityProjectState);

			UnityModeAddin.UnityProjectStateChanged += Refresh;
		}

		public void Refresh(object bah, UnityProjectStateChangedEventArgs args)
		{
			Refresh (args.State);
		}


		public void Refresh(UnityProjectState state)
		{
			TreeView.Clear ();

			_rootFolder = new FileSystemTreeBuilder (state.AssetDatabase).Create ();

			TreeView.AddChild (_rootFolder);
		}


		/*
		void OnWindowChanged (object ob, EventArgs args)
		{
			Gtk.Application.Invoke (delegate {
				SelectActiveFile ();
			});
		}

		void SelectActiveFile ()
		{
			Document doc = IdeApp.Workbench.ActiveDocument;
			if (doc == null || doc.Project == null)
				return;

			string file = doc.FileName;
			if (file == null)
				return;

			ProjectFile pf = doc.Project.Files.GetFile (file);
			if (pf == null)
				return;

			ITreeNavigator nav = treeView.GetNodeAtObject (pf, true);
			if (nav == null)
				return;

			nav.ExpandToNode ();
			nav.Selected = true;
		}*/
	}
	
}

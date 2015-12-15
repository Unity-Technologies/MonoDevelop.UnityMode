using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
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

		private FolderUpdater folderUpdater = new FolderUpdater();

		public AssetsFolderPad ()
		{
			Singleton = this;
			IdeApp.Workbench.ActiveDocumentChanged += new EventHandler (DocumentChanged);
		}

		public override void Initialize (NodeBuilder[] builders, TreePadOption[] options, string contextMenuPath)
		{
			base.Initialize (builders, options, contextMenuPath);

			UnityModeAddin.UnityAssetDatabaseChanged += Refresh;
			Refresh (UnityModeAddin.UnityAssetDatabase);

			TreeView.ShowSelectionPopupButton = true;
		}

		public void Refresh(object obj, UnityAssetDatabaseChangedEventArgs args)
		{
			Refresh (args.Database);
		}

		public void Refresh(UnityAssetDatabase database)
		{
			if (database == null)
				return;

			bool updated = folderUpdater.Update(database);

			DispatchService.GuiDispatch(() =>
			{
				if (updated && TreeView.GetRootNode() != null)
				{
					// Updated folder structure, refresh all root items
					var node = TreeView.GetRootNode();

					if(node != null)
					{
						do
						{
							TreeView.RefreshNode(node);
						}
						while(node.MoveNext());
					}
				}
				else
				{
					// Created a new folder structure, replace old tree
					TreeView.Clear();
					foreach (var child in folderUpdater.RootFolder.Children)
						TreeView.AddChild(child).Expanded = false;

					SelectActiveDocument();
				}
			});
		}

		void DocumentChanged (object ob, EventArgs args)
		{
			DispatchService.GuiDispatch (SelectActiveDocument);
		}

		void SelectActiveDocument()
		{
			if (IdeApp.Workbench.ActiveDocument == null)
				return;

			var file = folderUpdater.RootFolder.GetFile(IdeApp.Workbench.ActiveDocument.FileName);

			if(file != null)
			{
				var nav = TreeView.GetNodeAtObject(file, true);
				if(nav != null)
				{
					nav.ExpandToNode ();
					nav.Selected = true;
				}
			}
		}
	}
	
}

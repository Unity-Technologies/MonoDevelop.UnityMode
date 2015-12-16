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
using System.Collections.Generic;

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
					// Refresh root folders and remove any that no longer exist.
					var node = TreeView.GetRootNode();
					var refreshedFolders = new List<FileSystemEntry>();
					if(node != null)
					{
						var removeObjects = new List<object>();

						do
						{
							var folder = folderUpdater.RootFolder.GetChild(node.NodeName);
							if(folder != null)
							{
								TreeView.RefreshNode(node);
								refreshedFolders.Add(folder);
							}
							else
								removeObjects.Add(node.DataItem);
						}
						while(node.MoveNext());

						foreach(var @object in removeObjects)
							TreeView.RemoveChild(@object);
					}

					// Add new root folders
					foreach (var child in folderUpdater.RootFolder.Children.Where(f => !refreshedFolders.Contains(f)))
						TreeView.AddChild(child).Expanded = false;
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

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
using System.IO;

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
			Refresh (UnityModeAddin.UnityAssetDatabase, null);

			TreeView.ShowSelectionPopupButton = true;
		}

		public void Refresh(object obj, UnityAssetDatabaseChangedEventArgs args)
		{
			Refresh (args.Database, args.ProjectBaseDirectory);
		}

		void Refresh(UnityAssetDatabase database, string projectBaseDirectory)
		{
			if (database == null)
				return;

			string title = "Unity Project: " + (string.IsNullOrEmpty (projectBaseDirectory) ? "" : new DirectoryInfo (projectBaseDirectory).Name);

			if (Window.Title != title)
				Window.Title = title;

			Hint hint = database.Hint;
			bool updated = folderUpdater.Update(database);

			DispatchService.GuiDispatch(() =>
			{
				if (updated && TreeView.GetRootNode() != null)
				{
					TreeView.RefreshNode(TreeView.GetRootNode());

					if(hint is NewFileHint)
					{
						var newFileHint = hint as NewFileHint;
						UnityRestHelpers.OpenFile(Path.Combine(UnityModeAddin.UnityProjectState.AssetsDirectory, newFileHint.Path), 1, OpenDocumentOptions.BringToFront);
					}
					else if(hint is NewFolderHint)
					{
						var newFolderHint = hint as NewFolderHint;
						var newFolder = folderUpdater.RootFolder.FindEntry(newFolderHint.Path) as Folder;
						var nav = TreeView.GetNodeAtObject(newFolder);

						if(nav != null)
						{
							nav.Selected = true;
							TreeView.StartLabelEdit();
						}
					}
				}
				else
				{
					// Created a new folder structure, replace old tree
					TreeView.Clear();
					TreeView.AddChild(folderUpdater.RootFolder);

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
			if (IdeApp.Workbench.ActiveDocument == null || IdeApp.Workbench.ActiveDocument.FileName.IsNull || folderUpdater.RootFolder == null )
				return;

			var file = folderUpdater.RootFolder.FindEntry(IdeApp.Workbench.ActiveDocument.FileName) as File;

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

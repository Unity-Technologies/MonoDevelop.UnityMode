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

		private FolderUpdater folderUpdater = new FolderUpdater();

		public AssetsFolderPad ()
		{
			Singleton = this;
		}

		public override void Initialize (NodeBuilder[] builders, TreePadOption[] options, string contextMenuPath)
		{
			base.Initialize (builders, options, contextMenuPath);

			Refresh (UnityModeAddin.UnityProjectState);
			UnityModeAddin.UnityProjectStateChanged += Refresh;

			TreeView.ShowSelectionPopupButton = true;
		}

		public void Refresh(object obj, UnityProjectStateChangedEventArgs args)
		{
			Refresh (args.State);
		}

		public void Refresh(UnityProjectState state)
		{
			bool updated = folderUpdater.Update(state);

			DispatchService.GuiDispatch(() =>
			{
				if (updated)
				{
					// Updated folder structure, refresh tree
					TreeView.RefreshNode(TreeView.GetRootNode());
				}
				else
				{
					// Created a new folder structure, replace old tree
					TreeView.Clear();
					foreach (var child in folderUpdater.RootFolder.Children)
						TreeView.AddChild(child);					
				}

			});
		}
	}
	
}

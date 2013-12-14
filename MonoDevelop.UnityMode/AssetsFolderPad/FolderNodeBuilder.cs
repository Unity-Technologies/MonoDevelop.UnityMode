using System;
using MonoDevelop.Ide.Gui.Components;
using System.IO;
using System.Collections.Generic;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using Gdk;
using MonoDevelop.Ide.Gui;
using System.Linq;
using MonoDevelop.UnityMode;
using MonoDevelop.Projects;

namespace MonoDevelop.UnityMode
{
	public class FolderNodeBuilder: TypeNodeBuilder
	{
		public override void GetNodeAttributes (ITreeNavigator treeNavigator, object dataObject, ref NodeAttributes attributes)
		{
			attributes |= NodeAttributes.AllowRename;
		}

		static public string GetFolderPath (Folder folder)
		{
			return folder.Path.FullPath;
		}

		public override void BuildNode (ITreeBuilder treeBuilder, object dataObject, ref string label, ref Pixbuf icon, ref Pixbuf closedIcon)
		{
			label = ((Folder)dataObject).Path.FileName;
			icon = Context.GetIcon (Stock.SolutionFolderOpen);
			closedIcon = Context.GetIcon (Stock.SolutionFolderClosed);
		}


		public override void BuildChildNodes (ITreeBuilder builder, object dataObject)
		{
			BuildChildNodes2 (new TreeBuilderBuilder(builder), (Folder)dataObject);
		}

		public static void BuildChildNodes2 (IBuilder builder, Folder folder)
		{
			string path = GetFolderPath (folder);

			ProjectFileCollection files;
			List<string> folders;

			GetFolderContent (path, out files, out folders);

			foreach (ProjectFile file in files)
				builder.AddChild (file);

			foreach (string subfolder in folders)
				builder.AddChild (new Folder (subfolder, folder));
		}

		public interface IBuilder
		{
			void AddChild(object o);
		}

		class TreeBuilderBuilder : IBuilder
		{
			ITreeBuilder builder;

			public TreeBuilderBuilder(ITreeBuilder builder)
			{
				this.builder = builder;
			}

			#region IBuilder implementation

			public void AddChild (object o)
			{
				builder.AddChild(o);
			}

			#endregion
		}


		static IEnumerable<Project> AllProjects
		{
			get { 
				var unitySolution = IdeApp.Workspace.GetAllSolutions ().FirstOrDefault ();
				if (unitySolution == null)
					return new Project[0];

				return unitySolution.GetAllProjects();
			}
		}

		static ProjectFileCollection AllFiles
		{
			get {
				var files = new ProjectFileCollection ();
				foreach (var project in AllProjects)
					files.AddRange (project.Files);
				return files;
			}
		}

		public static void GetFolderContent (string folder, out ProjectFileCollection files, out List<string> folders)
		{
			string folderPrefix = folder + Path.DirectorySeparatorChar;

			files = new ProjectFileCollection ();
			folders = new List<string> ();

			foreach (ProjectFile file in AllFiles)
			{
				string dir;

				if (file.Subtype != Subtype.Directory) {
					if (file.DependsOnFile != null)
						continue;


					dir = 
						//file.IsLink
						//? project.BaseDirectory.Combine (file.ProjectVirtualPath).ParentDirectory
						//: 
						file.FilePath.ParentDirectory;

					if (dir == folder) {
						files.Add (file);
						continue;
					}
				} else
					dir = file.Name;

				// add the directory if it isn't already present
				if (dir.StartsWith (folderPrefix, StringComparison.Ordinal)) {
					int i = dir.IndexOf (Path.DirectorySeparatorChar, folderPrefix.Length);
					if (i != -1) dir = dir.Substring (0,i);
					if (!folders.Contains (dir))
						folders.Add (dir);
				}
			}

		}

		public override bool HasChildNodes (ITreeBuilder builder, object dataObject)
		{
			var allFiles = AllFiles;

			// For big projects, a real HasChildNodes value is too slow to get
			if (allFiles.Count > 500)
				return true;

			var folder = ((Folder) dataObject).Path;

			foreach (var file in allFiles) {
				FilePath path;

				/*
				if (file.Subtype != Subtype.Directory)
					path = file.IsLink ? project.BaseDirectory.Combine (file.ProjectVirtualPath) : file.FilePath;
				else*/
					path = file.FilePath;

				if (path.IsChildPathOf (folder))
					return true;
			}

			return false;
		}

		public override string GetNodeName (ITreeNavigator thisNode, object dataObject)
		{
			return ((Folder)dataObject).Name;
		}

		public override Type NodeDataType {
			get {
				return typeof(Folder);
			}
		}

	}
}


using System;
using MonoDevelop.Projects;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode.Tests
{
	[TestFixture]
	public class FolderUpdaterTests
	{
		private FolderUpdater folderUpdater;
		private UnityProjectState projectState;

		private Folder RootFolder { get { return folderUpdater.RootFolder; } }
		private List<String> Files { get { return projectState.AssetDatabase.Files; } }
		private List<String> EmptyDirectories { get { return projectState.AssetDatabase.EmptyDirectories; } }

		[SetUp]
		public void Setup()
		{
			folderUpdater = new FolderUpdater();
			projectState = new UnityProjectState();
		}

		[Test]
		public void SingleFile()
		{
			Files.Add("myfile.cs");
			Create();
			Is("myfile.cs", RootFolder.GetFiles().Single());
		}

		[Test]
		public void SingleFileInSubfolder()
		{
			Files.Add("sub/myfile.cs");
			Create();

			var folder = RootFolder.GetFolders().Single();

			Is("sub", folder);

			var file = folder.GetFiles().Single();
			Is("sub/myfile.cs", file);
		}

		[Test]
		public void TwoFilesInSubfolder()
		{
			Files.Add("sub/myfile1.cs");
			Files.Add("sub/myfile2.cs");
			Create();

			var folder = RootFolder.GetFolders().Single();
			Is("sub", folder);

			var files = folder.GetFiles().ToArray();
			Is("sub/myfile1.cs", files[0]);
			Is("sub/myfile2.cs", files[1]);
		}

		[Test]
		public void FileInNestedSubfolder()
		{
			Files.Add("sub1/sub2/file.cs");
			Create();

			var sub1 = RootFolder.GetFolders().Single();
			Is("sub1", sub1);

			var sub2 = sub1.GetFolders().Single();
			Is("sub1/sub2", sub2);

			var file = sub2.GetFiles().Single();
			Is("sub1/sub2/file.cs", file);
		}

		[Test]
		public void EmptyFolder()
		{
			EmptyDirectories.Add("sub");
			Create();

			var folder = RootFolder.GetFolders().Single();

			Is("sub", folder);
		}

		[Test]
		public void NestedEmptyFolder()
		{
			EmptyDirectories.Add("sub1/sub2/sub3");
			Create();

			var sub1 = RootFolder.GetFolders().Single();
			Is("sub1", sub1);

			var sub2 = sub1.GetFolders().Single();
			Is("sub1/sub2", sub2);

			var sub3 = sub2.GetFolders().Single();
			Is("sub1/sub2/sub3", sub3);
		}

		[Test]
		public void AddSingleFile()
		{
			Files.Add("myfile.cs");
			
			Create();
			
			Is("myfile.cs", RootFolder.GetFiles().Single());

			NewProjectState();
			Files.Add("myfile.cs");
			Files.Add("myfile2.cs");

			Update();

			Assert.AreEqual(2, RootFolder.Children.Count);
			Assert.IsTrue(Contains(RootFolder, "myfile.cs"));
			Assert.IsTrue(Contains(RootFolder, "myfile2.cs"));
		}

		[Test]
		public void AddSingleNestedFile()
		{
			Files.Add("myfile.cs");
			
			Create();
			
			Is("myfile.cs", RootFolder.GetFiles().Single());

			NewProjectState();
			Files.Add("myfile.cs");
			Files.Add("sub1/myfile2.cs");

			Update();

			Assert.AreEqual(2, RootFolder.Children.Count);
			Assert.IsTrue(Contains(RootFolder, "myfile.cs"));
			Assert.IsTrue(Contains(RootFolder, "sub1"));
			Assert.IsTrue(Contains(RootFolder.GetFolders().Single(), "sub1/myfile2.cs"));
		}

		[Test]
		public void RemoveSingleFile()
		{
			Files.Add("sub1/myfile.cs");

			Create();

			Is("sub1", RootFolder.GetFolders().Single());
			Is("sub1/myfile.cs", RootFolder.GetFolders().Single().GetFiles().Single());

			NewProjectState();
			EmptyDirectories.Add("sub1");

			Update();
			Is("sub1", RootFolder.GetFolders().Single());
			Assert.AreEqual(0, RootFolder.GetFolders().Single().GetFiles().Count());
		}

		[Test]
		public void RemoveFolder()
		{
			Files.Add("myfile.cs");
			Files.Add("sub1/myfile2.cs");
			EmptyDirectories.Add("sub1/sub2");
			
			Create();

			Is("myfile.cs", RootFolder.GetFiles().Single());
			Is("sub1", RootFolder.GetFolders().Single());
			Assert.IsTrue(Contains(RootFolder.GetFolders().Single(), "sub1/myfile2.cs"));
			Assert.IsTrue(Contains(RootFolder.GetFolders().Single(), "sub1/sub2"));

			NewProjectState();
			Files.Add("myfile.cs");

			Update();

			Assert.AreEqual(1, RootFolder.Children.Count);
			Assert.IsTrue(Contains(RootFolder, "myfile.cs"));
		}

		[Test]
		public void RenameFolder()
		{
			Files.Add("sub1/sub2/myfile.cs");
			Files.Add("sub1/sub2/myfile2.cs");

			Create();

			var sub1 = RootFolder.GetFolders().Single();

			Is("sub1", sub1);

			var sub2 = sub1.GetFolders().Single();

			Is("sub1/sub2", sub2);

			Assert.IsTrue(Contains(sub2, "sub1/sub2/myfile.cs"));
			Assert.IsTrue(Contains(sub2, "sub1/sub2/myfile2.cs"));

			NewProjectState();

			Files.Add("sub1/sub4/myfile.cs");
			Files.Add("sub1/sub4/myfile2.cs");

			Update();

			sub1 = RootFolder.GetFolders().Single();

			Is("sub1", sub1);

			var sub4 = sub1.GetFolders().Single();

			Is("sub1/sub4", sub4);

			Assert.IsTrue(Contains(sub4, "sub1/sub4/myfile.cs"));
			Assert.IsTrue(Contains(sub4, "sub1/sub4/myfile2.cs"));
		}

		[Test]
		public void RenameFolderHint()
		{
			Files.Add("sub1/sub2/myfile.cs");
			Files.Add("sub1/sub2/myfile2.cs");

			Create();

			var sub1 = RootFolder.GetFolders().Single();

			Is("sub1", sub1);

			var sub2 = sub1.GetFolders().Single();

			Is("sub1/sub2", sub2);

			Assert.IsTrue(Contains(sub2, "sub1/sub2/myfile.cs"));
			Assert.IsTrue(Contains(sub2, "sub1/sub2/myfile2.cs"));

			NewProjectState();

			Files.Add("sub1/sub4/myfile.cs");
			Files.Add("sub1/sub4/myfile2.cs");

			projectState.RenameHint = new RenameHint { OldPath = "sub1/sub2", newPath = "sub1/sub4" };

			Update();

			Is("sub1", sub1);
			Is("sub1/sub4", sub2);

			Assert.IsTrue(Contains(sub2, "sub1/sub4/myfile.cs"));
			Assert.IsTrue(Contains(sub2, "sub1/sub4/myfile2.cs"));
		}

		[Test]
		public void RenameFileHint()
		{
			Files.Add("sub1/myfile.cs");

			Create();

			var sub1 = RootFolder.GetFolders().Single();

			Is("sub1", sub1);
			Is("sub1/myfile.cs", sub1.GetFiles().Single());

			NewProjectState();
			Files.Add("sub1/myfile2.cs");

			projectState.RenameHint = new RenameHint { OldPath = "sub1/myfile.cs", newPath = "sub1/myfile2.cs" };

			Update();
			
			Is("sub1", sub1);
			Is("sub1/myfile2.cs", sub1.GetFiles().Single());
		}

		void NewProjectState()
		{
			projectState = new UnityProjectState();
		}

		void Create()
		{
			Assert.IsFalse(folderUpdater.Update(projectState));
		}

		void Update()
		{
			Assert.IsTrue(folderUpdater.Update(projectState));
		}

		static void Is(string expect, FileSystemEntry entry)
		{
			Assert.AreEqual(expect, entry.RelativePath);
		}

		static bool Contains(Folder folder, string path)
		{
			return folder.Children.Any(c => c.RelativePath == path);
		}

	}
}


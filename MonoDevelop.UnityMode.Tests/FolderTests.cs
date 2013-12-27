using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MonoDevelop.UnityMode.Tests
{
	[TestFixture]
	public class FolderTests
	{
		Folder _folder;

		[SetUp]
		public void Setup()
		{
			_folder = new Folder ();
		}

		[Test]
		public void ReturnsFilesInFolder()
		{
			_folder.Path = "sub/";
			_folder.Children.Add (new File("sub/file.cs"));
			ExpectFiles ("sub/file.cs");
		}

		[Test]
		public void DoesNotReturnsFilesNotInFolder()
		{
			_folder.Path = "sub";
			_folder.Children.Add (new File("sub2/file.cs"));
			ExpectNoFiles ();
		}

		[Test]
		public void DoesNotReturnFilesInNestedSubfolders()
		{
			_folder.Path = "sub";
			_folder.Children.Add (new File("sub/anothersub/file.cs"));
			ExpectNoFiles ();
		}

		[Test]
		public void ReturnsFoldersInFolder()
		{
			_folder.Path = "sub";
			_folder.Children.Add(new Folder("sub/sub2"));
			ExpectFolder ("sub/sub2");
		}

		void ExpectNoFiles ()
		{
			ExpectFiles (new string[] {});
		}

		void ExpectFiles(params string[] expectedFiles)
		{
			CollectionAssert.AreEquivalent (expectedFiles, _folder.GetFiles ().Select(f => f.Path.ToString()).ToArray());
		}

		void ExpectFolder (params string[] expectedFolders)
		{
			CollectionAssert.AreEquivalent (expectedFolders, _folder.GetFolders ().Select(f => f.Path.ToString()).ToArray());
		}
	}
}


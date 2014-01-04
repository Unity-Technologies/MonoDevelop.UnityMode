using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode.Tests
{
	[TestFixture]
	public class FileSystemTreeBuilderTests
	{
		AssetDatabaseDTO _assetDatabaseDTO;
		FileSystemTreeBuilder _fileSystemTreeBuilder;
		Folder _result;

		[SetUp]
		public void Setup()
		{
			_assetDatabaseDTO = new AssetDatabaseDTO ();
			_fileSystemTreeBuilder = new FileSystemTreeBuilder (_assetDatabaseDTO);
		}

		[Test]
		public void SingleFile()
		{
			AddFile("myfile.cs"); 
			Create ();
			Assert.AreEqual ("myfile.cs", _result.GetFiles ().Single ().PathString ());
		}

		[Test]
		public void SingleFileInSubfolder()
		{
			AddFile("sub/myfile.cs"); 
			Create ();

			var folder = _result.GetFolders ().Single();

			Is ("sub", folder);

			var file = folder.GetFiles ().Single ();
			Is ("sub/myfile.cs", file);
		}

		[Test]
		public void TwoFilesInSubfolder()
		{
			AddFile("sub/myfile1.cs"); 
			AddFile("sub/myfile2.cs"); 
			Create ();

			var folder = _result.GetFolders ().Single();
			Assert.AreEqual ("sub", folder.PathString ());
			Is ("sub", folder);

			var files = folder.GetFiles ().ToArray();
			Is ("sub/myfile1.cs", files [0]);
			Is ("sub/myfile2.cs", files [1]);
		}

		[Test]
		public void FileInNestedSubfolder()
		{
			AddFile ("sub1/sub2/file.cs");
			Create ();

			var sub1 = _result.GetFolders ().Single ();
			Is ("sub1", sub1);

			var sub2 = sub1.GetFolders ().Single ();
			Is ("sub1/sub2", sub2);

			var file = sub2.GetFiles ().Single ();
			Is ("sub1/sub2/file.cs",file);
		}

		static void Is (string expect, FileSystemEntry entry)
		{
			Assert.AreEqual (expect, entry.PathString ());
		}

		void AddFile(string file)
		{
			_assetDatabaseDTO.Files.Add (file);
		}

		void Create ()
		{
			_result = _fileSystemTreeBuilder.Create ();
		}
	}
}


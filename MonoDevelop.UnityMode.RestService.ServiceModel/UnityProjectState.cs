using System;
using System.Collections.Generic;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	public class UnityProjectState
	{
		public UnityProjectState()
		{
			Islands = new List<MonoIsland> ();
			AssetDatabase = new AssetDatabaseDTO ();
		}

		public List<MonoIsland> Islands { get; set; }
		public string BaseDirectory { get; set; }
		public AssetDatabaseDTO AssetDatabase { get; set; }
	}

	public class MonoIsland
	{
		public MonoIsland()
		{
			Files = new List<string>();
			Defines = new List<string>();
			References = new List<string>();
		}

		public string Name { get; set; }
		public string Language { get; set; }
		public List<string> Files { get; set; }
		public List<string> Defines { get; set; }
		public List<string> References { get; set; }
		public string BaseDirectory { get; set; }
	}

	public class AssetDatabaseDTO
	{
		public List<string> Files { get; set; }
		public List<string> EmptyDirectories { get; set; }

		public AssetDatabaseDTO()
		{
			Files = new List<string> ();
			EmptyDirectories = new List<string>();
		}
	}
}


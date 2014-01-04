using System;
using System.Collections.Generic;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	[Route("/unityprojectstate")]
	public class UnityProjectState
	{
		public UnityProjectState()
		{
			MonoIslands = new List<MonoIsland> ();
			AssetDatabase = new AssetDatabaseDTO ();
		}

		public List<MonoIsland> MonoIslands { get; set; }
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
		public List<string> EmptyFolders { get; set; }

		public AssetDatabaseDTO()
		{
			Files = new List<string> ();
			EmptyFolders = new List<string> ();
		}
	}
}


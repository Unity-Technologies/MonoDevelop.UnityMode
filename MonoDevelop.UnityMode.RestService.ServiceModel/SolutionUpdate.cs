using System;
using System.Collections.Generic;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	[Route("/solution")]
	public class SolutionUpdate
	{
		public List<Project> Projects { get; set; }
	}

	public class Project
	{
		public string Name { get; set; }
		public string Language { get; set; }
		public List<string> Files { get; set; }
		public List<string> Defines { get; set; }
		public List<string> References { get; set; }
	}
}


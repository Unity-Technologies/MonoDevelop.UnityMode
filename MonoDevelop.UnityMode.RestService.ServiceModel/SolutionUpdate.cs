using System;
using System.Collections.Generic;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	[Route("/solutioninformation")]
	public class SolutionUpdate
	{
		public SolutionUpdate()
		{
			Projects = new List<ProjectUpdate> ();
		}

		public List<ProjectUpdate> Projects { get; set; }
		public string BaseDirectory { get; set; }
	}

	[Route("/project")]
	public class ProjectUpdate
	{
		public ProjectUpdate()
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
}


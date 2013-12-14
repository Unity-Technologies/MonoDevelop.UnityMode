using System;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode
{
	[Route("/files")]
	public class File
	{
		public string Path { get; set; }
	}
}


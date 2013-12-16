using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	[Route("/openfile")]
	public class OpenFileRequest
	{
		public string File { get; set; }
		public int Line { get; set; }
	}
}


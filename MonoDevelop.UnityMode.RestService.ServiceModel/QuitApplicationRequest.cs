using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	[Route("/quit")]
	public class QuitApplicationRequest
	{
		public string UnityProject { get; set; }
	}
}
	
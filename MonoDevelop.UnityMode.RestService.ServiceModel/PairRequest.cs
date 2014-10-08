using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	[Route("/pair")]
	public class PairRequest
	{
		public int UnityProcessId { get; set; }
		public string UnityRestServerUrl { get; set; }
	}
}
	
using System;
using System.Net;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.ServiceHost;
using ServiceStack.Common.Web;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public class RestService
	{
		public RestService (SolutionUpdateCallback solutionUpdateCallback)
		{
			var listeningOn = "http://localhost:1339/";
			var appHost = new AppHost (solutionUpdateCallback);
			appHost.Init ();
			appHost.Start (listeningOn);
		}

		public delegate void SolutionUpdateCallback(SolutionUpdate update);

		public class SolutionUpdateService : IService
		{
			public SolutionUpdateCallback Callback { get; set; }

			public object Post(SolutionUpdate update)
			{
				Callback (update);
				return new HttpResult() { StatusCode = HttpStatusCode.OK };
			}
		}

		//Define the Web Services AppHost
		public class AppHost : AppHostHttpListenerBase
		{
			readonly SolutionUpdateCallback _solutionUpdateCallback;

			public AppHost(SolutionUpdateCallback solutionUpdateCallback)
				: base("UnityMode Rest Service", typeof(SolutionUpdateService).Assembly) {
				_solutionUpdateCallback = solutionUpdateCallback;
			}

			public override void Configure(Funq.Container container)
			{
				container.Register (_solutionUpdateCallback);
			}
		}
	}
}


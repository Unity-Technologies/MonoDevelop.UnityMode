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
		public RestService (SolutionUpdateCallback solutionUpdateCallback, OpenFileCallback openFileCallback)
		{
			var listeningOn = "http://localhost:1342/";
			var appHost = new AppHost (solutionUpdateCallback, openFileCallback);
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

		public delegate void OpenFileCallback(OpenFileRequest openFileRequest);

		public class OpenFileService : IService
		{
			public OpenFileCallback Callback { get; set; }

			public object Post(OpenFileRequest openFileRequest)
			{
				Callback (openFileRequest);
				return new HttpResult () { StatusCode = HttpStatusCode.OK };
			}
		}

		//Define the Web Services AppHost
		public class AppHost : AppHostHttpListenerBase
		{
			readonly SolutionUpdateCallback _solutionUpdateCallback;
			readonly OpenFileCallback _openFileCallback;

			public AppHost(SolutionUpdateCallback solutionUpdateCallback, OpenFileCallback openFileCallback)
				: base("UnityMode Rest Service", typeof(SolutionUpdateService).Assembly) {
				_openFileCallback = openFileCallback;
				_solutionUpdateCallback = solutionUpdateCallback;
			}

			public override void Configure(Funq.Container container)
			{
				container.Register (_openFileCallback);
				container.Register (_solutionUpdateCallback);
			}
		}
	}
}


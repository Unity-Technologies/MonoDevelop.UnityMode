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
		public string Url { get; set; }

		public RestService (UnityProjectStateUpdateCallback callback, OpenFileCallback openFileCallback)
		{
			var appHost = new AppHost (callback, openFileCallback);
			appHost.Init ();

			Url = "http://localhost:40000/";
			appHost.Start (Url);
		}

		public delegate void UnityProjectStateUpdateCallback(UnityProjectState update);

		public class UnityProjectStateUpdateService : IService
		{
			public UnityProjectStateUpdateCallback Callback { get; set; }

			public object Post(UnityProjectState update)
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
			readonly UnityProjectStateUpdateCallback unityProjectStateUpdateCallback;
			readonly OpenFileCallback _openFileCallback;

			public AppHost(UnityProjectStateUpdateCallback unityProjectStateUpdateCallback, OpenFileCallback openFileCallback)
				: base("UnityMode Rest Service", typeof(UnityProjectStateUpdateService).Assembly) {
				_openFileCallback = openFileCallback;
				this.unityProjectStateUpdateCallback = unityProjectStateUpdateCallback;
			}

			public override void Configure(Funq.Container container)
			{
				container.Register (_openFileCallback);
				container.Register (unityProjectStateUpdateCallback);
			}
		}
	}
}


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

		public RestService (OpenFileCallback openFileCallback)
		{
			var appHost = new AppHost (openFileCallback);
			appHost.Init ();

			int port = 40000;
			int timeout = 20;

			while (timeout-- > 0)
			{
				try
				{
					Url = "http://localhost:" + port + "/";
					appHost.Start(Url);
					break;
				}
				catch (Exception exception)
				{
					port++;
				}
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
			readonly OpenFileCallback openFileCallback;

			public AppHost(OpenFileCallback openFileCallback)
				: base("UnityMode Rest Service", typeof(RestService).Assembly)
			{
				this.openFileCallback = openFileCallback;
			}

			public override void Configure(Funq.Container container)
			{
				container.Register (openFileCallback);
			}
		}
	}
}


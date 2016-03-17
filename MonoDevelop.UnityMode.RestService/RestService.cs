using System;
using System.Net;
using System.Diagnostics;
using ServiceStack.WebHost.Endpoints;
using ServiceStack.ServiceHost;
using ServiceStack.Common.Web;
using MonoDevelop.UnityMode.RestServiceModel;

namespace MonoDevelop.UnityMode
{
	public class RestService
	{
		public string Url { get; set; }

		public delegate void OpenFileCallback(OpenFileRequest openFileRequest);

		public RestService (OpenFileCallback openFileCallback)
		{
			int port = 40000 + (Process.GetCurrentProcess().Id % 1000);
			Url = "http://localhost:" + port + "/";

			var appHost = new AppHost (openFileCallback);
			appHost.Init ();

			try
			{
				appHost.Start(Url);
			}
			catch (Exception)
			{
				Url = null;
			}
		}
			
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


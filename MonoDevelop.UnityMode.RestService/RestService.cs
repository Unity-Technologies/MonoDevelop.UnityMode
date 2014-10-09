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
		public delegate void PairCallback(PairRequest pairRequest);
		public delegate void QuitApplicationCallBack(QuitApplicationRequest quitRequest); 

		public RestService (OpenFileCallback openFileCallback, PairCallback pairCallback, QuitApplicationCallBack quitCallback)
		{
			int port = 40000 + (Process.GetCurrentProcess().Id % 1000);
			Url = "http://localhost:" + port + "/";

			var appHost = new AppHost (openFileCallback, pairCallback, quitCallback);
			appHost.Init ();

			try
			{
				appHost.Start(Url);
			}
			catch (Exception exception)
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

		public class PairService : IService
		{
			public PairCallback Callback { get; set; }

			public object Post(PairRequest pairRequest)
			{
				Callback (pairRequest);
				return new HttpResult () { StatusCode = HttpStatusCode.OK };
			}
		}

		public class QuitApplicationService : IService
		{
			public QuitApplicationCallBack Callback { get; set; }

			public object Post(QuitApplicationRequest quitRequest)
			{
				Callback (quitRequest);
				return new HttpResult () { StatusCode = HttpStatusCode.OK };
			}
		}

		//Define the Web Services AppHost
		public class AppHost : AppHostHttpListenerBase
		{
			readonly OpenFileCallback openFileCallback;
			readonly PairCallback pairCallback;
			readonly QuitApplicationCallBack quitCallback;

			public AppHost(OpenFileCallback openFileCallback, PairCallback pairCallback, QuitApplicationCallBack quitCallback)
				: base("UnityMode Rest Service", typeof(RestService).Assembly)
			{
				this.openFileCallback = openFileCallback;
				this.pairCallback = pairCallback;
				this.quitCallback = quitCallback;
			}

			public override void Configure(Funq.Container container)
			{
				container.Register (openFileCallback);
				container.Register (pairCallback);
				container.Register (quitCallback);
			}
		}
	}
}


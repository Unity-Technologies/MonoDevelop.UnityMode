using System;
using System.Collections.Generic;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.UnityRestClient
{
	public class ScriptCompilationRequest : IReturn<CompilationResult>
	{
	}

	public class LogEntry
	{
		public string LogString { get; set; }
		public string StackTrace { get; set; }
		public string File { get; set; }
		public int Line { get; set; }
	}

	public class CompilationResult
	{
		public CompilationResult() { Output = new List<LogEntry>(); }
		public List<LogEntry> Output { get; set; }
	}

	public class RestClient2
	{
		public static void SendSolutionInformationRequest ()
		{
			var client = new JsonServiceClient("http://localhost:1340/");
			client.Get<IReturnVoid>("/sendsolutioninformation");
		}

		public static CompilationResult CompileScripts()
		{
			var client = new JsonServiceClient("http://localhost:1340/");
			return client.Get<CompilationResult>("/assetpipeline/compilescripts");
		}
	}
}


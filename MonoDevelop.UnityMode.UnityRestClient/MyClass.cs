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

	public class RenameAssetRequest
	{
		public string OldPath { get; set; }
		public string NewPath { get; set; }
	}

	public class RestClient2
	{
		static JsonServiceClient _client = new JsonServiceClient("http://localhost:1340/");

		public static void SendSolutionInformationRequest ()
		{
			_client.Get<IReturnVoid>("/sendsolutioninformation");
		}

		public static CompilationResult CompileScripts()
		{
			return _client.Get<CompilationResult>("/assetpipeline/compilescripts");
		}

		public static void RenameAssetRequest(RenameAssetRequest r)
		{
			_client.Post<IReturnVoid> ("/assetpipeline/renameasset", r);
		}
	}
}


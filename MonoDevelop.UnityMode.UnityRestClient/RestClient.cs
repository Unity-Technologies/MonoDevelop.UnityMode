using System;
using System.Collections.Generic;
using MonoDevelop.UnityMode.RestServiceModel;
using ServiceStack.ServiceClient.Web;
using ServiceStack.ServiceHost;

namespace MonoDevelop.UnityMode.UnityRestClient
{
	public class ScriptCompilationRequest : IReturn<CompilationResult>
	{
	}

	public class CompilationMessage
	{
		public string Type { get; set; }
		public string File { get; set; }
		public string Message { get; set; }
		public int Line { get; set; }
		public int Column { get; set; }
	}

	public class ScriptRequest
	{
		public string action { get; set; }
	}

	public class CompilationResult
	{
		public CompilationResult() { Messages = new List<CompilationMessage>(); }
		public List<CompilationMessage> Messages { get; set; }
	}

	public class RenameAssetRequest
	{
		public string OldPath { get; set; }
		public string NewPath { get; set; }
	}

	public class PairRequest
	{
		public string url { get; set; }
	}

	public class PairResult
	{
		public string Result { get; set; } 
	}

	public class RestClient
	{
		private static JsonServiceClient client;

		public static void SetServerUrl(string url)
		{
			client = new JsonServiceClient(url);
		}

		public static UnityProjectState GetUnityProjectState ()
		{
			return client.Get<UnityProjectState>("/unity/projectstate");
		}

		public static CompilationResult CompileScripts()
		{
			return client.Post<CompilationResult>("unity/scripts", new ScriptRequest{action = "recompile"});
		}

		public static void RenameAssetRequest(RenameAssetRequest r)
		{
			client.Post<IReturnVoid> ("/assetpipeline/renameasset", r);
		}

		public static PairResult Pair(string url)
		{
			return client.Post<PairResult>("/unity/pair", new PairRequest {url = url});
		}
	}
}


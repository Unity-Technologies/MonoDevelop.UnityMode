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
		public string Action { get; set; }
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

	public class RestClient2
	{
		static JsonServiceClient _client = new JsonServiceClient("http://localhost:4040/");

		public static UnityProjectState SendSolutionInformationRequest ()
		{
			return _client.Get<UnityProjectState>("/unity/projectstate");
		}

		public static CompilationResult CompileScripts()
		{
			return _client.Post<CompilationResult>("unity/scripts", new ScriptRequest{Action = "recompile"});
		}

		public static void RenameAssetRequest(RenameAssetRequest r)
		{
			_client.Post<IReturnVoid> ("/assetpipeline/renameasset", r);
		}
	}
}


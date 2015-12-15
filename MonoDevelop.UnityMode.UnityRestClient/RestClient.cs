using System;
using System.Collections.Generic;
using System.Diagnostics;
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

	public class AssetRequest
	{
		public string action { get; set; }
	}

	public class MoveAssetRequest : AssetRequest
	{
		public string newpath { get; set; }
	}

	public class CreateAssetRequest : AssetRequest
	{
		public string type { get; set; }
	}

	public class PairRequest
	{
		public string url { get; set; }
		public string name { get; set; }
		public int processid { get; set; }
	}

	public class PairResult
	{
		PairResult()
		{
			unityprocessid = -1;
		}

		public string result { get; set; }
		public int unityprocessid { get; set; }
		public string unityproject { get; set; }
	}

	public class AssetDatabaseFolder
	{
		List<AssetDatabaseFolder> folders { get; set; }

		public AssetDatabaseFolder()
		{
			folders = new List<AssetDatabaseFolder> ();
		}
	}

	public class AssetDatabaseRequest
	{
		public AssetDatabaseFolder root { get; set; }
	}

	public class GenericResult
	{
		public string result { get; set; }
	}

	public class RestClient
	{
		private static JsonServiceClient client;

		public static void SetServerUrl(string url)
		{
			client = url == null ? null : new JsonServiceClient(url);
		}

		public static bool Available
		{
			get { return client != null; }
		}

		public static UnityProjectState GetUnityProjectState ()
		{
			return client.Get<UnityProjectState>("/unity/projectstate");
		}

		public static UnityAssetDatabase GetUnityAssetDatabase ()
		{
			return client.Get<UnityAssetDatabase>("/unity/assets");
		}

		public static CompilationResult CompileScripts()
		{
			return client.Post<CompilationResult>("unity/scripts", new ScriptRequest{action = "recompile"});
		}

		public static void CreateAsset(string path, string type)
		{
			client.Post<IReturnVoid>("unity/assets/" + path, new CreateAssetRequest { action = "create", type = type });
		}

		public static void MoveAsset(string oldpath, string newpath)
		{
			client.Post<IReturnVoid>("unity/assets/" + oldpath, new MoveAssetRequest { action = "move", newpath = newpath });
		}

		public static void DeleteAsset(string path)
		{
			client.Delete<IReturnVoid>("unity/assets/" + path);
		}

		public static void CreateDirectory(string path)
		{
			CreateAsset (path, "directory");
		}

		public static PairResult Pair(string url, string name)
		{
			return client.Post<PairResult>("/unity/pair", new PairRequest {url = url, name = name, processid = Process.GetCurrentProcess().Id});
		}

		public static void SaveUnityProjectSettings(UnityProjectSettings projectSettings)
		{
			client.Post<IReturnVoid> ("/unity/projectsettings", projectSettings);
		}

		public static UnityProjectSettings GetProjectSettings()
		{
			return client.Get<UnityProjectSettings> ("/unity/projectsettings");
		}

	
	}
}


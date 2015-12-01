using System;
using System.IO;
using ServiceStack.Text;

namespace MonoDevelop.UnityMode.ServiceModel
{
	public class UnityRestServiceSettings
	{
		public string EditorRestServiceUrl { get; private set; }
		public int EditorProcessID { get; private set; }

		public UnityRestServiceSettings(string editorRestServiceUrl, int editorProcessID)
		{
			EditorRestServiceUrl = editorRestServiceUrl;
			EditorProcessID = editorProcessID;
		}

		public static UnityRestServiceSettings Load(string projectPath)
		{
			var editorSettingsJson = File.ReadAllText(Path.Combine(projectPath, Path.Combine ("Library", "EditorRestService.json")));

			var jsonObject = JsonObject.Parse (editorSettingsJson);

			return new UnityRestServiceSettings (jsonObject.Get<string> ("EditorRestServiceUrl"), jsonObject.Get<int> ("EditorProcessID"));
		}
	}
}


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
			var path = Path.Combine(projectPath, Path.Combine ("Library", "EditorRestService.json"));

			if(!File.Exists(path))
				return null;

			var editorSettingsJson = File.ReadAllText(path);

			var jsonObject = JsonObject.Parse (editorSettingsJson);

			return new UnityRestServiceSettings (jsonObject.Get<string> ("EditorRestServiceUrl"), jsonObject.Get<int> ("EditorProcessID"));
		}
	}
}


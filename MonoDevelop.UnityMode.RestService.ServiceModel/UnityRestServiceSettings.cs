using System;
using System.IO;
using ServiceStack.Text;

namespace MonoDevelop.UnityMode.ServiceModel
{
	public class UnityRestServiceSettings
	{
		public string EditorRestServiceUrl { get; private set; }
		public int EditorProcessID { get; private set; }

		public static UnityRestServiceSettings Load(string projectPath)
		{
			var editorSettingsJson = File.ReadAllText(Path.Combine(projectPath, Path.Combine ("Library", "EditorRestService.json")));

			var jsonObject = JsonObject.Parse (editorSettingsJson);

			var settings = new UnityRestServiceSettings ();
			settings.EditorRestServiceUrl = jsonObject.Get<string> ("EditorRestServiceUrl");
			settings.EditorProcessID = jsonObject.Get<int> ("EditorProcessID");
			return settings; 
		}
	}
}


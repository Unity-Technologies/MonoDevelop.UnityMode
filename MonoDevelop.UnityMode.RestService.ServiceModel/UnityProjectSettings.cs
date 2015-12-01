using System;
using System.Collections.Generic;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	public class UnityProjectSettings
	{
		public string ProjectPath { get; set; }
		public List<String> OpenDocuments { get; set; }
		public List<String> Breakpoints { get; set; }

		public UnityProjectSettings()
		{
			OpenDocuments = new List<String> ();
			Breakpoints = new List<String> ();
		}
	}
}


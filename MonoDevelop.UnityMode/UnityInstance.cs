using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoDevelop.UnityMode
{
	public class UnityInstance
	{
		public int ProcessID { get; set; }
		public string RestServiceUrl { get; set; }
		public string ProjectPath { get; set; }
		public List<string> OpenDocuments { get; set; }

		internal UnityInstance()
		{
			OpenDocuments = new List<string> ();
		}

		public bool Paired
		{
			get { return ProcessID > 0; }
		}

		public bool Running
		{
			get 
			{
				try
				{
					Process.GetProcessById(ProcessID);
				}
				catch(Exception)
				{
					return false;
				}
					
				return true;
			}
		}
	}
}


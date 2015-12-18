using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MonoDevelop.UnityMode.RestServiceModel
{
	public class UnityProjectSettings
	{
		public class Breakpoint
		{
			public string Filename { get; set; }
			public int Line { get; set; }
			public int Column { get; set; }
			public bool Enabled { get; set; }

			public Breakpoint(string filename, int line, int column, bool enabled)
			{
				Filename = filename;
				Line = line;
				Column = column;
				Enabled = enabled;
			}
		}

		public class FunctionBreakpoint
		{
			public string Function { get; set; }
			public string Language { get; set; }
			public bool Enabled { get; set; }

			public FunctionBreakpoint(string function, string language, bool enabled)
			{
				Function = function;
				Language = language;
				Enabled = enabled;
			}
		}

		public class ExceptionBreak
		{
			public string Exception { get; set; }
			public bool IncludeSubclasses { get; set; }
			public bool Enabled { get; set; }

			public ExceptionBreak(string exception, bool includeSubclasses, bool enabled)
			{
				Exception = exception;
				IncludeSubclasses = includeSubclasses;
				Enabled = enabled;
			}
		}

		[IgnoreDataMember]
		public string ProjectPath { get; set; }
		public String ActiveDocument { get; set; }
		public List<String> Documents { get; set; }
		public List<Breakpoint> Breakpoints { get; set; }
		public List<FunctionBreakpoint> FunctionBreakpoints { get; set; }
		public List<ExceptionBreak> ExceptionBreaks { get; set; }

		public UnityProjectSettings()
		{
			Documents = new List<String> ();
			Breakpoints = new List<Breakpoint> ();
			FunctionBreakpoints = new List<FunctionBreakpoint> ();
			ExceptionBreaks = new List<ExceptionBreak> ();
		}
	}
}


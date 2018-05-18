#region using
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using Jellequin.Compiler;
using System.CodeDom.Compiler;
using System.Reflection.Emit;
#endregion using

namespace Jellequin.CompilerTasks
{
	public class JellequinCompilerTask : Task
	{
		/*public JellequinCompilerTask()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
		}

		static Assembly CurrentDomain_AssemblyResolve(object sender,ResolveEventArgs args)
		{
			System.Diagnostics.Debugger.Launch();
			Assembly result;
			result=Assembly.Load(args.Name);
			return result;
		}*/

		#region Public Properties and related Fields
		private string[] sourceFiles;
		/// <summary>
		/// List of Python source files that should be compiled into the assembly
		/// </summary>
		[Required()]
		public string[] SourceFiles
		{
			get { return sourceFiles; }
			set { sourceFiles = value; }
		}

		private string outputAssembly;
		/// <summary>
		/// Output Assembly (including extension)
		/// </summary>
		[Required()]
		public string OutputAssembly
		{
			get { return outputAssembly; }
			set { outputAssembly = value; }
		}

		private ITaskItem[] referencedAssemblies = new ITaskItem[0];
		/// <summary>
		/// List of dependent assemblies
		/// </summary>
		public ITaskItem[] ReferencedAssemblies
		{
			get { return referencedAssemblies; }
			set
			{
				if (value != null)
				{
					referencedAssemblies = value;
				}
				else
				{
					referencedAssemblies = new ITaskItem[0];
				}

			}
		}

		private string targetKind;
		/// <summary>
		/// Target type (exe, winexe, library)
		/// These will be mapped to System.Reflection.Emit.PEFileKinds
		/// </summary>
		public string TargetKind
		{
			get { return targetKind; }
			set { targetKind = value.ToLower(CultureInfo.InvariantCulture); }
		}

		private bool debugSymbols = true;
		/// <summary>
		/// Generate debug information
		/// </summary>
		public bool DebugSymbols
		{
			get { return debugSymbols; }
			set { debugSymbols = value; }
		}

		private string projectPath = null;
		/// <summary>
		/// This should be set to $(MSBuildProjectDirectory)
		/// </summary>
		public string ProjectPath
		{
			get { return projectPath; }
			set { projectPath = value; }
		}
		#endregion Public Properties and related Fields

		/// <summary>
		/// Main entry point for the task
		/// </summary>
		/// <returns></returns>
		public override bool Execute()
		{
			bool copyRuntimeMethods;
			bool generateExecutable;
			bool includeDebugInformation;
			string outputAssembly;
			string[] sourceFilename;
			bool generateInMemory = false;

			copyRuntimeMethods = true;
			FileKind fileKind;
			switch (this.TargetKind.ToLowerInvariant())
			{
				case "library":
					fileKind = FileKind.Dll;
					break;
				case "exe":
					fileKind = FileKind.ConsoleExe;
					break;
				//case "winexe":
				default:
					Log.LogCriticalMessage("", "", "", "", 0, 0, 0, 0, "Unsupported target. Choose Library or Exe.");
					return false;
			}
			generateExecutable = fileKind != FileKind.Dll;
			includeDebugInformation = this.DebugSymbols;
			outputAssembly = Path.Combine(this.ProjectPath, this.OutputAssembly);
			List<string> tempSources = new List<string>(this.SourceFiles.Length);
			foreach (string s in this.SourceFiles)
				if (Path.GetExtension(s).ToLowerInvariant() == ".js")
					tempSources.Add(Path.Combine(this.ProjectPath, s));
			sourceFilename = tempSources.ToArray();

			RuntimeMethodsUsage rmu = copyRuntimeMethods ? RuntimeMethodsUsage.Copy : RuntimeMethodsUsage.Call;
			Log.LogMessage($"Jellequin compiler: RuntimeMethodsUsage={rmu}, FileKind={fileKind}, IncludeDebugInformation={includeDebugInformation}, OutputAssembly={outputAssembly}");

			CompilerResults cr = null;
			try
			{
				cr = new JellequinCodeProvider().CompileAssemblyFromFile(new JellequinCompilerParameters()
				{
					FileKind = fileKind,
					RuntimeMethodsUsage = rmu,
					GenerateExecutable = generateExecutable,
					IncludeDebugInformation = includeDebugInformation,
					OutputAssembly = outputAssembly,
					GenerateInMemory = generateInMemory
				}, sourceFilename);
			}
			catch (Exception ex)
			{
				Log.LogCriticalMessage("", "", "", "", 0, 0, 0, 0, "Error using Jellequin compiler: " + ex.GetBaseException().Message);
				return false;
			}

			if ((cr != null) && (cr.Errors.HasErrors))
			{
				foreach (CompilerError ce in cr.Errors)
					if (ce.IsWarning)
						Log.LogWarning("", ce.ErrorNumber, "", ce.FileName, ce.Line, ce.Column, ce.Line, ce.Column, ce.ErrorText);
					else
						Log.LogError("", ce.ErrorNumber, "", ce.FileName, ce.Line, ce.Column, ce.Line, ce.Column, ce.ErrorText);
				return false;
			}

			return true;
		}
	}
}
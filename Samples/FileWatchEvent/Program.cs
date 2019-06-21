/////////////////////////////////////////////////////////
// This demoes .NET events support
/////////////////////////////////////////////////////////

#region using
using Jellequin.Compiler;
using Jellequin.Reflection.Emit;
using Jellequin.Runtime;
using System;
using System.IO;
using System.Reflection;
#endregion using

namespace FileWatchEvent
{
	class Program
	{
		static void Main(string[] args)
		{
			ISource jellCode = new StringSource(@"import consoleDll from ""System.Console, Version = 4.1.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a"";
import watcherDll from ""System.IO.FileSystem.Watcher, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"";
var Console = consoleDll.System.Console;
var FileSystemWatcher = watcherDll.System.IO.FileSystemWatcher;

function fileCreateCallback(sender,fileSystemEventArgs)
{
	Console.WriteLine(fileSystemEventArgs.FullPath);
}

var watcher=new FileSystemWatcher("""+ GetFilePath("").Replace("\\","\\\\") + @"\\Watch"");
//watcher.addEventListener(""Created"",fileCreateCallback);
watcher.Created+=fileCreateCallback;
watcher.EnableRaisingEvents=true;
Console.WriteLine(""Hint: create a file in Watch subfolder."");
Console.WriteLine(""Press any key once you are done..."");
Console.ReadKey();
");

			//Compile source code to .NET assembly.
			Assembly asm =Compile(jellCode);

			//Launch the application. Best way to see it works is to explore source code and try to imagine what does it do.
			//Hint: create a file in Watch subfolder.
			Executor executor = new Executor(asm);
			executor.ResolveExternalLibrary+= Executor.DefaultExternalLibraryResolver;
			executor.Execute(new string[0]);
		}

		#region compile
		static Assembly Compile(ISource jellCode)
		{
			//See manual to get further information about compilation and options.

			using (MemoryStream dll = new MemoryStream())
			using (MemoryStream pdb = new MemoryStream())
			{
				Compiler.Compile(jellCode.GetText(), new AssemblyName("FileWatchEventDemo"), new CompilerOptions()
				{
					FileKind = FileKind.Dll,
					RuntimeMethodsUsage = RuntimeMethodsUsage.Call,
					DontUseDynamicJsMembers = true,
					Debug = true
				})
				.Save(dll,new SaveOptions { Symbols=new SymbolsSaveOptions { Code=jellCode,EmbedSource=true,Pdb=pdb } });

				dll.Position = 0;
				pdb.Position = 0;
				return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(dll, pdb);
			}
		}
		#endregion compile

		#region GetFilePath
		static string GetFilePath(string relPath)
		{
			string path = Environment.CurrentDirectory;
			if (path.Contains(@"\bin\"))
				path = Path.Combine(path, @"..\..\..");
			return Path.Combine(path, relPath);
		}
		#endregion GetFilePath
	}
}

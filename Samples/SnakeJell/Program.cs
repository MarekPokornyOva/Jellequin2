/////////////////////////////////////////////////////////
// This demoes how to create JS application
/////////////////////////////////////////////////////////

#region using
using Jellequin.Compiler;
using Jellequin.Reflection.Emit;
using Jellequin.Runtime;
using System;
using System.IO;
using System.Reflection;
#endregion using

namespace SnakeJell
{
	class Program
	{
		static void Main(string[] args)
		{
			//Compile source code to .NET assembly.
			Assembly asm=Compile(GetFilePath("snake.jell"),true);

			//Launch the application. So this demo just compiles the JS app and launches it.
			Executor executor = new Executor(asm);
			executor.ResolveExternalLibrary += Executor.DefaultExternalLibraryResolver;
			executor.Execute(args);

			//If you explore Compile method below, you may find out the generated assembly is also saved to disk.
			//You can freely launch the JS app directly by "dotnet snake.dll" command.
		}

		#region compile
		static Assembly Compile(string path, bool debug)
		{
			//See manual to get further information about compilation and options.

			ISource code = new FileSource(path);

			Assembly asm;
			using (Stream dll = new MemoryStream())
			using (Stream pdb = new MemoryStream())
			using (Stream icon = File.OpenRead(GetFilePath(@"..\..\VsExtension\JellequinVs2017.ProjectType\Jellequin.ico")))
			{
				Compiler.Compile(code.GetText(), new AssemblyName("SnakeJell2") { Version = new Version(1, 2, 3, 4) }, new CompilerOptions
				{
					FileKind = FileKind.ConsoleExe,
					RuntimeMethodsUsage = RuntimeMethodsUsage.Call,
					DontUseDynamicJsMembers = true
				})
				.Save(dll,new SaveOptions { Icon=icon,Symbols=new SymbolsSaveOptions { Code=code,EmbedSource=false,Pdb=pdb } });

				//Generated assembly and debug symbols might be stored to files...
				string basePath = GetFilePath(Path.Combine("out", "snake."));
				dll.Position = 0;
				using (Stream s = File.Create(basePath + "dll"))
					dll.CopyTo(s);

				pdb.Position = 0;
				using (Stream s = File.Create(basePath + "pdb"))
					pdb.CopyTo(s);

				//...and/or loaded into application context and used
				dll.Position = 0;
				pdb.Position = 0;
				asm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(dll, pdb);
			}

			return asm;
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

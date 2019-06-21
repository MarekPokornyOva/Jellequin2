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

namespace CommandArgs
{
	class Program
	{
		static void Main(string[] args)
		{
			args = new[] { "t=12", "tatra=13", "zoro=" };
			string code = @"import consoleDll from ""System.Console, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"";
//debugger;
var args=arguments;
var console=consoleDll.System.Console;
var wl=console.WriteLine;
wl(""Jellequin application is alive!"");

function parseArg(arg)
{
    var fch=arg.substr(0,1);
    var arg2=(fch=='/')||(fch=='-')?arg.substr(1,arg.length-1):arg;
    var pos=arg2.indexOf(""="");
    var n;var v;
    //wl(pos);
    if (pos==-1)
    {
        n=arg2;
        v="""";
    }
    else
    {
        n=arg2.substr(0,pos);
        v=arg2.substr(pos+1,arg2.length);
    }
    return {orig:arg2,name:n,value:v};
}

var argsLen=args.length;
if (argsLen==0)
{
    wl(""No arguments"");
	//parseArg(""NoArg"");
}
for (var a=0;a<argsLen;a++)
{
    var arg=parseArg(args[a]);
    wl(""arg ""+(a+1)+"": ""+arg.name+'=""'+arg.value+'""');
}
console.Write(""Press any key to continue..."");
console.ReadKey();
console.WriteLine();
console.WriteLine();
";

			//Compile source code to .NET assembly.
			Assembly asm = Compile(code);

			//Various fun with the generated assembly.
			TestEntryPointInvoke(asm, args);
			TestExecutor(asm, args);
			TestScripting(asm, args);

			//If you explore Compile method below, you may find out the generated assembly is also saved to disk.
			//You can freely launch the JS app directly by "dotnet testik.dll" command.
		}

		static Assembly Compile(string code)
		{
			//See manual to get further information about compilation and options.

			using (Stream dll = new MemoryStream())
			using (Stream pdb = new MemoryStream())
			{
				Compiler.Compile(code, new AssemblyName("testik"), new CompilerOptions
				{
					FileKind = FileKind.ConsoleExe,
					RuntimeMethodsUsage = RuntimeMethodsUsage.Call,
					DontUseDynamicJsMembers = true,
					//Debug = new DebugOptions { Debug = true, EmbedSourceCode = true, Pdb = pdb },
					Debug = true
				})
				.Save(dll,new SaveOptions { Symbols=new SymbolsSaveOptions { Code=new StringSource(code),EmbedSource=false,Pdb=pdb } });

				//Generated assembly and debug symbols might be stored to files...
				string basePath = GetFilePath(Path.Combine("out", "testik."));
				dll.Position = 0;
				using (Stream s = File.Create(basePath + "dll"))
					dll.CopyTo(s);

				pdb.Position = 0;
				using (Stream s = File.Create(basePath + "pdb"))
					pdb.CopyTo(s);

				//The stored assebly might be loaded to application context
				return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(basePath + "dll");
			}
		}

		static void TestEntryPointInvoke(Assembly asm, string[] args)
		{
			//Any assembly might have entry point if developer is willing to.
			//Jellequin application might have one too.
			asm.EntryPoint.Invoke(null, new[] { args });
		}

		static void TestExecutor(Assembly asm, string[] args)
		{
			//It's up to you if you call entry point directly or through Executor but 2nd one brings more possibilities.
			Executor exe = new Executor(asm);
			exe.ResolveExternalLibrary += Executor.DefaultExternalLibraryResolver;
			exe.Execute(args);
		}

		static void TestScripting(Assembly asm, string[] args)
		{
			//Do you remember Executor brings some possibilities? Let's explore those.
			Executor exe = new Executor(asm);
			exe.ResolveExternalLibrary += Executor.DefaultExternalLibraryResolver;
			exe.Execute(args);

			//Executing JS code from C# is one of them.
			Func<string, object> parseArg = exe.ExternalVariables.GetFunc<string, object>("parseArg");
			object parsedArgRaw = parseArg("ex=sf56");
			IJsObject parsedArg = Executor.BridgeJsObject(parsedArgRaw);
			string parsedArgValue = parsedArg.GetValue("value") as string;
		}

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
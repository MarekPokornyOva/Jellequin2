/////////////////////////////////////////////////////////
// This demoes intercompatibility between C# and JS code
/////////////////////////////////////////////////////////

#region using
using Jellequin.Compiler;
using Jellequin.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#endregion using

namespace SortFunction01
{
	class Program
	{
		static void Main(string[] args)
		{
			//define source
			ISource jellCode = new FileSource(GetFilePath("CompareTest.jell"));
			//other source possibilities
			//ISource jellCode = new StringSource("function Compare(x,y) {/*debugger;*/ return x.Id.CompareTo(y.Id);}", "CompareTest.jell");
			//ISource jellCode = new StringSource("function Compare(x,y) {/*debugger;*/ var x2=x.Name; var y2=y.Name; return x2==y2?0:x2<y2?-1:1; }", "CompareTest.jell");

			//Compile source code to .NET assembly.
			Assembly asm = Compile(jellCode);

			//Although compiler produces standard .NET assembly, it might be tricky to consume it. Therefore it's recommended to use Executor.
			Executor executor = new Executor(asm);
			executor.Execute(new string[0]);
			//Let's get Compare function compiled from source. It's standard .NET method but bit bended for JS compatibility purpose. This simplifies its usage in C#.
			Func<Item, Item, int> compareFunc = executor.ExternalVariables.GetFunc<Item, Item, int>("Compare");

			//Test 1 - simple compare.
			int res1 = compareFunc(new Item { Id=1 }, new Item { Id=2 });
			int res2 = compareFunc(new Item { Id=1 }, new Item { Id=1 });
			int res3 = compareFunc(new Item { Id=2 }, new Item { Id=1 });

			//Test 2 - compare bunch of items.
			List<Item> list = new List<Item> { new Item { Id=15 }, new Item { Id=1 }, new Item { Id=8 }, new Item { Id=35 }, new Item { Id=1280 }, new Item { Id=11 }, new Item { Id=12 }, new Item { Id=115 }, new Item { Id=12801 }, new Item { Id=128 } };
			list.Sort(new Comparison<Item>(compareFunc));
		}

		#region compile
		static Assembly Compile(ISource jellCode)
		{
			//See manual to get further information about compilation and options.
			using (MemoryStream dll = new MemoryStream())
			using (MemoryStream pdb = new MemoryStream())
			{
				Compiler.Compile(jellCode, dll,
					new AssemblyName("CompareTest") { Version = new Version(1, 1, 5, 1) },
					new CompilerOptions()
					{
						FileKind = FileKind.Dll,
						RuntimeMethodsUsage = RuntimeMethodsUsage.Call,
						DontUseDynamicJsMembers = true,
						Debug = new DebugOptions { Debug = true, EmbedSourceCode = jellCode is StringSource, Pdb = pdb }
					});

				dll.Position = 0;
				pdb.Position = 0;
				return System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(dll, pdb);
			}
		}
		#endregion compile

		class Item
		{
			public int Id { get; set; }
			public string Name => Id.ToString();

			public override string ToString()
			{
				return Id.ToString();
			}
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

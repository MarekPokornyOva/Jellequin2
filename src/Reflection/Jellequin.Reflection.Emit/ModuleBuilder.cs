#region using
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class ModuleBuilder:Module
	{
		public ModuleBuilder(AssemblyName assemblyName)
		{
			AssemblyName=assemblyName;
		}

		public AssemblyName AssemblyName { get; }

		Dictionary<string,TypeBuilder> _types = new Dictionary<string,TypeBuilder>();
		public TypeBuilder DefineType(string name,string @namespace,TypeAttributes attributes,Type baseType,Type[] interfaces)
			=> _types.AddWithReturn(name,new TypeBuilder(this,name,@namespace,attributes,baseType,interfaces));

		public IReadOnlyCollection<TypeBuilder> Types => _types.Values;

		public override Type[] GetTypes()
			=> _types.Values.ToArray();

		public SymbolWriter DefineDocument(string name)
			=> new SymbolWriter(name);

		public override Type GetType(string className)
			=> _types.TryGetValue(className,out TypeBuilder result) ? result : null;
	}
}

#region using
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class AssemblyBuilder
	{
		readonly ModuleBuilder _modBldr;
		readonly MethodBuilder _entryPoint;

		public AssemblyBuilder(ModuleBuilder moduleBuilder,MethodBuilder entryPoint)
		{
			_modBldr=moduleBuilder;
			_entryPoint=entryPoint;
		}

		public void Save(Stream assembly,SaveOptions options)
			=> new AssemblyWriter().Write(_modBldr,_entryPoint,assembly,options);

		public Task SaveAsync(Stream assembly,SaveOptions options)
			=> new AssemblyWriter().WriteAsync(_modBldr,_entryPoint,assembly,options);
	}
}

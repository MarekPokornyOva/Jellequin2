#region using
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class SaveOptions
	{
		/*public SaveOptions(Assembly frameworkAssembly)
			=> FrameworkAssembly=frameworkAssembly;

		public Assembly FrameworkAssembly { get; set; }*/
		public SymbolsSaveOptions Symbols { get; set; }
		public Stream Icon { get; set; }
		public IEnumerable<CustomAttributeData> AssemblyAttributes { get; set; }
}

	public class SymbolsSaveOptions
	{
		public SourceHashAlgorithm HashAlgorithm { get; set; } = SourceHashAlgorithm.Sha1;
		public Stream Pdb { get; set; }
		public bool EmbedSource { get; set; }
		public ISource Code { get; set; }
	}
}

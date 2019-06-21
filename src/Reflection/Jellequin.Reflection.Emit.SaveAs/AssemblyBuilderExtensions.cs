#region using
using Jellequin.Reflection.Emit;
using System.IO;
#endregion using

namespace System.Reflection.Emit
{
	public static class AssemblyBuilderExtensions
	{
		public static void SaveAs(this Assembly assembly,string assemblyFileName)
			=> SaveAs(assembly,assemblyFileName,PortableExecutableKinds.ILOnly,ImageFileMachine.I386);

		public static void SaveAs(this Assembly assembly,string assemblyFileName,PortableExecutableKinds portableExecutableKind,ImageFileMachine imageFileMachine)
		{
			if (portableExecutableKind!=PortableExecutableKinds.ILOnly)
				throw new NotSupportedException();
			if (imageFileMachine!=ImageFileMachine.I386)
				throw new NotSupportedException();

			ModuleBuilder modB = new ModuleBuilder(assembly.GetName());
			AssemblyCopier copier = new AssemblyCopier(assembly,TargetFramework.SameAsSource);
			CopyMap map = copier.CopyTo(modB);
			AssemblyBuilder asmBuilder = new AssemblyBuilder(modB,map.EntryPoint);
			using (FileStream output = File.Create(assemblyFileName))
				asmBuilder.Save(output,new SaveOptions() { AssemblyAttributes=copier.CopyCustomAttributes(assembly.CustomAttributes) });
		}

		public static void SaveAs(this Assembly assembly,Stream output)
		{
			ModuleBuilder modB = new ModuleBuilder(assembly.GetName());
			AssemblyCopier copier = new AssemblyCopier(assembly,TargetFramework.SameAsSource);
			CopyMap map = copier.CopyTo(modB);
			new AssemblyBuilder(modB,map.EntryPoint).Save(output,new SaveOptions() { AssemblyAttributes=copier.CopyCustomAttributes(assembly.CustomAttributes) });
		}
	}
}

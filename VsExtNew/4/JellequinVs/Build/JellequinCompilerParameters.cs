#region using
using Jellequin.Compiler;
using System;
using System.CodeDom.Compiler;
#endregion using

namespace JellequinVs.Build
{
    [Serializable]
    public class JellequinCompilerParameters : CompilerParameters
    {
        public FileKind FileKind { get; set; }
        public RuntimeMethodsUsage RuntimeMethodsUsage { get; set; }
        public string AssemblyName { get; set; }
        public Version Version { get; set; }
        //public LocalsGeneration LocalsGeneration { get; set; }
    }
}

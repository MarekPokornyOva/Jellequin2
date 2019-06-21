#region using
using Jellequin.Compiler;
using System;
using System.CodeDom.Compiler;
using System.IO;
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
        public byte[] Icon;
    }
}

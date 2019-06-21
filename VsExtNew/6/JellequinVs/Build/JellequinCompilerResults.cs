#region using
using System;
using System.CodeDom.Compiler;
#endregion using

namespace JellequinVs.Build
{
    [Serializable]
    public class JellequinCompilerResults : CompilerResults
    {
        internal JellequinCompilerResults(TempFileCollection tempFiles) : base(tempFiles)
        { }

        public Exception CompilerException { get; set; }
    }
}

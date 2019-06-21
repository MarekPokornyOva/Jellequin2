#region using
using System;
using System.CodeDom.Compiler;
#endregion using

namespace JellequinVs.Build
{
    public class JellequinCodeProvider : CodeDomProvider
    {
        readonly JellequinCodeCompiler _compiler = new JellequinCodeCompiler();

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public override ICodeCompiler CreateCompiler()
        {
            return _compiler;
        }

        [Obsolete("Callers should not use the ICodeGenerator interface and should instead use the methods directly on the CodeDomProvider class. Those inheriting from CodeDomProvider must still implement this interface, and should exclude this warning or also obsolete this method.")]
        public override ICodeGenerator CreateGenerator()
        {
            throw new NotImplementedException();
        }
    }
}

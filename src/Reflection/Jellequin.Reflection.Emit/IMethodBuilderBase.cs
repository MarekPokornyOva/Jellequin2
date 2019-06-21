using System;
using System.Reflection;

namespace Jellequin.Reflection.Emit
{
	public interface IMethodBuilderBase:IMemberBuilderBase
	{
		MethodAttributes Attributes { get; }
		void SetParameters(Type[] types);
		ParameterBuilder DefineParameter(int position,ParameterAttributes attributes,string name);
		MethodImplAttributes GetMethodImplementationFlags();
		void SetImplementationFlags(MethodImplAttributes value);
		ParameterInfo[] GetParameters();
		ParameterBuilder[] GetParameterBuilders();
		bool IsStatic { get; }
		bool IsPublic { get; }
		ILGenerator GetILGenerator();
		MethodBody GetMethodBody();
		MethodBodyBuilder GetMethodBodyBuilder();
		bool InitLocals { get; set; }
	}
}

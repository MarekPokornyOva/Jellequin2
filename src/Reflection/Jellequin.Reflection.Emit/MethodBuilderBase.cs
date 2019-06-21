using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Jellequin.Reflection.Emit
{
	//http://msdn.microsoft.com/en-us/library/e514eeby.aspx
	//[DebuggerDisplay("{ValueToShow,nq}", Name = "{Key,nq}", Type = "{TypeToShow,nq}")]
	//[DebuggerDisplay("{DebugView,nq}")]
	class MethodBuilderBase:MemberBuilderBase
	{
		internal MethodAttributes Attributes { get; }
		internal Type[] ParameterTypes;
		List<ParameterBuilder> _parameters = new List<ParameterBuilder>();
		public CallingConventions CallingConvention { get; }

		internal MethodBuilderBase(TypeBuilder declaringType,string name,MethodAttributes attributes,CallingConventions callingConvention) : base(declaringType,name)
		{
			Attributes=attributes;
			_body=attributes.HasFlag(MethodAttributes.Abstract)?null:new MethodBodyBuilder(new ILGenerator(this));
			CallingConvention=callingConvention;
		}

		internal void SetParameters(Type[] types)
			=> ParameterTypes=types;

		internal ParameterBuilder DefineParameter(int index,ParameterAttributes attributes,string name)
		{
			ParameterBuilder result = new ParameterBuilder(index,attributes,name,ParameterTypes[index-1]);
			_parameters.Add(result);
			return result;
		}

		MethodImplAttributes _implFlags;
		internal MethodImplAttributes GetMethodImplementationFlags()
			=> _implFlags;
		internal void SetImplementationFlags(MethodImplAttributes value)
			=> _implFlags=value;

		internal ParameterBuilder[] GetParameters() => _parameters.Count==ParameterTypes.Length ? _parameters.ToArray() : throw new ReflectionException(ReflectionExceptionReason.UndefinedMethodParameter);

		internal bool IsStatic => (Attributes&MethodAttributes.Static)==MethodAttributes.Static;

		internal bool IsPublic => (Attributes&MethodAttributes.Public)==MethodAttributes.Public;

		MethodBodyBuilder _body;
		internal ILGenerator GetILGenerator()
			=> GetMethodBody()?.ILGenerator;

		internal MethodBodyBuilder GetMethodBody() => _implFlags.HasFlag(MethodImplAttributes.CodeTypeMask)?null:_body;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(Name);
			sb.Append("(");
			bool first = true;
			foreach (Type t in ParameterTypes)
			{
				if (first)
					first=false;
				else
					sb.Append(",");
				sb.Append(t.ToString());
			}
			sb.Append(")");
			return sb.ToString();
		}

		internal bool InitLocals { get; set; }
	}
}

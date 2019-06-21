#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class MethodBuilder:MethodInfo, IMethodBuilderBase
	{
		readonly MethodBuilderBase _base;
		internal MethodBuilder(TypeBuilder declaringType,string name,MethodAttributes attributes,CallingConventions callingConvention)
			=> _base=new MethodBuilderBase(declaringType,name,attributes,callingConvention);

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

		public override MethodAttributes Attributes => _base.Attributes;

		public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

		public override Type DeclaringType => _base.DeclaringType;

		public override string Name => _base.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override MethodInfo GetBaseDefinition() => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => _base.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _base.GetCustomAttributesData();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodImplAttributes GetMethodImplementationFlags() => _base.GetMethodImplementationFlags();

		public override ParameterInfo[] GetParameters() => _base.GetParameters();
		public ParameterBuilder[] GetParameterBuilders() => _base.GetParameters();

		public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture) => throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		Type _returnType;
		public override Type ReturnType => _returnType;

		public void SetReturnType(Type type)
			=> _returnType=type;

		public override string ToString()
			=> $"{ReturnType.ToString()} {_base.ToString()}";

		bool _isGenericNondef;
		Type[] _genericParameters;
		public GenericParameterBuilder[] DefineGenericParameters(string[] argumentNames)
		{
			GenericParameterBuilder[] result = new GenericParameterBuilder[argumentNames.Length];
			int a = 0;
			foreach (string name in argumentNames)
			{
				result[a]=new GenericParameterBuilder(name,a,this);
				a++;
			}
			_genericParameters=result;
			return result;
		}

		public override Type[] GetGenericArguments() => _genericParameters;

		MethodBuilder _genericDefMeth;

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			if (typeArguments==null)
				throw new ArgumentNullException(nameof(typeArguments));

			MethodBuilder result = new MethodBuilder((TypeBuilder)DeclaringType,Name,Attributes,CallingConvention) { _genericParameters=typeArguments,_isGenericNondef=true,_genericDefMeth=this };
			result.SetParameters(_base.ParameterTypes);
			foreach (ParameterBuilder parm in GetParameters())
				result.DefineParameter(parm.Position,parm.Attributes,parm.Name);
			result.SetReturnType(ReturnType);
			return result;
		}

		public override bool ContainsGenericParameters => _genericParameters!=null&&_genericParameters.Length!=0;

		public override MethodInfo GetGenericMethodDefinition() => _genericDefMeth;
		public MethodBuilder GetGenericMethodBuilderDefinition() => _genericDefMeth;

		public override bool IsGenericMethod => ContainsGenericParameters;

		public override bool IsGenericMethodDefinition => IsGenericMethod&&(!_isGenericNondef);

		public override Module Module => _base.Module;

		public void SetCustomAttribute(CustomAttributeData attribute) => _base.SetCustomAttribute(attribute);
		public override CallingConventions CallingConvention => _base.CallingConvention;
		public void SetParameters(Type[] types) => _base.SetParameters(types);
		public ParameterBuilder DefineParameter(int position,ParameterAttributes attributes,string name) => _base.DefineParameter(position,attributes,name);
		public void SetImplementationFlags(MethodImplAttributes value) => _base.SetImplementationFlags(value);
		public ILGenerator GetILGenerator() => _base.GetILGenerator();
		public override MethodBody GetMethodBody() => _base.GetMethodBody();
		public MethodBodyBuilder GetMethodBodyBuilder() => _base.GetMethodBody();
		public bool InitLocals { get => _base.InitLocals; set => _base.InitLocals=value; }
	}
}

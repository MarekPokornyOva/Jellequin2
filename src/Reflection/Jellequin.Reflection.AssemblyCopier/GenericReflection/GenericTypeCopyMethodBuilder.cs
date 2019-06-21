#region using
using System;
using System.Globalization;
using System.Reflection;
using Jellequin.Reflection.Emit;
#endregion using

namespace Jellequin.Reflection.Emit.Internal
{
	class GenericTypeCopyMethodBuilder:MethodInfo
	{
		MethodInfo _originalMethod;
		Type[] _genericParameters;
		ParameterInfo[] _parameters;
		MethodInfo _genericMethodDefinition;

		internal GenericTypeCopyMethodBuilder(Type declaringType,MethodInfo originalMethod,Type[] genericParameters,Type returnType,ParameterInfo[] parameters,MethodInfo genericMethodDefinition)
		{
			DeclaringType=declaringType;
			_originalMethod=originalMethod;
			_genericParameters=genericParameters;
			ReturnType=returnType;
			_parameters = parameters;
			_genericMethodDefinition=genericMethodDefinition;
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

		public override MethodAttributes Attributes => _originalMethod.Attributes;

		public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

		public override Type DeclaringType { get; }

		public override string Name => _originalMethod.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override MethodInfo GetBaseDefinition() => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodImplAttributes GetMethodImplementationFlags() => _originalMethod.MethodImplementationFlags;

		public override ParameterInfo[] GetParameters() => _parameters;

		public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			=> throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override bool ContainsGenericParameters => _genericParameters!=null;

		public override Type[] GetGenericArguments() => _genericParameters;

		public override Type ReturnType { get; }

		public override MethodInfo GetGenericMethodDefinition() => _genericMethodDefinition;

		public override bool IsGenericMethod => true;

		public override bool IsGenericMethodDefinition => false;
	}
}

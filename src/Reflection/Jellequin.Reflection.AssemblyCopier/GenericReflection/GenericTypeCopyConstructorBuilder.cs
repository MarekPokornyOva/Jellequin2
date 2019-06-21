#region using
using System;
using System.Globalization;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit.Internal
{
	class GenericTypeCopyConstructorBuilder:ConstructorInfo
	{
		ConstructorInfo _originalConstructor;
		ParameterInfo[] _parameters;

		internal GenericTypeCopyConstructorBuilder(Type declaringType,ConstructorInfo originalConstructor,ParameterInfo[] parameters)
		{
			DeclaringType=declaringType;
			_originalConstructor=originalConstructor;
			_parameters=parameters;
		}

		public override MethodAttributes Attributes => _originalConstructor.Attributes;

		public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

		public override Type DeclaringType { get; }

		public override string Name => _originalConstructor.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodImplAttributes GetMethodImplementationFlags() => throw new NotImplementedException();

		public override ParameterInfo[] GetParameters() => _parameters;

		public override object Invoke(BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			 => throw new NotImplementedException();

		public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			=> throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();
	}
}

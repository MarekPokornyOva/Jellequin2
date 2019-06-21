#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class ConstructorBuilder:ConstructorInfo,IMethodBuilderBase
	{
		readonly MethodBuilderBase _base;
		internal ConstructorBuilder(TypeBuilder declaringType,string name,MethodAttributes attributes,CallingConventions callingConventions,Type[] parameterTypes)
		{
			_base=new MethodBuilderBase(declaringType,name,attributes,callingConventions);
			SetImplementationFlags(MethodImplAttributes.IL);
			SetParameters(parameterTypes);
		}

		public override MethodAttributes Attributes => _base.Attributes;

		public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

		public override Type DeclaringType => _base.DeclaringType;

		public override string Name => _base.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => _base.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _base.GetCustomAttributesData();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodImplAttributes GetMethodImplementationFlags() => _base.GetMethodImplementationFlags();

		public override ParameterInfo[] GetParameters() => _base.GetParameters();
		public ParameterBuilder[] GetParameterBuilders() => _base.GetParameters();

		public override object Invoke(BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture) => throw new NotImplementedException();

		public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture) => throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

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

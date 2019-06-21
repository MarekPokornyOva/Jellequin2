#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class PropertyBuilder:PropertyInfo,IMemberBuilderBase,IConstantValueMember
	{
		readonly MemberBuilderBase _base;
		readonly ConstantValueContainer _constantValueContainer = new ConstantValueContainer();
		MethodBuilder _getter, _setter;
		internal PropertyBuilder(TypeBuilder declaringType,string name,PropertyAttributes attributes,Type propertyType)
		{
			_base=new MemberBuilderBase(declaringType,name);
			Attributes=attributes;
			PropertyType=propertyType??throw new ArgumentNullException(nameof(propertyType));
		}

		public override PropertyAttributes Attributes { get; }

		public override bool CanRead => _getter!=null;

		public override bool CanWrite => _setter!=null;

		public override Type PropertyType { get; }

		public override Type DeclaringType => _base.DeclaringType;

		public override string Name => _base.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override MethodInfo[] GetAccessors(bool nonPublic) => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => _base.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _base.GetCustomAttributesData();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodInfo GetGetMethod(bool nonPublic) => _getter;

		public override ParameterInfo[] GetIndexParameters() => throw new NotImplementedException();

		public override MethodInfo GetSetMethod(bool nonPublic) => _setter;

		public override object GetValue(object obj,BindingFlags invokeAttr,Binder binder,object[] index,CultureInfo culture) => throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override void SetValue(object obj,object value,BindingFlags invokeAttr,Binder binder,object[] index,CultureInfo culture) => throw new NotImplementedException();

		public override Module Module => _base.Module;

		public void SetGetMethod(MethodBuilder getter) => _getter=getter;

		public void SetSetMethod(MethodBuilder setter) => _setter=setter;

		public void SetCustomAttribute(CustomAttributeData attribute) => _base.SetCustomAttribute(attribute);

		public void SetConstant(object value) => _constantValueContainer.SetConstant(value);

		public bool HasRawConstantValue => _constantValueContainer.HasRawConstantValue;

		public override object GetRawConstantValue() => _constantValueContainer.GetRawConstantValue();

		public MethodBuilder GetMethodBuilder => _getter;

		public MethodBuilder SetMethodBuilder => _setter;
	}
}

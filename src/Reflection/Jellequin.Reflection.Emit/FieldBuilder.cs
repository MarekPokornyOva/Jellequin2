#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class FieldBuilder:FieldInfo, IMemberBuilderBase, IConstantValueMember
	{
		readonly MemberBuilderBase _base;
		readonly ConstantValueContainer _constantValueContainer=new ConstantValueContainer();
		internal FieldBuilder(TypeBuilder declaringType,string name,FieldAttributes attributes,Type fieldType)
		{
			_base=new MemberBuilderBase(declaringType,name);
			Attributes=attributes;
			FieldType=fieldType;
		}

		public override FieldAttributes Attributes { get; }

		public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

		public override Type FieldType { get; }

		public override Type DeclaringType => _base.DeclaringType;

		public override string Name => _base.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => _base.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _base.GetCustomAttributesData();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override object GetValue(object obj) => throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override void SetValue(object obj,object value,BindingFlags invokeAttr,Binder binder,CultureInfo culture) => throw new NotImplementedException();

		public void SetCustomAttribute(CustomAttributeData attribute) => _base.SetCustomAttribute(attribute);

		public void SetConstant(object value) => _constantValueContainer.SetConstant(value);

		public bool HasRawConstantValue => _constantValueContainer.HasRawConstantValue;

		public override object GetRawConstantValue() => _constantValueContainer.GetRawConstantValue();
	}
}

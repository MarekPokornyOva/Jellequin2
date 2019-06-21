using System;
using System.Globalization;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	class GenericTypeCopyFieldBuilder:FieldInfo
	{
		FieldInfo _originalField;

		public GenericTypeCopyFieldBuilder(Type declaringType,FieldInfo originalField,Type fieldType)
		{
			DeclaringType=declaringType;
			_originalField=originalField;
			FieldType=fieldType;
		}

		public override FieldAttributes Attributes => throw new NotImplementedException();

		public override RuntimeFieldHandle FieldHandle => throw new NotImplementedException();

		public override Type FieldType { get; }

		public override Type DeclaringType { get; }

		public override string Name => _originalField.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override object[] GetCustomAttributes(bool inherit) => throw new NotImplementedException();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override object GetValue(object obj) => throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override void SetValue(object obj,object value,BindingFlags invokeAttr,Binder binder,CultureInfo culture)
			=> throw new NotImplementedException();
	}
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	class FieldAssemblyFixer:FieldInfo
	{
		readonly FieldInfo _fi;
		readonly IAssemblyFixer _assemblyFixer;

		public FieldAssemblyFixer(FieldInfo fieldInfo,IAssemblyFixer assemblyFixer)
		{
			_fi=fieldInfo;
			_assemblyFixer=assemblyFixer;
		}

		public override FieldAttributes Attributes => _fi.Attributes;

		public override RuntimeFieldHandle FieldHandle => _fi.FieldHandle;

		public override Type FieldType => _assemblyFixer.FixType(_fi.FieldType);

		public override Type DeclaringType => _assemblyFixer.FixType(_fi.DeclaringType);

		public override string Name => _fi.Name;

		public override Type ReflectedType => _assemblyFixer.FixType(_fi.ReflectedType);

		public override object[] GetCustomAttributes(bool inherit) => _fi.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => GetCustomAttributes(attributeType,inherit);

		public override object GetValue(object obj) => _fi.GetValue(obj);

		public override bool IsDefined(Type attributeType,bool inherit) => _fi.IsDefined(attributeType,inherit);

		public override void SetValue(object obj,object value,BindingFlags invokeAttr,Binder binder,CultureInfo culture)
			=> _fi.SetValue(obj,value,invokeAttr,binder,culture);

		public override IEnumerable<CustomAttributeData> CustomAttributes => _fi.CustomAttributes;

		public override bool Equals(object obj) => _fi.Equals(obj);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _fi.GetCustomAttributesData();

		public override int GetHashCode() => _fi.GetHashCode();

		public override Type[] GetOptionalCustomModifiers() => _fi.GetOptionalCustomModifiers();

		public override object GetRawConstantValue() => _fi.GetRawConstantValue();

		public override Type[] GetRequiredCustomModifiers() => _fi.GetRequiredCustomModifiers();

		public override object GetValueDirect(TypedReference obj) => _fi.GetValueDirect(obj);

		public override bool IsSecurityCritical => _fi.IsSecurityCritical;

		public override bool IsSecuritySafeCritical => _fi.IsSecuritySafeCritical;

		public override bool IsSecurityTransparent => _fi.IsSecurityTransparent;

		public override MemberTypes MemberType => _fi.MemberType;

		public override int MetadataToken => _fi.MetadataToken;

		public override Module Module => _fi.Module;

		public override void SetValueDirect(TypedReference obj,object value) => _fi.SetValueDirect(obj,value);

		public override string ToString() => _fi.ToString();
	}
}
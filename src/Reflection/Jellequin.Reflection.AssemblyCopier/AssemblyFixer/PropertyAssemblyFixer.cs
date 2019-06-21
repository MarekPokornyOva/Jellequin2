using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	internal class PropertyAssemblyFixer:PropertyInfo
	{
		readonly PropertyInfo _pi;
		readonly IAssemblyFixer _assemblyFixer;

		public PropertyAssemblyFixer(PropertyInfo propertyInfo,IAssemblyFixer assemblyFixer)
		{
			_pi=propertyInfo;
			_assemblyFixer=assemblyFixer;
		}

		public override PropertyAttributes Attributes => _pi.Attributes;

		public override bool CanRead => _pi.CanRead;

		public override bool CanWrite => _pi.CanWrite;

		public override Type PropertyType => _assemblyFixer.FixType(_pi.PropertyType);

		public override Type DeclaringType => _assemblyFixer.FixType(_pi.DeclaringType);

		public override string Name => _pi.Name;

		public override Type ReflectedType => _assemblyFixer.FixType(_pi.ReflectedType);

		public override MethodInfo[] GetAccessors(bool nonPublic) => _pi.GetAccessors(nonPublic);

		public override object[] GetCustomAttributes(bool inherit) => _pi.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => _pi.GetCustomAttributes(attributeType,inherit);

		public override MethodInfo GetGetMethod(bool nonPublic) => _pi.GetGetMethod(nonPublic);

		public override ParameterInfo[] GetIndexParameters() => _pi.GetIndexParameters();

		public override MethodInfo GetSetMethod(bool nonPublic) => _pi.GetSetMethod(nonPublic);

		public override object GetValue(object obj,BindingFlags invokeAttr,Binder binder,object[] index,CultureInfo culture)
			 => _pi.GetValue(obj,invokeAttr,binder,index,culture);

		public override bool IsDefined(Type attributeType,bool inherit) => _pi.IsDefined(attributeType,inherit);

		public override void SetValue(object obj,object value,BindingFlags invokeAttr,Binder binder,object[] index,CultureInfo culture)
			 => _pi.SetValue(obj,value,invokeAttr,binder,index,culture);

		public override IEnumerable<CustomAttributeData> CustomAttributes => _pi.CustomAttributes;

		public override bool Equals(object obj)
		{
			return _pi.Equals(obj);
		}

		public override object GetConstantValue()
		{
			return _pi.GetConstantValue();
		}

		public override IList<CustomAttributeData> GetCustomAttributesData()
		{
			return _pi.GetCustomAttributesData();
		}

		public override int GetHashCode()
		{
			return _pi.GetHashCode();
		}

		public override MethodInfo GetMethod => _pi.GetMethod;

		public override Type[] GetOptionalCustomModifiers()
		{
			return _pi.GetOptionalCustomModifiers();
		}

		public override object GetRawConstantValue()
		{
			return _pi.GetRawConstantValue();
		}

		public override Type[] GetRequiredCustomModifiers()
		{
			return _pi.GetRequiredCustomModifiers();
		}

		public override object GetValue(object obj,object[] index)
		{
			return _pi.GetValue(obj,index);
		}

		public override MemberTypes MemberType => _pi.MemberType;

		public override int MetadataToken => _pi.MetadataToken;

		public override Module Module => _pi.Module;

		public override MethodInfo SetMethod => _pi.SetMethod;

		public override void SetValue(object obj,object value,object[] index)
		{
			_pi.SetValue(obj,value,index);
		}

		public override string ToString()
		{
			return _pi.ToString();
		}
	}
}
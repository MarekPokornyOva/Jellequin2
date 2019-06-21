using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	class ConstructorAssemblyFixer:ConstructorInfo
	{
		readonly ConstructorInfo _ci;
		readonly IAssemblyFixer _assemblyFixer;

		public ConstructorAssemblyFixer(ConstructorInfo constructorInfo,IAssemblyFixer assemblyFixer)
		{
			_ci=constructorInfo;
			_assemblyFixer=assemblyFixer;
		}

		public override MethodAttributes Attributes => _ci.Attributes;

		public override RuntimeMethodHandle MethodHandle => _ci.MethodHandle;

		public override Type DeclaringType => _assemblyFixer.FixType(_ci.DeclaringType);

		public override string Name => _ci.Name;

		public override Type ReflectedType => _assemblyFixer.FixType(_ci.ReflectedType);

		public override object[] GetCustomAttributes(bool inherit) => _ci.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => _ci.GetCustomAttributes(attributeType,inherit);

		public override MethodImplAttributes GetMethodImplementationFlags() => _ci.GetMethodImplementationFlags();

		public override ParameterInfo[] GetParameters() => _ci.GetParameters().Select(_assemblyFixer.FixParameter).ToArray();

		public override object Invoke(BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			 => _ci.Invoke(invokeAttr,binder,parameters,culture);

		public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			 => _ci.Invoke(obj,invokeAttr,binder,parameters,culture);

		public override bool IsDefined(Type attributeType,bool inherit) => _ci.IsDefined(attributeType,inherit);

		public override CallingConventions CallingConvention => _ci.CallingConvention;

		public override bool ContainsGenericParameters => _ci.ContainsGenericParameters;

		public override IEnumerable<CustomAttributeData> CustomAttributes => _ci.CustomAttributes;

		public override bool Equals(object obj) => _ci.Equals(obj);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _ci.GetCustomAttributesData();

		public override Type[] GetGenericArguments() => _ci.GetGenericArguments();

		public override int GetHashCode() => _ci.GetHashCode();

		public override MethodBody GetMethodBody() => _ci.GetMethodBody();

		public override bool IsGenericMethod => _ci.IsGenericMethod;

		public override bool IsGenericMethodDefinition => _ci.IsGenericMethodDefinition;

		public override bool IsSecurityCritical => _ci.IsSecurityCritical;

		public override bool IsSecuritySafeCritical => _ci.IsSecuritySafeCritical;

		public override bool IsSecurityTransparent => _ci.IsSecurityTransparent;

		public override MemberTypes MemberType => _ci.MemberType;

		public override int MetadataToken => _ci.MetadataToken;

		public override MethodImplAttributes MethodImplementationFlags => _ci.MethodImplementationFlags;

		public override Module Module => _ci.Module;

		public override string ToString() => _ci.ToString();
	}
}

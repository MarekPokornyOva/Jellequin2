using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	class MethodAssemblyFixer:MethodInfo
	{
		readonly MethodInfo _mi;
		readonly IAssemblyFixer _assemblyFixer;

		public MethodAssemblyFixer(MethodInfo methodInfo,IAssemblyFixer assemblyFixer)
		{
			_mi=methodInfo;
			_assemblyFixer=assemblyFixer;
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes => _mi.ReturnTypeCustomAttributes;

		public override MethodAttributes Attributes => _mi.Attributes;

		public override RuntimeMethodHandle MethodHandle => _mi.MethodHandle;

		public override Type DeclaringType => _assemblyFixer.FixType(_mi.DeclaringType);

		public override string Name => _mi.Name;

		public override Type ReflectedType => _assemblyFixer.FixType(_mi.ReflectedType);

		public override MethodInfo GetBaseDefinition() => _mi.GetBaseDefinition();

		public override object[] GetCustomAttributes(bool inherit) => _mi.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => _mi.GetCustomAttributes(attributeType,inherit);

		public override MethodImplAttributes GetMethodImplementationFlags() => _mi.GetMethodImplementationFlags();

		public override ParameterInfo[] GetParameters() => _mi.GetParameters().Select(_assemblyFixer.FixParameter).ToArray();

		public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			=> _mi.Invoke(obj,invokeAttr,binder,parameters,culture);

		public override bool IsDefined(Type attributeType,bool inherit)
			=> _mi.IsDefined(attributeType,inherit);

		public override CallingConventions CallingConvention => _mi.CallingConvention;

		public override bool ContainsGenericParameters => _mi.ContainsGenericParameters;

		public override Delegate CreateDelegate(Type delegateType) => _mi.CreateDelegate(delegateType);

		public override Delegate CreateDelegate(Type delegateType,object target) => _mi.CreateDelegate(delegateType,target);

		public override IEnumerable<CustomAttributeData> CustomAttributes => _mi.CustomAttributes;

		public override bool Equals(object obj) => _mi.Equals(obj);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _mi.GetCustomAttributesData();

		public override Type[] GetGenericArguments() => _mi.GetGenericArguments();

		public override MethodInfo GetGenericMethodDefinition() => _mi.GetGenericMethodDefinition();

		public override int GetHashCode() => _mi.GetHashCode();

		public override MethodBody GetMethodBody() => _mi.GetMethodBody();

		public override bool IsGenericMethod => _mi.IsGenericMethod;

		public override bool IsGenericMethodDefinition => _mi.IsGenericMethodDefinition;

		public override bool IsSecurityCritical => _mi.IsSecurityCritical;

		public override bool IsSecuritySafeCritical => _mi.IsSecuritySafeCritical;

		public override bool IsSecurityTransparent => _mi.IsSecurityTransparent;

		public override MethodInfo MakeGenericMethod(params Type[] typeArguments) => _mi.MakeGenericMethod(typeArguments);

		public override MemberTypes MemberType => _mi.MemberType;

		public override int MetadataToken => _mi.MetadataToken;

		public override MethodImplAttributes MethodImplementationFlags => _mi.MethodImplementationFlags;

		public override Module Module => _mi.Module;

		public override ParameterInfo ReturnParameter => _mi.ReturnParameter;

		public override Type ReturnType => _assemblyFixer.FixType(_mi.ReturnType);

		public override string ToString() => _mi.ToString();
	}
}
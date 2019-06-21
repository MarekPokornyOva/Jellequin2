using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Jellequin.Reflection.Emit.Internal
{
	class TypeAssemblyFixer:Type
	{
		readonly Type _type;
		readonly Assembly _asm;
		readonly IAssemblyFixer _assemblyFixer;

		public TypeAssemblyFixer(Type type,Assembly assembly,IAssemblyFixer assemblyFixer)
		{
			_type=type;
			_asm=assembly;
			_assemblyFixer=assemblyFixer;
		}

		public override Assembly Assembly => _asm;

		public override string AssemblyQualifiedName => _type.AssemblyQualifiedName;

		public override Type BaseType => _type.BaseType;

		public override string FullName => _type.FullName;

		public override Guid GUID => _type.GUID;

		public override Module Module => _type.Module;

		public override string Namespace => _type.Namespace;

		public override Type UnderlyingSystemType => _type.UnderlyingSystemType;

		public override string Name => _type.Name;

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => _type.GetConstructors(bindingAttr).Select(_assemblyFixer.FixConstructor).ToArray();

		public override object[] GetCustomAttributes(bool inherit) => _type.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => _type.GetCustomAttributes(attributeType,inherit);

		public override Type GetElementType() => _assemblyFixer.FixType(_type.GetElementType());

		public override EventInfo GetEvent(string name,BindingFlags bindingAttr) => _type.GetEvent(name,bindingAttr);

		public override EventInfo[] GetEvents(BindingFlags bindingAttr) => _type.GetEvents(bindingAttr);

		public override FieldInfo GetField(string name,BindingFlags bindingAttr) => _assemblyFixer.FixField(_type.GetField(name,bindingAttr));

		public override FieldInfo[] GetFields(BindingFlags bindingAttr) => _type.GetFields(bindingAttr).Select(_assemblyFixer.FixField).ToArray();

		public override Type GetInterface(string name,bool ignoreCase) => _type.GetInterface(name,ignoreCase);

		public override Type[] GetInterfaces() => _type.GetInterfaces();

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => _type.GetMembers(bindingAttr);

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => _type.GetMethods(bindingAttr).Select(_assemblyFixer.FixMethod).ToArray();

		public override Type GetNestedType(string name,BindingFlags bindingAttr) => _type.GetNestedType(name,bindingAttr);

		public override Type[] GetNestedTypes(BindingFlags bindingAttr) => _type.GetNestedTypes(bindingAttr);

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => _type.GetProperties(bindingAttr);

		public override object InvokeMember(string name,BindingFlags invokeAttr,Binder binder,object target,object[] args,ParameterModifier[] modifiers,CultureInfo culture,string[] namedParameters)
			 => _type.InvokeMember(name,invokeAttr,binder,target,args,modifiers,culture,namedParameters);

		public override bool IsDefined(Type attributeType,bool inherit) => _type.IsDefined(attributeType,inherit);

		protected override TypeAttributes GetAttributeFlagsImpl() => _type.Attributes;

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr,Binder binder,CallingConventions callConvention,Type[] types,ParameterModifier[] modifiers)
			 => _assemblyFixer.FixConstructor(_type.GetConstructor(bindingAttr,binder,callConvention,types,modifiers));

		protected override MethodInfo GetMethodImpl(string name,BindingFlags bindingAttr,Binder binder,CallingConventions callConvention,Type[] types,ParameterModifier[] modifiers)
			 => _assemblyFixer.FixMethod(_type.GetMethod(name,bindingAttr,binder,callConvention,types,modifiers));

		protected override PropertyInfo GetPropertyImpl(string name,BindingFlags bindingAttr,Binder binder,Type returnType,Type[] types,ParameterModifier[] modifiers)
			 => _type.GetProperty(name,bindingAttr,binder,returnType,types,modifiers);

		protected override bool HasElementTypeImpl() => _type.HasElementType;

		protected override bool IsArrayImpl() => _type.IsArray;

		protected override bool IsByRefImpl() => _type.IsByRef;

		protected override bool IsCOMObjectImpl() => _type.IsCOMObject;

		protected override bool IsPointerImpl() => _type.IsPointer;

		protected override bool IsPrimitiveImpl() => _type.IsPrimitive;

		public override bool ContainsGenericParameters => _type.ContainsGenericParameters;

		public override IEnumerable<CustomAttributeData> CustomAttributes => _type.CustomAttributes;

		public override MethodBase DeclaringMethod => _assemblyFixer.FixMethod((MethodInfo)_type.DeclaringMethod);

		public override Type DeclaringType => _assemblyFixer.FixType(_type.DeclaringType);

		public override bool Equals(object o) => _type.Equals(o);

		public override bool Equals(Type o) => _type.Equals(o);

		public override Type[] FindInterfaces(TypeFilter filter,object filterCriteria)
			=> _type.FindInterfaces(filter,filterCriteria);

		public override MemberInfo[] FindMembers(MemberTypes memberType,BindingFlags bindingAttr,MemberFilter filter,object filterCriteria)
			=> _type.FindMembers(memberType,bindingAttr,filter,filterCriteria);

		public override GenericParameterAttributes GenericParameterAttributes => _type.GenericParameterAttributes;

		public override int GenericParameterPosition => _type.GenericParameterPosition;

		public override Type[] GenericTypeArguments => _type.GenericTypeArguments.Select(_assemblyFixer.FixType).ToArray();

		public override int GetArrayRank() => _type.GetArrayRank();

		public override IList<CustomAttributeData> GetCustomAttributesData() => _type.GetCustomAttributesData();

		public override MemberInfo[] GetDefaultMembers() => _type.GetDefaultMembers();

		public override string GetEnumName(object value) => _type.GetEnumName(value);

		public override string[] GetEnumNames() => _type.GetEnumNames();

		public override Type GetEnumUnderlyingType() => _type.GetEnumUnderlyingType();

		public override Array GetEnumValues() => _type.GetEnumValues();

		public override EventInfo[] GetEvents() => _type.GetEvents();

		public override Type[] GetGenericArguments() => _type.GetGenericArguments().Select(_assemblyFixer.FixType).ToArray();

		public override Type[] GetGenericParameterConstraints() => _type.GetGenericParameterConstraints().Select(_assemblyFixer.FixType).ToArray();

		public override Type GetGenericTypeDefinition() => _assemblyFixer.FixType(_type.GetGenericTypeDefinition());

		public override int GetHashCode() => _type.GetHashCode();

		public override InterfaceMapping GetInterfaceMap(Type interfaceType) => _type.GetInterfaceMap(interfaceType);

		public override MemberInfo[] GetMember(string name,BindingFlags bindingAttr)
			=> _type.GetMember(name,bindingAttr);

		public override MemberInfo[] GetMember(string name,MemberTypes type,BindingFlags bindingAttr)
			=> _type.GetMember(name,type,bindingAttr);

		protected override TypeCode GetTypeCodeImpl() => Type.GetTypeCode(_type);

		public override bool IsAssignableFrom(Type c) => _type.IsAssignableFrom(c);

		public override bool IsConstructedGenericType => _type.IsConstructedGenericType;

		protected override bool IsContextfulImpl() => _type.IsContextful;

		public override bool IsEnum => _type.IsEnum;

		public override bool IsEnumDefined(object value) => _type.IsEnumDefined(value);

		public override bool IsEquivalentTo(Type other) => _type.IsEquivalentTo(other);

		public override bool IsGenericParameter => _type.IsGenericParameter;

		public override bool IsGenericType => _type.IsGenericType;

		public override bool IsGenericTypeDefinition => _type.IsGenericTypeDefinition;

		public override bool IsInstanceOfType(object o) => _type.IsInstanceOfType(o);

		protected override bool IsMarshalByRefImpl() => _type.IsMarshalByRef;

		public override bool IsSecurityCritical => _type.IsSecurityCritical;

		public override bool IsSecuritySafeCritical => _type.IsSecuritySafeCritical;

		public override bool IsSecurityTransparent => _type.IsSecurityTransparent;

		public override bool IsSerializable => _type.IsSerializable;

		public override bool IsSubclassOf(Type c) => _type.IsSubclassOf(c);

		protected override bool IsValueTypeImpl() => _type.IsValueType;

		public override Type MakeArrayType() => _assemblyFixer.FixType(_type.MakeArrayType());

		public override Type MakeArrayType(int rank) => _assemblyFixer.FixType(_type.MakeArrayType(rank));

		public override Type MakeByRefType() => _assemblyFixer.FixType(_type.MakeByRefType());

		public override Type MakeGenericType(params Type[] typeArguments) => _assemblyFixer.FixType(_type.MakeGenericType(typeArguments));

		public override Type MakePointerType() => _assemblyFixer.FixType(_type.MakePointerType());

		public override MemberTypes MemberType => _type.MemberType;

		public override int MetadataToken => _type.MetadataToken;

		public override Type ReflectedType => _assemblyFixer.FixType(_type.ReflectedType);

		public override StructLayoutAttribute StructLayoutAttribute => _type.StructLayoutAttribute;

		public override string ToString() => _type.ToString();

		public override RuntimeTypeHandle TypeHandle => _type.TypeHandle;
	}
}
using System;
using System.Globalization;
using System.Reflection;

namespace Jellequin.Reflection.Emit
{
	public class GenericParameterBuilder:Type
	{
		static readonly MethodBase _declaringMethodFake = new MethodBaseFake();

		internal GenericParameterBuilder(string name,int position,object declarer)
		{
			Name=name??throw new ArgumentNullException(nameof(name));
			GenericParameterPosition=position;
			_declarer=declarer??throw new ArgumentNullException(nameof(declarer));
		}

		internal object _declarer;

		public override Assembly Assembly => throw new NotImplementedException();

		public override string AssemblyQualifiedName => throw new NotImplementedException();

		public override Type BaseType => throw new NotImplementedException();

		public override string FullName => Name;

		public override Type DeclaringType => _declarer as TypeBuilder;

		public MethodBuilder DeclaringMethodBuilder => _declarer as MethodBuilder;

		public override MethodBase DeclaringMethod => DeclaringMethodBuilder==null?null:_declaringMethodFake;

		public override Guid GUID => throw new NotImplementedException();

		public override Module Module => throw new NotImplementedException();

		public override string Namespace => "";

		public override Type UnderlyingSystemType => this;

		public override string Name { get; }

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			throw new NotImplementedException();
		}

		public override object[] GetCustomAttributes(Type attributeType,bool inherit)
		{
			throw new NotImplementedException();
		}

		public override Type GetElementType()
		{
			throw new NotImplementedException();
		}

		public override EventInfo GetEvent(string name,BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override EventInfo[] GetEvents(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override FieldInfo GetField(string name,BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override FieldInfo[] GetFields(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetInterface(string name,bool ignoreCase)
		{
			throw new NotImplementedException();
		}

		public override Type[] GetInterfaces()
		{
			throw new NotImplementedException();
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type GetNestedType(string name,BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override Type[] GetNestedTypes(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
		{
			throw new NotImplementedException();
		}

		public override object InvokeMember(string name,BindingFlags invokeAttr,Binder binder,object target,object[] args,ParameterModifier[] modifiers,CultureInfo culture,string[] namedParameters)
		{
			throw new NotImplementedException();
		}

		public override bool IsDefined(Type attributeType,bool inherit)
		{
			throw new NotImplementedException();
		}

		protected override TypeAttributes GetAttributeFlagsImpl()
		{
			throw new NotImplementedException();
		}

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr,Binder binder,CallingConventions callConvention,Type[] types,ParameterModifier[] modifiers)
		{
			throw new NotImplementedException();
		}

		protected override MethodInfo GetMethodImpl(string name,BindingFlags bindingAttr,Binder binder,CallingConventions callConvention,Type[] types,ParameterModifier[] modifiers)
		{
			throw new NotImplementedException();
		}

		protected override PropertyInfo GetPropertyImpl(string name,BindingFlags bindingAttr,Binder binder,Type returnType,Type[] types,ParameterModifier[] modifiers)
		{
			throw new NotImplementedException();
		}

		protected override bool HasElementTypeImpl() => false;

		protected override bool IsArrayImpl() => false;

		protected override bool IsByRefImpl() => false;

		protected override bool IsCOMObjectImpl() => false;

		protected override bool IsPointerImpl() => false;

		protected override bool IsPrimitiveImpl() => false;

		public override bool IsGenericParameter => true;

		public override int GenericParameterPosition { get; }

		protected override bool IsValueTypeImpl() => false;

		public override bool IsConstructedGenericType => false;

		GenericParameterAttributes _genericParameterAttributes;
		public override GenericParameterAttributes GenericParameterAttributes => _genericParameterAttributes;
		public void SetGenericParameterAttributes(GenericParameterAttributes value) => _genericParameterAttributes=value;

		#region MethodBaseFake
		class MethodBaseFake:MethodBase
		{
			public override MethodAttributes Attributes => throw new NotImplementedException();

			public override RuntimeMethodHandle MethodHandle => throw new NotImplementedException();

			public override Type DeclaringType => throw new NotImplementedException();

			public override MemberTypes MemberType => throw new NotImplementedException();

			public override string Name => throw new NotImplementedException();

			public override Type ReflectedType => throw new NotImplementedException();

			public override object[] GetCustomAttributes(bool inherit)
			{
				throw new NotImplementedException();
			}

			public override object[] GetCustomAttributes(Type attributeType,bool inherit)
			{
				throw new NotImplementedException();
			}

			public override MethodImplAttributes GetMethodImplementationFlags()
			{
				throw new NotImplementedException();
			}

			public override ParameterInfo[] GetParameters()
			{
				throw new NotImplementedException();
			}

			public override object Invoke(object obj,BindingFlags invokeAttr,Binder binder,object[] parameters,CultureInfo culture)
			{
				throw new NotImplementedException();
			}

			public override bool IsDefined(Type attributeType,bool inherit)
			{
				throw new NotImplementedException();
			}

			public override string ToString()
				=> "Use DeclaringMethodBuilder instead of DeclaringMethod";
		}
		#endregion MethodBaseFake
	}
}

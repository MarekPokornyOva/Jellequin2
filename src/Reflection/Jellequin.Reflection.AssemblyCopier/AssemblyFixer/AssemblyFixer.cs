using Jellequin.Reflection.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	interface IAssemblyFixer
	{
		Type FixType(Type type);
		ConstructorInfo FixConstructor(ConstructorInfo constructorInfo);
		MethodInfo FixMethod(MethodInfo methodInfo);
		FieldInfo FixField(FieldInfo fieldInfo);
		ParameterInfo FixParameter(ParameterInfo parameterInfo);
		CustomAttributeData CustomAttributeData(CustomAttributeData customAttribute);
	}

	class NullAssemblyFixer:IAssemblyFixer
	{
		private NullAssemblyFixer()
		{ }

		internal static IAssemblyFixer Instance { get; } = new NullAssemblyFixer();

		public Type FixType(Type type) => type;
		public ConstructorInfo FixConstructor(ConstructorInfo constructorInfo) => constructorInfo;
		public MethodInfo FixMethod(MethodInfo methodInfo) => methodInfo;
		public FieldInfo FixField(FieldInfo fieldInfo) => fieldInfo;
		public ParameterInfo FixParameter(ParameterInfo parameterInfo) => parameterInfo;
		public CustomAttributeData CustomAttributeData(CustomAttributeData customAttribute) => customAttribute;
	}

	class DefaultAssemblyFixer:IAssemblyFixer
	{
		readonly Assembly[] _asms;
		internal DefaultAssemblyFixer(Assembly[] asms)
			=> _asms=asms;

		public Type FixType(Type type)
			=> type==null ? null : UseFixer(type,out Assembly asm) ? new TypeAssemblyFixer(type,asm,this) : type;
		//All types where name and token equals to netstandard's has to be wrapped with class which returns netstandard as its assembly

		public ConstructorInfo FixConstructor(ConstructorInfo constructorInfo)
			=> new ConstructorAssemblyFixer(constructorInfo,this);

		public MethodInfo FixMethod(MethodInfo methodInfo)
			=> new MethodAssemblyFixer(methodInfo,this);

		public FieldInfo FixField(FieldInfo fieldInfo)
			=> new FieldAssemblyFixer(fieldInfo,this);

		public ParameterInfo FixParameter(ParameterInfo parameterInfo)
		{
			Type parameterType=parameterInfo.ParameterType;
			return UseFixer(parameterType, out Assembly asm)
				? new ParameterAssemblyFixer(parameterInfo,this)
				: parameterInfo;
		}

		PropertyInfo FixProperty(PropertyInfo propertyInfo)
			=> new PropertyAssemblyFixer(propertyInfo,this);

		public CustomAttributeData CustomAttributeData(CustomAttributeData customAttribute)
		{
			CustomAttributeTypedArgument FixCustomAttributeTypedArgument(CustomAttributeTypedArgument customAttributeTypedArgument)
				=> new CustomAttributeTypedArgument(FixType(customAttributeTypedArgument.ArgumentType),customAttributeTypedArgument.Value);

			return new CustomAttributeBuilder(
				this.FixConstructor(customAttribute.Constructor),
				customAttribute.ConstructorArguments.Select(FixCustomAttributeTypedArgument).ToArray(),
				customAttribute.NamedArguments.Select(x=>new CustomAttributeNamedArgument(x.IsField?(MemberInfo)FixField((FieldInfo)x.MemberInfo):FixProperty((PropertyInfo)x.MemberInfo),FixCustomAttributeTypedArgument(x.TypedValue))).ToArray());
		}

		Type[] GetParameterTypes(MethodBase meth)
			=> meth.GetParameters().Select(x => x.ParameterType).ToArray();

		/*T FixMember<T>(T memberInfo,Func<Assembly,T> resolver) where T: MemberInfo
			=> UseFixer(memberInfo.DeclaringType,out Assembly asm) ? resolver(asm) : memberInfo;*/

		bool UseFixer(Type type,out Assembly asmToUse)
		{
			if (!IsGenericParameterDefinition(type))
			{
				if (type.IsByRef)
					return UseFixer(type.GetElementType(),out asmToUse);
				if (type.IsConstructedGenericType)
					type=type.GetGenericTypeDefinition();
				foreach (Assembly asm in _asms)
				{
					Type asmType = asm.GetType(type.FullName);
					if (asmType!=null&&asmType.MetadataToken==type.MetadataToken)
					{
						asmToUse=asm;
						return true;
					}
				}
			}
			asmToUse=null;
			return false;
		}

		static bool IsGenericParameterDefinition(Type type)
		{
			if (type.HasElementType)
				type=type.GetElementType();
			return type.IsGenericParameter&&(((type.IsGenericTypeParameter())&&(type.DeclaringType.IsGenericTypeDefinition))||type.DeclaringMethod.IsGenericMethodDefinition);
		}
	}
}

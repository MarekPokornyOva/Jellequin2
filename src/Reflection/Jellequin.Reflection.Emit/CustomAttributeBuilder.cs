#region using
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class CustomAttributeBuilder:CustomAttributeData
	{
		readonly ConstructorInfo _constructor;
		readonly IList<CustomAttributeTypedArgument> _constructorArguments;
		readonly IList<CustomAttributeNamedArgument> _namedArguments;

		public CustomAttributeBuilder(ConstructorInfo constructor,IList<CustomAttributeTypedArgument> constructorArguments,IList<CustomAttributeNamedArgument> namedArguments)
		{
			_constructor=constructor??throw new ArgumentNullException(nameof(constructor));
			_constructorArguments=constructorArguments??throw new ArgumentNullException(nameof(constructorArguments));
			_namedArguments=namedArguments??throw new ArgumentNullException(nameof(namedArguments));
		}

		public override ConstructorInfo Constructor => _constructor;
		public override IList<CustomAttributeTypedArgument> ConstructorArguments => _constructorArguments;
		public override IList<CustomAttributeNamedArgument> NamedArguments => _namedArguments;

		/*
		public ConstructorInfo Constructor { get; }
		public object[] Arguments { get; }
		public CustomAttributeValue[] Properties { get; }

		public CustomAttributeBuilder(ConstructorInfo constructor,object[] arguments,CustomAttributeValue[] props)
		{
			Constructor=constructor??throw new ArgumentNullException(nameof(constructor));
			Arguments=arguments??throw new ArgumentNullException(nameof(arguments));
			Properties=props??throw new ArgumentNullException(nameof(props));
		}

		public CustomAttributeBuilder(ConstructorInfo constructor,object[] constructorArgs) : this(constructor,constructorArgs,new CustomAttributeValue[0])
		{ }

		public CustomAttributeBuilder(ConstructorInfo constructor,object[] constructorArgs,PropertyInfo[] namedProperties,object[] propertyValues)
			: this(constructor,constructorArgs,ConvertProps(namedProperties,propertyValues))
		{ }

		static CustomAttributeValue[] ConvertProps(PropertyInfo[] namedProperties,object[] propertyValues)
		{
			int len;
			if ((len=namedProperties.Length)!=propertyValues.Length)
				throw new ReflectionException(ReflectionExceptionReason.InvalidNamedProperties);

			CustomAttributeValue[] result = new CustomAttributeValue[len];
			int a = 0;
			foreach (PropertyInfo pi in namedProperties)
			{
				result[a]=new CustomAttributeValue(false,pi.Name,propertyValues[a]);
				a++;
			}
			return result;
		}*/
	}

	/*public class CustomAttributeValue
	{
		public CustomAttributeValue(bool isField,string name,object value)
		{
			IsField=isField;
			Name=name;
			Value=value;
		}

		public bool IsField { get; }
		public string Name { get; }
		public object Value { get; }
	}*/
}

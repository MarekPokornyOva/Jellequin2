#region using
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class TypeBuilder:Type, ICustomAttributesContainer
	{
		readonly CustomAttributesContainer _customAttributesContainer = new CustomAttributesContainer();
		readonly Module _module;
		readonly string _name;
		readonly string _namespace;
		readonly TypeAttributes _attributes;
		readonly Type _baseType;
		readonly Type[] _interfaces;

		internal TypeBuilder(Module module,string name,string @namespace,TypeAttributes attributes,Type baseType,Type[] interfaces)
		{
			_module=module??throw new ArgumentNullException(nameof(module));
			_name=name??throw new ArgumentNullException(nameof(name));
			_namespace=@namespace;
			_attributes=attributes;
			_baseType=baseType;
			_interfaces=interfaces??throw new ArgumentNullException(nameof(interfaces));
		}

		public static TypeBuilder MakeFromGenericForeign(Type genericDefinition,Type[] typeArguments)
			=> genericDefinition is TypeBuilder tb
				? tb.MakeGenericTypeBuilder(typeArguments)
				: new TypeBuilder(genericDefinition.Module,genericDefinition.Name,genericDefinition.Namespace,genericDefinition.Attributes,genericDefinition.BaseType,genericDefinition.GetInterfaces())
					{ _genericParameters=typeArguments,_isGenericNondef=true,_genericDefType=genericDefinition };

		#region Type overrides
		public override Assembly Assembly => throw new NotSupportedException();

		public override string AssemblyQualifiedName => throw new NotSupportedException();

		public override Type BaseType => _baseType;

		public override string FullName
			=> IsNested
				? string.Concat(_declaringType.FullName,"+",_name)
				: string.IsNullOrEmpty(_namespace) ? _name : string.Concat(_namespace,".",_name);

		public override Guid GUID => throw new NotImplementedException();

		public override Module Module => _module;

		public override string Namespace => _namespace;

		public override Type UnderlyingSystemType => this;

		public override string Name => _name;

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => (_isGenericNondef ? GetGenNondefConstructors() : _constructors).ToArray();

		public override object[] GetCustomAttributes(bool inherit) => _customAttributesContainer.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _customAttributesContainer.GetCustomAttributesData();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override Type GetElementType() => _elementType;

		public override EventInfo GetEvent(string name,BindingFlags bindingAttr) => throw new NotImplementedException();

		public override EventInfo[] GetEvents(BindingFlags bindingAttr) => (_isGenericNondef ? GetGenNondefEvents() : _events).ToArray();

		public override FieldInfo GetField(string name,BindingFlags bindingAttr) => throw new NotImplementedException();

		public override FieldInfo[] GetFields(BindingFlags bindingAttr) => (_isGenericNondef ? GetGenNondefFields() : _fields).ToArray();

		public override Type GetInterface(string name,bool ignoreCase) => throw new NotImplementedException();

		public override Type[] GetInterfaces() => _interfaces.ToArray();

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => throw new NotImplementedException();

		public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => (_isGenericNondef ? GetGenNondefMethods() : _methods).ToArray();

		public override Type GetNestedType(string name,BindingFlags bindingAttr) => throw new NotImplementedException();

		public override Type[] GetNestedTypes(BindingFlags bindingAttr) => (_isGenericNondef ? GetGenNondefNestedTypes() : _nestedTypes).ToArray();

		public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => (_isGenericNondef ? GetGenNondefProperties() : _properies).ToArray();

		public override object InvokeMember(string name,BindingFlags invokeAttr,Binder binder,object target,object[] args,ParameterModifier[] modifiers,CultureInfo culture,string[] namedParameters)
			 => throw new NotImplementedException();

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		protected override TypeAttributes GetAttributeFlagsImpl() => _attributes;

		protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr,Binder binder,CallingConventions callConvention,Type[] types,ParameterModifier[] modifiers)
			 => throw new NotImplementedException();

		protected override MethodInfo GetMethodImpl(string name,BindingFlags bindingAttr,Binder binder,CallingConventions callConvention,Type[] types,ParameterModifier[] modifiers)
			=> throw new NotImplementedException();


		protected override PropertyInfo GetPropertyImpl(string name,BindingFlags bindingAttr,Binder binder,Type returnType,Type[] types,ParameterModifier[] modifiers)
			=> throw new NotImplementedException();

		protected override bool HasElementTypeImpl() => false;

		protected override bool IsArrayImpl() => _elementMode==1;

		protected override bool IsByRefImpl() => _elementMode==2;

		protected override bool IsCOMObjectImpl() => false;

		protected override bool IsPointerImpl() => false;

		protected override bool IsPrimitiveImpl() => false;

		public override Type MakeGenericType(params Type[] typeArguments)
			=> MakeGenericTypeBuilder(typeArguments);

		readonly CacheObject<Type[],TypeBuilder> _genericInstances = new CacheObject<Type[],TypeBuilder>();
		Type _genericDefType;
		public TypeBuilder MakeGenericTypeBuilder(params Type[] typeArguments)
		{
			if (typeArguments==null)
				throw new ArgumentNullException(nameof(typeArguments));
			if (!IsGenericTypeDefinition)
				throw new InvalidOperationException("Type is not a GenericTypeDefinition.");
			if (_genericParameters.Length!=typeArguments.Length)
				throw new ArgumentException("Invalid arguments count.");
			return _genericInstances.GetOrAdd(typeArguments,() => new TypeBuilder(_module,Name,Namespace,Attributes,_baseType,Type.EmptyTypes) { _genericParameters=typeArguments,_isGenericNondef=true,_genericDefType=this });
		}

		public override Type[] GetGenericArguments()
			=> _genericParameters;
		#endregion Type overrides

		readonly List<FieldBuilder> _fields = new List<FieldBuilder>();
		public FieldBuilder DefineField(string name,Type type,FieldAttributes attributes)
			=> _fields.AddWithReturn(new FieldBuilder(this,name,attributes,type));

		readonly List<MethodBuilder> _methods = new List<MethodBuilder>();
		public MethodBuilder DefineMethod(string name,MethodAttributes attributes,CallingConventions callingConventions)
				=> _methods.AddWithReturn(new MethodBuilder(this,name,attributes,callingConventions));

		readonly List<PropertyBuilder> _properies = new List<PropertyBuilder>();
		public PropertyBuilder DefineProperty(string name,PropertyAttributes attributes,Type type,Type[] argumentTypes)
			=> _properies.AddWithReturn(new PropertyBuilder(this,name,attributes,type));

		readonly List<ConstructorBuilder> _constructors = new List<ConstructorBuilder>();
		public ConstructorBuilder DefineConstructor(string name,MethodAttributes attributes,CallingConventions callingConventions,Type[] parameterTypes)
			=> _constructors.AddWithReturn(new ConstructorBuilder(this,name,attributes,callingConventions,parameterTypes));

		readonly List<EventBuilder> _events = new List<EventBuilder>();
		public EventBuilder DefineEvent(string name,EventAttributes attributes,Type handlerType)
			=> _events.AddWithReturn(new EventBuilder(this,name,attributes,handlerType));

		public void SetCustomAttribute(CustomAttributeData attribute) => _customAttributesContainer.SetCustomAttribute(attribute);

		public ICollection<FieldBuilder> GetFieldBuilders() => _isGenericNondef ? GetGenNondefFields() : _fields;
		public ICollection<MethodBuilder> GetMethodBuilders() => _isGenericNondef ? GetGenNondefMethods() : _methods;
		public ICollection<ConstructorBuilder> GetConstructorBuilders() => _isGenericNondef ? GetGenNondefConstructors() : _constructors;
		public ICollection<PropertyBuilder> GetPropertyBuilders() => _isGenericNondef ? GetGenNondefProperties() : _properies;
		public ICollection<EventBuilder> GetEventBuilders() => _isGenericNondef ? GetGenNondefEvents() : _events;
		public ICollection<TypeBuilder> GetNestedTypeBuilders() => _isGenericNondef ? GetGenNondefNestedTypes() : _nestedTypes.AsReadOnly();

		bool _isGenericNondef;
		Type[] _genericParameters;
		public GenericParameterBuilder[] DefineGenericParameters(string[] names)
		{
			GenericParameterBuilder[] result = new GenericParameterBuilder[names.Length];
			int a = 0;
			foreach (string name in names)
			{
				result[a]=new GenericParameterBuilder(name,a,this);
				a++;
			}
			_genericParameters=result;
			return result;
		}

		public override Type[] GenericTypeArguments => _genericParameters;

		public override bool ContainsGenericParameters => _genericParameters!=null&&_genericParameters.Length!=0;

		public override Type GetGenericTypeDefinition() => _genericDefType;

		public override bool IsGenericType => ContainsGenericParameters;

		public override bool IsGenericTypeDefinition => IsGenericType&&(!_isGenericNondef);

		public override bool IsConstructedGenericType => _isGenericNondef;

		readonly List<TypeBuilder> _nestedTypes = new List<TypeBuilder>();
		public TypeBuilder DefineNestedType(string name,TypeAttributes attributes,Type baseType,Type[] interfaces)
			=> _nestedTypes.AddWithReturn(new TypeBuilder(_module,name,null,attributes,baseType,interfaces) { _declaringType=this });

		TypeBuilder _elementType;
		int _elementMode;
		public override Type MakeArrayType()
			=> new TypeBuilder(_module,Name+"[]",Namespace,Attributes,typeof(Array),Type.EmptyTypes) { _elementType=this,_elementMode=1 };

		public override Type MakeByRefType()
			=> new TypeBuilder(_module,Name+"*",Namespace,Attributes,null,Type.EmptyTypes) { _elementType=this,_elementMode=2 };

		TypeBuilder _declaringType;
		public override Type DeclaringType => _declaringType;

		public void DefineMethodOverride(MethodBuilder newMethod,MethodInfo toOverrideMethod)
		{
#warning implement in AssemblyWriter
		}

		public override string ToString()
			=> FullName;

		public override bool Equals(Type o)
			=> (this.IsGenericType)&&(!this.IsGenericTypeDefinition)&&(o.IsGenericType)&&(!o.IsGenericTypeDefinition)
				? this.GetGenericTypeDefinition().Equals(o.GetGenericTypeDefinition())&&(this.GetGenericArguments().SequenceEqual(o.GetGenericArguments()))
				: base.Equals(o);

		public override int GetHashCode()
			=> (this.IsGenericType)&&(!this.IsGenericTypeDefinition)
				? (this.GetGenericTypeDefinition(), this.GetGenericArguments()).GetHashCode()
				: base.GetHashCode();

		const BindingFlags _bindingFlagsAll = unchecked((BindingFlags)uint.MaxValue);
		CacheObject<FieldInfo,FieldBuilder> _genNondefFieldsCache = new CacheObject<FieldInfo,FieldBuilder>();
		ICollection<FieldBuilder> GetGenNondefFields()
			=> _genericDefType.GetFields(_bindingFlagsAll).Select(x => _genNondefFieldsCache.GetOrAdd(x,()=> new FieldBuilder(this,x.Name,x.Attributes,ConvertType(x.FieldType)))).ToArray();

		ICollection<MethodBuilder> GetGenNondefMethods()
			=> _genericDefType.GetMethods(_bindingFlagsAll).Select(GetGenNondefMethod).ToArray();

		CacheObject<MethodInfo,MethodBuilder> _genNondefMethodsCache = new CacheObject<MethodInfo,MethodBuilder>();
		MethodBuilder GetGenNondefMethod(MethodInfo method)
			=> _genNondefMethodsCache.GetOrAdd(method,()=>
		{
			ParameterInfo[] parms = method.GetParameters();
			MethodBuilder result = new MethodBuilder(this,method.Name,method.Attributes,method.CallingConvention);
			result.SetParameters(method.GetParameters().Select(y => ConvertType(y.ParameterType)).ToArray());
			result.SetReturnType(ConvertType(method.ReturnType));
			foreach (ParameterBuilder p in parms)
				result.DefineParameter(p.Position,p.Attributes,p.Name);
			return result;
		});

		CacheObject<ConstructorInfo,ConstructorBuilder> _genNondefConstructorsCache = new CacheObject<ConstructorInfo,ConstructorBuilder>();
		ICollection<ConstructorBuilder> GetGenNondefConstructors()
			=> _genericDefType.GetConstructors(_bindingFlagsAll).Select(x => _genNondefConstructorsCache.GetOrAdd(x,() =>
			{
				ParameterInfo[] parms=x.GetParameters();
				ConstructorBuilder result=new ConstructorBuilder(this,x.Name,x.Attributes,x.CallingConvention,parms.Select(y=>ConvertType(y.ParameterType)).ToArray());
				foreach (ParameterInfo p in parms)
					result.DefineParameter(p.Position,p.Attributes,p.Name);
				return result;
			})).ToArray();

		Type ConvertType(Type type)
		{
			int ind = Array.IndexOf(_genericDefType.GenericTypeArguments,type);
			return ind==-1
				? type.IsConstructedGenericType
					? type.GetGenericTypeDefinition().MakeGenericType(type.GenericTypeArguments.Select(ConvertType).ToArray())
					: type
				: _genericParameters[ind];
			/*if (ind==-1)
				if ((type.IsGenericType)&&(!type.IsGenericTypeDefinition))
					return type.GetGenericTypeDefinition().MakeGenericType(type.GenericTypeArguments.Select(ConvertType).ToArray());
				else
					return type;
			else
				return _genericParameters[ind];*/
		}

		CacheObject<PropertyInfo,PropertyBuilder> _genNondefPropertiesCache = new CacheObject<PropertyInfo,PropertyBuilder>();
		ICollection<PropertyBuilder> GetGenNondefProperties()
			=> _genericDefType.GetProperties(_bindingFlagsAll).Select(x => _genNondefPropertiesCache.GetOrAdd(x,() =>
			{
				PropertyBuilder result=new PropertyBuilder(this,x.Name,x.Attributes,ConvertType(x.PropertyType));

	 			if ((int)(x.Attributes&PropertyAttributes.HasDefault)!=0)
					result.SetConstant(x.GetRawConstantValue());

				if (x.CanRead)
					result.SetGetMethod(CopyMethod(x.GetGetMethod()));
				if (x.CanWrite)
					result.SetSetMethod(CopyMethod(x.GetSetMethod()));

				return result;
			})).ToArray();

		MethodBuilder CopyMethod(MethodInfo method)
			=> GetGenNondefMethod(method);

		//result musi byt cacheovan
		ICollection<EventBuilder> GetGenNondefEvents()
			=> _genericDefType.GetEvents(_bindingFlagsAll).Select(x => (EventBuilder)null).ToArray();

		//result musi byt cacheovan
		ICollection<TypeBuilder> GetGenNondefNestedTypes()
			=> _genericDefType.GetNestedTypes(_bindingFlagsAll).Select(x => (TypeBuilder)null).ToArray();

		class CacheObject<TKey, TValue>
		{
			Dictionary<TKey,TValue> _cache = new Dictionary<TKey,TValue>();

			internal TValue GetOrAdd(TKey key,Func<TValue> addFunction)
			{
				if (_cache.TryGetValue(key,out TValue result))
					return result;
				_cache[key]=(result=addFunction());
				return result;
			}
		}
	}
}

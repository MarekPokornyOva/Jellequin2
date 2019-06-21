using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace Jellequin.Reflection.Emit
{
	class ModuleBuilder
	{
		AssemblyName _assemblyName;
		private ModuleBuilder(AssemblyName assemblyName)
		{
			_assemblyName=assemblyName;
		}

		internal AssemblyName AssemblyName => _assemblyName;

		internal static ModuleBuilder Create(AssemblyName assemblyName)
		{
			return new ModuleBuilder(assemblyName);
		}

		Dictionary<string, TypeBuilder> _types = new Dictionary<string, TypeBuilder>();
		internal TypeBuilder DefineType(string name, TypeAttributes attributes, IType baseType)
		{
			return DefineType(name, attributes, baseType, IType.EmptyTypes);
		}

		internal TypeBuilder DefineType(string name, TypeAttributes attributes, IType baseType, IType[] interfaces)
		{
			TypeBuilder result;
			_types.Add(name, result=new TypeBuilder(name, attributes, baseType, interfaces));
			return result;
		}

		internal IReadOnlyCollection<TypeBuilder> Types => _types.Values;

		internal SymbolWriter DefineDocument(string name)
		{
			return new SymbolWriter(name);
		}

		internal TypeBuilder GetType(string name)
		{
			return _types.TryGetValue(name, out TypeBuilder result) ? result : null;
		}
	}

	class TypeBuilder
	{
		internal string Name { get; }
		internal TypeAttributes Attributes { get; }
		internal IType BaseType { get; }
		internal IType[] Interfaces { get; }

		internal TypeBuilder(string name, TypeAttributes attributes, IType baseType, IType[] interfaces)
		{
			Name=name;
			Attributes=attributes;
			BaseType=baseType;
			Interfaces=interfaces;
		}

		List<FieldBuilder> _fields = new List<FieldBuilder>();
		internal FieldBuilder DefineField(string name, IType type, FieldAttributes attributes)
		{
			return AddToList(_fields, new FieldBuilder(this, name, type, attributes));
		}

		List<MethodBuilder> _methods = new List<MethodBuilder>();
		internal MethodBuilder DefineMethod(string name, MethodAttributes attributes, IType returnType, IType[] argumentTypes)
		{
			return AddToList(_methods, new MethodBuilder(this, name, attributes, returnType, argumentTypes));
		}

		List<PropertyBuilder> _properies = new List<PropertyBuilder>();
		internal PropertyBuilder DefineProperty(string name, PropertyAttributes attributes, IType type, IType[] argumentTypes)
		{
			return AddToList(_properies, new PropertyBuilder(this, name, attributes, type));
		}

		List<ConstructorBuilder> _constructors = new List<ConstructorBuilder>();
		internal ConstructorBuilder DefineConstructor(MethodAttributes attributes, CallingConventions callingConventions, IType[] argumentTypes)
		{
			return AddToList(_constructors, new ConstructorBuilder(this, attributes, callingConventions, argumentTypes));
		}

		List<CustomAttributeBuilder> _custAttributes = new List<CustomAttributeBuilder>();
		internal CustomAttributeBuilder AddCustomAttribute(ConstructorInfo ci, object[] arguments, CustomAttributeValue[] props)
		{
			return AddToList(_custAttributes, new CustomAttributeBuilder(ci,arguments,props));
		}

		internal IReadOnlyCollection<FieldBuilder> Fields => _fields;
		internal IReadOnlyCollection<MethodBuilder> Methods => _methods;
		internal IReadOnlyCollection<ConstructorBuilder> Constructors => _constructors;
		internal IReadOnlyCollection<PropertyBuilder> GetProperties() => _properies;
		internal IReadOnlyCollection<CustomAttributeBuilder> CustomAttributes => _custAttributes;

		T AddToList<T>(List<T> list, T item)
		{
			list.Add(item);
			return item;
		}

		internal void CreateType()
		{
			//probably do nothing - just for .NET FW compatibility
		}
	}

	internal abstract class MemberBuilderBase
	{
		internal string Name { get; }
		internal TypeBuilder DeclaringType { get; }

		internal MemberBuilderBase(TypeBuilder declaringType, string name)
		{
			DeclaringType=declaringType;
			Name=name;
		}
	}

	internal class FieldBuilder : MemberBuilderBase
	{
		internal IType Type { get; }
		internal FieldAttributes Attributes { get; }

		public FieldBuilder(TypeBuilder declaringType, string name, IType type, FieldAttributes attributes) : base(declaringType, name)
		{
			Type=type;
			Attributes=attributes;
		}

		object _constantValue;
		internal void SetConstant(object value)
		{
			HasRawConstantValue=true;
			_constantValue=value;
		}

		internal bool HasRawConstantValue { get; private set; }

		internal object GetRawConstantValue() => _constantValue;
	}

	internal class PropertyBuilder:MemberBuilderBase
	{
		internal PropertyAttributes Attributes;
		internal IType PropertyType;
		MethodBuilder _getter,_setter;

		public PropertyBuilder(TypeBuilder declaringType, string name, PropertyAttributes attributes, IType type):base(declaringType, name)
		{
			Attributes=attributes;
			PropertyType=type;
		}

		internal void SetGetMethod(MethodBuilder getter)
		{
			_getter=getter;
		}

		internal void SetSetMethod(MethodBuilder setter)
		{
			_setter=setter;
		}

		internal MethodBuilder GetGetMethod() => _getter;

		internal MethodBuilder GetSetMethod() => _setter;

		internal MethodBuilder GetMethod => _getter;

		internal MethodBuilder SetMethod => _setter;
	}

	//http://msdn.microsoft.com/en-us/library/e514eeby.aspx
	//[DebuggerDisplay("{ValueToShow,nq}", Name = "{Key,nq}", Type = "{TypeToShow,nq}")]
	//[DebuggerDisplay("{DebugView,nq}")]
	internal class MethodBuilderBase:MemberBuilderBase
	{
		internal MethodAttributes Attributes { get; }
		internal IType ReturnType { get; }
		internal IType[] ArgumentTypes { get; }
		List<ParameterBuilder> _parameters=new List<ParameterBuilder>();

		internal MethodBuilderBase(TypeBuilder declaringType, string name, MethodAttributes attributes, IType returnType, IType[] argumentTypes):base(declaringType, name)
		{
			Attributes=attributes;
			ReturnType=returnType;
			ArgumentTypes=argumentTypes;
			_body=new MethodBuilderBody(new ILGenerator(this));
		}

		internal ParameterBuilder DefineParameter(int index, ParameterAttributes attributes, string name)
		{
			ParameterBuilder result = new ParameterBuilder(index, attributes, name, ArgumentTypes[index-1]);
			_parameters.Add(result);
			return result;
		}

		internal ParameterBuilder[] GetParameters() => _parameters.ToArray();

		internal bool IsStatic => (Attributes&MethodAttributes.Static)==MethodAttributes.Static;

		MethodBuilderBody _body;
		internal ILGenerator GetILGenerator()
		{
			return _body.ILGenerator;
		}

		internal MethodBuilderBody GetMethodBody() => _body;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			FormatTypeName(ReturnType, sb);
			sb.Append(" ");
			FormatNameAndSig(sb);
			return sb.ToString();
		}

		void FormatTypeName(IType type, StringBuilder sb)
		{
			sb.Append(type is NetType net
				? net.Type.Name
				: type is NewType nt
					? nt.TypeBuilder.Name
					: "???");
		}

		void FormatNameAndSig(StringBuilder sb)
		{
			sb.Append(this.Name);
			sb.Append("(");
			bool first=true;
			foreach (IType t in ArgumentTypes)
			{
				if (first)
					first=false;
				else
					sb.Append(", ");
				FormatTypeName(t, sb);
			}
			sb.Append(")");
		}
	}

	internal class MethodBuilder: MethodBuilderBase
	{
		internal MethodBuilder(TypeBuilder declaringType, string name, MethodAttributes attributes, IType returnType, IType[] argumentTypes):base(declaringType,name,attributes,returnType,argumentTypes)
		{ }
	}

	internal class ConstructorBuilder : MethodBuilderBase
	{
		internal ConstructorBuilder(TypeBuilder declaringType, MethodAttributes attributes, CallingConventions callingConventions, IType[] argumentTypes):base(declaringType,".ctor",attributes,typeof(void),argumentTypes)
		{ }
	}

	internal class CustomAttributeBuilder
	{
		internal ConstructorInfo Constructor { get; }
		internal object[] Arguments { get; }
		internal CustomAttributeValue[] Props { get; }

		internal CustomAttributeBuilder(ConstructorInfo constructor, object[] arguments, CustomAttributeValue[] props)
		{
			Constructor = constructor;
			Arguments = arguments;
			Props = props;
		}
	}

	internal class CustomAttributeValue
	{
		internal CustomAttributeValue(bool isField, string name, object value)
		{
			IsField = isField;
			Name = name;
			Value = value;
		}

		internal bool IsField { get; }
		internal string Name { get; }
		internal object Value { get; }
	}

	internal class ParameterBuilder
	{
		internal int Index { get; }
		internal ParameterAttributes Attributes { get; }
		internal string Name { get; }
		internal IType ParameterType { get; }

		internal ParameterBuilder(int index, ParameterAttributes attributes, string name, IType type)
		{
			Index=index;
			Attributes=attributes;
			Name=name;
			ParameterType=type;
		}
	}

	internal class ILGenerator
	{
		MethodBuilderBase _methodBuilder;
		internal List<Instruction> Instructions { get; } = new List<Instruction>();
		internal List<ExceptionRegionInfo> Exceptions { get; } = new List<ExceptionRegionInfo>();

		public ILGenerator(MethodBuilderBase methodBuilder)
		{
			_methodBuilder=methodBuilder;
		}

		internal void MarkSequencePoint(SymbolWriter symbolWriter, int startLineNumber, int startColumn, int endLineNumber, int endColumn)
		{
			if (symbolWriter!=null)
				Instructions.Add(new InstructionSeqPoint((symbolWriter.DocumentName,startLineNumber,(ushort)startColumn,endLineNumber,(ushort)endColumn)));
		}

		internal void Emit(ILOpCode opCode)
		{
			Instructions.Add(new InstructionOpCode(opCode));
		}

		internal void Emit(ILOpCode opCode, ConstructorInfo constructorInfo)
		{
			if (constructorInfo==null)
				throw new ArgumentNullException(nameof(constructorInfo));
			Instructions.Add(new InstructionOpCode<ConstructorInfo>(opCode,constructorInfo));
		}

		internal void Emit(ILOpCode opCode, ConstructorBuilder constructorBuilder)
		{
			if (constructorBuilder==null)
				throw new ArgumentNullException(nameof(constructorBuilder));
			Instructions.Add(new InstructionOpCode<ConstructorBuilder>(opCode, constructorBuilder));
		}

		internal void Emit(ILOpCode opCode, MethodInfo methodInfo)
		{
			if (methodInfo==null)
				throw new ArgumentNullException(nameof(methodInfo));
			Instructions.Add(new InstructionOpCode<MethodInfo>(opCode, methodInfo));
		}

		internal void Emit(ILOpCode opCode, MethodBuilder methodBuilder)
		{
			if (methodBuilder==null)
				throw new ArgumentNullException(nameof(methodBuilder));
			Instructions.Add(new InstructionOpCode<MethodBuilder>(opCode, methodBuilder));
		}

		internal void Emit(ILOpCode opCode, int value)
		{
			Instructions.Add(new InstructionOpCode<int>(opCode, value));
		}

		internal void Emit(ILOpCode opCode, double value)
		{
			Instructions.Add(new InstructionOpCode<double>(opCode, value));
		}

		internal void Emit(ILOpCode opCode, string value)
		{
			Instructions.Add(new InstructionOpCode<string>(opCode, value));
		}

		internal void Emit(ILOpCode opCode, FieldBuilder fieldBuilder)
		{
			if (fieldBuilder==null)
				throw new ArgumentNullException(nameof(fieldBuilder));
			Instructions.Add(new InstructionOpCode<FieldBuilder>(opCode, fieldBuilder));
		}

		internal void Emit(ILOpCode opCode, FieldInfo fieldInfo)
		{
			if (fieldInfo==null)
				throw new ArgumentNullException(nameof(fieldInfo));
			Instructions.Add(new InstructionOpCode<FieldInfo>(opCode, fieldInfo));
		}

		internal void Emit(ILOpCode opCode, IType type)
		{
			if (type==null)
				throw new ArgumentNullException(nameof(type));
			Instructions.Add(new InstructionOpCode<IType>(opCode, type));
		}

		internal void Emit(ILOpCode opCode, LocalBuilder localBuilder)
		{
			if (localBuilder==null)
				throw new ArgumentNullException(nameof(localBuilder));
			Instructions.Add(new InstructionOpCode<LocalBuilder>(opCode, localBuilder));
		}

		internal void Emit(ILOpCode opCode, Label label)
		{
			if (label==null)
				throw new ArgumentNullException(nameof(label));
			Instructions.Add(new InstructionOpCode<Label>(opCode, label));
		}

		internal LocalBuilder DeclareLocal(IType type)
		{
			LocalBuilder result = new LocalBuilder(type, Locals.Count);
			Locals.Add(result);
			return result;
		}

		internal Label DefineLabel()
		{
			Label result = new Label();
			Labels.Add(result);
			return result;
		}

		internal void MarkLabel(Label label)
		{
			Instructions.Add(new InstructionMarkLabel(label));
		}

		internal ExceptionRegionInfo AddCatchRegion(IType exceptionType)
		{
			ExceptionRegionInfo result;
			Exceptions.Add(result=new ExceptionRegionInfo(this, true, exceptionType));
			return result;
		}

		internal ExceptionRegionInfo AddFinallyRegion()
		{
			ExceptionRegionInfo result;
			Exceptions.Add(result=new ExceptionRegionInfo(this, false, null));
			return result;
		}

		internal IList<LocalBuilder> Locals { get; private set; } = new List<LocalBuilder>();
		internal List<Label> Labels { get; private set; } = new List<Label>();
	}

	internal class SymbolWriter
	{
		internal string DocumentName { get; }

		internal SymbolWriter(string documentName)
		{
			DocumentName=documentName;
		}
	}

	internal class ExceptionRegionInfo
	{
		internal ExceptionRegionInfo(ILGenerator iLGenerator, bool isCatch, IType exceptionType)
		{
			ILGenerator=iLGenerator;
			TryStart=iLGenerator.DefineLabel();
			HandleStart=iLGenerator.DefineLabel();
			HandleEnd=iLGenerator.DefineLabel();
			IsCatch=isCatch;
			ExceptionType=exceptionType;
		}

		internal ILGenerator ILGenerator { get; }
		internal Label TryStart { get; }
		internal Label HandleStart { get; }
		internal Label HandleEnd { get; }
		internal bool IsCatch { get; }
		internal IType ExceptionType { get; }


		internal void MarkTryStart()
		{
			ILGenerator.MarkLabel(TryStart);
		}

		internal void MarkHandleStart()
		{
			ILGenerator.MarkLabel(HandleStart);
		}

		internal void MarkHandleEnd()
		{
			ILGenerator.MarkLabel(HandleEnd);
		}
	}

	internal class LocalBuilder
	{
		internal IType LocalType { get; private set; }
		internal int Index { get; private set; }
		internal LocalBuilder(IType type, int index)
		{
			LocalType=type;
			Index=index;
		}
	}

	internal class Label
	{
		internal Label()
		{ }
	}

	internal abstract class IType
	{
		static IType[] _emptyTypes = new IType[0];
		internal static IType[] EmptyTypes => _emptyTypes;

		public static implicit operator IType (Type type) => new NetType(type);
		public static implicit operator IType(TypeBuilder type) => new NewType(type);
	}

	internal class NetType:IType
	{
		Type _type;
		internal NetType(Type type)
		{
			_type=type;
		}

		internal Type Type => _type;
	}

	internal class NewType : IType
	{
		TypeBuilder _type;
		internal NewType(TypeBuilder type)
		{
			_type=type;
		}

		internal TypeBuilder TypeBuilder => _type;
		internal bool IsValueType => false;
	}

	internal class MethodBuilderBody
	{
		ILGenerator _ilGenerator;
		internal MethodBuilderBody(ILGenerator ilGenerator)
		{
			_ilGenerator=ilGenerator;
		}

		internal ILGenerator ILGenerator => _ilGenerator;

		internal IList<LocalBuilder> LocalVariables => _ilGenerator.Locals;

		internal List<Instruction> Instructions => _ilGenerator.Instructions;

		internal List<Label> Labels => _ilGenerator.Labels;

		internal List<ExceptionRegionInfo> Exceptions => _ilGenerator.Exceptions;
	}

	internal abstract class Instruction
	{ }

	internal class InstructionOpCode:Instruction
	{
		internal ILOpCode OpCode;

		internal InstructionOpCode(ILOpCode opCode)
		{
			OpCode=opCode;
		}
	}

	internal class InstructionOpCode<TData>: InstructionOpCode
	{
		internal TData Data;

		internal InstructionOpCode(ILOpCode opCode, TData data):base(opCode)
		{
			Data=data;
		}
	}

	internal class InstructionSeqPoint:Instruction
	{
		internal (string DocumentName, int StartLineNumber, ushort StartColumn, int EndLineNumber, ushort EndColumn) SeqPoint { get; private set; }

		internal InstructionSeqPoint((string DocumentName, int StartLineNumber, ushort StartColumn, int EndLineNumber, ushort EndColumn) seqPoint)
		{
			SeqPoint=seqPoint;
		}
	}

	internal class InstructionMarkLabel:Instruction
	{
		internal Label Label { get; private set; }

		internal InstructionMarkLabel(Label label)
		{
			Label=label;
		}
	}
}

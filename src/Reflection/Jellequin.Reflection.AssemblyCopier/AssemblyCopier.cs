#region using
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using OperandType = System.Reflection.Emit.OperandType;
using OpCodes = System.Reflection.Emit.OpCodes;
using System.Linq;
using System.Collections;
using Jellequin.Reflection.Emit.Internal;
using Microsoft.Cci;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class AssemblyCopier
	{
		#region fields
		ModuleBuilder _modBldr;
		readonly Assembly _asmToCopy;
		readonly IAssemblyFixer _assemblyFixer;
		readonly Dictionary<Type,TypeBuilder> _copiedTypes = new Dictionary<Type,TypeBuilder>();
		readonly Dictionary<MethodInfo,MethodBuilder> _copiedMethods = new Dictionary<MethodInfo,MethodBuilder>();
		readonly Dictionary<ConstructorInfo,ConstructorBuilder> _copiedConstructors = new Dictionary<ConstructorInfo,ConstructorBuilder>();
		readonly Dictionary<FieldInfo,FieldBuilder> _copiedFields = new Dictionary<FieldInfo,FieldBuilder>();
		readonly Dictionary<EventInfo,EventBuilder> _copiedEvents = new Dictionary<EventInfo,EventBuilder>();
		readonly Dictionary<PropertyInfo,PropertyBuilder> _copiedProperties = new Dictionary<PropertyInfo,PropertyBuilder>();
		#endregion fields

		#region .ctor
		public AssemblyCopier(Assembly asmToCopy,TargetFramework targetFramework)
		{
			_asmToCopy=asmToCopy;

			bool useDefaultAssemblyFixer=
				targetFramework==TargetFramework.Current ? false :
				targetFramework==TargetFramework.NetStandard ? _asmToCopy.GetCustomAttribute<System.Runtime.Versioning.TargetFrameworkAttribute>().FrameworkName.StartsWith(".NETStandard,",StringComparison.Ordinal):
				targetFramework==TargetFramework.SameAsSource ? true :
				throw new InvalidProgramException();
			_assemblyFixer=useDefaultAssemblyFixer ? new DefaultAssemblyFixer(_asmToCopy.GetReferencedAssemblies().Select(Assembly.Load).ToArray()) : NullAssemblyFixer.Instance;
		}
		#endregion .ctor

		#region copy
		#region copy definitions
		static BindingFlags _bindingFlags = BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static|BindingFlags.DeclaredOnly;
		public CopyMap CopyTo(ModuleBuilder moduleBuilder)
		{
			_modBldr=moduleBuilder;

			Type[] types = _asmToCopy.GetTypes().OrderBy(x => x.MetadataToken).ToArray();

			CopyMap result;
			lock (_copiedTypes)
			{
				foreach (Type typeToCopy in types)
					CopyTypeDefinition(typeToCopy);

				foreach (FieldInfo fi in types.SelectMany(x => x.GetFields(_bindingFlags))/*.OrderBy(x => x.MetadataToken)*/)
					CopyFieldDefinition(fi);

				foreach (ConstructorInfo mi in types.SelectMany(x => x.GetConstructors(_bindingFlags))/*.OrderBy(x => x.MetadataToken)*/)
					CopyConstructorDefinition(mi);

				foreach (MethodInfo mi in types.SelectMany(x => x.GetMethods(_bindingFlags))/*.OrderBy(x => x.MetadataToken)*/)
					CopyMethodDefinition(mi);

				foreach (PropertyInfo pi in types.SelectMany(x => x.GetProperties(_bindingFlags))/*.OrderBy(x => x.MetadataToken)*/)
					CopyPropertyDefinition(pi);

				foreach (EventInfo ei in types.SelectMany(x => x.GetEvents(_bindingFlags))/*.OrderBy(x => x.MetadataToken)*/)
					CopyEventDefinition(ei);

				foreach (KeyValuePair<ConstructorInfo,ConstructorBuilder> item in _copiedConstructors)
					CopyMethodBody(item.Key,item.Value);
				foreach (KeyValuePair<MethodInfo,MethodBuilder> item in _copiedMethods)
					CopyMethodBody(item.Key,item.Value);

				result = new CopyMap() { EntryPoint=_asmToCopy.EntryPoint==null?null:_copiedMethods[_asmToCopy.EntryPoint] };
				ClearCollections();
			}

			return result;
		}

		Type CopyTypeDefinition(Type typeToCopy)
		{
			if (typeToCopy==null)
				return null;
			if (!IsRuntimeType(typeToCopy))
				return _assemblyFixer.FixType(typeToCopy);
			if (_copiedTypes.TryGetValue(typeToCopy,out TypeBuilder already))
				return already;

			Type[] interfacesCopy = Array.ConvertAll(typeToCopy.GetInterfacesDirect(),CopyTypeDefinition);
			TypeBuilder typeCopy = typeToCopy.IsNested
				? _copiedTypes[typeToCopy.DeclaringType].DefineNestedType(typeToCopy.Name,typeToCopy.Attributes,CopyTypeDefinition(typeToCopy.BaseType),interfacesCopy)
				: _modBldr.DefineType(typeToCopy.Name,typeToCopy.Namespace,typeToCopy.Attributes,CopyTypeDefinition(typeToCopy.BaseType),interfacesCopy);
			_copiedTypes.Add(typeToCopy,typeCopy);

			Type[] genericArgs = typeToCopy.GetGenericArguments();
			if (genericArgs.Length!=0)
				typeCopy.DefineGenericParameters(Array.ConvertAll(genericArgs,item => item.Name));

			CopyAttributes(typeToCopy,typeCopy);

			return typeCopy;
		}

		void CopyFieldDefinition(FieldInfo fieldToCopy)
		{
			if (fieldToCopy==null)
				return;
			if (!IsRuntimeType(fieldToCopy.DeclaringType))
				return;
			if (_copiedFields.ContainsKey(fieldToCopy))
				return;

			FieldBuilder fieldCopy = _copiedTypes[fieldToCopy.DeclaringType].DefineField(fieldToCopy.Name,_assemblyFixer.FixType(GetTypeCopy(fieldToCopy.FieldType)),fieldToCopy.Attributes);

			if ((int)(fieldToCopy.Attributes&FieldAttributes.HasDefault)!=0)
				fieldCopy.SetConstant(fieldToCopy.GetRawConstantValue());

			CopyAttributes(fieldToCopy,fieldCopy);

			_copiedFields.Add(fieldToCopy,fieldCopy);
		}

		void CopyConstructorDefinition(ConstructorInfo constructorToCopy)
		{
			if (constructorToCopy==null)
				return;
			if (!IsRuntimeType(constructorToCopy.DeclaringType))
				return;
			if (_copiedConstructors.ContainsKey(constructorToCopy))
				return;

			ParameterInfo[] parms = constructorToCopy.GetParameters();
			ConstructorBuilder constructorCopy = _copiedTypes[constructorToCopy.DeclaringType].DefineConstructor(constructorToCopy.Name,constructorToCopy.Attributes,constructorToCopy.CallingConvention,Array.ConvertAll(parms,item => GetTypeCopy(item.ParameterType)));
			int a = 0;
			foreach (ParameterInfo item in parms)
				constructorCopy.DefineParameter(++a,item.Attributes,item.Name);
			constructorCopy.SetImplementationFlags(constructorToCopy.GetMethodImplementationFlags());

			CopyAttributes(constructorToCopy,constructorCopy);

			_copiedConstructors.Add(constructorToCopy,constructorCopy);
		}

		void CopyMethodDefinition(MethodInfo methodToCopy)
		{
			if (methodToCopy==null)
				return;
			if (!IsRuntimeType(methodToCopy.DeclaringType))
				return;
			if (_copiedMethods.ContainsKey(methodToCopy))
				return;

			TypeBuilder declaringTypeCopy = _copiedTypes[methodToCopy.DeclaringType];
			MethodBuilder methodCopy = declaringTypeCopy.DefineMethod(methodToCopy.Name,methodToCopy.Attributes,methodToCopy.CallingConvention);

			Type[] genArgs = methodToCopy.GetGenericArguments();
			if (genArgs.Length!=0)
				methodCopy.DefineGenericParameters(Array.ConvertAll(genArgs,x => x.Name));

			_copiedMethods.Add(methodToCopy,methodCopy);

			ParameterInfo[] parms = methodToCopy.GetParameters();
			methodCopy.SetParameters(Array.ConvertAll(parms,item => GetTypeCopy(item.ParameterType)));

			methodCopy.SetReturnType(GetTypeCopy(methodToCopy.ReturnType));

			MethodInfo interfaceDeclaration = FindMethodBase(methodToCopy);
			if (interfaceDeclaration!=null)
				declaringTypeCopy.DefineMethodOverride(methodCopy,interfaceDeclaration);

			int a = 0;
			foreach (ParameterInfo item in parms)
				methodCopy.DefineParameter(++a,item.Attributes,item.Name);
			methodCopy.SetImplementationFlags(methodToCopy.GetMethodImplementationFlags());

			CopyAttributes(methodToCopy,methodCopy);
		}

		void CopyPropertyDefinition(PropertyInfo propertyToCopy)
		{
			if (_copiedProperties.ContainsKey(propertyToCopy))
				return;

			PropertyBuilder result = _copiedTypes[propertyToCopy.DeclaringType].DefineProperty(propertyToCopy.Name,propertyToCopy.Attributes,GetTypeCopy(propertyToCopy.PropertyType),Array.ConvertAll(propertyToCopy.GetIndexParameters(),item => GetTypeCopy(item.ParameterType)));
			if ((int)(propertyToCopy.Attributes&PropertyAttributes.HasDefault)!=0)
				result.SetConstant(propertyToCopy.GetRawConstantValue());

			if (propertyToCopy.CanRead)
				result.SetGetMethod(_copiedMethods[propertyToCopy.GetGetMethod(true)]);
			if (propertyToCopy.CanWrite)
				result.SetSetMethod(_copiedMethods[propertyToCopy.GetSetMethod(true)]);

			CopyAttributes(propertyToCopy,result);

			_copiedProperties.Add(propertyToCopy,result);
		}

		void CopyEventDefinition(EventInfo eventToCopy)
		{
			if (_copiedEvents.ContainsKey(eventToCopy))
				return;

			EventBuilder eventCopy = _copiedTypes[eventToCopy.DeclaringType].DefineEvent(eventToCopy.Name,eventToCopy.Attributes,GetTypeCopy(eventToCopy.EventHandlerType));

			void SetAccessor(MethodInfo accessor,Action<MethodBuilder> setter)
			{
				if ((accessor!=null)&&(_copiedMethods.TryGetValue(accessor,out MethodBuilder mb)))
					setter(mb);
			}
			SetAccessor(eventToCopy.GetAddMethod(true),eventCopy.SetAddOnMethod);
			SetAccessor(eventToCopy.GetRemoveMethod(true),eventCopy.SetRemoveOnMethod);
			SetAccessor(eventToCopy.GetRaiseMethod(true),eventCopy.SetRaiseMethod);
			foreach (MethodInfo mi in eventToCopy.GetOtherMethods(true))
				SetAccessor(mi,eventCopy.SetOtherMethod);

			CopyAttributes(eventToCopy,eventCopy);

			_copiedEvents.Add(eventToCopy,eventCopy);
		}

		bool IsRuntimeTypeRaw(Type type)
			=> type.Assembly==_asmToCopy;

		bool HasRuntimeArgs(Type type)
			=> type.IsGenericType&&type.GetGenericArguments().Any(IsRuntimeType);

		bool IsRuntimeType(Type type)
			=> IsRuntimeTypeRaw(type)||HasRuntimeArgs(type);

		void CopyAttributes(MemberInfo src,ICustomAttributesContainer target)
		{
			foreach (CustomAttributeData cad in src.GetCustomAttributesData())
				target.SetCustomAttribute(_assemblyFixer.CustomAttributeData(cad));
		}
		#endregion copy definitions

		#region copy method body
		void CopyMethodBody(MethodBase source,IMethodBuilderBase copy)
		{
			MethodBody methodBodyToCopy = source.GetMethodBody();
			if (methodBodyToCopy==null)
				return;

			ILGenerator gen = copy.GetILGenerator();
			gen.ValidateStack=false;
			foreach (LocalVariableInfo item in methodBodyToCopy.LocalVariables)
			{
				Type t = item.LocalType;
				if (t.IsGenericParameter)
				{
					Type[] genericTypes;
					if (t.DeclaringMethod==null)
						genericTypes=GetTypeCopy(t.DeclaringType).GetGenericArguments();
					else
					{
						object miData = GetMethodCopy((MethodInfo)t.DeclaringMethod);
						genericTypes=miData is MethodBuilder mb
							? mb.GetGenericArguments()
							: ((MethodInfo)miData).GetGenericArguments();
					}
					t=Array.Find(genericTypes,x => t.Name.EqualsOrdinal(x.Name));
				}
				else
					t=GetTypeCopy(t);
				gen.DeclareLocal(_assemblyFixer.FixType(t),item.IsPinned);
			}

			copy.InitLocals=methodBodyToCopy.InitLocals;

			WriteMethodInstructions(gen,new IlReader().ReadMethod(methodBodyToCopy,source),methodBodyToCopy.ExceptionHandlingClauses);
		}

		void WriteMethodInstructions(ILGenerator gen,IlReader.IlInstruction[] instructions,IList<ExceptionHandlingClause> tryBlocks)
		{
			Dictionary<int,List<Label>> labels = new Dictionary<int,List<Label>>();

			foreach (IlReader.IlInstruction instruction in instructions)
			{
				if (labels.TryGetValue(instruction.Address,out List<Label> labelsOffset))
					foreach (Label item in labelsOffset)
						gen.MarkLabel(item);

				foreach (ExceptionHandlingClause item in tryBlocks)
					if (item.TryOffset==instruction.Address)
						gen.BeginExceptionBlock(); //try
				if (instruction.InstructionIndex!=0)
				{
					ILOpCode prevOpcode = instructions[instruction.InstructionIndex-1].Op;
					if ((prevOpcode==ILOpCode.Leave)||(prevOpcode==ILOpCode.Leave_s))
					{
						foreach (ExceptionHandlingClause item in tryBlocks)
							if (item.HandlerOffset==instruction.Address)
								if ((item.Flags&ExceptionHandlingClauseOptions.Finally)==ExceptionHandlingClauseOptions.Finally)
									gen.BeginFinallyBlock(); //finally
								else
									gen.BeginCatchBlock(item.CatchType); //catch
							else if (item.HandlerOffset+item.HandlerLength==instruction.Address)
								gen.EndExceptionBlock(); //end catch
					}
					else if (prevOpcode==ILOpCode.Endfinally)
					{
						foreach (ExceptionHandlingClause item in tryBlocks)
							if (item.HandlerOffset+item.HandlerLength==instruction.Address)
								gen.EndExceptionBlock(); //end finally
					}
				}

				if (instruction.Data==null)
					gen.Emit(instruction.Op);
				else if (instruction.Data is FieldInfo fi)
					gen.Emit(instruction.Op,GetFieldCopy(fi));
				else if (instruction.Data is Int32)
					gen.Emit(instruction.Op,(Int32)instruction.Data);
				else if (instruction.Data is Int64)
					gen.Emit(instruction.Op,(Int64)instruction.Data);
				else if (instruction.Data is MethodInfo mi)
					gen.Emit(instruction.Op,GetMethodCopy(mi));
				else if (instruction.Data is double)
					gen.Emit(instruction.Op,(double)instruction.Data);
				else if (instruction.Data is String)
					gen.Emit(instruction.Op,(String)instruction.Data);
				else if (instruction.Data is Type)
					gen.Emit(instruction.Op,GetTypeCopy((Type)instruction.Data));
				else if (instruction.Data is short)
					gen.Emit(instruction.Op,(short)instruction.Data);
				else if (instruction.Data is byte)
					gen.Emit(instruction.Op,(byte)instruction.Data);
				else if (instruction.Data is float)
					gen.Emit(instruction.Op,(float)instruction.Data);
				else if (instruction.Data is ConstructorInfo ci)
					gen.Emit(instruction.Op,GetConstructorCopy(ci));
				else if (instruction.Op==ILOpCode.Switch)
				{
					int[] labelsOffsets = (int[])instruction.Data;
					int a = 0;
					Label[] nowLabels = new Label[labelsOffsets.Length];
					int offset = instructions[instruction.InstructionIndex+1].Address;
					foreach (int item in labelsOffsets)
					{
						if (!labels.TryGetValue(item+offset,out labelsOffset))
						{
							labelsOffset=new List<Label>();
							labels.Add(item+offset,labelsOffset);
						}
						Label l = gen.DefineLabel();
						nowLabels[a++]=l;
						labelsOffset.Add(l);
					}
					foreach (Label l in nowLabels)
						gen.Emit(instruction.Op,l);
				}
				else
					throw new NotImplementedException();
			}
		}

		Dictionary<ConstructorInfo,ConstructorInfo> _constructorReferenceCache = new Dictionary<ConstructorInfo,ConstructorInfo>();
		ConstructorInfo GetConstructorCopy(ConstructorInfo ci)
		{
			Type declType = ci.DeclaringType;
			if (!IsRuntimeType(declType))
				return _assemblyFixer.FixConstructor(ci);

			if (_copiedConstructors.TryGetValue(ci,out ConstructorBuilder result))
				return result;

			Type typeCopy = GetTypeCopy(declType);
			if (typeCopy==declType)
				return ci;

			if ((!(typeCopy is TypeBuilder))&&(declType.IsGenericType)&&(!declType.IsGenericTypeDefinition))
				/*Pokud typeCopy is generic nondef, staci mit jen nejakou declaraci (bez tela).
				Takze bych mohl udelat nejakou lite tridu, podobne jako GenericParameterBuilder.
				Ta se vyuzije v GetConstructorCopy a GetMethodCopy.*/
				return _assemblyFixer.FixConstructor(_constructorReferenceCache.TryGetValue(ci,out ConstructorInfo result3)
					? result3
					: _constructorReferenceCache.AddWithReturn(ci,new GenericTypeCopyConstructorBuilder(typeCopy,ci,ci.GetParameters().Select(p => new ParameterBuilder(p.Position,p.Attributes,p.Name,GetTypeCopy(p.ParameterType))).ToArray())));

			Type[] copiedParameters = Array.ConvertAll(ci.GetParameters(),item => GetTypeCopy(item.ParameterType));

			ConstructorInfo result2 = _assemblyFixer.FixConstructor(typeCopy is TypeBuilder tb
				? tb.GetConstructors().FirstOrDefault(x => x.IsStatic==ci.IsStatic&&x.IsPublic==ci.IsPublic&&x.CallingConvention==ci.CallingConvention&&ArraysEqual(Array.ConvertAll(x.GetParameters(),item => item.ParameterType),copiedParameters,EqualTypes))
				: typeCopy.GetConstructor((ci.IsStatic ? BindingFlags.Static : BindingFlags.Instance)|(ci.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic)|BindingFlags.DeclaredOnly,null,ci.CallingConvention,copiedParameters,null));
			if (result2==null)
				throw new InvalidOperationException();
			return result2;
		}

		Dictionary<MethodInfo,MethodInfo> _methodReferenceCache = new Dictionary<MethodInfo,MethodInfo>();
		MethodInfo GetMethodCopy(MethodInfo mi)
		{
			Type declType = mi.DeclaringType;

			Type typeCopy = GetTypeCopy(mi.DeclaringType);
			string miName = mi.Name;
			if (typeCopy is TypeBuilder tb)
				return tb.GetMethods().First(x => miName.EqualsOrdinal(x.Name));

			if ((!(typeCopy is TypeBuilder))&&(declType.IsGenericType)&&(!declType.IsGenericTypeDefinition))
				/*Pokud typeCopy is generic nondef, staci mit jen nejakou declaraci (bez tela).
				Takze bych mohl udelat nejakou lite tridu, podobne jako GenericParameterBuilder.
				Ta se vyuzije v GetConstructorCopy a GetMethodCopy.*/
				return _assemblyFixer.FixMethod(_methodReferenceCache.TryGetValue(mi,out MethodInfo result)
					? result
					: new GenericTypeCopyMethodBuilder(typeCopy,mi,
						mi.ContainsGenericParameters?mi.GetGenericArguments().Select(GetTypeCopy).ToArray():null,
						GetTypeCopy(mi.ReturnType),
						mi.GetParameters().Select(p => new ParameterBuilder(p.Position,p.Attributes,p.Name,GetTypeCopy(p.ParameterType))).ToArray(),
						null/*GetMethodCopy(mi.IsGenericMethod&&!mi.IsGenericMethodDefinition ? mi.GetGenericMethodDefinition() : mi)*/));

			return _assemblyFixer.FixMethod(_copiedMethods.TryGetValue(mi,out MethodBuilder res)
				? res
				: FindMethod(mi,typeCopy));
		}

		MethodInfo FindMethod(MethodInfo mi,Type typeCopy)
		{
			if ((mi.IsGenericMethod)&&(!mi.IsGenericMethodDefinition))
			{
				MethodInfo miDef = mi.GetGenericMethodDefinition();
				return typeCopy.GetMethods().Where(x => mi.Name.EqualsOrdinal(x.Name)&&x.GetParameters().Length==mi.GetParameters().Length)
					.First(x => x==miDef);
			}

			return typeCopy.GetMethod(mi.Name,mi.GetParameters().Select(x => GetTypeCopy(x.ParameterType)).ToArray());
		}

		Dictionary<FieldInfo,FieldInfo> _fieldReferenceCache = new Dictionary<FieldInfo,FieldInfo>();

		FieldInfo GetFieldCopy(FieldInfo fi)
		{
			Type declType = fi.DeclaringType;

			Type typeCopy = GetTypeCopy(declType);
			if (typeCopy is TypeBuilder tb)
			{
				string fiName = fi.Name;
				return tb.GetFields().First(x => fiName.EqualsOrdinal(x.Name));
			}

			if (declType.IsConstructedGenericType)
				return _fieldReferenceCache.TryGetValue(fi,out FieldInfo result)
					? result
					: _fieldReferenceCache.AddWithReturn(fi,new GenericTypeCopyFieldBuilder(typeCopy,fi,GetTypeCopy(fi.FieldType)));

			return _copiedFields.TryGetValue(fi,out FieldBuilder res) ? res : _assemblyFixer.FixField(fi);
		}
		#endregion copy method body
		#endregion copy

		#region helpers
		Type GetTypeCopy(Type type)
		{
			if (_copiedTypes.TryGetValue(type,out TypeBuilder already))
				return already;

			if ((type.IsGenericParameter)&&(IsRuntimeType(type)))
			{
				if (type.IsGenericMethodParameter())
				{
					Type[] genArgs;
					switch (type.DeclaringMethod)
					{
						case MethodInfo mi:
							genArgs=_copiedMethods[mi].GetGenericArguments();
							break;
						/*case ConstructorInfo ci:
							genArgs=_copiedConstructors[ci].GetGenericArguments();
							break;*/
						default:
							throw new InvalidOperationException();
					}
					return genArgs[type.GenericParameterPosition];
				}
				else if (type.IsGenericTypeParameter())
					return _copiedTypes[type.DeclaringType].GetGenericArguments()[type.GenericParameterPosition];
				else
					throw new NotImplementedException();
			}

			bool isGenericNonDefType = (type.IsGenericType)&&(!type.IsGenericTypeDefinition);
			bool isByRefType = (!type.IsGenericType)&&(!type.IsGenericTypeDefinition)&&(type.IsByRef);
			bool isArray = type.IsArray;
			bool breakRuntimeBlock = isGenericNonDefType||isByRefType||isArray;
			//bool isRuntimeType = IsRuntimeType(type);

			if (!breakRuntimeBlock)
				return _assemblyFixer.FixType(type);

			#region special types
			Type AddToCopiedTypes(Type toAdd)
			{
				if (toAdd is TypeBuilder tb)
				{
					_copiedTypes.Add(type,tb);

					//TODO: proverit
					/*foreach (FieldBuilder fi in tb.GetNewFields())
						_copiedFields

					foreach (ConstructorInfo mi in tb.GetConstructors(_bindingFlags))
						CopyConstructorDefinition(mi);

					foreach (MethodInfo mi in tb.GetMethods(_bindingFlags))
						CopyMethodDefinition(mi);

					foreach (PropertyInfo pi in tb.GetProperties(_bindingFlags))
						CopyPropertyDefinition(pi);

					foreach (EventInfo ei in tb.GetEvents(_bindingFlags))
						CopyEventDefinition(ei);*/
				}
				return toAdd;
			}

			if (isArray)
				return AddToCopiedTypes(_assemblyFixer.FixType(GetTypeCopy(type.GetElementType()).MakeArrayType()));

			if (isGenericNonDefType)
			{
				Type genDef = type.GetGenericTypeDefinition();
				Type genDefCopy = GetTypeCopy(genDef);
				Type[] genArgs = Array.ConvertAll(type.GetGenericArguments(),item => GetTypeCopy(item));
				return AddToCopiedTypes(_assemblyFixer.FixType(IsRuntimeType(genDef) ? TypeBuilder.MakeFromGenericForeign(genDefCopy,genArgs) : genDefCopy.MakeGenericType(genArgs)));
			}

			if (isByRefType)
				return AddToCopiedTypes(_assemblyFixer.FixType(GetTypeCopy(type.GetElementType()).MakeByRefType()));
			#endregion special types

			return type;
		}

		MethodInfo FindMethodBase(MethodInfo methodToCopy)
		{
			string name = methodToCopy.Name;
			int pos = methodToCopy.Name.LastIndexOf('.');
			if (pos==-1)
				return null;
			//return Type.GetType(name.Substring(0,pos))?.GetMethod(name.Substring(pos+1),_bindingFlags,null,methodToCopy.CallingConvention,Array.ConvertAll(methodToCopy.GetParameters(),x => x.ParameterType),null);
			string interfaceName = name.Substring(0,pos);
			Type @interface = methodToCopy.DeclaringType.GetInterfaces().FirstOrDefault(x => interfaceName.EqualsOrdinal(x.Name));
			return @interface?.GetMethod(name.Substring(pos+1),_bindingFlags,null,methodToCopy.CallingConvention,Array.ConvertAll(methodToCopy.GetParameters(),x => x.ParameterType),null);
		}

		bool EqualTypes(Type x,Type y)
		{
			if (x.Equals(y))
				return true;

			if ((x.IsGenericType)&&(y.IsGenericType))
				return x.GetGenericTypeDefinition().Equals(y.GetGenericTypeDefinition())&&ArraysEqual(x.GetGenericArguments(),y.GetGenericArguments(),EqualTypes);

			return false;
		}

		static bool ArraysEqual<T>(T[] x,T[] y,Func<T,T,bool> itemComparer)
		{
			if (x.Length!=y.Length)
				return false;

			IEnumerator yEn = y.GetEnumerator();
			foreach (T xItem in x)
			{
				if ((!yEn.MoveNext())||(!itemComparer(xItem,(T)yEn.Current)))
					return false;
			}

			return true;
		}
		#endregion helpers

		#region IlReader
		internal class IlReader
		{
			private static readonly Dictionary<ushort,ILOpCode> _instructionLookup = new Dictionary<ushort,ILOpCode>();
			Module _module;
			bool _declaringTypeIsGeneric;
			Type[] _typeGenericArguments;
			bool _methodIsGeneric;
			Type[] _methodGenericArguments;

			static IlReader()
			{
				FillLookupTable();
			}

			static object _lock = new object();
			internal IlInstruction[] ReadMethod(MethodBody methodBody,MethodBase methodBase)
			{
				lock (_lock)
					return ReadMethodNoLock(methodBody,methodBase);
			}

			internal IlInstruction[] ReadMethodNoLock(MethodBody methodBody,MethodBase methodBase)
			{
				_module=methodBase.Module;
				Type declaringType = methodBase.DeclaringType;
				_declaringTypeIsGeneric=declaringType.IsGenericType;
				_typeGenericArguments=_declaringTypeIsGeneric ? declaringType.GetGenericArguments() : null;
				_methodIsGeneric=methodBase.IsGenericMethod;
				_methodGenericArguments=_methodIsGeneric ? methodBase.GetGenericArguments() : null;

				List<IlInstruction> result = new List<IlInstruction>();

				byte[] instructionBytes = methodBody.GetILAsByteArray();
				int instructionIndex = 0;
				int startAddress;
				for (int position = 0;position<instructionBytes.Length;)
				{
					startAddress=position;

					ushort operationData = instructionBytes[position];
					if (IsInstructionPrefix(operationData))
						operationData=(ushort)((operationData<<8)|instructionBytes[++position]);

					position++;

					if (!_instructionLookup.TryGetValue(operationData,out ILOpCode code))
						throw new InvalidProgramException(string.Format("0x{0:X2} is not a valid op code.",operationData));
					OperandType operandType = GetOperandType(code);

					int dataSize = GetDataSize(operandType);
					byte[] data = new byte[dataSize];
					Buffer.BlockCopy(instructionBytes,position,data,0,dataSize);

					object objData = this.GetData(operandType,data);
					position+=dataSize;

					if (operandType==OperandType.InlineSwitch)
					{
						dataSize=(int)objData;
						int[] labels = new int[dataSize];
						for (int index = 0;index<labels.Length;index++)
						{
							labels[index]=BitConverter.ToInt32(instructionBytes,position);
							position+=4;
						}

						objData=labels;
					}

					result.Add(new IlInstruction(code,data,startAddress,objData,instructionIndex++));
				}

				return result.ToArray();
			}

			static bool IsInstructionPrefix(ushort value)
				=> ((value&OpCodes.Prefix1.Value)==OpCodes.Prefix1.Value)||((value&OpCodes.Prefix2.Value)==OpCodes.Prefix2.Value)
					||((value&OpCodes.Prefix3.Value)==OpCodes.Prefix3.Value)||((value&OpCodes.Prefix4.Value)==OpCodes.Prefix4.Value)
					||((value&OpCodes.Prefix5.Value)==OpCodes.Prefix5.Value)||((value&OpCodes.Prefix6.Value)==OpCodes.Prefix6.Value)
					||((value&OpCodes.Prefix7.Value)==OpCodes.Prefix7.Value)||((value&OpCodes.Prefixref.Value)==OpCodes.Prefixref.Value);

			static OperandType GetOperandType(ILOpCode opCode)
			{
				int position = 0;
				byte b1 = (byte)(((ushort)opCode)>>8);
				byte b2 = (byte)(((ushort)opCode)&255);
				byte[] opcodeBytes = b1==0xfe ? new byte[] { b1,b2 } : new byte[] { b2,0 };
				return InstructionOperandTypes.ReadOperandType(opcodeBytes,ref position);
			}

			static int GetDataSize(OperandType operandType)
			{
				switch (operandType)
				{
					case OperandType.InlineNone:
						return 0;
					case OperandType.ShortInlineBrTarget:
					case OperandType.ShortInlineI:
					case OperandType.ShortInlineVar:
						return 1;
					case OperandType.InlineVar:
						return 2;
					case OperandType.InlineBrTarget:
					case OperandType.InlineField:
					case OperandType.InlineI:
					case OperandType.InlineMethod:
					case OperandType.InlineSig:
					case OperandType.InlineString:
					case OperandType.InlineSwitch:
					case OperandType.InlineTok:
					case OperandType.InlineType:
					case OperandType.ShortInlineR:
						return 4;
					case OperandType.InlineI8:
					case OperandType.InlineR:
						return 8;
					default:
						return 0;
				}
			}

			private object GetData(OperandType operandType,byte[] rawData)
			{
				object data = null;
				switch (operandType)
				{
					case OperandType.InlineField:
						if ((_declaringTypeIsGeneric)||(_methodIsGeneric))
							data=_module.ResolveField(BitConverter.ToInt32(rawData,0),_typeGenericArguments,_methodGenericArguments);
						else
							data=_module.ResolveField(BitConverter.ToInt32(rawData,0));
						break;
					case OperandType.InlineSwitch:
						data=BitConverter.ToInt32(rawData,0);
						break;
					case OperandType.InlineBrTarget:
					case OperandType.InlineI:
						data=BitConverter.ToInt32(rawData,0);
						break;
					case OperandType.InlineI8:
						data=BitConverter.ToInt64(rawData,0);
						break;
					case OperandType.InlineMethod:
						if ((_declaringTypeIsGeneric)||(_methodIsGeneric))
							data=_module.ResolveMethod(BitConverter.ToInt32(rawData,0),_typeGenericArguments,_methodGenericArguments);
						else
							data=_module.ResolveMethod(BitConverter.ToInt32(rawData,0));
						break;
					case OperandType.InlineR:
						data=BitConverter.ToDouble(rawData,0);
						break;
					case OperandType.InlineSig:
						data=_module.ResolveSignature(BitConverter.ToInt32(rawData,0));
						break;
					case OperandType.InlineString:
						data=_module.ResolveString(BitConverter.ToInt32(rawData,0));
						break;
					case OperandType.InlineTok:
					case OperandType.InlineType:
						if ((_declaringTypeIsGeneric)||(_methodIsGeneric))
							data=_module.ResolveType(BitConverter.ToInt32(rawData,0),_typeGenericArguments,_methodGenericArguments);
						else
							data=_module.ResolveType(BitConverter.ToInt32(rawData,0));
						break;
					case OperandType.InlineVar:
						data=BitConverter.ToInt16(rawData,0);
						break;
					case OperandType.ShortInlineVar:
					case OperandType.ShortInlineI:
					case OperandType.ShortInlineBrTarget:
						data=rawData[0];
						break;
					case OperandType.ShortInlineR:
						data=BitConverter.ToSingle(rawData,0);
						break;
				}
				return data;
			}

			static void FillLookupTable()
			{
				// Might be better to do an array lookup.  Use a seperate arrary for instructions without a prefix and array for each prefix.
				FieldInfo[] fields = typeof(ILOpCode).GetFields(BindingFlags.Static|BindingFlags.Public);
				foreach (FieldInfo field in fields)
				{
					ILOpCode code = (ILOpCode)field.GetValue(null);
					_instructionLookup.Add((ushort)code,code);
				}
			}

			[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
			internal struct IlInstruction
			{
				private readonly ILOpCode operationCode; // 40.  56-64.  The entire structure is very big.  maybe do array lookup for opcode instead.
				private readonly byte[] instructionRawData;
				private readonly object instructionData;
				private readonly int instructionAddress;
				private readonly int index;

				internal IlInstruction(ILOpCode code,byte[] instructionRawData,int instructionAddress,object instructionData,int index)
				{
					this.operationCode=code;
					this.instructionRawData=instructionRawData;
					this.instructionAddress=instructionAddress;
					this.instructionData=instructionData;
					this.index=index;
				}

				public ILOpCode Op { get { return this.operationCode; } }
				public byte[] RawData { get { return this.instructionRawData; } }
				public object Data { get { return this.instructionData; } }
				public int Address { get { return this.instructionAddress; } }
				public int InstructionIndex { get { return this.index; } }

				public int DataValue
				{
					get
					{
						if (this.Data!=null)
						{
							if (this.Data is byte)
								return (byte)this.Data;
							else if (this.Data is short)
								return (short)this.Data;
							else if (this.Data is int)
								return (int)this.Data;
						}
						return 0;
					}
				}

				//public int Length { get { return this.Op.Size+(this.RawData==null ? 0 : this.RawData.Length); } }

#if DEBUG
				public override string ToString()
				{
					StringBuilder builder = new StringBuilder();
					builder.AppendFormat("0x{0:x4} {1,-10}",this.Address,this.Op);

					if (this.Data!=null)
						builder.Append(this.Data.ToString());

					if (this.RawData!=null&&this.RawData.Length>0)
					{
						builder.Append(" [0x");
						for (int i = this.RawData.Length-1;i>=0;i--)
							builder.Append(this.RawData[i].ToString("x2",System.Globalization.CultureInfo.InvariantCulture));
						builder.Append(']');
					}

					return builder.ToString();
				}
#endif
			}
		}
		#endregion IlReader

		#region ClearCollections
		void ClearCollections()
		{
			_copiedTypes.Clear();
			_copiedMethods.Clear();
			_copiedConstructors.Clear();
			_copiedFields.Clear();
			_copiedEvents.Clear();
			_copiedProperties.Clear();

			_constructorReferenceCache.Clear();
			_methodReferenceCache.Clear();
			_fieldReferenceCache.Clear();
		}
		#endregion ClearCollections

		#region assembly CopyCustomAttributes
		public IEnumerable<CustomAttributeData> CopyCustomAttributes(IEnumerable<CustomAttributeData> customAttributes)
			=> customAttributes.Select(_assemblyFixer.CustomAttributeData);
		#endregion assembly CopyCustomAttributes
	}

	#region extensions
	static class StringExtensions
	{
		internal static bool EqualsOrdinal(this string value,string other)
			=> string.Equals(value,other,StringComparison.Ordinal);
	}

	static class ICollectionExtensions
	{
		internal static TValue AddWithReturn<TKey, TValue>(this IDictionary<TKey,TValue> dict,TKey key,TValue value)
		{
			dict.Add(key,value);
			return value;
		}

		internal static T AddWithReturn<T>(this ICollection<T> coll,T value)
		{
			coll.Add(value);
			return value;
		}
	}

	static class TypeExtensions
	{
		internal static Type[] GetInterfacesDirect(this Type type)
		{
			Type[] ints = type.GetInterfaces();
			return ints
				 .Except(type.BaseType?.GetInterfaces()??Enumerable.Empty<Type>())
				 .Except(ints.SelectMany(i => i.GetInterfaces()))
			.ToArray();
		}
	}
	#endregion extensions
}

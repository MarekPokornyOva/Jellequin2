//MethodDebugInformation table is either empty(missing) or has exactly as many rows as MethodDef table
//https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md
//https://github.com/dotnet/symreader-portable

#region using
using Jellequin.Compiler;
using Microsoft.Cci;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SequencePoint = System.Reflection.Metadata.Ecma335.SequencePoint;
#endregion using

namespace Jellequin.Reflection.Emit
{
	internal class AssemblyWriter
	{
		#region fields
		ModuleBuilder _moduleBuilder;

		PEHeaderBuilder _pEHeaderBuilder;
		MetadataBuilder _tablesAndHeaps;
		BlobBuilder _ilStream;
		MethodBodyStreamEncoder _methodBodies;
		//AssemblyReferenceHandle _frameworkAssemblyReferenceHandle;

		readonly Dictionary<TypeBuilder, (TypeDefineInfo Info, EntityHandle Handle, TypeDefinitionHandle TypeHandle)> _definedTypes=new Dictionary<TypeBuilder, (TypeDefineInfo Info, EntityHandle Handle, TypeDefinitionHandle TypeHandle)>();
		readonly Dictionary<Type,GenericParameterHandle> _genericParameters = new Dictionary<Type,GenericParameterHandle>();
		readonly Dictionary<FieldBuilder, FieldDefinitionHandle> _definedFields = new Dictionary<FieldBuilder, FieldDefinitionHandle>();
		readonly Dictionary<IMethodBuilderBase,(EntityHandle Handle, MethodDefinitionHandle MethodHandle)> _definedMethods = new Dictionary<IMethodBuilderBase,(EntityHandle Handle, MethodDefinitionHandle MethodHandle)>();
		readonly List<(SequencePoint[],int)> _seqPoints = new List<(SequencePoint[],int)>();

		readonly Dictionary<IMethodBuilderBase,MethodSpecificationHandle> _methodsSpecification=new Dictionary<IMethodBuilderBase, MethodSpecificationHandle>();

		MetadataBuilder _pdbTablesAndHeaps;
		DocumentHandle _docPdb;
		#endregion fields

		#region main Write method
		internal void Write(ModuleBuilder module,MethodBuilder entryPoint,Stream assembly,SaveOptions options)
		{
			(BlobBuilder output, BlobBuilder outputPdb)=WriteInt(module,entryPoint,assembly,options);

			void WriteBlobBuilder(BlobBuilder bb,Stream stream)
			{
				if (stream!=null)
					foreach (Blob blob in bb.GetBlobs())
					{
						ArraySegment<byte> data = blob.GetBytes();
						stream.Write(data.Array,data.Offset,data.Count);
					}
			}

			if (outputPdb!=null)
				WriteBlobBuilder(outputPdb,options.Symbols.Pdb);
			WriteBlobBuilder(output,assembly);
		}

		internal Task WriteAsync(ModuleBuilder module,MethodBuilder entryPoint,Stream assembly,SaveOptions options)
		{
			(BlobBuilder output,BlobBuilder outputPdb)=WriteInt(module,entryPoint,assembly,options);

			Task WriteBlobBuilder(BlobBuilder bb,Stream stream)
			{
				Task resultW = Task.CompletedTask;
				if (stream!=null)
					foreach (Blob blob in bb.GetBlobs())
					{
						ArraySegment<byte> data = blob.GetBytes();
						resultW=resultW.ContinueWith(t=>stream.WriteAsync(data.Array,data.Offset,data.Count));
					}
				return resultW;
			}

			Task result=WriteBlobBuilder(output,assembly);
			if (outputPdb!=null)
				result=Task.WhenAll(new[] { result,WriteBlobBuilder(outputPdb,options.Symbols.Pdb) });
			return result;
		}

		(BlobBuilder assembly, BlobBuilder symbols) WriteInt(ModuleBuilder module,MethodBuilder entryPoint,Stream assembly,SaveOptions options)
		{
			//Assembly frameworkAssembly=options.FrameworkAssembly??throw new NullReferenceException("Framework assembly must be specified.");

			//Stream runtimeAsmToCopy
			_moduleBuilder=module;

			_pEHeaderBuilder=entryPoint==null ? PEHeaderBuilder.CreateLibraryHeader() : PEHeaderBuilder.CreateExecutableHeader();
			//_pEHeaderBuilder.Subsystem - Win/Console/...
			_tablesAndHeaps=new MetadataBuilder();
			_ilStream=new BlobBuilder();
			_methodBodies=new MethodBodyStreamEncoder(_ilStream);

			//_frameworkAssemblyReferenceHandle=GetAssemblyReference(frameworkAssembly); //_tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString("System.Runtime"), new Version(4, 2, 0, 0), default(StringHandle), _tablesAndHeaps.GetOrAddBlob(ImmutableArray.Create<byte>(0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x07, 0xd1, 0xfa, 0x57, 0xc4, 0xae, 0xd9, 0xf0, 0xa3, 0x2e, 0x84, 0xaa, 0x0f, 0xae, 0xfd, 0x0d, 0xe9, 0xe8, 0xfd, 0x6a, 0xec, 0x8f, 0x87, 0xfb, 0x03, 0x76, 0x6c, 0x83, 0x4c, 0x99, 0x92, 0x1e, 0xb2, 0x3b, 0xe7, 0x9a, 0xd9, 0xd5, 0xdc, 0xc1, 0xdd, 0x9a, 0xd2, 0x36, 0x13, 0x21, 0x02, 0x90, 0x0b, 0x72, 0x3c, 0xf9, 0x80, 0x95, 0x7f, 0xc4, 0xe1, 0x77, 0x10, 0x8f, 0xc6, 0x07, 0x77, 0x4f, 0x29, 0xe8, 0x32, 0x0e, 0x92, 0xea, 0x05, 0xec, 0xe4, 0xe8, 0x21, 0xc0, 0xa5, 0xef, 0xe8, 0xf1, 0x64, 0x5c, 0x4c, 0x0c, 0x93, 0xc1, 0xab, 0x99, 0x28, 0x5d, 0x62, 0x2c, 0xaa, 0x65, 0x2c, 0x1d, 0xfa, 0xd6, 0x3d, 0x74, 0x5d, 0x6f, 0x2d, 0xe5, 0xf1, 0x7e, 0x5e, 0xaf, 0x0f, 0xc4, 0x96, 0x3d, 0x26, 0x1c, 0x8a, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6d, 0xc0, 0x93, 0x34, 0x4d, 0x5a, 0xd2, 0x93)), AssemblyFlags.PublicKey, _tablesAndHeaps.GetOrAddBlob(BitConverter.GetBytes((int)AssemblyHashAlgorithm.Sha1)));

			SymbolsSaveOptions symbolsOptions = options.Symbols;
			bool debug = symbolsOptions!=null;
			if (debug)
			{
				//pdb
				/*_pdbTablesAndHeaps = new MetadataBuilder();
				BlobBuilder sourceCodeBytes = new BlobBuilder();
				byte[] sourceCodeBytesOrg = code.GetBytes();
				sourceCodeBytes.WriteInt32(0);
				sourceCodeBytes.WriteBytes(sourceCodeBytesOrg);

				_docPdb = _pdbTablesAndHeaps.AddDocument(_pdbTablesAndHeaps.GetOrAddDocumentName(code.GetEmbedFilename()),
						_pdbTablesAndHeaps.GetOrAddGuid(HashAlgorithmGuids.Sha1), _pdbTablesAndHeaps.GetOrAddBlob(CalculateChecksum(sourceCodeBytesOrg, SourceHashAlgorithm.Sha1)),
						_pdbTablesAndHeaps.GetOrAddGuid(new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1")));

				if (debugOptions.EmbedSourceCode)
					_pdbTablesAndHeaps.AddCustomDebugInformation(
						parent: _docPdb,
						kind: _pdbTablesAndHeaps.GetOrAddGuid(Microsoft.CodeAnalysis.Debugging.PortableCustomDebugInfoKinds.EmbeddedSource),
						value: _pdbTablesAndHeaps.GetOrAddBlob(sourceCodeBytes));*/

				_pdbTablesAndHeaps=new MetadataBuilder();
				ISource code = options.Symbols.Code;
				EmbeddedText et = EmbeddedText.FromBytes(code.GetEmbedFilename(),new ArraySegment<byte>(code.GetBytes()),symbolsOptions.HashAlgorithm);

				_docPdb=_pdbTablesAndHeaps.AddDocument(_pdbTablesAndHeaps.GetOrAddDocumentName(et.FilePath),
					_pdbTablesAndHeaps.GetOrAddGuid(HashAlgorithmGuids.Sha1),_pdbTablesAndHeaps.GetOrAddBlob(et.Checksum),
					_pdbTablesAndHeaps.GetOrAddGuid(new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1")));

				if (symbolsOptions.EmbedSource)
					_pdbTablesAndHeaps.AddCustomDebugInformation(
						parent: _docPdb,
						kind: _pdbTablesAndHeaps.GetOrAddGuid(Microsoft.CodeAnalysis.Debugging.PortableCustomDebugInfoKinds.EmbeddedSource),
						value: _pdbTablesAndHeaps.GetOrAddBlob(et.Blob));
			}

			//write
			WriteAssemblyAndModule(options.AssemblyAttributes,debug);
			WriteTypes();
			WriteFields();
			WriteMethods();
			WriteProperties();
			WriteEvents();
			if (debug)
				WriteSeqSymbols();

			//finalize
			MethodDefinitionHandle entryPointMethodHandle = entryPoint==null ? default(MethodDefinitionHandle) : _definedMethods[entryPoint].MethodHandle;

			BlobBuilder outputPdb;
			DebugDirectoryBuilder ddBuilder;
			if (debug)
			{
				outputPdb = new BlobBuilder();
				PortablePdbBuilder pdbBuilder = new PortablePdbBuilder(_pdbTablesAndHeaps, _tablesAndHeaps.GetRowCounts(), entryPointMethodHandle);
				BlobContentId idPdb = pdbBuilder.Serialize(outputPdb);

				ddBuilder = new DebugDirectoryBuilder();
				ddBuilder.AddCodeViewEntry(_moduleBuilder.AssemblyName.Name + ".pdb", idPdb, pdbBuilder.FormatVersion);
			}
			else
			{
				outputPdb = null;
				ddBuilder = null;
			}

			Stream icon = options.Icon;
			ResourceSectionBuilder nativeResources = icon==null?null:new Win32ResourceSectionBuilder(null,icon);

			BlobBuilder output = new BlobBuilder();
			new ManagedPEBuilder(_pEHeaderBuilder, new MetadataRootBuilder(_tablesAndHeaps), _ilStream, entryPoint: entryPointMethodHandle, debugDirectoryBuilder: ddBuilder, nativeResources : nativeResources).Serialize(output);

			return (output, debug ? outputPdb : null);
		}
		#endregion main Write method

		#region write methods
		void WriteAssemblyAndModule(IEnumerable<CustomAttributeData> assemblyAttributes,bool debug)
		{
			AssemblyName asmName = _moduleBuilder.AssemblyName;
			StringHandle name = _tablesAndHeaps.GetOrAddString(asmName.Name);
			string culture = asmName.CultureName;
			byte[] publicKey = asmName.GetPublicKey();
			AssemblyDefinitionHandle asmDefHandle = _tablesAndHeaps.AddAssembly(name,asmName.Version??new Version(),culture==null||culture.Length==0 ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(culture),publicKey==null||publicKey.Length==0 ? default(BlobHandle) : _tablesAndHeaps.GetOrAddBlob(publicKey),(AssemblyFlags)asmName.Flags,(AssemblyHashAlgorithm)asmName.HashAlgorithm);
			AddBaseAssemblyAttributes(asmDefHandle,debug);
			if (assemblyAttributes!=null)
				AddCustomAttributes(asmDefHandle,assemblyAttributes);
			_tablesAndHeaps.AddModule(1, _tablesAndHeaps.GetOrAddString("<module>"), _tablesAndHeaps.GetOrAddGuid(Guid.NewGuid()), default(GuidHandle), default(GuidHandle));

			//Assembly works perfectly without following <Module> type but asm.GetType() returns empty array
			_tablesAndHeaps.AddTypeDefinition(default(TypeAttributes), default(StringHandle), _tablesAndHeaps.GetOrAddString("<Module>"), default(EntityHandle), MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
		}

		void WriteTypes()
		{
			int copiedFieldsCount = _tablesAndHeaps.GetRowCount(TableIndex.Field);
			int copiedMethodsCount = _tablesAndHeaps.GetRowCount(TableIndex.MethodDef);

			var temp=_moduleBuilder.Types.Union(_moduleBuilder.Types.SelectMany(x => x.GetNestedTypeBuilders())).ToArray();
			TypeDefineInfo[] typesToCreate = temp.Select(type =>
			{
				TypeDefineInfo result = new TypeDefineInfo { Type=type, FieldIndex=copiedFieldsCount, MethodIndex=copiedMethodsCount, Methods=type.GetMethodBuilders().Cast<IMethodBuilderBase>().Concat(type.GetConstructorBuilders()).ToArray() };
				copiedFieldsCount+=type.GetFieldBuilders().Count;
				//copiedMethodsCount+=type.Methods.Count+type.Constructors.Count;

				foreach (IMethodBuilderBase mb in type.GetMethodBuilders().Cast<IMethodBuilderBase>().Concat(type.GetConstructorBuilders()))
				{
					MethodDefinitionHandle mdh=MetadataTokens.MethodDefinitionHandle(++copiedMethodsCount);
					_definedMethods.Add(mb,(mdh,mdh));
				}

				return result;
			}).ToArray();

			foreach (TypeDefineInfo typeInfo in typesToCreate)
			{
				TypeBuilder type = typeInfo.Type;
				TypeDefinitionHandle typeDef = _tablesAndHeaps.AddTypeDefinition(type.Attributes,_tablesAndHeaps.GetOrAddString(type.Namespace??""),_tablesAndHeaps.GetOrAddString(type.Name),GetTypeHandle(type.BaseType),MetadataTokens.FieldDefinitionHandle(typeInfo.FieldIndex+1),MetadataTokens.MethodDefinitionHandle(typeInfo.MethodIndex+1));

				if (type.IsNested)
					_tablesAndHeaps.AddNestedType(typeDef,_definedTypes[(TypeBuilder)type.DeclaringType].TypeHandle);

				foreach (Type interfaceType in type.GetInterfaces())
					_tablesAndHeaps.AddInterfaceImplementation(typeDef,GetTypeHandle(interfaceType));

				AddCustomAttributes(typeDef,type);

				_definedTypes.Add(type,(typeInfo, typeDef, typeDef));
			}

			void TypeUpdater(KeyValuePair<TypeBuilder,(TypeDefineInfo Info, EntityHandle Handle, TypeDefinitionHandle TypeHandle)> item)
				=> _definedTypes[item.Key]=(item.Value.Info, _tablesAndHeaps.AddTypeSpecification(_tablesAndHeaps.GetOrAddBlob(BuildSignature(be => BuildSignature(be.TypeSpecificationSignature(),item.Key)))), item.Value.TypeHandle);
			void MethodUpdater(KeyValuePair<IMethodBuilderBase,(EntityHandle Handle, MethodDefinitionHandle MethodHandle)> item)
			{
				if (item.Key is MethodBuilder mbb)
					_definedMethods[item.Key]=(_tablesAndHeaps.AddMethodSpecification(item.Value.MethodHandle,_tablesAndHeaps.GetOrAddBlob(BuildMethodSpecSignature(mbb.GetGenericArguments()))), item.Value.MethodHandle);
			}
			(EntityHandle Handle, Type[] GenericTypes, Action Updater)[] genericParmsToAdd = _definedTypes.Where(x => x.Key.IsGenericType).Select(x => (x.Value.Handle, GenericTypes: x.Key.GetGenericArguments(), Updater: (Action)(()=>TypeUpdater(x))))
				.Union(_definedMethods.Where(x => x.Key is MethodBuilder mb&&mb.IsGenericMethodDefinition).Select(x => (x.Value.Handle, GenericTypes: (x.Key is MethodBuilder mb ? mb.GetGenericArguments() : null), Updater: (Action)(()=>MethodUpdater(x)))))
				.OrderBy(x => CodedIndex.TypeOrMethodDef(x.Handle))
				.ToArray();
			foreach ((EntityHandle Handle, Type[] GenericTypes, Action Updater) item in genericParmsToAdd)
			{
				foreach (Type genParm in item.GenericTypes)
					_genericParameters.Add(genParm,_tablesAndHeaps.AddGenericParameter(item.Handle,genParm.GenericParameterAttributes,_tablesAndHeaps.GetOrAddString(genParm.Name),genParm.GenericParameterPosition));
				item.Updater?.Invoke();
			}
		}

		void WriteFields()
		{
			foreach (KeyValuePair<TypeBuilder,(TypeDefineInfo Info, EntityHandle Handle, TypeDefinitionHandle TypeHandle)> item in _definedTypes)
				foreach (FieldBuilder fb in item.Key.GetFieldBuilders())
				{
					FieldDefinitionHandle fieldDefinitionHandle = _tablesAndHeaps.AddFieldDefinition(fb.Attributes, _tablesAndHeaps.GetOrAddString(fb.Name), _tablesAndHeaps.GetOrAddBlob(BuildFieldSignature(fb.FieldType)));
					_definedFields[fb]=fieldDefinitionHandle;
					if (fb.HasRawConstantValue)
						_tablesAndHeaps.AddConstant(fieldDefinitionHandle, fb.GetRawConstantValue());

					AddCustomAttributes(fieldDefinitionHandle,fb);
				}
		}

		void WriteProperties()
		{
			PropertyDefinitionHandle propDefHandle;
			bool isFirst;
			foreach (KeyValuePair<TypeBuilder, (TypeDefineInfo Info, EntityHandle Handle,TypeDefinitionHandle TypeHandle)> item in _definedTypes)
			{
				isFirst=true;
				TypeDefinitionHandle typeDefHandle = item.Value.TypeHandle;
				foreach (PropertyBuilder pb in item.Key.GetPropertyBuilders())
				{
					propDefHandle=_tablesAndHeaps.AddProperty(pb.Attributes, _tablesAndHeaps.GetOrAddString(pb.Name), _tablesAndHeaps.GetOrAddBlob(BuildSignature(e => e.PropertySignature(!(pb.GetMethod??pb.SetMethod).IsStatic).Parameters(0, returnType => BuildReturnSignature(returnType, pb.PropertyType), parameters => { }))));
					if (isFirst)
					{
						_tablesAndHeaps.AddPropertyMap(typeDefHandle, propDefHandle);
						isFirst=false;
					}
					if (pb.GetMethodBuilder!=null)
						_tablesAndHeaps.AddMethodSemantics(propDefHandle,MethodSemanticsAttributes.Getter,_definedMethods[pb.GetMethodBuilder].MethodHandle);
					if (pb.SetMethodBuilder!=null)
						_tablesAndHeaps.AddMethodSemantics(propDefHandle,MethodSemanticsAttributes.Setter,_definedMethods[pb.SetMethodBuilder].MethodHandle);

					if (pb.HasRawConstantValue)
						_tablesAndHeaps.AddConstant(propDefHandle,pb.GetRawConstantValue());

					AddCustomAttributes(propDefHandle,pb);
				}
			}
		}

		void WriteEvents()
		{
			EventDefinitionHandle eventDefHandle;
			bool isFirst;
			foreach (KeyValuePair<TypeBuilder,(TypeDefineInfo Info, EntityHandle Handle, TypeDefinitionHandle TypeHandle)> item in _definedTypes)
			{
				isFirst=true;
				TypeDefinitionHandle typeDefHandle = item.Value.TypeHandle;
				foreach (EventBuilder eb in item.Key.GetEventBuilders())
				{
					eventDefHandle=_tablesAndHeaps.AddEvent(eb.Attributes,_tablesAndHeaps.GetOrAddString(eb.Name),GetTypeHandle(eb.EventHandlerType));
					if (isFirst)
					{
						_tablesAndHeaps.AddEventMap(typeDefHandle,eventDefHandle);
						isFirst=false;
					}

					_tablesAndHeaps.AddMethodSemantics(eventDefHandle,MethodSemanticsAttributes.Adder,eb.AddMethodBuilder==null ? default(MethodDefinitionHandle) : _definedMethods[eb.AddMethodBuilder].MethodHandle);
					_tablesAndHeaps.AddMethodSemantics(eventDefHandle,MethodSemanticsAttributes.Remover,eb.RemoveMethodBuilder==null ? default(MethodDefinitionHandle) : _definedMethods[eb.RemoveMethodBuilder].MethodHandle);
					if (eb.RaiseMethodBuilder!=null)
						_tablesAndHeaps.AddMethodSemantics(eventDefHandle,MethodSemanticsAttributes.Raiser,_definedMethods[eb.RaiseMethodBuilder].MethodHandle);
					foreach (MethodBuilder mb in eb.OtherMethodBuilders)
						_tablesAndHeaps.AddMethodSemantics(eventDefHandle,MethodSemanticsAttributes.Other,_definedMethods[mb].MethodHandle);

					AddCustomAttributes(eventDefHandle,eb);
				}
			}
		}

		void WriteMethods()
		{
			int paramIndex = _tablesAndHeaps.GetRowCount(TableIndex.Param) + 1;
			foreach (KeyValuePair<IMethodBuilderBase,(EntityHandle Handle, MethodDefinitionHandle MethodHandle)> item in _definedMethods.ToArray()) //ToArray is used to omit CollectionModified exception caused by _definedMethods[mb]=.AddMethodSpecification()
			{
				IMethodBuilderBase mb = item.Key;
				ParameterBuilder[] pis = mb.GetParameterBuilders();
				ParameterHandle firstParameter = MetadataTokens.ParameterHandle(paramIndex);

				int a = 0;
				foreach (ParameterBuilder pb in pis)
					AddCustomAttributes(_tablesAndHeaps.AddParameter(pb.Attributes,pb.Name==null ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(pb.Name),++a),pb);
				paramIndex += pis.Length;

				MethodDefinitionHandle mdhPre = item.Value.MethodHandle;

				(int bodyOffset, int instructionsSize)=WriteMethodBody(mb);
				int genericParameterCount = mb is MethodBuilder mba ? mba.GetGenericArguments()?.Length??0 : 0;
				MethodDefinitionHandle mdh = _tablesAndHeaps.AddMethodDefinition(mb.Attributes,
						mb.GetMethodImplementationFlags(),
						_tablesAndHeaps.GetOrAddString(mb.Name),
						_tablesAndHeaps.GetOrAddBlob(BuildMethodSignature(!mb.IsStatic,mb.ReturnType(),pis.Select(x=>x.ParameterType).ToArray(),genericParameterCount)),
						bodyOffset, firstParameter);
				if (mdhPre!=mdh)
					throw new InvalidOperationException();

				AddCustomAttributes(mdh,mb);

				if (_pdbTablesAndHeaps != null)
					_pdbTablesAndHeaps.AddLocalScope(
							method: mdh,
							importScope: default(ImportScopeHandle),
							variableList: MetadataTokens.LocalVariableHandle(1),
							constantList: MetadataTokens.LocalConstantHandle(1),
							startOffset: 0,
							length: instructionsSize);
			}
		}
		#endregion write methods

		#region methodbody
		(int bodyOffset, int instructionsSize) WriteMethodBody(IMethodBuilderBase mb)
		{
			MethodBodyBuilder methodBody = mb.GetMethodBodyBuilder();
			if (methodBody==null)
			{
				_seqPoints.Add((new SequencePoint[0], 0));
				return (-1, 0);
			}
			IList<LocalBuilder> localVariables = methodBody.ILGenerator.Locals;
			bool hasLocals = localVariables.Count!=0;

			StandaloneSignatureHandle localVariablesSignature;
			if (hasLocals)
			{
				LocalVariablesEncoder encoder = new BlobEncoder(new BlobBuilder()).LocalVariableSignature(localVariables.Count);
				foreach (LocalBuilder local in localVariables)
					BuildSignature(encoder.AddVariable().Type(local.IsPinned),local.LocalType);
				localVariablesSignature=_tablesAndHeaps.AddStandaloneSignature(_tablesAndHeaps.GetOrAddBlob(encoder.Builder));
			}
			else
				localVariablesSignature=default;

			InstructionEncoder instructionEncoder = new InstructionEncoder(new BlobBuilder(), new ControlFlowBuilder());
			List<SequencePoint> seqPoints = new List<SequencePoint>();
			int maxStackSize = WriteMethodInstructions(methodBody, instructionEncoder, seqPoints, mb.ReturnType().Equals(typeof(void)),mb);
			bool initLocals = mb.InitLocals&hasLocals;
			//if (initLocals)
			_seqPoints.Add((seqPoints.ToArray(), MetadataTokens.GetRowNumber(localVariablesSignature)));
			int offset = _methodBodies.AddMethodBody(instructionEncoder,maxStackSize,localVariablesSignature,initLocals ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);

			return (offset, instructionEncoder.CodeBuilder.Count);
		}

		int WriteMethodInstructions(MethodBodyBuilder methodBody, InstructionEncoder instructionEncoder, List<SequencePoint> seqPoints, bool returnsVoid, IMethodBuilderBase debug)
		{
			ILGenerator gen = methodBody.ILGenerator;
			int curStack=0;
			int maxStack=0;

			void AdjustStack(int change)
			{
				curStack+=change;
				if (maxStack<curStack)
					maxStack=curStack;
			}

			Dictionary<Label, ILabelHandle> definedLabels = gen.Labels.ToDictionary(x => x, x => new ILabelHandle());
			Dictionary<Label, LabelHandle> definedExLabels = new Dictionary<Label, LabelHandle>();

			foreach (ExceptionRegionInfo exRegionInfo in gen.Exceptions)
			{
				LabelHandle tryStart = instructionEncoder.DefineLabel();
				LabelHandle handleStart = instructionEncoder.DefineLabel();
				LabelHandle handleEnd = instructionEncoder.DefineLabel();
				definedExLabels.Add(exRegionInfo.TryStart, tryStart);
				definedExLabels.Add(exRegionInfo.HandleStart, handleStart);
				definedExLabels.Add(exRegionInfo.HandleEnd, handleEnd);

				if (exRegionInfo.IsCatch)
					instructionEncoder.ControlFlowBuilder.AddCatchRegion(tryStart, handleStart, handleStart, handleEnd,GetTypeHandle(exRegionInfo.ExceptionType));
				else
					instructionEncoder.ControlFlowBuilder.AddFinallyRegion(tryStart, handleStart, handleStart, handleEnd);
			}

			foreach (Instruction instruction in gen.Instructions)
			{
				if (instruction.SourceLocation!=default)
				{
					(string documentName, int startLineNumber, ushort startColumn, int endLineNumber, ushort endColumn)=instruction.SourceLocation;
					bool replace = (seqPoints.Count!=0)&&(seqPoints[seqPoints.Count-1].Offset==instructionEncoder.Offset);

					if (SequencePoint.IsValidPoint(startLineNumber,startColumn,endLineNumber,endColumn))
					{
						if (replace)
							seqPoints.RemoveAt(seqPoints.Count-1);
						seqPoints.Add(SequencePoint.Create(_docPdb,instructionEncoder.Offset,startLineNumber,startColumn,endLineNumber,endColumn));
					}
					else
						if (!replace)
							seqPoints.Add(SequencePoint.Hidden(_docPdb,instructionEncoder.Offset));
				}

				foreach (Label theLabel in instruction.Labels)
				{
					definedLabels[theLabel].Offset=instructionEncoder.Offset;
					if (definedExLabels.TryGetValue(theLabel,out LabelHandle labelHandle))
					{
						instructionEncoder.MarkLabel(labelHandle);

						//if current offset is HandleStart, we have to call AdjustStack(1) as .NET puts exception variable onto stack
						if (gen.Exceptions.Any(x => x.HandleStart==theLabel))
							AdjustStack(1);
					}
				}

				if ((instruction is Instruction<int>)||(instruction is Instruction<double>)||(instruction is Instruction<Label>))
				{
					//don't write opcode
					if (instruction.OpCode!=ILOpCode.Switch) //... neither adjust stack for Switch instructions
						AdjustStack(instruction.OpCode.NetStackBehavior());
				}
				else
				{
					ILOpCode code = instruction.OpCode;
					instructionEncoder.OpCode(code);
					if (code!=ILOpCode.Ret)
						AdjustStack(code.NetStackBehavior());
				}

				if (instruction.GetType().Equals(typeof(Instruction)))
				{
					//don't do anything;
				}
				else if (instruction is Instruction<ConstructorInfo> ciIns)
				{
					instructionEncoder.Token(GetMemberHandle(ciIns.Data));
					AdjustStack(-ciIns.Data.GetParameters().Length-1);
				}
				else if (instruction is Instruction<ConstructorBuilder> cbIns)
				{
					instructionEncoder.Token(GetMemberHandle(cbIns.Data));
					AdjustStack(-cbIns.Data.GetParameters().Length-1);
				}
				else if (instruction is Instruction<MethodInfo> miIns)
				{
					instructionEncoder.Token(GetMemberHandle(miIns.Data));
					if (instruction.OpCode!=ILOpCode.Ldftn)
						AdjustStack(-miIns.Data.GetParameters().Length+(miIns.Data.ReturnType.Equals(typeof(void))?0:1)-(miIns.Data.IsStatic?0:1));
				}
				else if (instruction is Instruction<MethodBuilder> mbIns)
				{
					instructionEncoder.Token(GetMemberHandle(mbIns.Data));
					if (instruction.OpCode!=ILOpCode.Ldftn)
						AdjustStack(-mbIns.Data.GetParameters().Length+((mbIns.Data.ReturnType.Equals(typeof(void)))?0:1)-(mbIns.Data.IsStatic?0:1));
				}
				else if (instruction is Instruction<FieldInfo> fiIns)
					instructionEncoder.Token(GetMemberHandle(fiIns.Data));
				else if (instruction is Instruction<FieldBuilder> fbIns)
					instructionEncoder.Token(_definedFields[fbIns.Data]);
				else if (instruction is Instruction<Type> typeIns)
					instructionEncoder.Token(GetTypeHandle(typeIns.Data));
				else if (instruction is Instruction<LocalBuilder> lbIns)
					instructionEncoder.Token(lbIns.Data.Index);
				else if (instruction is Instruction<Label> labelIns)
				{
					ILOpCode opCode = instruction.OpCode;
					int operandSize = opCode==ILOpCode.Switch ? 4 : opCode.GetBranchOperandSize();
					instructionEncoder.OpCode(opCode);
					definedLabels[labelIns.Data].ReferenceOffsets.Add((instructionEncoder.Offset, operandSize, instructionEncoder.CodeBuilder.ReserveBytes(operandSize)));
				}
				else if (instruction is Instruction<int> intIns)
				{
					if (intIns.OpCode==ILOpCode.Ldc_i4)
						instructionEncoder.LoadConstantI4(intIns.Data);
					else if ((intIns.OpCode==ILOpCode.Ldarg)||(intIns.OpCode==ILOpCode.Ldarg_s))
						instructionEncoder.LoadArgument(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Starg)
						instructionEncoder.StoreArgument(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Ldloc)
						instructionEncoder.LoadLocal(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Stloc)
						instructionEncoder.StoreLocal(intIns.Data);
					else if (intIns.OpCode.IsBranch())
					{
						ILOpCode opCode = instruction.OpCode;
						if (opCode.GetBranchOperandSize()==1)
						{
							instructionEncoder.OpCode(opCode);
							instructionEncoder.CodeBuilder.WriteSByte((sbyte)intIns.Data);
						}
						else
						{
							instructionEncoder.OpCode(opCode);
							instructionEncoder.CodeBuilder.WriteInt32(intIns.Data);
						}
					}
					else if ((intIns.OpCode==ILOpCode.Ldloca_s)||(intIns.OpCode==ILOpCode.Ldc_i4_s)||(intIns.OpCode==ILOpCode.Stloc_s)||(intIns.OpCode==ILOpCode.Ldloc_s)
						||(intIns.OpCode==ILOpCode.Starg_s)||(intIns.OpCode==ILOpCode.Blt_s)||(intIns.OpCode==ILOpCode.Ldarga_s))
					{
						instructionEncoder.OpCode(intIns.OpCode);
						instructionEncoder.CodeBuilder.WriteSByte((sbyte)intIns.Data);
					}
					else
						throw new NotSupportedException();
				}
				else if (instruction is Instruction<double> doubleIns)
				{
					if (doubleIns.OpCode==ILOpCode.Ldc_r8)
						instructionEncoder.LoadConstantR8(doubleIns.Data);
					else
						throw new NotSupportedException();
				}
				else if (instruction is Instruction<string> stringIns)
					instructionEncoder.Token(MetadataTokens.GetToken(_tablesAndHeaps.GetOrAddUserString(stringIns.Data)));
				else
					throw new NotImplementedException();
			}

			foreach (KeyValuePair<Label, ILabelHandle> labelPair in definedLabels)
			{
				ILabelHandle label = labelPair.Value;
				foreach ((int referenceOffset, int operandSize, Blob placeToWrite) in label.ReferenceOffsets)
				{
					int offsetToWrite = label.Offset-referenceOffset-operandSize;
					if (operandSize==1)
						new BlobWriter(placeToWrite).WriteSByte((sbyte)offsetToWrite);
					else if (operandSize==4)
						new BlobWriter(placeToWrite).WriteInt32(offsetToWrite);
					else
						throw new NotSupportedException();
				}
			}

			/*gave up on code validation
			//if (curStack!=(returnsVoid ? 0 : 1)) can't calculate stack correctly (can't follow jumps/branches) so can't compare precisely to expected value
			if ((curStack<(returnsVoid ? 0 : 1))&&(!gen.Instructions.Any(x => x.OpCode==ILOpCode.Throw)))
			{
#if DEBUG
				(ILOpCode code, object data, int behavior, int sum)[] GetInstructionBehavior()
				{
					int sum2 = 0;
					return gen.Instructions.Where(i => i is Instruction).Select(instruction =>
					{
						ILOpCode code = instruction.OpCode;
						int result = code==ILOpCode.Ret||code==ILOpCode.Switch ? 0 : code.NetStackBehavior();

						if (instruction is Instruction<ConstructorInfo> ciIns)
							result+=-ciIns.Data.GetParameters().Length-1;
						else if (instruction is Instruction<ConstructorBuilder> cbIns)
							result+=-cbIns.Data.GetParameters().Length-1;
						else if ((instruction is Instruction<MethodInfo> miIns)&&(code!=ILOpCode.Ldftn))
							result+=-miIns.Data.GetParameters().Length+(miIns.Data.ReturnType.Equals(typeof(void)) ? 0 : 1)-(miIns.Data.IsStatic ? 0 : 1);
						else if ((instruction is Instruction<MethodBuilder> mbIns)&&(code!=ILOpCode.Ldftn))
							result+=-mbIns.Data.GetParameters().Length+((mbIns.Data.ReturnType.Equals(typeof(void))) ? 0 : 1)-(mbIns.Data.IsStatic ? 0 : 1);

						sum2+=result;

						//return (code, instruction.GetType().BaseType.Equals(typeof(Instruction<>)) ? instruction.GetType().GetField(nameof(Instruction<int>.Data), BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instruction) : null, result, sum2);
						return (code, instruction.GetType().GetProperty(nameof(Instruction<int>.Data),BindingFlags.Public|BindingFlags.Instance)?.GetValue(instruction), result, sum2);
					}).ToArray();
				}

				(ILOpCode code, object data, int behavior, int sum)[] array = GetInstructionBehavior();
				int sum = array.Sum(x=>x.behavior);
#endif
				throw new InvalidOperationException();
			}*/
			return maxStack;
		}
		#endregion methodbody

//		#region AddRuntimeAttribute
//		void AddRuntimeAttributeOld(AssemblyDefinitionHandle asmDefHandle)
//		{
//			AssemblyReferenceHandle attrAsmRef = _frameworkAssemblyReferenceHandle;
//			TypeReferenceHandle attrTypeRef = _tablesAndHeaps.AddTypeReference(attrAsmRef, _tablesAndHeaps.GetOrAddString("System.Runtime.Versioning"), _tablesAndHeaps.GetOrAddString("TargetFrameworkAttribute"));

//			//BlobBuilder ctorSignature = new BlobBuilder();
//			//new BlobEncoder(ctorSignature).MethodSignature().Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); });
//			BlobBuilder ctorSignature = BuildMethodSignature(false,typeof(void),new Type[] { typeof(string) },0);
//			MemberReferenceHandle attrCtorRef = _tablesAndHeaps.AddMemberReference(attrTypeRef, _tablesAndHeaps.GetOrAddString(".ctor"), _tablesAndHeaps.GetOrAddBlob(ctorSignature));


//			//System.Runtime, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
//			//[assembly: TargetFramework(".NETCoreApp,Version=v2.0", FrameworkDisplayName = "")]
//			/*
//			 .custom instance void [System.Runtime]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = (
//					01 00 18 2e 4e 45 54 43 6f 72 65 41 70 70 2c 56
//					65 72 73 69 6f 6e 3d 76 32 2e 30 01 00 54 0e 14
//					46 72 61 6d 65 77 6f 72 6b 44 69 73 70 6c 61 79
//					4e 61 6d 65 00
//				)*/

//			BlobBuilder ctorParms = new BlobBuilder();
//			ctorParms.WriteBytes(new byte[] {
//						0x01,0x00,0x18,0x2e,0x4e,0x45,0x54,0x43,0x6f,0x72,0x65,0x41,0x70,0x70,0x2c,0x56,
//						0x65,0x72,0x73,0x69,0x6f,0x6e,0x3d,0x76,0x32,0x2e,0x30,0x01,0x00,0x54,0x0e,0x14,
//						0x46,0x72,0x61,0x6d,0x65,0x77,0x6f,0x72,0x6b,0x44,0x69,0x73,0x70,0x6c,0x61,0x79,
//						0x4e,0x61,0x6d,0x65,0x00
//				});

//			_tablesAndHeaps.AddCustomAttribute(asmDefHandle, attrCtorRef, _tablesAndHeaps.GetOrAddBlob(ctorParms));

//			//.custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = (01 00 07 01 00 00 00 00)
//			attrTypeRef=_tablesAndHeaps.AddTypeReference(attrAsmRef, _tablesAndHeaps.GetOrAddString("System.Diagnostics"), _tablesAndHeaps.GetOrAddString("DebuggableAttribute"));
//			//ctorSignature=new BlobBuilder();
//			//new BlobEncoder(ctorSignature).MethodSignature().Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().Int32(); });
//			ctorSignature=BuildMethodSignature(false,typeof(void),new Type[] { typeof(int) },0);
//			attrCtorRef = _tablesAndHeaps.AddMemberReference(attrTypeRef, _tablesAndHeaps.GetOrAddString(".ctor"), _tablesAndHeaps.GetOrAddBlob(ctorSignature));
//			ctorParms=new BlobBuilder();
//			ctorParms.WriteBytes(new byte[] { 0x01, 0x00, 0x07, 0x01, 0x00, 0x00, 0x00, 0x00 });
//			_tablesAndHeaps.AddCustomAttribute(asmDefHandle, attrCtorRef, _tablesAndHeaps.GetOrAddBlob(ctorParms));
//		}

//		readonly static Type[] _parmsInt = new Type[] { typeof(int) };
//		readonly static Type[] _parmsString = new Type[] { typeof(string) };

//		void AddRuntimeAttributesOld2(AssemblyDefinitionHandle asmDefHandle)
//		{
//			/*
//[assembly: AssemblyVersion("1.0.0.0")]
//[assembly: Debuggable]
//[assembly: AssemblyCompany("AsmSaveAsTest")]
//[assembly: AssemblyConfiguration("Debug")]
//[assembly: AssemblyDescription("Package Description")]
//[assembly: AssemblyFileVersion("1.0.0.0")]
//[assembly: AssemblyInformationalVersion("1.0.0")]
//[assembly: AssemblyProduct("AsmSaveAsTest")]
//[assembly: AssemblyTitle("AsmSaveAsTest")]
//[assembly: CompilationRelaxations(8)]
//[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
//[assembly: TargetFramework(".NETCoreApp,Version=v2.0", FrameworkDisplayName = "")]
//			*/
//			/*
//.assembly AsmSaveAsTest
//{
//	.custom instance void [System.Runtime]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = (
//		01 00 00 00
//	)
//	.custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = (
//		01 00 08 00 00 00 00 00
//	)
//	.custom instance void [System.Runtime]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = (
//		01 00 01 00 54 02 16 57 72 61 70 4e 6f 6e 45 78
//		63 65 70 74 69 6f 6e 54 68 72 6f 77 73 01
//	)
//	.custom instance void [System.Runtime]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggableAttribute/DebuggingModes) = (
//		01 00 07 01 00 00 00 00
//	)
//	.custom instance void [System.Runtime]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = (
//		01 00 18 2e 4e 45 54 43 6f 72 65 41 70 70 2c 56
//		65 72 73 69 6f 6e 3d 76 32 2e 30 01 00 54 0e 14
//		46 72 61 6d 65 77 6f 72 6b 44 69 73 70 6c 61 79
//		4e 61 6d 65 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = (
//		01 00 0d 41 73 6d 53 61 76 65 41 73 54 65 73 74
//		00 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = (
//		01 00 05 44 65 62 75 67 00 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = (
//		01 00 13 50 61 63 6b 61 67 65 20 44 65 73 63 72
//		69 70 74 69 6f 6e 00 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = (
//		01 00 07 31 2e 30 2e 30 2e 30 00 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyInformationalVersionAttribute::.ctor(string) = (
//		01 00 05 31 2e 30 2e 30 00 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyProductAttribute::.ctor(string) = (
//		01 00 0d 41 73 6d 53 61 76 65 41 73 54 65 73 74
//		00 00
//	)
//	.custom instance void [System.Runtime]System.Reflection.AssemblyTitleAttribute::.ctor(string) = (
//		01 00 0d 41 73 6d 53 61 76 65 41 73 54 65 73 74
//		00 00
//	)
//}
//*/

//			AddRuntimeAttribute(asmDefHandle,"System.Runtime.CompilerServices","ExtensionAttribute",Type.EmptyTypes,new byte[] { 0x01,0x00,0x00,0x00 });

//			AddRuntimeAttribute(asmDefHandle,"System.Runtime.CompilerServices","CompilationRelaxationsAttribute",_parmsInt,new byte[] { 0x01,0x00,0x08,0x00,0x00,0x00,0x00,0x00 });

//			AddRuntimeAttribute(asmDefHandle,"System.Runtime.CompilerServices","RuntimeCompatibilityAttribute",Type.EmptyTypes,
//				new byte[] {
//						0x01,0x00,0x01,0x00,0x54,0x02,0x16,0x57,0x72,0x61,0x70,0x4e,0x6f,0x6e,0x45,0x78,
//						0x63,0x65,0x70,0x74,0x69,0x6f,0x6e,0x54,0x68,0x72,0x6f,0x77,0x73,0x01
//				});

//			AddRuntimeAttribute(asmDefHandle,"System.Reflection","AssemblyConfigurationAttribute",_parmsString,
//				new byte[] { 0x01, 0x00, 0x05, 0x44, 0x65, 0x62, 0x75, 0x67, 0x00, 0x00 });

//			AddRuntimeAttribute(asmDefHandle,"System.Diagnostics","DebuggableAttribute",_parmsInt,
//				new byte[] { 0x01, 0x00, 0x07, 0x01, 0x00, 0x00, 0x00, 0x00 });

//			AddRuntimeAttribute(asmDefHandle,"System.Runtime.Versioning","TargetFrameworkAttribute",_parmsString,
//				new byte[] {
//						0x01,0x00,0x18,0x2e,0x4e,0x45,0x54,0x43,0x6f,0x72,0x65,0x41,0x70,0x70,0x2c,0x56,
//						0x65,0x72,0x73,0x69,0x6f,0x6e,0x3d,0x76,0x32,0x2e,0x30,0x01,0x00,0x54,0x0e,0x14,
//						0x46,0x72,0x61,0x6d,0x65,0x77,0x6f,0x72,0x6b,0x44,0x69,0x73,0x70,0x6c,0x61,0x79,
//						0x4e,0x61,0x6d,0x65,0x00
//				});

//			/*AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyCompanyAttribute",
//				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
//				new byte[] {
//					0x01,0x00,0x0d,0x41,0x73,0x6d,0x53,0x61,0x76,0x65,0x41,0x73,0x54,0x65,0x73,0x74,
//					0x00,0x00
//				});

//			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyDescriptionAttribute",
//				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
//				new byte[] {
//					0x01,0x00,0x13,0x50,0x61,0x63,0x6b,0x61,0x67,0x65,0x20,0x44,0x65,0x73,0x63,0x72,
//					0x69,0x70,0x74,0x69,0x6f,0x6e,0x00,0x00
//				});

//			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyFileVersionAttribute",
//				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
//				new byte[] { 0x01,0x00,0x07,0x31,0x2e,0x30,0x2e,0x30,0x2e,0x30,0x00,0x00 });

//			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyInformationalVersionAttribute",
//				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
//				new byte[] { 0x01,0x00,0x05,0x31,0x2e,0x30,0x2e,0x30,0x00,0x00 });

//			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyProductAttribute",
//				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
//				new byte[] {
//					0x01,0x00,0x0d,0x41,0x73,0x6d,0x53,0x61,0x76,0x65,0x41,0x73,0x54,0x65,0x73,0x74,
//					0x00,0x00
//				});

//			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyTitleAttribute",
//				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
//				new byte[] {
//					0x01,0x00,0x0d,0x41,0x73,0x6d,0x53,0x61,0x76,0x65,0x41,0x73,0x54,0x65,0x73,0x74,
//					0x00,0x00
//				});*/
//		}

//		void AddRuntimeAttribute(AssemblyDefinitionHandle asmDefHandle,string @namespace,string typename,Type[] parms,byte[] data)
//		{
//			TypeReferenceHandle attrTypeRef = _tablesAndHeaps.AddTypeReference(_frameworkAssemblyReferenceHandle, _tablesAndHeaps.GetOrAddString(@namespace), _tablesAndHeaps.GetOrAddString(typename));
//			BlobBuilder ctorSignature=BuildMethodSignature(true,typeof(void),parms,0);
//			MemberReferenceHandle attrCtorRef = _tablesAndHeaps.AddMemberReference(attrTypeRef, _tablesAndHeaps.GetOrAddString(".ctor"), _tablesAndHeaps.GetOrAddBlob(ctorSignature));
//			_tablesAndHeaps.AddCustomAttribute(asmDefHandle, attrCtorRef, _tablesAndHeaps.GetOrAddBlob(data));
//		}
//		#endregion AddRuntimeAttribute

		#region helpers
		#region get assembly/type/member reference
		/*readonly CacheObject<Assembly, AssemblyReferenceHandle> _asmReferenceCache = new CacheObject<Assembly, AssemblyReferenceHandle>();
		AssemblyReferenceHandle GetAssemblyReference(Assembly asm)
			=> _asmReferenceCache.GetOrAdd(asm, () =>
			{
				AssemblyName asmName = asm.GetName();

				/*if (asmName.Name=="Jellequin.Runtime")
					throw new InvalidOperationException("Internal error - it seems source haven't been copied correctly.");

				if (!((asmName.Name=="netstandard")||(asmName.Name=="System.Reflection.Emit.ILGeneration")||(asmName.Name=="System.Reflection.Emit.Lightweight")))
					throw new InvalidOperationException("Internal error - it seems source haven't been copied correctly.");*/

				/*string culture = asmName.CultureName;
				byte[] publicKey = asmName.GetPublicKey();
				return _tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString(asmName.Name), asmName.Version, culture==null||culture.Length==0 ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(culture), publicKey==null||publicKey.Length==0 ? default(BlobHandle) : _tablesAndHeaps.GetOrAddBlob(publicKey), (AssemblyFlags)asmName.Flags, _tablesAndHeaps.GetOrAddBlob(BitConverter.GetBytes((uint)asmName.HashAlgorithm)));
			});*/

		AssemblyReferenceHandle GetAssemblyReference(Assembly asm)
			=> GetAssemblyReference(asm.GetName());

		readonly CacheObject<string,AssemblyReferenceHandle> _asmNameReferenceCache = new CacheObject<string,AssemblyReferenceHandle>();
		AssemblyReferenceHandle GetAssemblyReference(AssemblyName asmName)
			=> _asmNameReferenceCache.GetOrAdd(asmName.ToString(), () =>
			{
				/*if (asmName.Name=="Jellequin.Runtime")
					throw new InvalidOperationException("Internal error - it seems source haven't been copied correctly.");

				if (!((asmName.Name=="netstandard")||(asmName.Name=="System.Reflection.Emit.ILGeneration")||(asmName.Name=="System.Reflection.Emit.Lightweight")))
					throw new InvalidOperationException("Internal error - it seems source haven't been copied correctly.");*/

				string culture = asmName.CultureName;
				byte[] publicKey = asmName.GetPublicKey();
				/*if (asmName.Flags.HasFlag(AssemblyNameFlags.PublicKey))
					return _tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString(asmName.Name),asmName.Version,culture==null||culture.Length==0?default(StringHandle):_tablesAndHeaps.GetOrAddString(culture),publicKey==null||publicKey.Length==0?default(BlobHandle):_tablesAndHeaps.GetOrAddBlob(publicKey),(AssemblyFlags)asmName.Flags,default(BlobHandle));
				else*/
					return _tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString(asmName.Name),asmName.Version,culture==null||culture.Length==0?default(StringHandle):_tablesAndHeaps.GetOrAddString(culture),publicKey==null||publicKey.Length==0?default(BlobHandle):_tablesAndHeaps.GetOrAddBlob(asmName.GetPublicKeyToken()),(AssemblyFlags)0,default(BlobHandle));
			});


		CacheObject<Type,TypeSpecificationHandle> _typeSpecCache = new CacheObject<Type,TypeSpecificationHandle>();
		CacheObject<int,TypeSpecificationHandle> _typeGenParmCache = new CacheObject<int,TypeSpecificationHandle>();
		CacheObject<(EntityHandle,StringHandle,StringHandle),TypeReferenceHandle> _typeRefCache = new CacheObject<(EntityHandle,StringHandle,StringHandle),TypeReferenceHandle>();
		TypeReferenceHandle GetOrAddTypeReference(EntityHandle resolutionScope,StringHandle @namespace,StringHandle name)
			=> _typeRefCache.GetOrAdd((resolutionScope, @namespace, name),() => _tablesAndHeaps.AddTypeReference(resolutionScope,@namespace,name));

		EntityHandle GetTypeHandle(Type type) => GetTypeHandle(type,false);
		EntityHandle GetTypeHandle(Type type,bool needDefinition)
		{
			if (type==null)
				return default;

			if (type.IsConstructedGenericType)
				return _typeSpecCache.GetOrAdd(type,()=>_tablesAndHeaps.AddTypeSpecification(_tablesAndHeaps.GetOrAddBlob(BuildSignature(be => BuildSignature(be.TypeSpecificationSignature(),type)))));
			if (type is TypeBuilder tb)
				return needDefinition ? _definedTypes[tb].TypeHandle : _definedTypes[tb].Handle;
			if (type is GenericParameterBuilder gpb)
				return _typeGenParmCache.GetOrAdd(type.GenericParameterPosition,() => _tablesAndHeaps.AddTypeSpecification(_tablesAndHeaps.GetOrAddBlob(BuildSignature(be => BuildSignature(be.TypeSpecificationSignature(),gpb)))));
			if (type.IsNested)
				return GetOrAddTypeReference(GetTypeHandle(type.DeclaringType),default,_tablesAndHeaps.GetOrAddString(type.Name));
			return GetOrAddTypeReference(GetAssemblyReference(type.Assembly),_tablesAndHeaps.GetOrAddString(type.Namespace??""),_tablesAndHeaps.GetOrAddString(type.Name));
		}

		CacheObject<(EntityHandle,StringHandle,BlobHandle),MemberReferenceHandle> _getMemberRefHandleCache = new CacheObject<(EntityHandle,StringHandle,BlobHandle),MemberReferenceHandle>();
		MemberReferenceHandle GetOrAddMemberReferenceHandle(Type type,string name,BlobBuilder signature)
			=> GetOrAddMemberReferenceHandle(GetTypeHandle(type),name,signature);

		MemberReferenceHandle GetOrAddMemberReferenceHandle(EntityHandle parent,string name,BlobBuilder signature)
		{
			StringHandle sh = _tablesAndHeaps.GetOrAddString(name);
			BlobHandle bh = _tablesAndHeaps.GetOrAddBlob(signature);
			return _getMemberRefHandleCache.GetOrAdd((parent, sh, bh),() => _tablesAndHeaps.AddMemberReference(parent,sh,bh));
		}

		EntityHandle GetMemberHandle(MemberInfo member)
		{
			if (member==null)
				throw new ArgumentNullException(nameof(member));

			if (member is IMemberBuilderBase mbNew)
				return GetMemberBuilderHandle(mbNew);

			switch (member.MemberType)
			{
				case MemberTypes.Field:
					FieldInfo fi = (FieldInfo)member;
					return GetOrAddMemberReferenceHandle(fi.DeclaringType,fi.Name,BuildFieldSignature(fi.FieldType));
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					MethodBase mb = (MethodBase)member;
					MethodInfo mii = mb as MethodInfo;
					ParameterInfo[] pis;
					Type declType = mb.DeclaringType;
					Type returnType;

					if (declType.IsConstructedGenericType)
					{
						Type genTypeDef = declType.GetGenericTypeDefinition();
						MethodBase genericDef = mii==null
							? (MethodBase)genTypeDef.GetConstructors().First(x => x.GetParameters().Length==mb.GetParameters().Length)
							: genTypeDef.GetMethods().First(x => x.Name==mb.Name&&x.GetParameters().Length==mb.GetParameters().Length);
						pis=genericDef.GetParameters();
						returnType=mii==null ? typeof(void) : ((MethodInfo)genericDef).ReturnType;
					}
					else
					{
						pis=mb.GetParameters();
						returnType=mii==null ? typeof(void) : mii.ReturnType;
					}

					return GetOrAddMemberReferenceHandle(declType,mb.Name,BuildMethodSignature(!mb.IsStatic,returnType,pis.Select(x => x.ParameterType).ToArray(),mii==null ? null : mb.GetGenericArguments()));
				default:
					throw new NotSupportedException();
			}
		}

		CacheObject<IMemberBuilderBase,EntityHandle> _getMemberHandleBuilderCache = new CacheObject<IMemberBuilderBase,EntityHandle>();
		EntityHandle GetMemberBuilderHandle(IMemberBuilderBase member)
		{
			if (member==null)
				throw new ArgumentNullException(nameof(member));

			EntityHandle AddMethodReference(IMethodBuilderBase mbb,Type returnType,MethodBuilder genericMethodDefinition,Type[] genericMethodParameters)
			{
				Type declType = mbb.DeclaringType;
				Type[] parameterTypes = mbb.GetParameterBuilders().Select(x=>x.ParameterType).ToArray();
				bool isGenNondefType = (declType.IsGenericType)&&(!declType.IsGenericTypeDefinition);
				/*
				if (genericMethodDefinition==null)
					if (isGenNondefType)
						return GetOrAddMemberReferenceHandle(declType,mbb.Name,BuildMethodSignature(!mbb.IsStatic,returnType,parameterTypes,genericMethodParameters));
					else
						return _definedMethods[mbb];
				else
					if (isGenNondefType)
						return GetOrAddMemberReferenceHandle(declType,mbb.Name,BuildMethodSignature(!mbb.IsStatic,returnType,parameterTypes,genericMethodParameters));
					else
						return _tablesAndHeaps.AddMethodSpecification(_definedMethods[genericMethodDefinition],_tablesAndHeaps.GetOrAddBlob(BuildMethodSpecSignature(genericMethodParameters)));
				*/
				return genericMethodDefinition==null
					? (isGenNondefType
						? GetOrAddMemberReferenceHandle(declType,mbb.Name,BuildMethodSignature(!mbb.IsStatic,returnType,parameterTypes,genericMethodParameters))
						: _definedMethods[mbb].Handle)
					: isGenNondefType
						? (EntityHandle)GetOrAddMemberReferenceHandle(declType,mbb.Name,BuildMethodSignature(!mbb.IsStatic,returnType,parameterTypes,genericMethodParameters))
						: _tablesAndHeaps.AddMethodSpecification(_definedMethods[genericMethodDefinition].Handle,_tablesAndHeaps.GetOrAddBlob(BuildMethodSpecSignature(genericMethodParameters)));
			}

			return _getMemberHandleBuilderCache.GetOrAdd(member,() =>
			{
				switch (member)
				{
					case FieldBuilder fb:
						Type declType = fb.DeclaringType;
						return declType.IsGenericType
							? (EntityHandle)GetOrAddMemberReferenceHandle(declType,fb.Name,BuildFieldSignature(fb.FieldType))
							: _definedFields[fb];
					//case MethodBuilderBase mbb:
					case ConstructorBuilder cb:
						return AddMethodReference(cb,typeof(void),null,Type.EmptyTypes);
					case MethodBuilder mb:
						return AddMethodReference(mb,mb.ReturnType,mb.GetGenericMethodBuilderDefinition(),mb.GetGenericArguments());
					default:
						throw new InvalidOperationException();
				}
			});
		}
		#endregion get assembly/type/member reference

		#region CacheObject
		class CacheObject<TKey, TValue>
		{
			Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

			internal TValue GetOrAdd(TKey key, Func<TValue> addFunction)
			{
				if (_cache.TryGetValue(key, out TValue result))
					return result;
				_cache[key]=(result=addFunction());
				return result;
			}
		}
		#endregion CacheObject

		#region BuildSignature
		void BuildSignature(SignatureTypeEncoder ste, Action<SignatureTypeEncoder> action)
			=> action(ste);

		void BuildSignature(SignatureTypeEncoder ste, Type type)
		{
			if (type.IsByRef)
				BuildSignature(ste.Pointer(),type.GetElementType());
			else if (type.Equals(typeof(int)))
				ste.Int32();
			else if (type.Equals(typeof(bool)))
				ste.Boolean();
			else if (type.Equals(typeof(string)))
				ste.String();
			else if (type.IsArray)
				BuildSignature(ste.SZArray(),type.GetElementType());
			else if (type.Equals(typeof(object)))
				ste.Object();
			else if (type.Equals(typeof(IntPtr)))
				ste.IntPtr();
			else if (type.Equals(typeof(byte)))
				ste.Byte();
			else if (type.Equals(typeof(char)))
				ste.Char();
			else if (type.Equals(typeof(double)))
				ste.Double();
			else if (type.Equals(typeof(short)))
				ste.Int16();
			else if (type.Equals(typeof(long)))
				ste.Int64();
			else if (type.Equals(typeof(sbyte)))
				ste.SByte();
			else if (type.Equals(typeof(Single)))
				ste.Single();
			else if (type.Equals(typeof(ushort)))
				ste.UInt16();
			else if (type.Equals(typeof(uint)))
				ste.UInt32();
			else if (type.Equals(typeof(ulong)))
				ste.UInt64();
			else if (type.Equals(typeof(UIntPtr)))
				ste.UIntPtr();
			/*
				MethodSignatureEncoder FunctionPointer();
				void PrimitiveType(PrimitiveTypeCode type);
				void VoidPointer();
			*/
			else if (type.IsGenericParameter)
				if (type.DeclaringType==null)
					ste.GenericMethodTypeParameter(type.GenericParameterPosition);
				else
					ste.GenericTypeParameter(type.GenericParameterPosition);
			else if (type.IsGenericType)
			{
				Type[] genericArguments = type.GetGenericArguments();
				GenericTypeArgumentsEncoder gtae = ste.GenericInstantiation(GetTypeHandle(type.IsGenericTypeDefinition?type:type.GetGenericTypeDefinition(),true),genericArguments.Length,type.IsValueType);
				foreach (Type ga in genericArguments)
					BuildSignature(gtae.AddArgument(),ga);
			}
			else
				ste.Type(GetTypeHandle(type),type.IsValueType);
		}

		void BuildSignature(NamedArgumentTypeEncoder nate, Type type)
		{
			CustomAttributeElementTypeEncoder? eteN = null;

			if (type.Equals(typeof(object)))
				nate.Object();
			else if (type.IsArray)
			{
				CustomAttributeArrayTypeEncoder ate=nate.SZArray();
				Type et = type.GetElementType();
				if (et.Equals(typeof(object)))
				{
					ate.ObjectArray();
					return;
				}
				else
					eteN=ate.ElementType();
			}
			else
				eteN = nate.ScalarType();

			if (eteN.HasValue)
			{
				CustomAttributeElementTypeEncoder ete = eteN.Value;

				if (type.Equals(typeof(int)))
					ete.Int32();
				else if (type.Equals(typeof(bool)))
					ete.Boolean();
				else if (type.Equals(typeof(string)))
					ete.String();
				else if (type.Equals(typeof(byte)))
					ete.Byte();
				else if (type.Equals(typeof(char)))
					ete.Char();
				else if (type.Equals(typeof(double)))
					ete.Double();
				/*else if (type.IsEnum)
					ete.Enum(string enumTypeName);*/
				else if (type.Equals(typeof(short)))
					ete.Int16();
				else if (type.Equals(typeof(long)))
					ete.Int64();
				else if (type.Equals(typeof(sbyte)))
					ete.SByte();
				else if (type.Equals(typeof(Single)))
					ete.Single();
				//ete.SystemType();
				else if (type.Equals(typeof(ushort)))
					ete.UInt16();
				else if (type.Equals(typeof(uint)))
					ete.UInt32();
				else if (type.Equals(typeof(ulong)))
					ete.UInt64();
				else
					throw new NotImplementedException();
			}
		}

		BlobBuilder BuildSignature(Action<BlobEncoder> action)
		{
			BlobBuilder builder = new BlobBuilder();
			action(new BlobEncoder(builder));
			return builder;
		}

		BlobBuilder BuildFieldSignature(Type type)
			=> BuildSignature(be => BuildSignature(be.FieldSignature(), type));

		void BuildReturnSignature(ReturnTypeEncoder rte, Type type)
		{
			if (type.Equals(typeof(void)))
				rte.Void();
			else
				BuildSignature(rte.Type(), type);
		}

		void BuildParameterSignature(ParameterTypeEncoder pte, Type type)
			=> BuildSignature(pte.Type(), type);

		void BuildLocalSignature(LocalVariableTypeEncoder lvte, Type type)
			=> BuildSignature(lvte.Type(), type);

		BlobBuilder BuildMethodSignature(bool isInstanceMethod,Type returnType,Type[] parameterTypes,Type[] genericArguments)
			=> BuildMethodSignature(isInstanceMethod,returnType,parameterTypes,genericArguments==null ? 0 : genericArguments.Length);

		BlobBuilder BuildMethodSignature(bool isInstanceMethod,Type returnType,Type[] parameterTypes,int genericArgumentCount)
			=> BuildSignature(be =>
				be.MethodSignature(GetSignatureCallingConventions(isInstanceMethod),genericArgumentCount,isInstanceMethod).
					Parameters(parameterTypes.Length,
						returnTypeL => BuildReturnSignature(returnTypeL,returnType),
						parameters => parameterTypes.Process(pt => BuildParameterSignature(parameters.AddParameter(),pt)))
			);

		BlobBuilder BuildMethodSpecSignature(Type[] genericArguments)
			=> BuildSignature(be =>
			{
				GenericTypeArgumentsEncoder gtae = be.MethodSpecificationSignature(genericArguments.Length);
				foreach (Type arg in genericArguments)
					BuildSignature(gtae.AddArgument(),arg);
			});

		#region BuildCustomAttributeSignature
		BlobBuilder BuildCustomAttributeSignature(CustomAttributeData ca)
		{
			//Inspired by Roslyn's Microsoft.Cci.MetadataWriter.SerializeCustomAttributeSignature, Microsoft.CodeAnalysis

			BlobBuilder builder = new BlobBuilder();
			new BlobEncoder(builder).CustomAttributeSignature(out FixedArgumentsEncoder fixedArgsEncoder,out CustomAttributeNamedArgumentsEncoder namedArgsEncoder);
			foreach (CustomAttributeTypedArgument cata in ca.ConstructorArguments)
				SerializeCustomAttributeTypedArguments(fixedArgsEncoder.AddArgument(),cata);
			NamedArgumentsEncoder nae = namedArgsEncoder.Count(ca.NamedArguments.Count);
			foreach (CustomAttributeNamedArgument cana in ca.NamedArguments)
				SerializeCustomAttributeNamedArguments(nae,cana);
			return builder;
		}

		static void SerializeCustomAttributeTypedArguments(LiteralEncoder encoder,CustomAttributeTypedArgument argument)
		{
			if (argument.ArgumentType.IsArray)
			{
				System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument> coll = (System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)argument.Value;
				LiteralsEncoder subEnencoder = encoder.Vector().Count(coll.Count);
				foreach (CustomAttributeTypedArgument item in coll)
					SerializeCustomAttributeTypedArguments(subEnencoder.AddLiteral(),item);
			}
			else
				encoder.Scalar().Constant(argument.Value);
		}

		void SerializeCustomAttributeNamedArguments(NamedArgumentsEncoder encoder,CustomAttributeNamedArgument argument)
			=> encoder.AddArgument(argument.IsField,nate => BuildSignature(nate,argument.TypedValue==null ? typeof(string) : argument.TypedValue.ArgumentType),ne => ne.Name(argument.MemberName),le => le.Scalar().Constant(argument.TypedValue.Value));
		#endregion BuildCustomAttributeSignature

		SignatureCallingConvention GetSignatureCallingConventions(bool isInstanceMethod)
		{
			/*SignatureCallingConvention result;

			CallingConventions src;
			if (src.HasFlag(CallingConventions.HasThis))
				result=|SignatureCallingConvention.ThisCall;
			if (src.HasFlag(CallingConventions.))*/

			//return (isInstanceMethod ? SignatureCallingConvention.ThisCall : SignatureCallingConvention.Default);
			return SignatureCallingConvention.Default;

			//mb.Attributes

			/*mb.CallingConvention;
			SignatureHeader;
			CallingConventions;
			SignatureCallingConvention;
			//this=new SignatureHeader((byte)((int)kind|(int)convention|(int)attributes));
			

			mb.CallingConvention;

			MdSigCallingConvention mdSigCallingConvention = MdSigCallingConvention.Default;
			if ((callingConvention&CallingConventions.VarArgs)==CallingConventions.VarArgs)
			{
				mdSigCallingConvention=MdSigCallingConvention.Vararg;
			}
			if (cGenericParam>0)
			{
				mdSigCallingConvention|=MdSigCallingConvention.Generic;
			}
			if ((callingConvention&CallingConventions.HasThis)==CallingConventions.HasThis)
			{
				mdSigCallingConvention|=MdSigCallingConvention.HasThis;
			}*/

			/*src:
			//
			// Summary:
			//     Specifies the default calling convention as determined by the common language
			//     runtime. Use this calling convention for static methods. For instance or virtual
			//     methods use HasThis.
			Standard=1,
		//
		// Summary:
		//     Specifies the calling convention for methods with variable arguments.
		VarArgs=2,
		//
		// Summary:
		//     Specifies that either the Standard or the VarArgs calling convention may be used.
		Any=3,
		//
		// Summary:
		//     Specifies an instance or virtual method (not a static method). At run-time, the
		//     called method is passed a pointer to the target object as its first argument
		//     (the this pointer). The signature stored in metadata does not include the type
		//     of this first argument, because the method is known and its owner class can be
		//     discovered from metadata.
		HasThis=32,
		//
		// Summary:
		//     Specifies that the signature is a function-pointer signature, representing a
		//     call to an instance or virtual method (not a static method). If ExplicitThis
		//     is set, HasThis must also be set. The first argument passed to the called method
		//     is still a this pointer, but the type of the first argument is now unknown. Therefore,
		//     a token that describes the type (or class) of the this pointer is explicitly
		//     stored into its metadata signature.
		ExplicitThis=64


				dest:
			//
			// Summary:
			//     A managed calling convention with a fixed-length argument list.
			Default=0,
		//
		// Summary:
		//     An unmanaged C/C++ style calling convention where the call stack is cleaned by
		//     the caller.
		CDecl=1,
		//
		// Summary:
		//     An unmanaged calling convention where the call stack is cleaned up by the callee.
		StdCall=2,
		//
		// Summary:
		//     An unmanaged C++ style calling convention for calling instance member functions
		//     with a fixed argument list.
		ThisCall=3,
		//
		// Summary:
		//     An unmanaged calling convention where arguments are passed in registers when
		//     possible.
		FastCall=4,
		//
		// Summary:
		//     A managed calling convention for passing extra arguments.
		VarArgs=5*/

		}
		#endregion BuildSignature

		#region custom attributes
		readonly static Type[] _parmsString = new Type[] { typeof(string) };
		void AddBaseAssemblyAttributes(AssemblyDefinitionHandle asmDefHandle,bool debug)
		{
			AssemblyReferenceHandle frameworkAssemblyReferenceHandle = GetAssemblyReference(new AssemblyName("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51"));

			AddRuntimeAttribute(asmDefHandle,"System.Runtime.CompilerServices","ExtensionAttribute",
				parms => parms.Parameters(0,returnType => returnType.Void(),parameters => { }),new byte[] { 0x01,0x00,0x00,0x00 },frameworkAssemblyReferenceHandle);

			AddRuntimeAttribute(asmDefHandle,"System.Runtime.CompilerServices","CompilationRelaxationsAttribute",
				parms => parms.Parameters(1,returnType => returnType.Void(),parameters => { parameters.AddParameter().Type().Int32(); }),
				new byte[] { 0x01,0x00,0x08,0x00,0x00,0x00,0x00,0x00 },frameworkAssemblyReferenceHandle);

			AddRuntimeAttribute(asmDefHandle,"System.Runtime.CompilerServices","RuntimeCompatibilityAttribute",
				parms => parms.Parameters(0,returnType => returnType.Void(),parameters => { }),
				new byte[] {
						0x01,0x00,0x01,0x00,0x54,0x02,0x16,0x57,0x72,0x61,0x70,0x4e,0x6f,0x6e,0x45,0x78,
						0x63,0x65,0x70,0x74,0x69,0x6f,0x6e,0x54,0x68,0x72,0x6f,0x77,0x73,0x01
				},frameworkAssemblyReferenceHandle);

			AddRuntimeAttribute(asmDefHandle,"System.Runtime.Versioning","TargetFrameworkAttribute",
				parms => parms.Parameters(1,returnType => returnType.Void(),parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] {
						0x01,0x00,0x18,0x2e,0x4e,0x45,0x54,0x43,0x6f,0x72,0x65,0x41,0x70,0x70,0x2c,0x56,
						0x65,0x72,0x73,0x69,0x6f,0x6e,0x3d,0x76,0x32,0x2e,0x30,0x01,0x00,0x54,0x0e,0x14,
						0x46,0x72,0x61,0x6d,0x65,0x77,0x6f,0x72,0x6b,0x44,0x69,0x73,0x70,0x6c,0x61,0x79,
						0x4e,0x61,0x6d,0x65,0x00
				},frameworkAssemblyReferenceHandle);

			if (debug)
			{
				AddRuntimeAttribute(asmDefHandle,"System.Diagnostics","DebuggableAttribute",
					parms => parms.Parameters(1,returnType => returnType.Void(),parameters => parameters.AddParameter().Type()
									.Type(GetOrAddTypeReference(
											GetOrAddTypeReference(frameworkAssemblyReferenceHandle,_tablesAndHeaps.GetOrAddString("System.Diagnostics"),_tablesAndHeaps.GetOrAddString("DebuggableAttribute"))
											,default
											,_tablesAndHeaps.GetOrAddString("DebuggingModes")),true)
									),
					new byte[] { 0x01,0x00,0x07,0x01,0x00,0x00,0x00,0x00 },frameworkAssemblyReferenceHandle);
			}
		}

		void AddRuntimeAttribute(AssemblyDefinitionHandle asmDefHandle,string @namespace,string typename,Action<MethodSignatureEncoder> parms,byte[] data,AssemblyReferenceHandle frameworkAssemblyReferenceHandle)
		{
			TypeReferenceHandle attrTypeRef = GetOrAddTypeReference(frameworkAssemblyReferenceHandle,_tablesAndHeaps.GetOrAddString(@namespace),_tablesAndHeaps.GetOrAddString(typename));
			BlobBuilder ctorSignature = new BlobBuilder();
			parms(new BlobEncoder(ctorSignature).MethodSignature(isInstanceMethod: true));
			MemberReferenceHandle attrCtorRef = GetOrAddMemberReferenceHandle(attrTypeRef, ConstructorInfo.ConstructorName,ctorSignature);
			_tablesAndHeaps.AddCustomAttribute(asmDefHandle,attrCtorRef,_tablesAndHeaps.GetOrAddBlob(data));
		}

		void AddCustomAttributes(EntityHandle parent,IEnumerable<CustomAttributeData> assemblyAttributes)
			=> assemblyAttributes.Process(ca =>
				_tablesAndHeaps.AddCustomAttribute(parent,GetMemberHandle(ca.Constructor),_tablesAndHeaps.GetOrAddBlob(BuildCustomAttributeSignature(ca)))
			);

		void AddCustomAttributes(EntityHandle parent,ICustomAttributesContainer source)
			=> AddCustomAttributes(parent,source.GetCustomAttributesData());
		#endregion custom attributes
		#endregion helpers

		#region pdb
		void WriteSeqSymbols()
		{
			foreach ((SequencePoint[] seqPoints, int localSignatureRowId) in _seqPoints)
			{
				if (seqPoints.Length==0)
				{
					_pdbTablesAndHeaps.AddMethodDebugInformation(_docPdb, _pdbTablesAndHeaps.GetOrAddBlob(CreateSequencePoints(spe => spe.WriteSequencePoints(
						new SequencePoint[] { SequencePoint.Hidden(_docPdb, 0) }
						), localSignatureRowId)));
					continue;
				}

				_pdbTablesAndHeaps.AddMethodDebugInformation(_docPdb, _pdbTablesAndHeaps.GetOrAddBlob(CreateSequencePoints(spe => spe.WriteSequencePoints(seqPoints), localSignatureRowId)));
			}
		}

		static byte[] CalculateChecksum(byte[] data, SourceHashAlgorithm algorithmId)
		{
			using (HashAlgorithm ha = GetAlgorithm(algorithmId))
				return ha.ComputeHash(data);
		}

		static HashAlgorithm GetAlgorithm(SourceHashAlgorithm algorithmId)
		{
			switch (algorithmId)
			{
				case SourceHashAlgorithm.Sha1:
					return SHA1.Create();

				case SourceHashAlgorithm.Sha256:
					return SHA256.Create();

				default:
					return null;
			}
		}

		static BlobBuilder CreateSequencePoints(Action<SequencePointsEncoder> action, int localSignatureRowId)
		{
			BlobBuilder builder = new BlobBuilder();
			action(new SequencePointsEncoder(builder, localSignatureRowId));
			return builder;
		}
		#endregion pdb

		#region helper classes
		class TypeDefineInfo
		{
			internal TypeBuilder Type;
			internal int FieldIndex;
			internal int MethodIndex;
			internal IMethodBuilderBase[] Methods;
		}

		class ILabelHandle
		{
			internal int Offset;
			internal List<(int, int, Blob)> ReferenceOffsets = new List<(int, int, Blob)>();
		}
		#endregion helper classes

		#region Win32ResourceSectionBuilder
		class Win32ResourceSectionBuilder : ResourceSectionBuilder
		{
			readonly Stream _manifestStream, _iconStream;
			internal Win32ResourceSectionBuilder(Stream manifestStream, Stream iconStream)
			{
				_manifestStream = manifestStream;
				_iconStream = iconStream;
			}

			protected override void Serialize(BlobBuilder builder, SectionLocation location)
			{
				List<Win32Resource> resources;
				using (Stream win32Resources = new MemoryStream())
				{
					AppendNullResource(win32Resources);
					if (_manifestStream!=null)
						Win32ResourceConversions.AppendManifestToResourceStream(win32Resources, _manifestStream, false);
					if (_iconStream!=null)
						Win32ResourceConversions.AppendIconToResourceStream(win32Resources, _iconStream);

					win32Resources.Position = 0;
					resources = MakeWin32ResourceList(win32Resources);
				}

				if (resources.Count>1)
					NativeResourceWriter.SerializeWin32Resources(builder, resources, location.RelativeVirtualAddress);
			}

			internal List<Win32Resource> MakeWin32ResourceList(Stream win32Resources)
			{
				if (win32Resources == null)
					return null;
				List<RESOURCE> resources;

				resources = CvtResFile.ReadResFile(win32Resources);
				if (resources == null)
					return null;

				var resourceList = new List<Win32Resource>();

				foreach (var r in resources)
				{
					var result = new Win32Resource(
						 data: r.data,
						 codePage: 0,
						 languageId: r.LanguageId,
						 //EDMAURER converting to int from ushort. 
						 //Go to short first to avoid sign extension. 
						 id: unchecked((short)r.pstringName.Ordinal),
						 name: r.pstringName.theString,
						 typeId: unchecked((short)r.pstringType.Ordinal),
						 typeName: r.pstringType.theString
					);

					resourceList.Add(result);
				}

				return resourceList;
			}

			static void AppendNullResource(Stream resourceStream)
			{
				var writer = new BinaryWriter(resourceStream);
				writer.Write((UInt32)0);
				writer.Write((UInt32)0x20);
				writer.Write((UInt16)0xFFFF);
				writer.Write((UInt16)0);
				writer.Write((UInt16)0xFFFF);
				writer.Write((UInt16)0);
				writer.Write((UInt32)0);            //DataVersion
				writer.Write((UInt16)0);            //MemoryFlags
				writer.Write((UInt16)0);            //LanguageId
				writer.Write((UInt32)0);            //Version 
				writer.Write((UInt32)0);            //Characteristics 
			}
		}
		#endregion Win32ResourceSectionBuilder
	}

	#region extensions
	static class IEnumerationExtensions
	{
		internal static void Process<T>(this IEnumerable<T> list,Action<T> action)
		{
			foreach (T item in list)
				action(item);
		}
	}

	static class DictionaryExtensions
	{
		internal static TValue AddWithReturn<TKey,TValue>(this Dictionary<TKey,TValue> dict,TKey key,TValue value)
		{
			dict.Add(key,value);
			return value;
		}
	}

	static class MethodBuilderBaseExtensions
	{
		internal static Type ReturnType(this IMethodBuilderBase mbb)
			=> mbb is MethodBuilder mb ? mb.ReturnType : typeof(void);
	}
	#endregion extensions
}

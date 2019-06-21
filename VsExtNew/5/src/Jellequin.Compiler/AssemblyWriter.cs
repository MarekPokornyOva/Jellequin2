//MethodDebugInformation table is either empty(missing) or has exactly as many rows as MethodDef table
//https://github.com/dotnet/corefx/blob/master/src/System.Reflection.Metadata/specs/PortablePdb-Metadata.md
//https://github.com/dotnet/symreader-portable

#region using
using Jellequin.Reflection.Emit;
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
using SequencePoint = System.Reflection.Metadata.Ecma335.SequencePoint;
#endregion using

namespace Jellequin.Compiler
{
	internal class AssemblyWriter
	{
		#region fields
		ModuleBuilder _moduleBuilder;
		bool _copyRuntime;

		PEHeaderBuilder _pEHeaderBuilder;
		MetadataBuilder _tablesAndHeaps;
		BlobBuilder _ilStream;
		MethodBodyStreamEncoder _methodBodies;
		AssemblyReferenceHandle _systemRuntimeAssemblyReferenceHandle;

		Dictionary<TypeBuilder, (TypeDefineInfo Info, TypeDefinitionHandle Handle)> _definedTypes = new Dictionary<TypeBuilder, (TypeDefineInfo, TypeDefinitionHandle)>();
		Dictionary<FieldBuilder, FieldDefinitionHandle> _definedFields = new Dictionary<FieldBuilder, FieldDefinitionHandle>();
		Dictionary<MethodBuilderBase, MethodDefinitionHandle> _definedMethods = new Dictionary<MethodBuilderBase, MethodDefinitionHandle>();
		List<(SequencePoint[],int)> _seqPoints = new List<(SequencePoint[],int)>();

		MetadataBuilder _pdbTablesAndHeaps;
		DocumentHandle _docPdb;
		#endregion fields

		#region main Write method
		internal void Write(ModuleBuilder module, DebugOptions debugOptions, ISource code, MethodBuilder entryPoint, Stream dll, Stream runtimeAsmToCopy, Stream icon)
		{
			_moduleBuilder = module;
			_copyRuntime=runtimeAsmToCopy!=null;

			_pEHeaderBuilder=PEHeaderBuilder.CreateExecutableHeader(); //CreateLibraryHeader()
			//_pEHeaderBuilder.Subsystem - Win/Console/...
			_tablesAndHeaps=new MetadataBuilder();
			_ilStream=new BlobBuilder();
			_methodBodies=new MethodBodyStreamEncoder(_ilStream);

			if (_copyRuntime)
				new RuntimeCopier(runtimeAsmToCopy).Copy(_tablesAndHeaps, _methodBodies);

			_systemRuntimeAssemblyReferenceHandle = GetAssemblyReference(typeof(object).Assembly); //_tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString("System.Runtime"), new Version(4, 2, 0, 0), default(StringHandle), _tablesAndHeaps.GetOrAddBlob(ImmutableArray.Create<byte>(0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x07, 0xd1, 0xfa, 0x57, 0xc4, 0xae, 0xd9, 0xf0, 0xa3, 0x2e, 0x84, 0xaa, 0x0f, 0xae, 0xfd, 0x0d, 0xe9, 0xe8, 0xfd, 0x6a, 0xec, 0x8f, 0x87, 0xfb, 0x03, 0x76, 0x6c, 0x83, 0x4c, 0x99, 0x92, 0x1e, 0xb2, 0x3b, 0xe7, 0x9a, 0xd9, 0xd5, 0xdc, 0xc1, 0xdd, 0x9a, 0xd2, 0x36, 0x13, 0x21, 0x02, 0x90, 0x0b, 0x72, 0x3c, 0xf9, 0x80, 0x95, 0x7f, 0xc4, 0xe1, 0x77, 0x10, 0x8f, 0xc6, 0x07, 0x77, 0x4f, 0x29, 0xe8, 0x32, 0x0e, 0x92, 0xea, 0x05, 0xec, 0xe4, 0xe8, 0x21, 0xc0, 0xa5, 0xef, 0xe8, 0xf1, 0x64, 0x5c, 0x4c, 0x0c, 0x93, 0xc1, 0xab, 0x99, 0x28, 0x5d, 0x62, 0x2c, 0xaa, 0x65, 0x2c, 0x1d, 0xfa, 0xd6, 0x3d, 0x74, 0x5d, 0x6f, 0x2d, 0xe5, 0xf1, 0x7e, 0x5e, 0xaf, 0x0f, 0xc4, 0x96, 0x3d, 0x26, 0x1c, 0x8a, 0x12, 0x43, 0x65, 0x18, 0x20, 0x6d, 0xc0, 0x93, 0x34, 0x4d, 0x5a, 0xd2, 0x93)), AssemblyFlags.PublicKey, _tablesAndHeaps.GetOrAddBlob(BitConverter.GetBytes((int)AssemblyHashAlgorithm.Sha1)));

			bool debug = debugOptions.Debug;
			if (debug)
			{
				//pdb
				_pdbTablesAndHeaps = new MetadataBuilder();
				BlobBuilder sourceCodeBytes = new BlobBuilder();
				sourceCodeBytes.WriteInt32(0);
				sourceCodeBytes.WriteBytes(code.GetBytes());

				_docPdb = _pdbTablesAndHeaps.AddDocument(_pdbTablesAndHeaps.GetOrAddDocumentName(code.GetEmbedFilename()),
						_pdbTablesAndHeaps.GetOrAddGuid(HashAlgorithmGuids.Sha1), _pdbTablesAndHeaps.GetOrAddBlob(CalculateChecksum(sourceCodeBytes.ToArray(sizeof(int), sourceCodeBytes.Count - sizeof(int)), SourceHashAlgorithm.Sha1)),
						_pdbTablesAndHeaps.GetOrAddGuid(new Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1")));

				if (debugOptions.EmbedSourceCode)
					_pdbTablesAndHeaps.AddCustomDebugInformation(
						parent: _docPdb,
						kind: _pdbTablesAndHeaps.GetOrAddGuid(Microsoft.CodeAnalysis.Debugging.PortableCustomDebugInfoKinds.EmbeddedSource),
						value: _pdbTablesAndHeaps.GetOrAddBlob(sourceCodeBytes));
			}

			//write
			WriteAssemblyAndModule();
			WriteTypes();
			WriteFields();
			WriteMethods();
			WriteProperties();
			if (debug)
				WriteSeqSymbols();

			//finalize
			MethodDefinitionHandle entryPointMethodHandle = entryPoint==null ? default(MethodDefinitionHandle) : _definedMethods[entryPoint];

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

			ResourceSectionBuilder nativeResources = icon==null?null:new Win32ResourceSectionBuilder(null,icon);

			BlobBuilder output = new BlobBuilder();
			new ManagedPEBuilder(_pEHeaderBuilder, new MetadataRootBuilder(_tablesAndHeaps), _ilStream, entryPoint: entryPointMethodHandle, debugDirectoryBuilder: ddBuilder, nativeResources : nativeResources).Serialize(output);

			void WriteBlobBuilder(BlobBuilder bb,Stream stream)
			{
				if (stream==null)
					return;

				foreach (Blob blob in bb.GetBlobs())
				{
					ArraySegment<byte> data = blob.GetBytes();
					stream.Write(data.Array, data.Offset, data.Count);
				}
			}

			if (debug)
				WriteBlobBuilder(outputPdb, debugOptions.Pdb);
			WriteBlobBuilder(output,dll);
		}
		#endregion main Write method

		#region write methods
		void WriteAssemblyAndModule()
		{
			AssemblyName asmName = _moduleBuilder.AssemblyName;
			StringHandle name = _tablesAndHeaps.GetOrAddString(asmName.Name);
			string culture = asmName.CultureName;
			byte[] publicKey = asmName.GetPublicKey();
			AddRuntimeAttributes(_tablesAndHeaps.AddAssembly(name, asmName.Version??new Version(), culture==null||culture.Length==0 ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(culture), publicKey==null||publicKey.Length==0 ? default(BlobHandle) : _tablesAndHeaps.GetOrAddBlob(publicKey), (AssemblyFlags)asmName.Flags, (AssemblyHashAlgorithm)asmName.HashAlgorithm));
			_tablesAndHeaps.AddModule(1, _tablesAndHeaps.GetOrAddString("<module>"), _tablesAndHeaps.GetOrAddGuid(Guid.NewGuid()), default(GuidHandle), default(GuidHandle));

			//Assembly works perfectly without following <Module> type but asm.GetType() returns empty array
			if (!_copyRuntime)
				_tablesAndHeaps.AddTypeDefinition(default(TypeAttributes), default(StringHandle), _tablesAndHeaps.GetOrAddString("<Module>"), baseType: default(EntityHandle), fieldList: MetadataTokens.FieldDefinitionHandle(1), methodList: MetadataTokens.MethodDefinitionHandle(1));
		}

		void WriteTypes()
		{
			int copiedFieldsCount = _tablesAndHeaps.GetRowCount(TableIndex.Field);
			int copiedMethodsCount = _tablesAndHeaps.GetRowCount(TableIndex.MethodDef);
			TypeDefineInfo[] typesToCreate = _moduleBuilder.Types.Select(type =>
			{
				TypeDefineInfo result = new TypeDefineInfo { Type=type, FieldIndex=copiedFieldsCount, MethodIndex=copiedMethodsCount, Methods=type.Methods.Cast<MethodBuilderBase>().Concat(type.Constructors).ToArray() };
				copiedFieldsCount+=type.Fields.Count;
				//copiedMethodsCount+=type.Methods.Count+type.Constructors.Count;

				foreach (MethodBuilderBase mb in type.Methods.Cast<MethodBuilderBase>().Concat(type.Constructors))
					_definedMethods.Add(mb, MetadataTokens.MethodDefinitionHandle(++copiedMethodsCount));

				return result;
			}).ToArray();

			_definedTypes=typesToCreate.ToDictionary(type => type.Type, typeInfo => 
			{
				TypeBuilder type = typeInfo.Type;
				TypeDefinitionHandle typeDef = _tablesAndHeaps.AddTypeDefinition(type.Attributes, _tablesAndHeaps.GetOrAddString(""), _tablesAndHeaps.GetOrAddString(type.Name), GetTypeReference(type.BaseType), MetadataTokens.FieldDefinitionHandle(typeInfo.FieldIndex+1), MetadataTokens.MethodDefinitionHandle(typeInfo.MethodIndex+1));

				foreach (IType interfaceType in type.Interfaces)
					_tablesAndHeaps.AddInterfaceImplementation(typeDef, GetTypeDefOrRef(interfaceType));

				type.CustomAttributes.Process(ca =>
					_tablesAndHeaps.AddCustomAttribute(typeDef, GetMemberReference(ca.Constructor), _tablesAndHeaps.GetOrAddBlob(BuildCustomAttributeSignature(ca)))
				);

				return (typeInfo, typeDef);
			});
		}

		void WriteFields()
		{
			foreach (KeyValuePair<TypeBuilder, (TypeDefineInfo, TypeDefinitionHandle)> item in _definedTypes)
				foreach (FieldBuilder fb in item.Key.Fields)
				{
					FieldDefinitionHandle fieldDefinitionHandle = _tablesAndHeaps.AddFieldDefinition(fb.Attributes, _tablesAndHeaps.GetOrAddString(fb.Name), _tablesAndHeaps.GetOrAddBlob(BuildFieldSignature(fb.Type)));
					_definedFields[fb]=fieldDefinitionHandle;
					if (fb.HasRawConstantValue)
						_tablesAndHeaps.AddConstant(fieldDefinitionHandle, fb.GetRawConstantValue());
				}
		}

		void WriteProperties()
		{
			PropertyDefinitionHandle propDefHandle;
			bool isFirst;
			foreach (KeyValuePair<TypeBuilder, (TypeDefineInfo Info, TypeDefinitionHandle Handle)> item in _definedTypes)
			{
				isFirst=true;
				TypeDefinitionHandle typeDefHandle = item.Value.Handle;
				foreach (PropertyBuilder pb in item.Key.GetProperties())
				{
					propDefHandle=_tablesAndHeaps.AddProperty(pb.Attributes, _tablesAndHeaps.GetOrAddString(pb.Name), _tablesAndHeaps.GetOrAddBlob(BuildSignature(e => e.PropertySignature(!(pb.GetMethod??pb.SetMethod).IsStatic).Parameters(0, returnType => BuildReturnSignature(returnType, pb.PropertyType), parameters => { }))));
					if (isFirst)
					{
						_tablesAndHeaps.AddPropertyMap(typeDefHandle, propDefHandle);
						isFirst=false;
					}
					_tablesAndHeaps.AddMethodSemantics(propDefHandle, MethodSemanticsAttributes.Getter, pb.GetMethod==null ? default(MethodDefinitionHandle) : _definedMethods[pb.GetMethod]);
					_tablesAndHeaps.AddMethodSemantics(propDefHandle, MethodSemanticsAttributes.Setter, pb.SetMethod==null ? default(MethodDefinitionHandle) : _definedMethods[pb.SetMethod]);
				}
			}
		}

		void WriteMethods()
		{
			int paramIndex = _tablesAndHeaps.GetRowCount(TableIndex.Param) + 1;
			foreach (KeyValuePair<MethodBuilderBase, MethodDefinitionHandle> item in _definedMethods) //has to enumerate in same order as there were assigned TOKENS
			{
				MethodBuilderBase mb = item.Key;
				ParameterBuilder[] pis = mb.GetParameters();
				ParameterHandle firstParameter = MetadataTokens.ParameterHandle(paramIndex);

				int a = 0;
				foreach (ParameterBuilder pi in pis)
					_tablesAndHeaps.AddParameter(pi.Attributes, pi.Name==null ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(pi.Name), ++a);
				paramIndex += pis.Length;

				(int bodyOffset, int instructionsSize)=WriteMethodBody(mb);

				MethodDefinitionHandle mdh = _tablesAndHeaps.AddMethodDefinition(mb.Attributes,
						MethodImplAttributes.IL,
						_tablesAndHeaps.GetOrAddString(mb.Name),
						_tablesAndHeaps.GetOrAddBlob(BuildSignature(e => e.MethodSignature(genericParameterCount: 0, isInstanceMethod: !mb.IsStatic)
							.Parameters(pis.Length, returnType => BuildReturnSignature(returnType, mb.ReturnType), parameters => { foreach (ParameterBuilder pi in pis) BuildParameterSignature(parameters.AddParameter(), pi.ParameterType); }))),
						bodyOffset, firstParameter);

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
		(int bodyOffset, int instructionsSize) WriteMethodBody(MethodBuilderBase mb)
		{
			MethodBuilderBody methodBody = mb.GetMethodBody();
			LocalBuilder[] localVariables = methodBody.LocalVariables.ToArray();

			StandaloneSignatureHandle localVariablesSignature;
			LocalVariablesEncoder encoder = new BlobEncoder(new BlobBuilder()).LocalVariableSignature(localVariables.Length);
			foreach (LocalBuilder local in localVariables)
				BuildSignature(encoder.AddVariable().Type(), local.LocalType);
			localVariablesSignature=_tablesAndHeaps.AddStandaloneSignature(_tablesAndHeaps.GetOrAddBlob(encoder.Builder));

			InstructionEncoder instructionEncoder = new InstructionEncoder(new BlobBuilder(), new ControlFlowBuilder());
			List<SequencePoint> seqPoints = new List<SequencePoint>();
			int maxStackSize = WriteMethodInstructions(methodBody, instructionEncoder, seqPoints, (mb.ReturnType is NetType nt) && (nt.Type.Equals(typeof(void))));
			_seqPoints.Add((seqPoints.ToArray(), MetadataTokens.GetRowNumber(localVariablesSignature)));
			int offset = _methodBodies.AddMethodBody(instructionEncoder, maxStackSize, localVariablesSignature, MethodBodyAttributes.InitLocals);

			return (offset, instructionEncoder.CodeBuilder.Count);
		}

		int WriteMethodInstructions(MethodBuilderBody methodBody, InstructionEncoder instructionEncoder, List<SequencePoint> seqPoints, bool returnsVoid)
		{
			int curStack=0;
			int maxStack=0;

			void AdjustStack(int change)
			{
				curStack+=change;
				if (maxStack<curStack)
					maxStack=curStack;
			}

			Dictionary<Label, ILabelHandle> definedLabels = methodBody.Labels.ToDictionary(x => x, x => new ILabelHandle());
			Dictionary<Label, LabelHandle> definedExLabels = new Dictionary<Label, LabelHandle>();

			foreach (ExceptionRegionInfo exRegionInfo in methodBody.Exceptions)
			{
				LabelHandle tryStart = instructionEncoder.DefineLabel();
				LabelHandle handleStart = instructionEncoder.DefineLabel();
				LabelHandle handleEnd = instructionEncoder.DefineLabel();
				definedExLabels.Add(exRegionInfo.TryStart, tryStart);
				definedExLabels.Add(exRegionInfo.HandleStart, handleStart);
				definedExLabels.Add(exRegionInfo.HandleEnd, handleEnd);

				if (exRegionInfo.IsCatch)
					instructionEncoder.ControlFlowBuilder.AddCatchRegion(tryStart, handleStart, handleStart, handleEnd, GetTypeDefOrRef(exRegionInfo.ExceptionType));
				else
					instructionEncoder.ControlFlowBuilder.AddFinallyRegion(tryStart, handleStart, handleStart, handleEnd);
			}

			foreach (Instruction instruction in methodBody.Instructions)
			{
				if (instruction is InstructionSeqPoint seqPointIns)
				{
					(string documentName, int startLineNumber, ushort startColumn, int endLineNumber, ushort endColumn)=seqPointIns.SeqPoint;

					bool replace = (seqPoints.Count!=0)&&(seqPoints[seqPoints.Count-1].Offset==instructionEncoder.Offset);

					if (SequencePoint.IsValidPoint(startLineNumber, startColumn, endLineNumber, endColumn))
					{
						if (replace)
							seqPoints.RemoveAt(seqPoints.Count-1);
						seqPoints.Add(SequencePoint.Create(_docPdb, instructionEncoder.Offset, startLineNumber, startColumn, endLineNumber, endColumn));
					}
					else
						if (!replace)
						seqPoints.Add(SequencePoint.Hidden(_docPdb, instructionEncoder.Offset));
				}
				else if (instruction is InstructionMarkLabel markLabelIns)
				{
					Label theLabel = markLabelIns.Label;
					definedLabels[theLabel].Offset=instructionEncoder.Offset;
					if (definedExLabels.TryGetValue(theLabel, out LabelHandle labelHandle))
					{
						instructionEncoder.MarkLabel(labelHandle);

						//if current offset is HandleStart, we have to call AdjustStack(1) as .NET puts exception variable onto stack
						if (methodBody.Exceptions.Any(x => x.HandleStart == theLabel))
							AdjustStack(1);
					}
				}
				else if ((instruction is InstructionOpCode<int>)||(instruction is InstructionOpCode<double>)||(instruction is InstructionOpCode<Label>))
				{
					//don't write opcode
					AdjustStack(((InstructionOpCode)instruction).OpCode.NetStackBehavior());
				}
				else
				{
					ILOpCode code = ((InstructionOpCode)instruction).OpCode;
					instructionEncoder.OpCode(code);
					if (code!=ILOpCode.Ret)
						AdjustStack(code.NetStackBehavior());
				}

				if ((instruction.GetType().Equals(typeof(InstructionOpCode)))||(instruction is InstructionSeqPoint)||(instruction is InstructionMarkLabel))
				{
					//don't do anything;
				}
				else if (instruction is InstructionOpCode<ConstructorInfo> ciIns)
				{
					instructionEncoder.Token(GetMemberReference(ciIns.Data));
					AdjustStack(-ciIns.Data.GetParameters().Length-1);
				}
				else if (instruction is InstructionOpCode<ConstructorBuilder> cbIns)
				{
					instructionEncoder.Token(_definedMethods[cbIns.Data]);
					AdjustStack(-cbIns.Data.GetParameters().Length-1);
				}
				else if (instruction is InstructionOpCode<MethodInfo> miIns)
				{
					instructionEncoder.Token(GetMemberReference(miIns.Data));
					AdjustStack(-miIns.Data.GetParameters().Length+(miIns.Data.ReturnType.Equals(typeof(void))?0:1)-(miIns.Data.IsStatic?0:1));
				}
				else if (instruction is InstructionOpCode<MethodBuilder> mbIns)
				{
					instructionEncoder.Token(_definedMethods[mbIns.Data]);
					AdjustStack(-mbIns.Data.GetParameters().Length+((mbIns.Data.ReturnType is NetType nt)&&(nt.Type.Equals(typeof(void)))?0:1)-(mbIns.Data.IsStatic?0:1));
				}
				else if (instruction is InstructionOpCode<FieldInfo> fiIns)
					instructionEncoder.Token(GetMemberReference(fiIns.Data));
				else if (instruction is InstructionOpCode<FieldBuilder> fbIns)
					instructionEncoder.Token(_definedFields[fbIns.Data]);
				else if (instruction is InstructionOpCode<IType> typeIns)
					instructionEncoder.Token(GetTypeDefOrRef(typeIns.Data));
				else if (instruction is InstructionOpCode<LocalBuilder> lbIns)
					instructionEncoder.Token(lbIns.Data.Index);
				else if (instruction is InstructionOpCode<Label> labelIns)
				{
					ILOpCode opCode = ((InstructionOpCode)instruction).OpCode;
					int operandSize = opCode.GetBranchOperandSize();
					instructionEncoder.OpCode(opCode);
					definedLabels[labelIns.Data].ReferenceOffsets.Add((instructionEncoder.Offset, operandSize, instructionEncoder.CodeBuilder.ReserveBytes(operandSize)));
				}
				else if (instruction is InstructionOpCode<int> intIns)
				{
					if (intIns.OpCode==ILOpCode.Ldc_i4)
						instructionEncoder.LoadConstantI4(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Ldarg)
						instructionEncoder.LoadArgument(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Starg)
						instructionEncoder.StoreArgument(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Ldloc)
						instructionEncoder.LoadLocal(intIns.Data);
					else if (intIns.OpCode==ILOpCode.Stloc)
						instructionEncoder.StoreLocal(intIns.Data);
					else
						throw new NotSupportedException();
				}
				else if (instruction is InstructionOpCode<double> doubleIns)
				{
					if (doubleIns.OpCode==ILOpCode.Ldc_r8)
						instructionEncoder.LoadConstantR8(doubleIns.Data);
					else
						throw new NotSupportedException();
				}
				else if (instruction is InstructionOpCode<string> stringIns)
					instructionEncoder.Token(MetadataTokens.GetToken(_tablesAndHeaps.GetOrAddUserString(stringIns.Data)));
				else
					throw new NotImplementedException();
			}

			foreach (KeyValuePair<Label, ILabelHandle> labelPair in definedLabels)
			{
				ILabelHandle label = labelPair.Value;
				foreach ((int ReferenceOffset, int OperandSize, Blob PlaceToWrite) labelRef in label.ReferenceOffsets)
				{
					int offsetToWrite = label.Offset-labelRef.ReferenceOffset-labelRef.OperandSize;
					if (labelRef.OperandSize==1)
						new BlobWriter(labelRef.PlaceToWrite).WriteSByte((sbyte)offsetToWrite);
					else if (labelRef.OperandSize==4)
						new BlobWriter(labelRef.PlaceToWrite).WriteInt32(offsetToWrite);
					else
						throw new NotSupportedException();
				}
			}

			//if (curStack!=(returnsVoid ? 0 : 1)) can't calculate stack correctly (can't follow jumps) so can't compare precisely to expected value
			if (curStack<(returnsVoid ? 0 : 1))
			{
#if DEBUG
				(ILOpCode code, object data, int behavior, int sum)[] GetInstructionBehavior()
				{
					int sum2 = 0;
					return methodBody.Instructions.Where(i => i is InstructionOpCode).Select(instruction =>
					{
						ILOpCode code = ((InstructionOpCode)instruction).OpCode;
						int result = code==ILOpCode.Ret ? 0 : code.NetStackBehavior();

						if (instruction is InstructionOpCode<ConstructorInfo> ciIns)
							result+=-ciIns.Data.GetParameters().Length-1;
						else if (instruction is InstructionOpCode<ConstructorBuilder> cbIns)
							result+=-cbIns.Data.GetParameters().Length-1;
						else if (instruction is InstructionOpCode<MethodInfo> miIns)
							result+=-miIns.Data.GetParameters().Length+(miIns.Data.ReturnType.Equals(typeof(void)) ? 0 : 1)-(miIns.Data.IsStatic ? 0 : 1);
						else if (instruction is InstructionOpCode<MethodBuilder> mbIns)
							result+=-mbIns.Data.GetParameters().Length+((mbIns.Data.ReturnType is NetType nt)&&(nt.Type.Equals(typeof(void))) ? 0 : 1)-(mbIns.Data.IsStatic ? 0 : 1);

						sum2+=result;

						return (code, instruction.GetType().BaseType.Equals(typeof(InstructionOpCode)) ? instruction.GetType().GetField("Data", BindingFlags.NonPublic|BindingFlags.Instance).GetValue(instruction) : null, result, sum2);
					}).ToArray();
				}

				(ILOpCode code, object data, int behavior, int sum)[] array = GetInstructionBehavior();
				int sum = array.Sum(x=>x.behavior);
#endif
				throw new InvalidOperationException();
			}
			return maxStack;
		}
		#endregion methodbody

		#region AddRuntimeAttribute
		void AddRuntimeAttributeOld(AssemblyDefinitionHandle asmDefHandle)
		{
			AssemblyReferenceHandle attrAsmRef = _systemRuntimeAssemblyReferenceHandle;
			TypeReferenceHandle attrTypeRef = _tablesAndHeaps.AddTypeReference(attrAsmRef, _tablesAndHeaps.GetOrAddString("System.Runtime.Versioning"), _tablesAndHeaps.GetOrAddString("TargetFrameworkAttribute"));

			BlobBuilder ctorSignature = new BlobBuilder();
			new BlobEncoder(ctorSignature).MethodSignature().Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); });
			MemberReferenceHandle attrCtorRef = _tablesAndHeaps.AddMemberReference(attrTypeRef, _tablesAndHeaps.GetOrAddString(".ctor"), _tablesAndHeaps.GetOrAddBlob(ctorSignature));


			//System.Runtime, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
			//[assembly: TargetFramework(".NETCoreApp,Version=v2.0", FrameworkDisplayName = "")]
			/*
			 .custom instance void [System.Runtime]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = (
					01 00 18 2e 4e 45 54 43 6f 72 65 41 70 70 2c 56
					65 72 73 69 6f 6e 3d 76 32 2e 30 01 00 54 0e 14
					46 72 61 6d 65 77 6f 72 6b 44 69 73 70 6c 61 79
					4e 61 6d 65 00
				)*/

			BlobBuilder ctorParms = new BlobBuilder();
			ctorParms.WriteBytes(new byte[] {
						0x01,0x00,0x18,0x2e,0x4e,0x45,0x54,0x43,0x6f,0x72,0x65,0x41,0x70,0x70,0x2c,0x56,
						0x65,0x72,0x73,0x69,0x6f,0x6e,0x3d,0x76,0x32,0x2e,0x30,0x01,0x00,0x54,0x0e,0x14,
						0x46,0x72,0x61,0x6d,0x65,0x77,0x6f,0x72,0x6b,0x44,0x69,0x73,0x70,0x6c,0x61,0x79,
						0x4e,0x61,0x6d,0x65,0x00
				});

			_tablesAndHeaps.AddCustomAttribute(asmDefHandle, attrCtorRef, _tablesAndHeaps.GetOrAddBlob(ctorParms));

			//.custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = (01 00 07 01 00 00 00 00)
			attrTypeRef=_tablesAndHeaps.AddTypeReference(attrAsmRef, _tablesAndHeaps.GetOrAddString("System.Diagnostics"), _tablesAndHeaps.GetOrAddString("DebuggableAttribute"));
			ctorSignature=new BlobBuilder();
			new BlobEncoder(ctorSignature).MethodSignature().Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().Int32(); });
			attrCtorRef=_tablesAndHeaps.AddMemberReference(attrTypeRef, _tablesAndHeaps.GetOrAddString(".ctor"), _tablesAndHeaps.GetOrAddBlob(ctorSignature));
			ctorParms=new BlobBuilder();
			ctorParms.WriteBytes(new byte[] { 0x01, 0x00, 0x07, 0x01, 0x00, 0x00, 0x00, 0x00 });
			_tablesAndHeaps.AddCustomAttribute(asmDefHandle, attrCtorRef, _tablesAndHeaps.GetOrAddBlob(ctorParms));
		}

		void AddRuntimeAttributes(AssemblyDefinitionHandle asmDefHandle)
		{
			/*
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: Debuggable]
[assembly: AssemblyCompany("AsmSaveAsTest")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("Package Description")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0")]
[assembly: AssemblyProduct("AsmSaveAsTest")]
[assembly: AssemblyTitle("AsmSaveAsTest")]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: TargetFramework(".NETCoreApp,Version=v2.0", FrameworkDisplayName = "")]
			*/
			/*
.assembly AsmSaveAsTest
{
	.custom instance void [System.Runtime]System.Runtime.CompilerServices.ExtensionAttribute::.ctor() = (
		01 00 00 00
	)
	.custom instance void [System.Runtime]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = (
		01 00 08 00 00 00 00 00
	)
	.custom instance void [System.Runtime]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = (
		01 00 01 00 54 02 16 57 72 61 70 4e 6f 6e 45 78
		63 65 70 74 69 6f 6e 54 68 72 6f 77 73 01
	)
	.custom instance void [System.Runtime]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [System.Runtime]System.Diagnostics.DebuggableAttribute/DebuggingModes) = (
		01 00 07 01 00 00 00 00
	)
	.custom instance void [System.Runtime]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = (
		01 00 18 2e 4e 45 54 43 6f 72 65 41 70 70 2c 56
		65 72 73 69 6f 6e 3d 76 32 2e 30 01 00 54 0e 14
		46 72 61 6d 65 77 6f 72 6b 44 69 73 70 6c 61 79
		4e 61 6d 65 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = (
		01 00 0d 41 73 6d 53 61 76 65 41 73 54 65 73 74
		00 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = (
		01 00 05 44 65 62 75 67 00 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = (
		01 00 13 50 61 63 6b 61 67 65 20 44 65 73 63 72
		69 70 74 69 6f 6e 00 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = (
		01 00 07 31 2e 30 2e 30 2e 30 00 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyInformationalVersionAttribute::.ctor(string) = (
		01 00 05 31 2e 30 2e 30 00 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyProductAttribute::.ctor(string) = (
		01 00 0d 41 73 6d 53 61 76 65 41 73 54 65 73 74
		00 00
	)
	.custom instance void [System.Runtime]System.Reflection.AssemblyTitleAttribute::.ctor(string) = (
		01 00 0d 41 73 6d 53 61 76 65 41 73 54 65 73 74
		00 00
	)
}
*/

			AddRuntimeAttribute(asmDefHandle, "System.Runtime.CompilerServices", "ExtensionAttribute",
				parms => parms.Parameters(0, returnType => returnType.Void(), parameters => { }), new byte[] { 0x01, 0x00, 0x00, 0x00 });

			AddRuntimeAttribute(asmDefHandle, "System.Runtime.CompilerServices", "CompilationRelaxationsAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().Int32(); }),
				new byte[] { 0x01, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 });

			AddRuntimeAttribute(asmDefHandle, "System.Runtime.CompilerServices", "RuntimeCompatibilityAttribute",
				parms => parms.Parameters(0, returnType => returnType.Void(), parameters => { }),
				new byte[] {
						0x01,0x00,0x01,0x00,0x54,0x02,0x16,0x57,0x72,0x61,0x70,0x4e,0x6f,0x6e,0x45,0x78,
						0x63,0x65,0x70,0x74,0x69,0x6f,0x6e,0x54,0x68,0x72,0x6f,0x77,0x73,0x01
				});

			AddRuntimeAttribute(asmDefHandle, "System.Reflection", "AssemblyConfigurationAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] { 0x01, 0x00, 0x05, 0x44, 0x65, 0x62, 0x75, 0x67, 0x00, 0x00 });

			AddRuntimeAttribute(asmDefHandle, "System.Diagnostics", "DebuggableAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().Int32(); }),
				new byte[] { 0x01, 0x00, 0x07, 0x01, 0x00, 0x00, 0x00, 0x00 });

			AddRuntimeAttribute(asmDefHandle, "System.Runtime.Versioning", "TargetFrameworkAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] {
						0x01,0x00,0x18,0x2e,0x4e,0x45,0x54,0x43,0x6f,0x72,0x65,0x41,0x70,0x70,0x2c,0x56,
						0x65,0x72,0x73,0x69,0x6f,0x6e,0x3d,0x76,0x32,0x2e,0x30,0x01,0x00,0x54,0x0e,0x14,
						0x46,0x72,0x61,0x6d,0x65,0x77,0x6f,0x72,0x6b,0x44,0x69,0x73,0x70,0x6c,0x61,0x79,
						0x4e,0x61,0x6d,0x65,0x00
				});

			/*AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyCompanyAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] {
					0x01,0x00,0x0d,0x41,0x73,0x6d,0x53,0x61,0x76,0x65,0x41,0x73,0x54,0x65,0x73,0x74,
					0x00,0x00
				});

			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyDescriptionAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] {
					0x01,0x00,0x13,0x50,0x61,0x63,0x6b,0x61,0x67,0x65,0x20,0x44,0x65,0x73,0x63,0x72,
					0x69,0x70,0x74,0x69,0x6f,0x6e,0x00,0x00
				});

			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyFileVersionAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] { 0x01,0x00,0x07,0x31,0x2e,0x30,0x2e,0x30,0x2e,0x30,0x00,0x00 });

			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyInformationalVersionAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] { 0x01,0x00,0x05,0x31,0x2e,0x30,0x2e,0x30,0x00,0x00 });

			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyProductAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] {
					0x01,0x00,0x0d,0x41,0x73,0x6d,0x53,0x61,0x76,0x65,0x41,0x73,0x54,0x65,0x73,0x74,
					0x00,0x00
				});

			AddRuntimeAttribute(tablesAndHeaps, mscorlibAssemblyRef, asmDefHandle, "System.Reflection", "AssemblyTitleAttribute",
				parms => parms.Parameters(1, returnType => returnType.Void(), parameters => { parameters.AddParameter().Type().String(); }),
				new byte[] {
					0x01,0x00,0x0d,0x41,0x73,0x6d,0x53,0x61,0x76,0x65,0x41,0x73,0x54,0x65,0x73,0x74,
					0x00,0x00
				});*/
		}

		void AddRuntimeAttribute(AssemblyDefinitionHandle asmDefHandle, string @namespace, string typename, Action<MethodSignatureEncoder> parms, byte[] data)
		{
			TypeReferenceHandle attrTypeRef = _tablesAndHeaps.AddTypeReference(_systemRuntimeAssemblyReferenceHandle, _tablesAndHeaps.GetOrAddString(@namespace), _tablesAndHeaps.GetOrAddString(typename));
			BlobBuilder ctorSignature = new BlobBuilder();
			parms(new BlobEncoder(ctorSignature).MethodSignature(isInstanceMethod: true));
			MemberReferenceHandle attrCtorRef = _tablesAndHeaps.AddMemberReference(attrTypeRef, _tablesAndHeaps.GetOrAddString(".ctor"), _tablesAndHeaps.GetOrAddBlob(ctorSignature));
			_tablesAndHeaps.AddCustomAttribute(asmDefHandle, attrCtorRef, _tablesAndHeaps.GetOrAddBlob(data));
		}
		#endregion AddRuntimeAttribute

		#region helpers
		CacheObject<Assembly, AssemblyReferenceHandle> _asmReferenceCache = new CacheObject<Assembly, AssemblyReferenceHandle>();
		AssemblyReferenceHandle GetAssemblyReference(Assembly asm)
		{
			return _asmReferenceCache.GetOrAdd(asm, x =>
			{
				AssemblyName asmName = x.GetName();
				string culture = asmName.CultureName;
				byte[] publicKey = asmName.GetPublicKey();
				//return _tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString(asmName.Name), asmName.Version, culture==null||culture.Length==0 ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(culture), publicKey==null||publicKey.Length==0 ? default(BlobHandle) : _tablesAndHeaps.GetOrAddBlob(publicKey), (AssemblyFlags)asmName.Flags, _tablesAndHeaps.GetOrAddBlob(BitConverter.GetBytes((uint)asmName.HashAlgorithm)));
				return _tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString(asmName.Name), asmName.Version, culture==null||culture.Length==0 ? default(StringHandle) : _tablesAndHeaps.GetOrAddString(culture), publicKey==null||publicKey.Length==0 ? default(BlobHandle) : _tablesAndHeaps.GetOrAddBlob(asmName.GetPublicKeyToken()), (AssemblyFlags)0, default(BlobHandle));
			});
		}

		CacheObject<Type, EntityHandle> _typeReferenceCache = new CacheObject<Type, EntityHandle>();
		EntityHandle GetTypeReference(IType type)
		{
			Type typeNet = ((NetType)type).Type;
			return _typeReferenceCache.GetOrAdd(typeNet, x =>
			{
				//AssemblyReferenceHandle mscorlibAssemblyRef = _tablesAndHeaps.AddAssemblyReference(_tablesAndHeaps.GetOrAddString("mscorlib"), new Version(4, 0, 0, 0), default(StringHandle), _tablesAndHeaps.GetOrAddBlob(ImmutableArray.Create<byte>(0xB7, 0x7A, 0x5C, 0x56, 0x19, 0x34, 0xE0, 0x89)), default(AssemblyFlags), default(BlobHandle));
				//result=_tablesAndHeaps.AddTypeReference(mscorlibAssemblyRef, _tablesAndHeaps.GetOrAddString("System"), _tablesAndHeaps.GetOrAddString("Object"));

				/*return type==typeof(object)
					? _tablesAndHeaps.AddTypeReference(_systemRuntimeAssemblyReferenceHandle, _tablesAndHeaps.GetOrAddString("System"), _tablesAndHeaps.GetOrAddString("Object"))
					: _tablesAndHeaps.AddTypeReference(GetAssemblyReference(x.Assembly), _tablesAndHeaps.GetOrAddString(type.Namespace), _tablesAndHeaps.GetOrAddString(type.Name));*/
				
				if ((typeNet.IsGenericType)&&(!typeNet.IsGenericTypeDefinition))
					return _tablesAndHeaps.AddTypeSpecification(_tablesAndHeaps.GetOrAddBlob(BuildSignature(be => BuildSignature(be.TypeSpecificationSignature(), typeNet))));
				if ((_copyRuntime) && (x.Assembly == typeof(Jellequin.Runtime.Executor).Assembly))
					return MetadataTokens.EntityHandle(typeNet.MetadataToken);
				return _tablesAndHeaps.AddTypeReference(GetAssemblyReference(x.Assembly), _tablesAndHeaps.GetOrAddString(typeNet.Namespace), _tablesAndHeaps.GetOrAddString(typeNet.Name));
			});
		}

		EntityHandle GetMemberReference(MemberInfo member)
		{
			switch (member.MemberType)
			{
				case MemberTypes.Field:
					FieldInfo fi = (FieldInfo)member;
					return _tablesAndHeaps.AddMemberReference(GetTypeReference(fi.DeclaringType), _tablesAndHeaps.GetOrAddString(fi.Name), _tablesAndHeaps.GetOrAddBlob(BuildFieldSignature(fi.FieldType)));
				case MemberTypes.Constructor:
				case MemberTypes.Method:
					MethodBase mb = (MethodBase)member;

					ParameterInfo[] pis = mb.GetParameters();
					BlobBuilder newMethodSignature;

					if (mb.DeclaringType.ContainsGenericParameters)
					{
						Type genType = mb.DeclaringType.GetGenericTypeDefinition();
						Type[] genArguments = genType.GetGenericArguments();
						MethodInfo genMethod = genType.GetMethod(mb.Name);
						pis=genMethod.GetParameters();

						new BlobEncoder(newMethodSignature=new BlobBuilder()).
							MethodSignature(genericParameterCount: mb.GetGenericArguments().Length, isInstanceMethod: !mb.IsStatic).
							Parameters(pis.Length,
								 returnType => BuildSignature(returnType.Type(), ste => ste.GenericTypeParameter(Array.IndexOf(genArguments, genMethod.ReturnType))),
								 parameters => { foreach (ParameterInfo pi in pis) BuildSignature(parameters.AddParameter().Type(), ste => ste.GenericTypeParameter(Array.IndexOf(genArguments, pi.ParameterType))); });
						return _tablesAndHeaps.AddMemberReference(GetTypeReference(mb.DeclaringType), _tablesAndHeaps.GetOrAddString(mb.Name), _tablesAndHeaps.GetOrAddBlob(newMethodSignature));
					}
					else
					{
						MethodInfo mii = mb as MethodInfo;
						new BlobEncoder(newMethodSignature=new BlobBuilder()).
							MethodSignature(genericParameterCount: mii==null ? 0 : mb.GetGenericArguments().Length, isInstanceMethod: !mb.IsStatic).
							Parameters(pis.Length,
								 returnType => BuildReturnSignature(returnType, mii==null ? typeof(void) : mii.ReturnType),
								 parameters => { foreach (ParameterInfo pi in pis) BuildParameterSignature(parameters.AddParameter(), pi.ParameterType); });
						return _tablesAndHeaps.AddMemberReference(GetTypeReference(mb.DeclaringType), _tablesAndHeaps.GetOrAddString(mb.Name), _tablesAndHeaps.GetOrAddBlob(newMethodSignature));
					}
				default:
					throw new NotSupportedException();
			}
		}

		class CacheObject<TKey, TValue>
		{
			Dictionary<TKey, TValue> _cache = new Dictionary<TKey, TValue>();

			internal TValue GetOrAdd(TKey key, Func<TKey, TValue> addFunction)
			{
				TValue result;
				if (_cache.TryGetValue(key, out result))
					return result;
				_cache[key]=(result=addFunction(key));
				return result;
			}
		}

		EntityHandle GetTypeDefOrRef(IType type)
		{
			if (type is NetType netType)
				return GetTypeReference(netType);
			else if (type is NewType newType)
				return _definedTypes[newType.TypeBuilder].Handle;
			else
				throw new NotImplementedException();
		}

		void BuildSignature(SignatureTypeEncoder ste, Action<SignatureTypeEncoder> action)
		{
			action(ste);
		}

		void BuildSignature(SignatureTypeEncoder ste, IType type)
		{
			//should be type.BuildSignature(ste);

			if (type is NetType netType)
			{
				Type typeNet = netType.Type;

				if (typeNet.Equals(typeof(int)))
					ste.Int32();
				else if (typeNet.Equals(typeof(bool)))
					ste.Boolean();
				else if (typeNet.Equals(typeof(IntPtr)))
					ste.IntPtr();
				else if (typeNet.Equals(typeof(string)))
					ste.String();
				else if (typeNet.IsArray)
					BuildSignature(ste.SZArray(), typeNet.GetElementType());
				else if (typeNet.Equals(typeof(object)))
					ste.Object();
				else
				{
					if ((typeNet.IsGenericType)&&(!typeNet.IsGenericTypeDefinition))
					{
						Type[] genericArguments = typeNet.GetGenericArguments();
						GenericTypeArgumentsEncoder gtae = ste.GenericInstantiation(GetTypeReference(typeNet.GetGenericTypeDefinition()), genericArguments.Length, false);
						foreach (Type ga in genericArguments)
							BuildSignature(gtae.AddArgument(), ga);
						return;
					}

					ste.Type(GetTypeReference(type), typeNet.IsValueType);
				}
			}
			else if (type is NewType newType)
				ste.Type(_definedTypes[newType.TypeBuilder].Handle, newType.IsValueType);
			else
				throw new NotImplementedException();
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
					ate.ObjectArray();
				else
					eteN = ate.ElementType();
				return;
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

		BlobBuilder BuildFieldSignature(IType type)
		{
			return BuildSignature(be => {
				BuildSignature(be.FieldSignature(), type);
			});
		}

		void BuildReturnSignature(ReturnTypeEncoder rte, IType type)
		{
			if ((type is NetType netType)&&(netType.Type.Equals(typeof(void))))
				rte.Void();
			else
				BuildSignature(rte.Type(), type);
		}

		void BuildParameterSignature(ParameterTypeEncoder pte, IType type)
		{
			BuildSignature(pte.Type(), type);
		}

		void BuildLocalSignature(LocalVariableTypeEncoder lvte, IType type)
		{
			BuildSignature(lvte.Type(), type);
		}

		BlobBuilder BuildCustomAttributeSignature(CustomAttributeBuilder ca)
		{
			//Inspired by Roslyn's SerializeCustomAttributeSignature

			BlobBuilder builder = new BlobBuilder();
			new BlobEncoder(builder).CustomAttributeSignature(out FixedArgumentsEncoder fixedArgsEncoder, out CustomAttributeNamedArgumentsEncoder namedArgsEncoder);
			ca.Arguments.Process(a => fixedArgsEncoder.AddArgument().Scalar().Constant(a));
			NamedArgumentsEncoder nae=namedArgsEncoder.Count(ca.Props.Length);
			ca.Props.Process(cav=>nae.AddArgument(cav.IsField, nate => BuildSignature(nate, cav.Value==null?typeof(string):cav.Value.GetType()), ne => ne.Name(cav.Name), le => le.Scalar().Constant(cav.Value)));
			return builder;
		}
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
			return GetAlgorithm(algorithmId).ComputeHash(data);
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
			internal MethodBuilderBase[] Methods;
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
			Stream _manifestStream, _iconStream;
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
				{
					return null;
				}
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
}

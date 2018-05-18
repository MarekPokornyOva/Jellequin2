#region using
using Microsoft.Cci;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
#endregion using

namespace Jellequin.Compiler
{
	class RuntimeCopier:IDisposable
	{
		PEReader _pEReader;

		internal RuntimeCopier(Stream asmToCopy)
		{
			_pEReader = new PEReader(asmToCopy);
		}

		public void Dispose()
		{
			_pEReader.Dispose();
		}

		MetadataReader _source;
		MetadataBuilder _target;
		internal void Copy(MetadataBuilder metadataBuilder, MethodBodyStreamEncoder methodBodies)
		{
			_source = _pEReader.GetMetadataReader(MetadataReaderOptions.None);
			_target = metadataBuilder;

			#region pre-requirements
			_source.ManifestResources.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetManifestResource(x)).Process(x =>
				metadataBuilder.AddManifestResource(x.Attributes, CopyString(x.Name), CopyEntityHandle(x.Implementation), (uint)x.Offset)
			);
			_source.AssemblyFiles.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetAssemblyFile(x)).Process(x =>
				metadataBuilder.AddAssemblyFile(CopyString(x.Name), CopyBlob(x.HashValue), x.ContainsMetadata)
			);
			_source.AssemblyReferences.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(handle => (handle, _source.GetAssemblyReference(handle))).Process(x =>
			{
				if (metadataBuilder.AddAssemblyReference(CopyString(x.Item2.Name), x.Item2.Version, CopyString(x.Item2.Culture), CopyBlob(x.Item2.PublicKeyOrToken), x.Item2.Flags, CopyBlob(x.Item2.HashValue)) != x.handle)
					throw new InvalidCastException();
			});
			_source.TypeReferences.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(handle => (handle, _source.GetTypeReference(handle))).Process(x =>
			{
				if (metadataBuilder.AddTypeReference(x.Item2.ResolutionScope, CopyString(x.Item2.Namespace), CopyString(x.Item2.Name)) != x.handle)
					throw new InvalidCastException();
			});
			_source.MemberReferences.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetMemberReference(x)).Process(x =>
				metadataBuilder.AddMemberReference(CopyEntityHandle(x.Parent), CopyString(x.Name), CopySignature(x.Signature))
			);
			_source.CustomAttributes.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetCustomAttribute(x)).Process(x =>
				metadataBuilder.AddCustomAttribute(x.Parent, x.Constructor, CopyBlob(x.Value))
			);
			_source.CustomDebugInformation.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetCustomDebugInformation(x)).Process(x =>
				metadataBuilder.AddCustomDebugInformation(x.Parent, CopyGuid(x.Kind), CopyBlob(x.Value))
			);
			_source.DeclarativeSecurityAttributes.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetDeclarativeSecurityAttribute(x)).Process(x =>
				metadataBuilder.AddDeclarativeSecurityAttribute(x.Parent, x.Action, CopyBlob(x.PermissionSet))
			);
			_source.Documents.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetDocument(x)).Process(x =>
				metadataBuilder.AddDocument(CopyBlob(x.Name), CopyGuid(x.HashAlgorithm), CopyBlob(x.Hash), CopyGuid(x.Language))
			);
			_source.EventDefinitions.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetEventDefinition(x)).Process(x =>
				metadataBuilder.AddEvent(x.Attributes, CopyString(x.Name), CopyEntityHandle(x.Type))
			);
			_source.ExportedTypes.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetExportedType(x)).Process(x =>
				metadataBuilder.AddExportedType(x.Attributes, CopyString(x.Namespace), CopyString(x.Name), x.Implementation, x.GetTypeDefinitionId())
			);
			#endregion pre-requirements

			#region main part - type+members
			FieldDefinitionHandle lastFirstField = MetadataTokens.FieldDefinitionHandle(1);
			MethodDefinitionHandle lastFirstMethod = MetadataTokens.MethodDefinitionHandle(1);
			Dictionary<PropertyDefinitionHandle, TypeDefinitionHandle> firstProps = new Dictionary<PropertyDefinitionHandle, TypeDefinitionHandle>();
			_source.TypeDefinitions.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(handle => (handle,_source.GetTypeDefinition(handle))).Process(x =>
			{
				FieldDefinitionHandleCollection typeFields = x.Item2.GetFields();
				if (typeFields.Count != 0)
					lastFirstField = typeFields.First();
				MethodDefinitionHandleCollection typeMethods = x.Item2.GetMethods();
				if (typeMethods.Count != 0)
					lastFirstMethod = typeMethods.First();
				TypeDefinitionHandle td = metadataBuilder.AddTypeDefinition(x.Item2.Attributes, CopyString(x.Item2.Namespace), CopyString(x.Item2.Name), x.Item2.BaseType, lastFirstField, lastFirstMethod);
				if (td != x.handle)
					throw new InvalidCastException();
				foreach (TypeDefinitionHandle nest in x.Item2.GetNestedTypes())
					metadataBuilder.AddNestedType(nest, td);

				PropertyDefinitionHandleCollection props = x.Item2.GetProperties();
				if (props.Count != 0)
					firstProps.Add(props.First(), td);

				if (typeFields.Count != 0)
					lastFirstField = MetadataTokens.FieldDefinitionHandle(MetadataTokens.GetRowNumber(typeFields.Last())+1);
				if (typeMethods.Count != 0)
					lastFirstMethod = MetadataTokens.MethodDefinitionHandle(MetadataTokens.GetRowNumber(typeMethods.Last())+1);
			});
			_source.TypeDefinitions.SelectMany(typeHandle => _source.GetTypeDefinition(typeHandle).GetInterfaceImplementations().Select(intHandle=>(typeHandle,intHandle)))
				.OrderBy(x => MetadataTokens.GetRowNumber(x.intHandle)).Select(x => (x.typeHandle, _source.GetInterfaceImplementation(x.intHandle))).Process(x =>
					metadataBuilder.AddInterfaceImplementation(x.typeHandle, x.Item2.Interface));
			_source.TypeDefinitions.SelectMany(typeHandle => _source.GetTypeDefinition(typeHandle).GetGenericParameters().Select(genParmHandle => ((EntityHandle)typeHandle, genParmHandle)))
				.Union(_source.MethodDefinitions.SelectMany(methodHandle => _source.GetMethodDefinition(methodHandle).GetGenericParameters().Select(genParmHandle => ((EntityHandle)methodHandle, genParmHandle))))
				.OrderBy(x => CodedIndex.TypeOrMethodDef(x.Item1)).Select(x => (x.Item1, _source.GetGenericParameter(x.genParmHandle))).Process(x =>
					metadataBuilder.AddGenericParameter(x.Item1, x.Item2.Attributes, CopyString(x.Item2.Name), x.Item2.Index));
			_source.FieldDefinitions.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(handle => (handle, _source.GetFieldDefinition(handle))).Process(x =>
			{
				FieldDefinitionHandle fd =metadataBuilder.AddFieldDefinition(x.Item2.Attributes, CopyString(x.Item2.Name), CopySignature(x.Item2.Signature));
				if ((fd != x.handle)||(MetadataTokens.GetRowNumber(fd) != MetadataTokens.GetRowNumber(x.handle)))
					throw new InvalidCastException();
				ConstantHandle ch = x.Item2.GetDefaultValue();
				if (!ch.IsNil)
					metadataBuilder.AddConstant(fd, ReadConstant(_source.GetConstant(ch)));
			});
			ParameterHandle lastFirstParm= MetadataTokens.ParameterHandle(1);
			_source.MethodDefinitions.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(handle => (handle, _source.GetMethodDefinition(handle))).Process(x =>
			{
				int bodyOffset;
				if (x.Item2.RelativeVirtualAddress == 0)
					bodyOffset = -1;
				else
				{
					MethodBodyBlock mbb = _pEReader.GetMethodBody(x.Item2.RelativeVirtualAddress);
					StandaloneSignatureHandle localSignature = mbb.LocalSignature.IsNil
						? mbb.LocalSignature
						: metadataBuilder.AddStandaloneSignature(CopySignature(_source.GetStandaloneSignature(mbb.LocalSignature).Signature));
					byte[] ilBytes = mbb.GetILBytes();
					MethodBodyStreamEncoder.MethodBody mbb2 = methodBodies.AddMethodBody(ilBytes.Length, mbb.MaxStack, mbb.ExceptionRegions.Length, true, localSignature, mbb.LocalVariablesInitialized ? MethodBodyAttributes.InitLocals : MethodBodyAttributes.None);
					mbb.ExceptionRegions.Process(exr =>
						mbb2.ExceptionRegions.Add(exr.Kind, exr.TryOffset, exr.TryLength, exr.HandlerOffset, exr.HandlerLength, exr.CatchType, exr.FilterOffset)
					);

					TransferIl(ilBytes, new BlobWriter(mbb2.Instructions));
					bodyOffset = mbb2.Offset;
				}

				ParameterHandleCollection parms = x.Item2.GetParameters();
				if (parms.Count != 0)
					lastFirstParm = parms.First();

				MethodDefinitionHandle md = metadataBuilder.AddMethodDefinition(x.Item2.Attributes, x.Item2.ImplAttributes, CopyString(x.Item2.Name), CopySignature(x.Item2.Signature), bodyOffset, lastFirstParm);
				if (x.handle != md)
					throw new InvalidCastException();

				if (parms.Count != 0)
					lastFirstParm = MetadataTokens.ParameterHandle(MetadataTokens.GetRowNumber(parms.Last()) + 1);

				x.Item2.GetParameters().Select(pHandle => (pHandle, _source.GetParameter(pHandle))).Process(p =>
				{
					if (MetadataTokens.GetRowNumber(metadataBuilder.AddParameter(p.Item2.Attributes, CopyString(p.Item2.Name), p.Item2.SequenceNumber))!= MetadataTokens.GetRowNumber(p.pHandle))
						throw new InvalidCastException();
				});

				CopyEntityHandle(x.handle);
			});
			_source.PropertyDefinitions.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(handle => (handle, _source.GetPropertyDefinition(handle))).Process(x =>
			{
				PropertyDefinitionHandle pd = metadataBuilder.AddProperty(x.Item2.Attributes, CopyString(x.Item2.Name), CopySignature(x.Item2.Signature));
				if (pd != x.handle)
					throw new InvalidCastException();

				TypeDefinitionHandle td;
				if (firstProps.TryGetValue(pd,out td))
					metadataBuilder.AddPropertyMap(td, pd);

				PropertyAccessors propAcc = x.Item2.GetAccessors();
				metadataBuilder.AddMethodSemantics(pd, MethodSemanticsAttributes.Getter, propAcc.Getter);
				metadataBuilder.AddMethodSemantics(pd, MethodSemanticsAttributes.Setter, propAcc.Setter);
			});
			_source.LocalConstants.OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetLocalConstant(x)).Process(x =>
				metadataBuilder.AddLocalConstant(CopyString(x.Name), CopySignature(x.Signature))
			);
			#endregion main part - type+members
		}

		#region TransferIl
		void TransferIl(byte[] sourceIlstructionBytes, BlobWriter writer)
		{
			writer.WriteBytes(sourceIlstructionBytes);
			int sourceLength = sourceIlstructionBytes.Length;

			EntityHandle TransferParentHandle(EntityHandle entityHandle)
			{
				return entityHandle.Kind == HandleKind.TypeSpecification
					? _target.AddTypeSpecification(CopySignatureTypeSpecification(_source.GetTypeSpecification((TypeSpecificationHandle)entityHandle).Signature))
					: entityHandle;
			}

			EntityHandle TransferMemberReference(EntityHandle entityHandle)
			{
				MemberReference mr = _source.GetMemberReference((MemberReferenceHandle)entityHandle);
				return _target.AddMemberReference(TransferParentHandle(mr.Parent), CopyString(mr.Name), CopySignatureMemberReference(mr.Signature));
			}

			int offset = 0;
			while (offset < sourceLength)
			{
				OperandType operandType = InstructionOperandTypes.ReadOperandType(sourceIlstructionBytes, ref offset);

				switch (operandType)
				{
					case OperandType.InlineType:
					case OperandType.InlineTok:
						EntityHandle th =MetadataTokens.EntityHandle(ReadInt32(sourceIlstructionBytes, offset));
						if (th.Kind==HandleKind.TypeSpecification)
						{
							writer.Offset = offset;
							writer.WriteInt32(MetadataTokens.GetToken(_target.AddTypeSpecification(CopySignatureTypeSpecification(_source.GetTypeSpecification((TypeSpecificationHandle)th).Signature))));
						}
						else if (th.Kind == HandleKind.TypeDefinition)
						{
							//no need to re-write anything as copied TypeDefinition token is same as origin
						}
						else if (th.Kind == HandleKind.TypeReference)
						{
							//no need to copy anything as copied TypeReference token is same as origin
						}
						else
							throw new NotImplementedException();
						offset += 4;
						break;
					case OperandType.InlineMethod:
					case OperandType.InlineField:
						EntityHandle ent = MetadataTokens.EntityHandle(ReadInt32(sourceIlstructionBytes, offset));
						if (ent.Kind == HandleKind.MemberReference)
						{
							writer.Offset = offset;
							writer.WriteInt32(MetadataTokens.GetToken(TransferMemberReference(ent)));
						}
						else if (ent.Kind == HandleKind.MethodDefinition)
						{
							//no need to re-write anything as copied MethodDefinition token is same as origin
						}
						else if (ent.Kind == HandleKind.MethodSpecification)
						{
							MethodSpecification ms = _source.GetMethodSpecification((MethodSpecificationHandle)ent);
							EntityHandle parent;
							if (ms.Method.Kind == HandleKind.MemberReference)
								parent = TransferMemberReference(ms.Method);
							else if (ms.Method.Kind==HandleKind.MethodDefinition)
								//no need to copy anything as copied MethodDefinition token is same as origin
								parent = ms.Method;
							else
								throw new NotImplementedException();

							writer.Offset = offset;
							writer.WriteInt32(MetadataTokens.GetToken(_target.AddMethodSpecification(parent, CopySignatureMemberReference(ms.Signature))));
						}
						else if (ent.Kind == HandleKind.FieldDefinition)
						{
							//no need to re-write anything as copied FieldDefinition token is same as origin
						}
						else
							throw new NotImplementedException();

						offset += 4;
						break;
					case OperandType.InlineString:
						writer.Offset = offset;
						writer.WriteInt32(MetadataTokens.GetToken(_target.GetOrAddUserString(_source.GetUserString(MetadataTokens.UserStringHandle(ReadInt32(sourceIlstructionBytes, offset))))));

						offset += 4;
						break;
					case OperandType.InlineSig: // calli
					case OperandType.InlineBrTarget:
						offset += 4;
						break;
					case OperandType.InlineI:
					case OperandType.ShortInlineR:
						offset += 4;
						break;

					case OperandType.InlineSwitch:
						int argCount = ReadInt32(sourceIlstructionBytes, offset);
						// skip switch arguments count and arguments
						offset+=(argCount+1)*4;
						break;

					case OperandType.InlineI8:
					case OperandType.InlineR:
						offset += 8;
						break;

					case OperandType.InlineNone:
						break;

					case OperandType.InlineVar:
						offset += 2;
						break;

					case OperandType.ShortInlineBrTarget:
					case OperandType.ShortInlineI:
					case OperandType.ShortInlineVar:
						offset += 1;
						break;

					default:
						throw new InvalidDataException(); //operandType
				}
			}
		}

		static int ReadInt32(byte[] buffer, int pos)
		{
			return buffer[pos] | buffer[pos + 1] << 8 | buffer[pos + 2] << 16 | buffer[pos + 3] << 24;
		}
		#endregion TransferIl

		#region help copy methods
		StringHandle CopyString(StringHandle @string)
			=> @string.IsNil ? @string : _target.GetOrAddString(_source.GetString(@string));

		GuidHandle CopyGuid(GuidHandle guid)
			=> guid.IsNil ? guid : _target.GetOrAddGuid(_source.GetGuid(guid));

		EntityHandle CopyEntityHandle(EntityHandle entity)
		{
			/*_source.GetCustomAttributes(entity).OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetCustomAttribute(x)).Process(x =>
				_target.AddCustomAttribute(x.Parent, x.Constructor, CopyBlob(x.Value))
			);*/
			_source.GetCustomDebugInformation(entity).OrderBy(x => MetadataTokens.GetRowNumber(x)).Select(x => _source.GetCustomDebugInformation(x)).Process(x =>
				_target.AddCustomDebugInformation(x.Parent, CopyGuid(x.Kind), CopyBlob(x.Value))
			);

			return entity;
		}

		BlobHandle CopyBlob(BlobHandle blob)
			=> blob.IsNil ? blob : _target.GetOrAddBlob(_source.GetBlobBytes(blob));

		BlobHandle CopySignature(BlobHandle blob, Action<SignatureCopier> action)
		{
			BlobBuilder bb = new BlobBuilder();
			action(new SignatureCopier(_source.GetBlobReader(blob), bb, _target));
			return _target.GetOrAddBlob(bb);
		}

		BlobHandle CopySignature(BlobHandle blob)
		{
			return CopySignature(blob, x => x.Copy());
		}

		BlobHandle CopySignatureTypeSpecification(BlobHandle blob)
		{
			return CopySignature(blob, x => x.CopyTypeSpecification());
		}

		BlobHandle CopySignatureMemberReference(BlobHandle blob)
		{
			return CopySignature(blob, x => x.CopyMemberReference());
		}

		object ReadConstant(Constant constant)
		{
			if (constant.TypeCode== ConstantTypeCode.NullReference)
				return null;

			BlobReader br = _source.GetBlobReader(constant.Value);

			switch (constant.TypeCode)
			{
				case ConstantTypeCode.Boolean:
					return br.ReadBoolean();
				case ConstantTypeCode.Char:
					return br.ReadChar();
				case ConstantTypeCode.SByte:
					return br.ReadSByte();
				case ConstantTypeCode.Byte:
					return br.ReadByte();
				case ConstantTypeCode.Int16:
					return br.ReadInt16();
				case ConstantTypeCode.UInt16:
					return br.ReadUInt16();
				case ConstantTypeCode.Int32:
					return br.ReadInt32();
				case ConstantTypeCode.UInt32:
					return br.ReadUInt32();
				case ConstantTypeCode.Int64:
					return br.ReadInt64();
				case ConstantTypeCode.UInt64:
					return br.ReadUInt64();
				case ConstantTypeCode.Single:
					return br.ReadSingle();
				case ConstantTypeCode.Double:
					return br.ReadDouble();
				case ConstantTypeCode.String:
					//return br.ReadSerializedString();
					return br.ReadUTF16(br.RemainingBytes);
			}
			throw new NotImplementedException();			 
		}

		class SignatureCopier
		{
			BlobReader blobReader;
			BlobBuilder _target;
			MetadataBuilder _metadataBuilder;

			internal SignatureCopier(BlobReader source, BlobBuilder target, MetadataBuilder metadataBuilder)
			{
				blobReader = source;
				_target = target;
				_metadataBuilder = metadataBuilder;
			}

			internal void Copy()
			{
				SignatureHeader header = CopySignatureHeader();
				if (header.Kind == SignatureKind.Field)
					CopyFieldSignature();
				else if (header.Kind == SignatureKind.LocalVariables)
					CopyLocalVariableSignature();
				else if ((header.Kind == SignatureKind.Method) || (header.Kind == SignatureKind.Property))
					CopyMethodSignature(header);
				else if (header.Kind == SignatureKind.MethodSpecification)
					CopyMethodSpecificationSignature();
				else
					throw new NotImplementedException();
			}

			internal void CopyTypeSpecification()
			{
				CopyType(true);
			}

			internal void CopyMemberReference()
			{
				Copy();
			}

			void CopyFieldSignature()
			{
				CopyType(false);
			}

			void CopyMethodSignature(SignatureHeader header)
			{
				int genericParameterCount = header.IsGeneric? CopyCompressedInteger() : 0;
				int num = CopyCompressedInteger();
				CopyType(false); //return type
				int requiredParameterCount;
				if (num != 0)
				{
					int i;
					for (i = 0; i < num; i++)
					{
						int num2 = CopyCompressedInteger();
						if (num2 == 65)
							break;
						CopyType(false, num2);
					}
					requiredParameterCount = i;
					while (i < num)
					{
						CopyType(false);
						i++;
					}
				}
			}

			void CopyLocalVariableSignature()
			{
				CopyTypeSequence();
			}

			SignatureHeader CopySignatureHeader()
			{
				SignatureHeader header;
				WriteSignatureHeader(header = blobReader.ReadSignatureHeader());
				return header;
			}

			int CopyCompressedInteger()
			{
				int num;
				_target.WriteCompressedInteger(num = blobReader.ReadCompressedInteger());
				return num;
			}

			int CopyCompressedSignedInteger()
			{
				int num;
				_target.WriteCompressedSignedInteger(num = blobReader.ReadCompressedSignedInteger());
				return num;
			}

			void WriteSignatureHeader(SignatureHeader header)
			{
				_target.WriteByte(header.RawValue);
			}

			EntityHandle CopyType(bool allowTypeSpecifications = false)
			{
				return CopyType(allowTypeSpecifications, CopyCompressedInteger());
			}

			EntityHandle CopyType(bool allowTypeSpecifications, int typeCode)
			{
				switch (typeCode)
				{
					case 1:
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
					case 9:
					case 10:
					case 11:
					case 12:
					case 13:
					case 14:
					case 22:
					case 24:
					case 25:
					case 28:
						return MetadataTokens.GenericParameterHandle(typeCode+2000);
						//return this._provider.GetPrimitiveType((PrimitiveTypeCode)typeCode);
					case 15:
						return CopyType(false);
					case 16:
						return CopyType(false);
					case 17:
					case 18:
						return CopyTypeHandle((byte)typeCode, allowTypeSpecifications);
					case 19: //GetGenericTypeParameter
						return MetadataTokens.GenericParameterHandle(CopyCompressedInteger());
					case 20:
						CopyArrayType();
						break;
					case 21:
						return CopyGenericTypeInstance();
					case 23:
					case 26:
						break;
					case 27:
						Copy();
						break;
					case 29:
						return CopyType(false);
					case 30: //GetGenericMethodParameter
						return MetadataTokens.GenericParameterHandle(CopyCompressedInteger()+1000);
					case 31:
						CopyModifiedType(true);
						break;
					case 32:
						CopyModifiedType(false);
						break;
					case 69:
						CopyType(false);
						break;
					default:
						throw new BadImageFormatException("UnexpectedSignatureTypeCode: " + typeCode);
				}

				return default(EntityHandle);
			}

			EntityHandle[] CopyTypeSequence()
			{
				int num = CopyCompressedInteger();
				if (num == 0)
					throw new BadImageFormatException("SignatureTypeSequenceMustHaveAtLeastOneElement");
				EntityHandle[] result = new EntityHandle[num];
				for (int i = 0; i < num; i++)
					result[i]=CopyType(false);
				return result;
			}

			void CopyArrayType()
			{
				CopyType(false);
				CopyCompressedInteger();
				int num = CopyCompressedInteger();
				if (num > 0)
					for (int i = 0; i < num; i++)
						CopyCompressedInteger();
				num = CopyCompressedInteger();
				if (num > 0)
					for (int j = 0; j < num; j++)
						blobReader.ReadCompressedSignedInteger();
			}

			void CopyMethodSpecificationSignature()
			{
				CopyTypeSequence();
			}

			EntityHandle CopyGenericTypeInstance()
			{
				EntityHandle genTypeDef = CopyType(false);
				EntityHandle[] genParms = CopyTypeSequence();
				BlobEncoder be = new BlobEncoder(new BlobBuilder());
				SignatureTypeEncoder ste = be.TypeSpecificationSignature();
				ste.Type(genTypeDef, false);
				genParms.Process(x=> 
				{
					if (x.Kind!=HandleKind.GenericParameter && x.IsNil)
						throw new NotImplementedException();
					int index = MetadataTokens.GetRowNumber(x);
					if (index > 1999)
						ste.PrimitiveType((PrimitiveTypeCode)(index-2000));
					else if (index > 999)
						ste.GenericMethodTypeParameter(index-1000);
					else
						ste.GenericTypeParameter(index);
				});
				return _metadataBuilder.AddTypeSpecification(_metadataBuilder.GetOrAddBlob(be.Builder));
			}

			void CopyModifiedType(bool isRequired)
			{
				CopyTypeHandle(0, true);
				CopyType(false);
			}

			EntityHandle CopyTypeHandle(byte rawTypeKind, bool allowTypeSpecifications)
			{
				EntityHandle handle = blobReader.ReadTypeHandle();
				if (handle.IsNil)
					throw new BadImageFormatException("NotTypeDefOrRefOrSpecHandle");

				HandleKind kind = handle.Kind;

				if (kind == HandleKind.TypeReference)
					_target.WriteCompressedInteger(CodedIndex.TypeDefOrRef(handle));
				else if (kind == HandleKind.TypeDefinition)
					_target.WriteCompressedInteger(CodedIndex.TypeDefOrRef(handle));
				else if (kind == HandleKind.TypeSpecification)
				{
					if (!allowTypeSpecifications)
						throw new BadImageFormatException("NotTypeDefOrRefHandle");
					throw new NotImplementedException();
				}
				else
					throw new NotImplementedException();

				return handle;
			}
		}
		#endregion help copy methods
	}

	#region IEnumerationExtensions
	static class IEnumerationExtensions
	{
		internal static void Process<T>(this IEnumerable<T> list, Action<T> action)
		{
			foreach (T item in list)
				action(item);
		}
	}
	#endregion IEnumerationExtensions
}

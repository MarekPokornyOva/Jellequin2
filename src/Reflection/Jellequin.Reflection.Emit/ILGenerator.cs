﻿#region using
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class ILGenerator
	{
		readonly MethodBuilderBase _methodBuilder;
		public List<Instruction> Instructions { get; } = new List<Instruction>();
		public List<ExceptionRegionInfo> Exceptions { get; } = new List<ExceptionRegionInfo>();

		int _stack;
		public int Stack { get { return _stack; } private set { if (ValidateStack && value<0) throw new ReflectionException(ReflectionExceptionReason.WrongCallStackGeneration); _stack=value; } }

		internal ILGenerator(MethodBuilderBase methodBuilder)
		{
			_methodBuilder=methodBuilder;
		}

		public bool ValidateStack { get; set; }

		(string DocumentName, int StartLineNumber, ushort StartColumn, int EndLineNumber, ushort EndColumn) _noteSeqPoint = default;
		public void MarkSequencePoint(SymbolWriter symbolWriter,int startLineNumber,ushort startColumn,int endLineNumber,ushort endColumn)
		{
			if (symbolWriter!=null)
				_noteSeqPoint=(symbolWriter.DocumentName, startLineNumber, startColumn, endLineNumber, endColumn);
		}

		public void Emit(ILOpCode opCode)
		{
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction(opCode)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,ConstructorInfo constructorInfo)
		{
			if (constructorInfo==null)
				throw new ArgumentNullException(nameof(constructorInfo));
			Stack-=constructorInfo.GetParameters().Length;
			Instructions.Add(FulfillInstruction(new Instruction<ConstructorInfo>(opCode,constructorInfo)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,MethodInfo methodInfo)
		{
			if (methodInfo==null)
				throw new ArgumentNullException(nameof(methodInfo));
			Stack-=methodInfo.GetParameters().Length;
			if (!methodInfo.IsStatic)
				Stack--;
			Instructions.Add(FulfillInstruction(new Instruction<MethodInfo>(opCode,methodInfo)));
			if (methodInfo.ReturnType!=typeof(void))
				Stack++;
		}

		public void Emit(ILOpCode opCode,int value)
		{
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<int>(opCode,value)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,double value)
		{
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<double>(opCode,value)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,string value)
		{
			if (value==null)
				throw new ArgumentNullException(nameof(value));
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<string>(opCode,value)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,FieldInfo fieldInfo)
		{
			if (fieldInfo==null)
				throw new ArgumentNullException(nameof(fieldInfo));

			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<FieldInfo>(opCode,fieldInfo)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,Type type)
		{
			if (type==null)
				throw new ArgumentNullException(nameof(type));
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<Type>(opCode,type)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,LocalBuilder localBuilder)
		{
			if (localBuilder==null)
				throw new ArgumentNullException(nameof(localBuilder));
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<LocalBuilder>(opCode,localBuilder)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,Label label)
		{
			if (label==null)
				throw new ArgumentNullException(nameof(label));
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<Label>(opCode,label)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public void Emit(ILOpCode opCode,Label[] labels)
		{
			if (labels==null)
				throw new ArgumentNullException(nameof(labels));
			Stack-=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPopCount(opCode);
			Instructions.Add(FulfillInstruction(new Instruction<Label[]>(opCode,labels)));
			Stack+=Microsoft.CodeAnalysis.CodeGen.ILOpCodeExtensions.StackPushCount(opCode);
		}

		public LocalBuilder DeclareLocal(Type type,bool isPinned)
		{
			if (type==null)
				throw new ArgumentNullException(nameof(type));
			LocalBuilder result = new LocalBuilder(type,Locals.Count,isPinned);
			Locals.Add(result);
			return result;
		}

		public Label DefineLabel()
		{
			Label result = new Label();
			Labels.Add(result);
			return result;
		}

		List<Label> _notedLabels = new List<Label>();
		public void MarkLabel(Label label)
		{
			if (label==null)
				throw new ArgumentNullException(nameof(label));
			_notedLabels.Add(label);
		}

		Instruction FulfillInstruction(Instruction instruction)
		{
			instruction.SourceLocation=_noteSeqPoint;
			_noteSeqPoint=default;
			instruction.Labels.AddRange(_notedLabels);
			_notedLabels.Clear();
			return instruction;
		}

		public ExceptionRegionInfo AddCatchRegion(Type exceptionType)
		{
			if (exceptionType==null)
				throw new ArgumentNullException(nameof(exceptionType));
			ExceptionRegionInfo result;
			Exceptions.Add(result=new ExceptionRegionInfo(this,true,exceptionType));
			return result;
		}

		public ExceptionRegionInfo AddFinallyRegion()
		{
			ExceptionRegionInfo result;
			Exceptions.Add(result=new ExceptionRegionInfo(this,false,null));
			return result;
		}

		public IList<LocalBuilder> Locals { get; private set; } = new List<LocalBuilder>();
		public List<Label> Labels { get; private set; } = new List<Label>();

		public void PopBranch()
		{
			Stack--;
		}

		Stack<ExceptionRegionInfo> _exceptionRegionInfo = new Stack<ExceptionRegionInfo>();
		public void BeginExceptionBlock() //try
		{
			ExceptionRegionInfo eri = new ExceptionRegionInfo(this);
			_exceptionRegionInfo.Push(Exceptions.AddWithReturn(eri));
			MarkLabel(eri.TryStart);
		}

		public void BeginCatchBlock(Type exceptionType) //catch
		{
			_exceptionRegionInfo.Peek().MarkCatchStart(exceptionType);
		}

		public void BeginFinallyBlock() //finally
		{
			_exceptionRegionInfo.Peek().MarkFinallyStart();
		}

		public void EndExceptionBlock() //end of catch/finally
		{
			_exceptionRegionInfo.Pop().MarkHandleEnd();
		}
	}
}

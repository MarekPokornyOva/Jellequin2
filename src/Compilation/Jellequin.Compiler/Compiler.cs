#region using
using Jellequin.Reflection.Emit;
using Jellequin.Runtime;
using Jellequin.Runtime.Diagnostics;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
#endregion using

namespace Jellequin.Compiler
{
	public class Compiler
	{
		#region public
		readonly static CodeSettings _parseSettings = new CodeSettings { ReorderScopeDeclarations=false, StripDebugStatements=false, LocalRenaming=LocalRenaming.KeepAll, PreserveFunctionNames=true, RemoveFunctionExpressionNames=false };
		public static void Compile(ISource code, Stream assembly, AssemblyName assemblyName, CompilerOptions options)
		{
			Block block = new JSParser().Parse(code.GetText(), _parseSettings);
			//string s = OutNode(block);
			new Compiler().CompileInt(code, block, assembly, assemblyName, options);
		}

		/*public static string OutNode(AstNode node)
		{
			return OutputVisitor.Apply(node, _parseSettings);
		}*/
		#endregion public

		#region fields
		ModuleBuilder _modBldr;
		SymbolWriter _symbolWriter;
		readonly static Type _objectType = typeof(object);
		bool _debug;
		bool _useDynamicJsMembers;
		Stack<FunctionInfo> _functions = new Stack<FunctionInfo>();
		int _anonymousCounter;
		RuntimeMethodsUsage _runtimeMethodsUsage;
		#endregion fields

		#region root compile
		internal void CompileInt(ISource code, Block block, Stream assembly, AssemblyName assemblyName, CompilerOptions options)
		{
			DebugOptions debugOptions = options.Debug ?? new DebugOptions();
			_debug=debugOptions.Debug;
			_runtimeMethodsUsage=options.RuntimeMethodsUsage;
			_useDynamicJsMembers=!options.DontUseDynamicJsMembers;

			_modBldr=ModuleBuilder.Create(assemblyName);
			if (_debug)
				_symbolWriter=_modBldr.DefineDocument(assemblyName.Name+".jell"/*, SymDocumentType.Text, SymLanguageType.JScript, Guid.Empty*/);

			TypeBuilder typBldr = _modBldr.DefineType("Root", TypeAttributes.BeforeFieldInit|TypeAttributes.Sealed|TypeAttributes.Public, GetBaseType(true));
			AddDebuggerTypeProxyAttribute(typBldr);
			typBldr.DefineField("~externalLibraryResolver", typeof(Action<>).MakeGenericType(GetRuntimeType(typeof(ResolveExternalLibraryEventArgs))), FieldAttributes.Private);
			GenerateRuntimeInfo();

			MethodBuilder m = typBldr.DefineMethod("Main", MethodAttributes.Public, typeof(void), new IType[] { typeof(object[]) });
			m.DefineParameter(1, ParameterAttributes.None, "arguments");
			ILGenerator gen = m.GetILGenerator();
			if (_debug)
				gen.MarkSequencePoint(_symbolWriter, block.Context.StartLineNumber, block.Context.StartColumn, block.Context.EndLineNumber, block.Context.EndColumn);
			Label endLabel;
			FunctionInfo funInfo = new FunctionInfo { TypBldr=typBldr, Gen=gen, Args = new[] { "arguments" }, EndLabel = (endLabel=gen.DefineLabel()) };
			_functions.Push(funInfo);
			CompileNode(block);
			FinalizeFunctionPushes(funInfo.Stack);
			gen.MarkLabel(endLabel);
			gen.MarkSequencePoint(_symbolWriter, block.Context.EndLineNumber, block.Context.EndColumn, block.Context.EndLineNumber, block.Context.EndColumn+1);
			gen.Emit(ILOpCode.Ret);

			ConstructorBuilder cb = typBldr.DefineConstructor(MethodAttributes.Public|MethodAttributes.RTSpecialName|MethodAttributes.SpecialName|MethodAttributes.HideBySig, CallingConventions.Standard, new IType[0]);
			GenerateConstructorBaseCall(cb);
			gen=cb.GetILGenerator();
			gen.Emit(ILOpCode.Ret);

			/*
			.NET FW compatibility
			TypeBuilder[] funcs = _createdFunctions.ToArray();
			Array.Reverse(funcs);
			foreach (TypeBuilder item in funcs)
				item.CreateType();*/

			MethodBuilder entryPoint = GenerateStaticExecutor(typBldr, options.FileKind);
			typBldr.CreateType();

			Stream runtimeAsm=null;
			try
			{
				runtimeAsm=_runtimeMethodsUsage==RuntimeMethodsUsage.Copy?File.OpenRead(typeof(Jellequin.Runtime.IJsObject).Assembly.Location):null;
				new AssemblyWriter().Write(_modBldr, debugOptions, code, entryPoint, assembly, runtimeAsm, options.Icon);
			}
			finally
			{
				if (runtimeAsm!=null)
					runtimeAsm.Dispose();
			}
		}

		Type GetBaseType() => GetBaseType(false);

		Type GetBaseType(bool forRoot)
		{
			//TODO: reflect options.BaseType
			return GetRuntimeType(_useDynamicJsMembers ? typeof(DynamicJsObject) : typeof(StaticJsObject));
		}

		ConstructorBuilder DefineSimpleConstructor(TypeBuilder typBldr)
		{
			ConstructorBuilder cb = typBldr.DefineConstructor(MethodAttributes.Public|MethodAttributes.RTSpecialName|MethodAttributes.SpecialName|MethodAttributes.HideBySig, CallingConventions.Standard, new IType[0]);
			ILGenerator gen = cb.GetILGenerator();
			GenerateConstructorBaseCall(cb);
			gen.Emit(ILOpCode.Ret);
			return cb;
		}

		void GenerateConstructorBaseCall(ConstructorBuilder cb)
		{
			ILGenerator gen = cb.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Call, ((NetType)cb.DeclaringType.BaseType).Type.GetConstructor(Type.EmptyTypes));
		}

		void GenerateRuntimeInfo()
		{
			Assembly asm = typeof(Jellequin.Runtime.IJsObject).Assembly;
			TypeBuilder typBldr = _modBldr.DefineType("CompileRuntimeInfo", TypeAttributes.BeforeFieldInit|TypeAttributes.Sealed|TypeAttributes.Public, _objectType);
			FieldBuilder fb = typBldr.DefineField("Version", typeof(string), FieldAttributes.Public|FieldAttributes.Literal|FieldAttributes.HasDefault);
			fb.SetConstant(asm.GetName().Version.ToString());
			fb=typBldr.DefineField("ModuleVersionId", typeof(string), FieldAttributes.Public|FieldAttributes.Literal|FieldAttributes.HasDefault);
			fb.SetConstant(asm.ManifestModule.ModuleVersionId.ToString());
			fb=typBldr.DefineField("RuntimeCopied", typeof(bool), FieldAttributes.Public|FieldAttributes.Literal|FieldAttributes.HasDefault);
			fb.SetConstant(_runtimeMethodsUsage==RuntimeMethodsUsage.Copy);
			typBldr.CreateType();
		}

		MethodBuilder GenerateStaticExecutor(TypeBuilder typBldr, FileKind fileKind)
		{
			if (fileKind==FileKind.Dll)
				return null;

			MethodBuilder m = typBldr.DefineMethod("~~Entry", MethodAttributes.Public|MethodAttributes.Static, typeof(void), new IType[] { typeof(string[]) });
			m.DefineParameter(1,ParameterAttributes.None,"arguments");
			ILGenerator gen = m.GetILGenerator();
			gen.Emit(ILOpCode.Call, typeof(Assembly).GetMethod("GetExecutingAssembly", BindingFlags.Public|BindingFlags.Static));
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Call, GetRuntimeMethod(typeof(Executor).GetMethod("RunExe", BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static, null, new Type[] { typeof(Assembly), typeof(string[]) }, null)));
			gen.Emit(ILOpCode.Pop);
			gen.Emit(ILOpCode.Ret);
			return m;
		}
		#endregion root compile

		#region compile node
		void CompileNode(AstNode node)
		{
			if (node is Var)
				CompileVar((Var)node);
			else if (node is VariableDeclaration)
				CompileVariableDeclaration((VariableDeclaration)node);
			else if (node is ConstantWrapper)
				CompileConstantWrapper((ConstantWrapper)node);
			else if (node is CommaOperator)
				CompileCommaOperator((CommaOperator)node);
			else if (node is BinaryOperator)
				CompileBinaryOperator((BinaryOperator)node);
			else if (node is UnaryOperator)
				CompileUnaryOperator((UnaryOperator)node);
			else if (node is Lookup)
				CompileLookup((Lookup)node);
			else if (node is ForNode)
				CompileForNode((ForNode)node);
			else if (node is ForIn)
				CompileForIn((ForIn)node);
			else if (node is Break)
				CompileBreak((Break)node);
			else if (node is ContinueNode)
				CompileContinueNode((ContinueNode)node);
			else if (node is WhileNode)
				CompileWhileNode((WhileNode)node);
			else if (node is DoWhile)
				CompileDoWhile((DoWhile)node);
			else if (node is Block)
				CompileBlock((Block)node);
			else if (node is AstNodeList)
				CompileAstNodeList((AstNodeList)node);
			else if (node is FunctionObject)
				CompileFunctionObject((FunctionObject)node);
			else if (node is ReturnNode)
				CompileReturnNode((ReturnNode)node);
			else if (node is CallNode)
				CompileCallNode((CallNode)node);
			else if (node is ObjectLiteral)
				CompileObjectLiteral((ObjectLiteral)node);
			else if (node is Member)
				CompileMember((Member)node);
			else if (node is IfNode)
				CompileIf((IfNode)node);
			else if (node is Conditional)
				CompileConditional((Conditional)node);
			else if (node is Microsoft.Ajax.Utilities.Switch)
				CompileSwitch((Microsoft.Ajax.Utilities.Switch)node);
			else if (node is ImportNode)
				CompileImportNode((ImportNode)node);
			else if (node is ArrayLiteral)
				CompileArrayLiteral((ArrayLiteral)node);
			else if (node is DebuggerNode)
				CompileDebuggerNode((DebuggerNode)node);
			else if (node is ImportantComment)
				CompileImportantComment((ImportantComment)node);
			else if (node is ThrowNode)
				CompileThrow((ThrowNode)node);
			else if (node is ThisLiteral)
				CompileThisLiteral((ThisLiteral)node);
			else if (node is RegExpLiteral)
				CompileRegExpLiteral((RegExpLiteral)node);
			else if (node is GroupingOperator)
				CompileGroupingOperator((GroupingOperator)node);
			else if (node is LexicalDeclaration)
				CompileLexicalDeclaration((LexicalDeclaration)node);
			else if (node is TryNode)
				CompileTryNode((TryNode)node);
			else if (node is TemplateLiteral)
				CompileTemplateLiteral((TemplateLiteral)node);
			else
				throw new NotImplementedException();
		}
		
		void CompileBlock(Block node)
		{
			ModuleDeclaration moduleDeclarationNode=null;
			foreach (AstNode item in node.Children)
			{
				MarkSequencePoint(item);
				if (item is ModuleDeclaration itemMD)
					moduleDeclarationNode=itemMD;
				else
				{
					if ((item is BinaryOperator bo)&&(moduleDeclarationNode!=null)&&(bo.Operand1 is Member m)&&(m.Root==null))
						m.Root=new Lookup(moduleDeclarationNode.Context) { Name="module" };
					CompileNodeWithPopToBase(item);
					moduleDeclarationNode=null;
				}
			}
			MarkSequencePointEnd(node);
		}

		void CompileCommaOperator(CommaOperator node)
		{
			MarkSequencePoint(node);
			CompileNodeWithPopToBase(node.Operand1);
			MarkSequencePoint(node);
			CompileNode(node.Operand2);
		}

		void CompileAstNodeList(AstNodeList node)
		{
			if (node.Parent is CallNode)
			{
				foreach (AstNode item in node.Children)
					CompileNode(item);
				return;
			}

			StackInfo stackInfo = GetStackInfo();
			int childrenLen = node.Children.Count();
			int a = 0;
			foreach (AstNode item in node.Children)
			{
				MarkSequencePoint(item);
				CompileNode(item);
				if (++a!=childrenLen)
					PopToStackInfo(stackInfo);
			}
		}

		void CompileVar(Var node)
		{
			foreach (VariableDeclaration varNode in node.Children)
				CompileNode(varNode);
		}

		void CompileVariableDeclaration(VariableDeclaration node)
		{
			BindingIdentifier varName;
			if (node.Binding is ObjectLiteral)
			{
				BindingIdentifier Dig(AstNode x)
				{
					AstNode child=x.Children.FirstOrDefault();
					return child==null ? null : child is BindingIdentifier res ? res : Dig(child);
				}

				varName=Dig(node.Binding);
			}
			else
				varName = (BindingIdentifier)node.Binding;

			if (_useDynamicJsMembers)
			{
				if (node.Initializer==null)
				{
					FunctionInfo funInfo = _functions.Peek();
					PreSetVar(varName);
					funInfo.Gen.Emit(ILOpCode.Ldnull);
					funInfo.Stack++;
					SetVar(varName);
				}
			}
			else
				DefineVar(varName.Name);

			if (node.Initializer!=null)
			{
				MarkSequencePoint(node);
				PreSetVar(varName);
				CompileNode(node.Initializer);
				SetVar(varName);
			}
		}

		void CompileLexicalDeclaration(LexicalDeclaration node)
		{
			foreach (AstNode child in node.Children)
				CompileNode(child);
		}

		void CompileThisLiteral(ThisLiteral node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;
			funInfo.Gen.Emit(ILOpCode.Ldarg_0);
			funInfo.Gen.Emit(ILOpCode.Ldfld, funInfo.ThisField);
			funInfo.Stack++;
		}

		void CompileConstantWrapper(ConstantWrapper node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;
			object val = node.Value;

			if (val==null)
				gen.Emit(ILOpCode.Ldnull);
			else
			{
				Type type = val.GetType();
				if (type==typeof(int))
					gen.Emit(ILOpCode.Ldc_i4, (int)val);
				else if (type==typeof(double))
					if (node.IsIntegerLiteral)
						gen.Emit(ILOpCode.Ldc_i4, (int)Convert.ChangeType(val, type=typeof(int)));
					else
						gen.Emit(ILOpCode.Ldc_r8, (double)val);
				else if (type==typeof(bool))
					gen.Emit((bool)val ? ILOpCode.Ldc_i4_1 : ILOpCode.Ldc_i4_0);
				else if (type==typeof(string))
				{
					string valStr = (string)val;
					if ((node.Parent is Block)&&(valStr=="use strict"))
						return;
					gen.Emit(ILOpCode.Ldstr, valStr);
				}
				else if (type==typeof(Microsoft.Ajax.Utilities.StringList))
				{
					gen.Emit(ILOpCode.Ldstr, ((Microsoft.Ajax.Utilities.StringList)val).ToString());
					type=typeof(string);
				}
				else
					throw new NotImplementedException();
				gen.Emit(ILOpCode.Box, type);
			}

			funInfo.Stack++;
		}

		void CompileBinaryOperator(BinaryOperator node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			if (node.OperatorToken==JSToken.Assign)
			{
				if ((node.Operand1 is Lookup)||(node.Operand1 is Member))
				{
					PreSetVar(node.Operand1);
					CompileNode(node.Operand2);
					LocalBuilder lb = null;
					if (node.IsExpression)
					{
						lb=gen.DeclareLocal(_objectType);
						gen.Emit(ILOpCode.Dup);
						gen.Emit(ILOpCode.Stloc, lb);
					}
					SetVar(node.Operand1);
					if (node.IsExpression)
					{
						gen.Emit(ILOpCode.Ldloc, lb);
						funInfo.Stack++;
					}
				}
				else if (node.Operand1 is CallNode operAsCall)
				{
					CompileNode(operAsCall.Function);
					AstNode argNode = operAsCall.Arguments[0];
					CompileNode(argNode);
					CompileNode(node.Operand2);
					LocalBuilder lb = null;
					if (node.IsExpression)
					{
						lb=gen.DeclareLocal(_objectType);
						gen.Emit(ILOpCode.Dup);
						gen.Emit(ILOpCode.Stloc, lb);
					}
					CallRuntimeMethod(operAsCall.InBrackets ? nameof(RuntimeMethods.SetArrayItem) : nameof(RuntimeMethods.SetMember));
					funInfo.Stack-=3;
					if (node.IsExpression)
					{
						gen.Emit(ILOpCode.Ldloc, lb);
						funInfo.Stack++;
					}
				}
				else
					throw new NotImplementedException();
			}
			else if ((node.OperatorToken==JSToken.BitwiseAndAssign)||(node.OperatorToken==JSToken.BitwiseOrAssign))
			{
				MarkSequencePoint(node);

				PreSetVar(node.Operand1);
				CompileNode(node.Operand1);
				CompileNode(node.Operand2);
				CallRuntimeMethod(node.OperatorToken==JSToken.BitwiseAndAssign ? nameof(RuntimeMethods.BitwiseAnd) : nameof(RuntimeMethods.BitwiseOr));
				funInfo.Stack--;
				LocalBuilder lb=null;
				if (node.IsExpression)
				{
					lb=gen.DeclareLocal(_objectType);
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Stloc,lb);
				}
				SetVar(node.Operand1);
				if (node.IsExpression)
				{
					gen.Emit(ILOpCode.Ldloc,lb);
					funInfo.Stack++;
				}
			}
			else if (node.Precedence==OperatorPrecedence.Equality)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(nameof(RuntimeMethods.Equals));
				if (node.OperatorToken==JSToken.NotEqual)
				{
					gen.Emit(ILOpCode.Ldc_i4_0);
					gen.Emit(ILOpCode.Ceq);
				}
				gen.Emit(ILOpCode.Box, typeof(bool));
				funInfo.Stack--;
			}
			else if (node.OperatorToken==JSToken.Minus)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(nameof(RuntimeMethods.Sub));
				funInfo.Stack--;
			}
			else if (node.OperatorToken==JSToken.FirstBinaryOperator)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(nameof(RuntimeMethods.Add));
				funInfo.Stack--;
			}
			else if ((node.OperatorToken==JSToken.PlusAssign)||(node.OperatorToken==JSToken.MinusAssign))
			{
				MarkSequencePoint(node);

				PreSetVar(node.Operand1);
				CompileNode(node.Operand1);
				CompileNode(node.Operand2);
				CallRuntimeMethod(node.OperatorToken==JSToken.PlusAssign?nameof(RuntimeMethods.Add):nameof(RuntimeMethods.Sub));
				funInfo.Stack--;
				LocalBuilder lb = null;
				if (node.IsExpression)
				{
					lb=gen.DeclareLocal(_objectType);
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Stloc,lb);
				}
				SetVar(node.Operand1);
				if (node.IsExpression)
				{
					gen.Emit(ILOpCode.Ldloc,lb);
					funInfo.Stack++;
				}
			}
			else if (node.OperatorToken==JSToken.Multiply)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(nameof(RuntimeMethods.Mul));
				funInfo.Stack--;
			}
			else if (node.OperatorToken==JSToken.Divide)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(nameof(RuntimeMethods.Div));
				funInfo.Stack--;
			}
			else if (node.OperatorToken==JSToken.Modulo)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(nameof(RuntimeMethods.Modulo));
				funInfo.Stack--;
			}
			else if (node.OperatorToken==JSToken.LogicalAnd)
				CompileLogicalInternal(node, ILOpCode.Brfalse, ILOpCode.Ldc_i4_0);
			else if (node.OperatorToken==JSToken.LogicalOr)
				CompileLogicalInternal(node, ILOpCode.Brtrue, ILOpCode.Ldc_i4_1);
			else if (node.Precedence==OperatorPrecedence.Relational)
			{
				MarkSequencePoint(node);

				if (node.OperatorToken==JSToken.In)
				{
					CompileNode(node.Operand2);
					CompileNode(node.Operand1);
					CallRuntimeMethod(nameof(RuntimeMethods.HasMember));
					funInfo.Stack--;
				}
				else if (node.OperatorToken==JSToken.InstanceOf)
				{
					CompileNode(node.Operand1);
					CompileNode(node.Operand2);
					CallRuntimeMethod(nameof(RuntimeMethods.InstanceOf));
					funInfo.Stack--;
				}
				else
				{
					CompileNode(node.Operand1);
					CompileNode(node.Operand2);

					bool compareToM1 = (node.OperatorToken==JSToken.LessThan)||(node.OperatorToken==JSToken.GreaterThanEqual);
					bool negate = (node.OperatorToken==JSToken.LessThanEqual)||(node.OperatorToken==JSToken.GreaterThanEqual);
					if ((!compareToM1)&&(!negate)&&(node.OperatorToken!=JSToken.GreaterThan))
						throw new NotImplementedException(); //shouldn't happen

					CallRuntimeMethod(nameof(RuntimeMethods.Compare));
					gen.Emit(compareToM1 ? ILOpCode.Ldc_i4_m1 : ILOpCode.Ldc_i4_1);
					gen.Emit(ILOpCode.Ceq);
					if (negate)
					{
						gen.Emit(ILOpCode.Ldc_i4_0);
						gen.Emit(ILOpCode.Ceq);
					}
					gen.Emit(ILOpCode.Box, typeof(bool));
					funInfo.Stack--;
				}
			}
			else if ((node.Precedence==OperatorPrecedence.BitwiseAnd)||(node.Precedence==OperatorPrecedence.BitwiseOr))
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(node.Precedence==OperatorPrecedence.BitwiseAnd ? nameof(RuntimeMethods.BitwiseAnd) : nameof(RuntimeMethods.BitwiseOr));
				funInfo.Stack--;
			}
			else if ((node.OperatorToken==JSToken.RightShift)||(node.OperatorToken==JSToken.LeftShift))//if (node.Precedence == OperatorPrecedence.Shift)
			{
				MarkSequencePoint(node);

				CompileNode(node.Operand1);
				CompileNode(node.Operand2);

				CallRuntimeMethod(node.OperatorToken==JSToken.RightShift ? nameof(RuntimeMethods.ShiftRight) : nameof(RuntimeMethods.ShiftLeft));
				funInfo.Stack--;
			}
			else if ((node.OperatorToken==JSToken.RightShiftAssign)||(node.OperatorToken==JSToken.LeftShiftAssign))
			{
				//n >>= 1
				MarkSequencePoint(node);

				PreSetVar(node.Operand1);
				CompileNode(node.Operand1);
				CompileNode(node.Operand2);
				CallRuntimeMethod(node.OperatorToken==JSToken.RightShiftAssign ? nameof(RuntimeMethods.ShiftRight) : nameof(RuntimeMethods.ShiftLeft));
				if (node.IsExpression)
				{
					gen.Emit(ILOpCode.Dup);
					funInfo.Stack++;
				}
				funInfo.Stack--;
				SetVar(node.Operand1);
			}
			else
				throw new NotImplementedException();
		}

		void CompileLogicalInternal(BinaryOperator node, ILOpCode jumpInstruction, ILOpCode jumpValueInstruction)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			//bool trueIf = (node.Parent as Conditional)?.QuestionContext == null;

			Label lJump = gen.DefineLabel();
			Label lEnd = gen.DefineLabel();

			CompileNode(node.Operand1);
			CallRuntimeMethod(nameof(RuntimeMethods.EvalToBool));
			gen.Emit(ILOpCode.Unbox_any, typeof(bool));
			gen.Emit(jumpInstruction, lJump);
			funInfo.Stack--;

			CompileNode(node.Operand2);
			gen.Emit(ILOpCode.Br, lEnd);

			gen.MarkLabel(lJump);
			gen.Emit(jumpValueInstruction);
			gen.Emit(ILOpCode.Box, typeof(bool));
			gen.MarkLabel(lEnd);
		}

		void CompileUnaryOperator(UnaryOperator node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			bool tempBool;
			if ((tempBool=node.OperatorToken==JSToken.Increment)||(node.OperatorToken==JSToken.Decrement))
			{
				MarkSequencePoint(node);
				string opName = tempBool ? nameof(RuntimeMethods.Add) : nameof(RuntimeMethods.Sub);
				AstNode varName = node.Operand;
				if (node.IsPostfix)
				{
					LocalBuilder lb = gen.DeclareLocal(_objectType);
					PreSetVar(varName);
					GetVar(varName);
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Stloc, lb);

					gen.Emit(ILOpCode.Ldc_i4_1);
					gen.Emit(ILOpCode.Box, typeof(int));
					CallRuntimeMethod(opName);

					SetVar(varName);

					gen.Emit(ILOpCode.Ldloc, lb);
					funInfo.Stack++;
				}
				else
				{
					PreSetVar(varName);
					GetVar(varName);

					gen.Emit(ILOpCode.Ldc_i4_1);
					gen.Emit(ILOpCode.Box, typeof(int));
					CallRuntimeMethod(opName);

					LocalBuilder lb = gen.DeclareLocal(_objectType);
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Stloc,lb);

					SetVar(varName);
					gen.Emit(ILOpCode.Ldloc,lb);
				}
			}
			else if (node.OperatorToken==JSToken.Minus)
			{
				MarkSequencePoint(node);

				gen.Emit(ILOpCode.Ldc_i4_m1);
				gen.Emit(ILOpCode.Box, typeof(int));
				CompileNode(node.Operand);

				CallRuntimeMethod(nameof(RuntimeMethods.Mul));
			}
			else if (node.OperatorToken==JSToken.FirstBinaryOperator) //unknown reason of this; however tests have shown noop
			{
				MarkSequencePoint(node);
				CompileNode(node.Operand);
			}
			else if (node.OperatorToken==JSToken.LogicalNot)
			{
				MarkSequencePoint(node);
				CompileNode(node.Operand);
				CallRuntimeMethod(nameof(RuntimeMethods.Not));
			}
			else if (node.OperatorToken==JSToken.BitwiseNot)
			{
				MarkSequencePoint(node);
				CompileNode(node.Operand);
				CallRuntimeMethod(nameof(RuntimeMethods.BitwiseNot));
			}
			else if (node.OperatorToken==JSToken.TypeOf)
			{
				MarkSequencePoint(node);
				Label lJump = gen.DefineLabel();
				Label lEnd = gen.DefineLabel();
				LocalBuilder temp = gen.DeclareLocal(_objectType);

				if (node.Operand is Lookup l)
				{
					gen.Emit(ILOpCode.Ldarg_0);
					gen.Emit(ILOpCode.Ldstr, l.Name);
					gen.Emit(ILOpCode.Callvirt, GetRuntimeType(typeof(IJsObject)).GetMethod("HasMember"));
					funInfo.Stack++;
				}
				else if (node.Operand is Member m)
				{
					CompileNode(m.Root);
					gen.Emit(ILOpCode.Ldstr, m.Name);
					CallRuntimeMethod(nameof(RuntimeMethods.HasMember));
				}
				else if ((node.Operand is CallNode c)&&(c.InBrackets))
				{
					CompileNode(c.Function);
					CompileNode(c.Arguments[0]);
					CallRuntimeMethod(nameof(RuntimeMethods.HasArrayItem));
					funInfo.Stack--;
				}
				else
					throw new NotImplementedException();

				gen.Emit(ILOpCode.Brfalse, lJump);
				funInfo.Stack--;
				CompileNode(node.Operand);
				CallRuntimeMethod(nameof(RuntimeMethods.TypeOf));
				gen.Emit(ILOpCode.Br, lEnd);
				gen.MarkLabel(lJump);
				gen.Emit(ILOpCode.Ldstr, "undefined");
				gen.Emit(ILOpCode.Box, typeof(string));
				gen.MarkLabel(lEnd);
			}
			else if (node.OperatorToken==JSToken.Delete)
				DeleteVar(node.Operand);
			else if (node.OperatorToken==JSToken.Void)
			{
				gen.Emit(ILOpCode.Ldnull); //void0
				funInfo.Stack++;
			}
			else
				throw new NotImplementedException();
		}

		void CompileLookup(Lookup node)
		{
			GetVar(node);
		}

		void CompileForNode(ForNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			Label forBegin = gen.DefineLabel();
			Label forEnd = gen.DefineLabel();
			Label forNext = gen.DefineLabel();
			funInfo.LoopLabels.Push(new LoopInfo() { Next=forNext, Finish=forEnd });

			if (node.Initializer!=null)
				CompileNodeWithPopToBase(node.Initializer);
			gen.MarkLabel(forBegin);

			if (node.Condition!=null)
			{
				CompileNode(node.Condition);
				CallRuntimeMethod(nameof(RuntimeMethods.EvalToBool));
				gen.Emit(ILOpCode.Unbox_any, typeof(bool));
				gen.Emit(ILOpCode.Brfalse, forEnd);
				funInfo.Stack--;
			}

			if (node.Body!=null)
				CompileNodeWithPopToBase(node.Body);
			gen.MarkLabel(forNext);
			if (node.Incrementer!=null)
				CompileNodeWithPopToBase(node.Incrementer);
			gen.Emit(ILOpCode.Br, forBegin);
			gen.MarkLabel(forEnd);
			funInfo.LoopLabels.Pop();
		}

		void CompileForIn(ForIn node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			Label forEnd = gen.DefineLabel();
			Label forNext = gen.DefineLabel();
			funInfo.LoopLabels.Push(new LoopInfo() { Next=forNext, Finish=forEnd });

			CompileNode(node.Collection);
			CallRuntimeMethod(nameof(RuntimeMethods.GetPropertyEnumerator));
			LocalBuilder enumer = gen.DeclareLocal(typeof(IEnumerator<object>));
			gen.Emit(ILOpCode.Stloc, enumer);

			gen.MarkLabel(forNext);
			gen.Emit(ILOpCode.Ldloc, enumer);
			gen.Emit(ILOpCode.Callvirt, typeof(System.Collections.IEnumerator).GetMethod("MoveNext"));
			gen.Emit(ILOpCode.Brfalse, forEnd);
			AstNode varName;
			if (node.Variable is Var varDecl)
			{
				CompileNode(varDecl);
				varName=((VariableDeclaration)varDecl.Children.Single()).Binding;
			}
			else
				varName=node.Variable;
			PreSetVar(varName);
			gen.Emit(ILOpCode.Ldloc, enumer);
			gen.Emit(ILOpCode.Callvirt, typeof(IEnumerator<object>).GetProperty("Current").GetGetMethod());
			SetVar(varName);
			if (node.Body!=null)
			{
				int p = funInfo.Stack;
				CompileNode(node.Body);
				FinalizeFunctionPushes(funInfo.Stack-p);
			}
			gen.Emit(ILOpCode.Br, forNext);
			gen.MarkLabel(forEnd);
		}

		void CompileBreak(Break node)
		{
			FunctionInfo funInfo = _functions.Peek();
			funInfo.Gen.Emit(ILOpCode.Br, funInfo.LoopLabels.Peek().Finish);
		}

		void CompileContinueNode(ContinueNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			funInfo.Gen.Emit(ILOpCode.Br, funInfo.LoopLabels.Peek().Next);
		}

		void CompileWhileNode(WhileNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			Label forEnd = gen.DefineLabel();
			Label forNext = gen.DefineLabel();
			funInfo.LoopLabels.Push(new LoopInfo() { Next=forNext, Finish=forEnd });

			gen.MarkLabel(forNext);
			CompileNode(node.Condition);
			CallRuntimeMethod(nameof(RuntimeMethods.EvalToBool));
			gen.Emit(ILOpCode.Unbox_any, typeof(bool));
			gen.Emit(ILOpCode.Brfalse, forEnd);
			funInfo.Stack--;

			if (node.Body!=null)
				CompileNodeWithPopToBase(node.Body);
			gen.Emit(ILOpCode.Br, forNext);
			gen.MarkLabel(forEnd);
			funInfo.LoopLabels.Pop();
		}

		void CompileDoWhile(DoWhile node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			Label forEnd = gen.DefineLabel();
			Label forNext = gen.DefineLabel();
			funInfo.LoopLabels.Push(new LoopInfo() { Next=forNext, Finish=forEnd });

			gen.MarkLabel(forNext);
			if (node.Body!=null)
				CompileNodeWithPopToBase(node.Body);

			CompileNode(node.Condition);
			CallRuntimeMethod(nameof(RuntimeMethods.EvalToBool));
			gen.Emit(ILOpCode.Unbox_any, typeof(bool));
			gen.Emit(ILOpCode.Brtrue, forNext);
			funInfo.Stack--;

			gen.MarkLabel(forEnd);
		}

		void CompileFunctionObject(FunctionObject node)
		{
			string funName = node.Binding==null
				? "anonymous"+(_anonymousCounter++).ToString()+(string.IsNullOrEmpty(node.NameGuess) ? "" : "_"+node.NameGuess)
				: node.Binding.Name;

			if (!_useDynamicJsMembers)
				DefineVar(funName);

			FunctionInfo funInfoPar = _functions.Peek();
			TypeBuilder parType = funInfoPar.TypBldr;

			if (_modBldr.GetType(funName)!=null)
				funName+="-"+node.Context.StartPosition;

			#region define real function with its body
			TypeBuilder typBldr = _modBldr.DefineType(funName+"!", TypeAttributes.BeforeFieldInit|TypeAttributes.Sealed|TypeAttributes.Public, GetRuntimeType(_useDynamicJsMembers ? typeof(DynamicJsObject) : typeof(StaticJsObject)));
			GenerateSourceConstant(typBldr, node);
			AddDebuggerDisplayAttribute(typBldr, "function" + (node.Binding == null ? "" : " " + node.Binding.Name), "Jellequin function");
			AddDebuggerTypeProxyAttribute(typBldr);

			FieldBuilder parScope = typBldr.DefineField("~~parScope", parType, FieldAttributes.Private);
			FieldBuilder thisField = typBldr.DefineField("~~this", _objectType, FieldAttributes.Private);

			MethodBuilder runMethod = typBldr.DefineMethod("~~Run", MethodAttributes.HideBySig|MethodAttributes.Assembly, _objectType, new IType[] { typeof(object[]) });
			runMethod.DefineParameter(1, ParameterAttributes.None, "arguments");
			int parameterCount = node.ParameterDeclarations.Count;
			string[] argNames = new string[parameterCount];
			int a = 0;
			for (a = 0; a<parameterCount; a++)
				argNames[a]=((BindingIdentifier)((ParameterDeclaration)node.ParameterDeclarations[a]).Binding).Name;

			ILGenerator gen = runMethod.GetILGenerator();
			FunctionInfo funTypeChild = new FunctionInfo { TypBldr=typBldr, Gen=gen, EndLabel=gen.DefineLabel(), ResultValue=gen.DeclareLocal(_objectType), Args=new[] { "arguments" }, Parent=funInfoPar, ParentScopeField=parScope, ThisField=thisField };
			_functions.Push(funTypeChild);

			if (!_useDynamicJsMembers)
			{
				//override all parent vars
				foreach (KeyValuePair<string, VarInfo> varDef in funInfoPar.Vars)
					if (varDef.Key!="arguments")
						funTypeChild.Vars.Add(varDef.Key, GeneratePropertyRefParentOne(funTypeChild, varDef.Value.Property));
			}

			//store "arguments" argument to variable "arguments"
			if (_useDynamicJsMembers)
			{
				//declare arguments
				Lookup argsVarName = new Lookup(node.Context) { Name="arguments" };
				PreSetVar(argsVarName);
				gen.Emit(ILOpCode.Ldarg_1);
				funTypeChild.Stack++;
				SetVar(argsVarName);
			}
			//args => vars
			if (parameterCount>0)
			{
				a=0;
				foreach (string argName in argNames)
				{
					Label l = gen.DefineLabel();
					gen.Emit(ILOpCode.Ldarg_1);
					gen.Emit(ILOpCode.Call, typeof(Array).GetProperty("Length").GetMethod);
					gen.Emit(ILOpCode.Ldc_i4, a+1);
					gen.Emit(ILOpCode.Blt, l);

					Lookup argVarName = new Lookup(node.Context) { Name=argName };
					PreSetVar(argVarName);
					gen.Emit(ILOpCode.Ldarg_1);
					gen.Emit(ILOpCode.Ldc_i4, a);
					gen.Emit(ILOpCode.Ldelem_ref);
					funTypeChild.Stack++;
					SetVar(argVarName);

					gen.MarkLabel(l);
					a++;
				}
			}

			//compile function body
			CompileNodeWithPopToBase(node.Body);
			gen.MarkLabel(funTypeChild.EndLabel);
			gen.Emit(ILOpCode.Ldloc_s, funTypeChild.ResultValue);
			gen.MarkSequencePoint(_symbolWriter, node.Context.EndLineNumber, node.Context.EndColumn+2, node.Context.EndLineNumber, node.Context.EndColumn+2);
			gen.Emit(ILOpCode.Ret);
			_functions.Pop();

			ConstructorBuilder cb = typBldr.DefineConstructor(MethodAttributes.Assembly|MethodAttributes.RTSpecialName|MethodAttributes.SpecialName|MethodAttributes.HideBySig, CallingConventions.Standard, new IType[] { parType, _objectType });
			cb.DefineParameter(1, ParameterAttributes.None, "parScope");
			cb.DefineParameter(2, ParameterAttributes.None, "this");
			GenerateConstructorBaseCall(cb);
			gen=cb.GetILGenerator();
			//this.~~parScope = parScope;
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Stfld, parScope);
			//this.~~this = @this == null ? this : @this;
			Label thisIsNotNull = gen.DefineLabel();
			Label thisIsNotNullEnd = gen.DefineLabel();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_2);
			gen.Emit(ILOpCode.Ldnull);
			gen.Emit(ILOpCode.Ceq);
			gen.Emit(ILOpCode.Brtrue_s, thisIsNotNull);
			gen.Emit(ILOpCode.Ldarg_2);
			gen.Emit(ILOpCode.Br_s, thisIsNotNullEnd);
			gen.MarkLabel(thisIsNotNull);
			gen.Emit(ILOpCode.Ldarg_0);
			gen.MarkLabel(thisIsNotNullEnd);
			gen.Emit(ILOpCode.Stfld, thisField);
			//ret
			gen.Emit(ILOpCode.Ret);

			typBldr.CreateType();
			#endregion define real function with its body

			#region define container
			ConstructorBuilder cbToInvoke = cb;
			MethodBuilder runMethodToInvoke = runMethod;
			typBldr = _modBldr.DefineType(funName, TypeAttributes.BeforeFieldInit|TypeAttributes.Sealed|TypeAttributes.Public, GetRuntimeType(_useDynamicJsMembers ? typeof(DynamicJsObject) : typeof(StaticJsObject)), new IType[] { typeof(IJsFunction) });
			parScope = typBldr.DefineField("~~parScope", parType, FieldAttributes.Private);
			thisField = typBldr.DefineField("~~this", _objectType, FieldAttributes.Private);

			//prototype property
			FieldBuilder prototypeField =null;
			if (!_useDynamicJsMembers)
			{
				IType prototypeType = GetRuntimeType(typeof(StaticJsObject));
				prototypeField = typBldr.DefineField($"<prototype>k__BackingField", prototypeType, FieldAttributes.Private);
				PropertyBuilder prototypePropBldr = typBldr.DefineProperty("prototype", PropertyAttributes.None, prototypeType, IType.EmptyTypes);
				MethodBuilder varGetter = typBldr.DefineMethod("get_prototype", MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, prototypeType, IType.EmptyTypes);
				prototypePropBldr.SetGetMethod(varGetter);
				//GenerateVarGetter
				gen=varGetter.GetILGenerator();
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldfld, prototypeField);
				gen.Emit(ILOpCode.Ret);
			}

			//constructor
			cb = typBldr.DefineConstructor(MethodAttributes.Assembly|MethodAttributes.RTSpecialName|MethodAttributes.SpecialName|MethodAttributes.HideBySig, CallingConventions.Standard, new IType[] { parType, _objectType });
			cb.DefineParameter(1, ParameterAttributes.None, "parScope");
			cb.DefineParameter(2, ParameterAttributes.None, "this");
			GenerateConstructorBaseCall(cb);
			gen=cb.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Stfld, parScope);
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_2);
			gen.Emit(ILOpCode.Stfld, thisField);
			if (_useDynamicJsMembers)
			{
				//set prototype member value
				Lookup prototypeVarName = new Lookup(node.Context) { Name="prototype" };
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldstr, "prototype");
				gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(DynamicJsObject)).GetConstructors()[0]);
				gen.Emit(ILOpCode.Call, GetRuntimeType(typeof(DynamicJsObject)).GetMethod("SetValue"));
			}
			else
			{
				gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(StaticJsObject)).GetConstructors()[0]);
				gen.Emit(ILOpCode.Stfld, prototypeField);
			}
			gen.Emit(ILOpCode.Ret);

			//Invoke method
			runMethod = typBldr.DefineMethod("Invoke", MethodAttributes.Public|MethodAttributes.Final|MethodAttributes.HideBySig|MethodAttributes.NewSlot|MethodAttributes.Virtual, _objectType, new IType[] { typeof(object[]) });
			runMethod.DefineParameter(1, ParameterAttributes.None, "arguments");
			gen=runMethod.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, parScope);
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, thisField);
			gen.Emit(ILOpCode.Newobj, cbToInvoke);
			gen.Emit(ILOpCode.Ldarg, 1);
			gen.Emit(ILOpCode.Call, runMethodToInvoke);
			gen.Emit(ILOpCode.Ret);

			//implicit cast operator - public static implicit operator Func<object>(TestClsA testClsA) => () => testClsA;
			/*MethodBuilder castMain = typBldr.DefineMethod("~~op_Main", MethodAttributes.Private, _objectType, argNames.Select(x=>(IType)_objectType).ToArray() );
			a = 0;
			foreach (string argName in argNames)
				castMain.DefineParameter(++a, ParameterAttributes.None, argName);
			gen=castMain.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldc_i4, parameterCount);
			gen.Emit(ILOpCode.Newarr, _objectType);
			for (a = 0; a<parameterCount; a++)
			{
				gen.Emit(ILOpCode.Dup);
				gen.Emit(ILOpCode.Ldc_i4, a);
				gen.Emit(ILOpCode.Ldarg, a+1);
				gen.Emit(ILOpCode.Stelem_ref);
			}
			gen.Emit(ILOpCode.Callvirt, runMethod);
			gen.Emit(ILOpCode.Ret);

			MethodBuilder castOperator = typBldr.DefineMethod("op_Implicit", MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName|MethodAttributes.Static, RuntimeMethods.GetFuncType(parameterCount), new IType[] { typBldr });
			castOperator.DefineParameter(1, ParameterAttributes.None, "self");
			gen=castOperator.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldftn, castMain);
			gen.Emit(ILOpCode.Newobj, RuntimeMethods.GetFuncType(parameterCount).GetConstructors()[0]);
			gen.Emit(ILOpCode.Ret);*/

			//Invoke method
			runMethod = typBldr.DefineMethod("Instantiate", MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual, _objectType, new IType[] { typeof(object[]) });
			runMethod.DefineParameter(1, ParameterAttributes.None, "arguments");
			gen = runMethod.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, parScope);
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, thisField);
			gen.Emit(ILOpCode.Newobj, cbToInvoke);
			gen.Emit(ILOpCode.Dup);
			gen.Emit(ILOpCode.Ldarg, 1);
			gen.Emit(ILOpCode.Call, runMethodToInvoke);
			gen.Emit(ILOpCode.Pop);
			gen.Emit(ILOpCode.Ret);

			typBldr.CreateType();
			#endregion define container

			gen=funInfoPar.Gen;
			bool saveToVar = node.FunctionType!=FunctionType.Expression&&node.Precedence!=OperatorPrecedence.Assignment;
			if (_useDynamicJsMembers)
			{
				Lookup l = null;
				if (saveToVar)
				{
					l=new Lookup(node.Context) { Name=funName };
					PreSetVar(l);
				}
				gen.Emit(ILOpCode.Ldarg_0);
				if (funInfoPar.ThisVar == null)
					gen.Emit(ILOpCode.Ldnull);
				else
					gen.Emit(ILOpCode.Ldloc, funInfoPar.ThisVar);
				/*gen.Emit(ILOpCode.Ldftn, runMethodNew);
				gen.Emit(ILOpCode.Newobj, RuntimeMethods.GetFuncType(parameterCount).GetConstructors()[0]);*/
				gen.Emit(ILOpCode.Newobj, cb);
				funInfoPar.Stack++;
				if (saveToVar)
					SetVar(l);
			}
			else
			{
				if (saveToVar)
					gen.Emit(ILOpCode.Ldarg_0);
				gen.Emit(ILOpCode.Ldarg_0);
				if (funInfoPar.ThisVar==null)
					gen.Emit(ILOpCode.Ldnull);
				else
					gen.Emit(ILOpCode.Ldloc, funInfoPar.ThisVar);
				/*gen.Emit(ILOpCode.Ldftn, runMethodNew);
				gen.Emit(ILOpCode.Newobj, RuntimeMethods.GetFuncType(parameterCount).GetConstructors()[0]);*/
				gen.Emit(ILOpCode.Newobj, cb);
				funInfoPar.Stack++;
				if (saveToVar)
				{
					gen.Emit(ILOpCode.Stfld, _functions.Peek().Vars[funName].Field);
					funInfoPar.Stack--;
				}
			}
		}

		void CompileReturnNode(ReturnNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			AstNode operand = node.Operand;
			if (operand!=null)
			{
				CompileNode(operand);
				gen.Emit(ILOpCode.Stloc, funInfo.ResultValue);
				funInfo.Stack--;
			}
			gen.Emit(ILOpCode.Br, funInfo.EndLabel);
		}

		void CompileCallNode(CallNode node)
		{
			MarkSequencePoint(node);

			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;
			int argsCount = node.Arguments.Count;

			void CallRuntimeCallWithArgs()
			{
				gen.Emit(ILOpCode.Ldc_i4, argsCount);
				gen.Emit(ILOpCode.Newarr, _objectType);
				funInfo.Stack++;
				int a = 0;
				foreach (AstNode arg in node.Arguments)
				{
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Ldc_i4, a++);
					CompileNode(arg);
					gen.Emit(ILOpCode.Stelem_ref);
					funInfo.Stack--;
				}
				CallRuntimeMethod(node.IsConstructor ? nameof(RuntimeMethods.CreateInstance) : nameof(RuntimeMethods.CallMethod));
				funInfo.Stack--;
			}

			if (node.InBrackets) //is array element access
			{
				CompileNode(node.Function);
				CompileNode(node.Arguments.First());
				CallRuntimeMethod(nameof(RuntimeMethods.GetArrayItem));
				funInfo.Stack--;
			}
			else if (node.IsConstructor)
			{
				if (node.Function is Lookup funAsLookup)
				{
					string objName = funAsLookup.Name;
					Type[] types = new Type[argsCount];
					for (int a = 0; a<argsCount; a++)
						types[a]=_objectType;
					ConstructorInfo ci;
					switch (objName)
					{
						case "Array":
							gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.ArrayObject)).GetConstructor(Type.EmptyTypes));
							funInfo.Stack++;
							break;
						case "Date":
							ci=GetRuntimeType(typeof(Jellequin.Runtime.DateObject)).GetConstructors().FirstOrDefault(x => x.GetParameters().Length==argsCount);
							if (ci==null)
								throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, objName, ".ctor", argsCount);
							CompileNode(node.Arguments);
							funInfo.Stack-=argsCount;
							gen.Emit(ILOpCode.Newobj, ci);
							funInfo.Stack++;
							break;
						case "RegExp":
							if (argsCount>2)
								throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, objName, ".ctor", argsCount);
							CompileNode(node.Arguments);
							if (argsCount==1)
							{
								gen.Emit(ILOpCode.Ldnull);
								funInfo.Stack++;
								Array.Resize(ref types, 2);
								types[1]=_objectType;
							}
							gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.RegExpObject)).GetConstructor(types));
							funInfo.Stack--;
							break;
						case "Error":
						case "TypeError":
							if (argsCount!=1)
								throw new RuntimeException(RuntimeExceptionReason.NoCompatibleMethod, objName, ".ctor", argsCount);
							CompileNode(node.Arguments);
							funInfo.Stack-=argsCount;
							gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.JsError)).GetConstructor(types));
							funInfo.Stack++;
							break;
						default:
							if (_useDynamicJsMembers)
								GetVar(funAsLookup);
							else
							{
								if (!funInfo.Vars.TryGetValue(objName, out VarInfo customObj))
									throw new CompilerException(CompilerExceptionReason.UnknownObjectType, new object[] { objName });

								gen.Emit(ILOpCode.Ldarg_0);
								if (customObj.Field==null) //property uses parent's one
									gen.Emit(ILOpCode.Call, customObj.Property.GetMethod);
								else
									gen.Emit(ILOpCode.Ldfld, customObj.Field);
								funInfo.Stack++;
							}

							CallRuntimeCallWithArgs();
							break;
					}
				}
				else if (node.Function is Member funAsMember)
				{
					CompileNode(funAsMember);
					CallRuntimeCallWithArgs();
				}
				else
					throw new NotImplementedException();
			}
			else
			{
				CompileNode(node.Function);
				CallRuntimeCallWithArgs();
			}
		}

		void CompileObjectLiteral(ObjectLiteral node)
		{
			TypeBuilder typBldr = _modBldr.DefineType("~object~"+node.GetHashCode(), TypeAttributes.BeforeFieldInit|TypeAttributes.Sealed|TypeAttributes.Public, GetBaseType());
			AddDebuggerDisplayAttribute(typBldr, "object", "Jellequin object");
			AddDebuggerTypeProxyAttribute(typBldr);
			ConstructorBuilder cb = DefineSimpleConstructor(typBldr);
			GenerateSourceConstant(typBldr, node);

			MarkSequencePoint(node);
			FunctionInfo fi = _functions.Peek();
			ILGenerator gen = fi.Gen;
			LocalBuilder thisVarOld = fi.ThisVar;
			gen.Emit(ILOpCode.Newobj, cb);
			gen.Emit(ILOpCode.Dup);
			gen.Emit(ILOpCode.Stloc, fi.ThisVar=gen.DeclareLocal(typBldr));

			fi.Stack++;

			if (_useDynamicJsMembers)
				foreach (ObjectLiteralProperty item in node.Properties)
				{
					string varName = item.Name?.Name??(item.Value as Lookup).Name;
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Ldstr, varName);
					CompileNode(item.Value);
					gen.Emit(ILOpCode.Callvirt, GetRuntimeType(typeof(IJsObject)).GetMethod("SetValue"));
					fi.Stack--;
				}
			else
				foreach (ObjectLiteralProperty item in node.Properties)
				{
					string fName = item.Name?.Name??(item.Value as Lookup).Name;
					VarInfo varInfo = GenerateSimpleProperty(typBldr, fName);

					gen.Emit(ILOpCode.Dup);
					CompileNode(item.Value);
					gen.Emit(ILOpCode.Call, varInfo.Property.GetSetMethod());
                    fi.Stack--;
				}

			typBldr.CreateType();

			fi.ThisVar=thisVarOld;
		}

		void CompileMember(Member node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			/*if (ThisLiteralWrapper.CheckType(w.RootObject))
				GetVar(gen, w.Name, w.Context, true);
			else
			{*/

			/*if ((node.Root is Lookup lf)&&(lf.Name=="Object"))
			{
				//new StaticObject { Type=typeof(Jellequin.Runtime.Object) };
				Type staticObjectType = GetRuntimeType(typeof(StaticObject));
				gen.Emit(ILOpCode.Newobj, staticObjectType.GetConstructors()[0]);
				gen.Emit(ILOpCode.Dup);
				gen.Emit(ILOpCode.Ldtoken, typeof(Jellequin.Runtime.Object));
				gen.Emit(ILOpCode.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
				gen.Emit(ILOpCode.Stfld, staticObjectType.GetField("Type"));
				funInfo.Pushes++;
			}
			else*/
				CompileNode(node.Root);
			gen.Emit(ILOpCode.Ldstr, node.Name);

			CallRuntimeMethod(nameof(RuntimeMethods.GetMember));
			//}
		}

		void CompileConditional(Conditional node)
		{
			CompileIf(node.Condition, node.TrueExpression, node.FalseExpression, !node.IsExpression);
		}

		void CompileIf(IfNode node)
		{
			CompileIf(node.Condition, node.TrueBlock, node.FalseBlock, true);
		}

		void CompileIf(AstNode condition, AstNode trueExpression, AstNode falseExpression, bool realIf)
		{
			int stackDelta;
			if (realIf)
			{
				MarkSequencePoint(condition);
				stackDelta=0;
			}
			else
				stackDelta=1;

			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			if (falseExpression==null)
			{
				Label labelFinish = gen.DefineLabel();

				CompileNode(condition);
				CallRuntimeMethod(nameof(RuntimeMethods.EvalToBool));
				gen.Emit(ILOpCode.Unbox_any, typeof(bool));
				gen.Emit(ILOpCode.Brfalse, labelFinish);
				funInfo.Stack--;

				CompileExpression(trueExpression, stackDelta);

				gen.MarkLabel(labelFinish);
			}
			else
			{
				Label labelFinish = gen.DefineLabel();
				Label labelElse = gen.DefineLabel();

				CompileNode(condition);
				CallRuntimeMethod(nameof(RuntimeMethods.EvalToBool));
				gen.Emit(ILOpCode.Unbox_any, typeof(bool));
				gen.Emit(ILOpCode.Brtrue, labelElse);
				funInfo.Stack--;

				CompileExpression(falseExpression, stackDelta);

				gen.Emit(ILOpCode.Br, labelFinish);
				gen.MarkLabel(labelElse);
				CompileExpression(trueExpression, stackDelta);

				gen.MarkLabel(labelFinish);
			}
			if (!realIf)
				funInfo.Stack--;
		}

		void CompileSwitch(Microsoft.Ajax.Utilities.Switch node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			LocalBuilder expr = gen.DeclareLocal(_objectType);
			CompileExpression(node.Expression);
			gen.Emit(ILOpCode.Stloc, expr);
			funInfo.Stack--;

			Label lEnd = gen.DefineLabel();
			funInfo.LoopLabels.Push(new LoopInfo { Finish=lEnd, Next=funInfo.LoopLabels.Count==0?null:funInfo.LoopLabels.Peek().Next }); //switch has no itself Next (for continue purpose) so let's use previous one
			Label lNextCase = gen.DefineLabel();
			Label lStats = gen.DefineLabel();
			foreach (SwitchCase @case in node.Cases)
			{
				if (@case.CaseValue==null) //default
					gen.Emit(ILOpCode.Br, lStats);
				else
				{
					CompileExpression(@case.CaseValue);
					gen.Emit(ILOpCode.Ldloc, expr);
					CallRuntimeMethod(nameof(RuntimeMethods.Equals));
					gen.Emit(ILOpCode.Brtrue, lStats);
					funInfo.Stack--;
				}

				if ((@case.Statements != null) && (@case.Statements.Count != 0))
				{
					gen.Emit(ILOpCode.Br, lNextCase);
					gen.MarkLabel(lStats);
					lStats = gen.DefineLabel();
					CompileNodeWithPopToBase(@case.Statements);
					gen.MarkLabel(lNextCase);
					lNextCase = gen.DefineLabel();
				}
			}
			gen.MarkLabel(lEnd);
			funInfo.LoopLabels.Pop();
		}

		void CompileImportNode(ImportNode node)
		{
			string varName = null;
			IEnumerator<AstNode> en = node.Children.GetEnumerator();
			if ((en.MoveNext()))
				varName=(en.Current as BindingIdentifier)?.Name;
			string asmDef = node.ModuleName;

			if ((varName==null)||(asmDef==null))
				throw new CompilerException(CompilerExceptionReason.InvalidImport);

			if (!_useDynamicJsMembers)
				DefineVar(varName);

			Lookup l = new Lookup(node.Context) { Name=varName };
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;
			PreSetVar(l);
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldstr, varName);
			gen.Emit(ILOpCode.Ldstr, asmDef);
			CallRuntimeMethod(nameof(RuntimeMethods.GetExternalLibrary));
			funInfo.Stack++;
			SetVar(l);
		}

		void CompileArrayLiteral(ArrayLiteral node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			AstNodeList initValues = node.Elements;

			Type orgType = GetRuntimeType(typeof(Jellequin.Runtime.ArrayObject));
			gen.Emit(ILOpCode.Newobj, orgType.GetConstructor(Type.EmptyTypes));
			if (initValues.Count!=0)
			{
				MethodInfo mi = GetRuntimeMethod(orgType.GetMethod("push", BindingFlags.Public|BindingFlags.Instance));
				foreach (AstNode elNode in initValues)
				{
					gen.Emit(ILOpCode.Dup);
					CompileNode(elNode);
					gen.Emit(ILOpCode.Call, mi);
					gen.Emit(ILOpCode.Pop);
					funInfo.Stack--;
				}
			}
			funInfo.Stack++;
		}

		void CompileDebuggerNode(DebuggerNode node)
		{
			if (_debug)
			{
				MarkSequencePoint(node);
				CallRuntimeMethod(nameof(RuntimeMethods.Debug));
			}
		}

		void CompileImportantComment(ImportantComment node)
		{
			//might be passed to assembly as a constant field
			//typBldr.DefineField("~~comment", typeof(string), FieldAttributes.Private | FieldAttributes.Static | FieldAttributes.Literal).SetConstant(node.Comment);
		}

		void CompileThrow(ThrowNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			CompileNode(node.Operand);
			funInfo.Gen.Emit(ILOpCode.Throw);
			funInfo.Stack--;
		}

		void CompileRegExpLiteral(RegExpLiteral node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			gen.Emit(ILOpCode.Ldstr, node.Pattern);
			gen.Emit(ILOpCode.Ldstr, node.PatternSwitches);
			gen.Emit(ILOpCode.Newobj, GetRuntimeType(typeof(Jellequin.Runtime.RegExpObject)).GetConstructor(new Type[] { typeof(string), typeof(string) }));
			_functions.Peek().Stack++;
		}

		void CompileGroupingOperator(GroupingOperator node)
		{
			CompileNode(node.Operand);
		}

		void CompileTryNode(TryNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			bool hasCatchBlock = node.CatchBlock!=null;
			bool hasFinallyBlock = node.FinallyBlock!=null;

			if (node.TryBlock==null)
				throw new NotSupportedException();
			if ((!hasCatchBlock)&&(!hasFinallyBlock))
				throw new NotSupportedException();

			ExceptionRegionInfo catchRegion,finallyRegion;
			catchRegion=finallyRegion=null;
			if (hasFinallyBlock)
			{
				finallyRegion=gen.AddFinallyRegion();
				finallyRegion.MarkTryStart(); //try
			}
			if (hasCatchBlock)
			{
				catchRegion=gen.AddCatchRegion(typeof(Exception));
				catchRegion.MarkTryStart(); //try
			}

			CompileNode(node.TryBlock);

			if (hasCatchBlock)
			{
				gen.Emit(ILOpCode.Leave, catchRegion.HandleEnd);
				catchRegion.MarkHandleStart(); //catch

				BindingIdentifier exceptionVar = (BindingIdentifier)node.CatchParameter?.Binding;
				if (exceptionVar!=null)
				{
					LocalBuilder var = gen.DeclareLocal(typeof(Exception));
					gen.Emit(ILOpCode.Stloc, var);
					PreSetVar(exceptionVar);
					gen.Emit(ILOpCode.Ldloc, var);
					CallRuntimeMethod(nameof(RuntimeMethods.MakeJsError));
					funInfo.Stack++;
					SetVar(exceptionVar);
				}

				CompileNode(node.CatchBlock);
				gen.Emit(ILOpCode.Leave, catchRegion.HandleEnd);
				catchRegion.MarkHandleEnd(); //end catch
			}

			if (hasFinallyBlock)
			{
				gen.Emit(ILOpCode.Leave, finallyRegion.HandleEnd);
				finallyRegion.MarkHandleStart(); //finally
				CompileNode(node.CatchBlock);
				gen.Emit(ILOpCode.Endfinally);
				finallyRegion.MarkHandleEnd(); //end finally
			}
		}

		void CompileTemplateLiteral(TemplateLiteral node)
		{
			string SanitizeString(string text) => text=="}`" ? "" : text.Substring(1, text.Length-2);

			if (node.Function!=null)
				throw new NotImplementedException();

			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			gen.Emit(ILOpCode.Ldstr, SanitizeString(node.Text));
			funInfo.Stack++;

			if (node.Expressions!=null)
				foreach (TemplateLiteralExpression item in node.Expressions)
				{
					CompileNode(item.Expression);
					CallRuntimeMethod(nameof(RuntimeMethods.Add));
					funInfo.Stack--;

					string text = SanitizeString(item.Text);
					if (text.Length>0)
					{
						gen.Emit(ILOpCode.Ldstr, SanitizeString(item.Text));
						CallRuntimeMethod(nameof(RuntimeMethods.Add));
					}
				}
		}

		void GenerateSourceConstant(TypeBuilder typBldr, AstNode node)
		{
			typBldr.DefineField("~~source", typeof(string), FieldAttributes.Private|FieldAttributes.Static|FieldAttributes.Literal).SetConstant(node.Context.StartLineNumber.ToString()+"-"+node.Context.EndLineNumber.ToString()+"-"+node.Context.ToString());
		}

		void AddDebuggerDisplayAttribute(TypeBuilder typBldr, string value, string type)
		{
			typBldr.AddCustomAttribute(typeof(DebuggerDisplayAttribute).GetConstructor(new[] { typeof(string) }), new object[] { value }, new[] { new CustomAttributeValue(false, "Type", type) });
		}

		void AddDebuggerTypeProxyAttribute(TypeBuilder typBldr)
		{
			//[DebuggerTypeProxy(typeof(JsObjectDebugView))]
			typBldr.AddCustomAttribute(typeof(DebuggerTypeProxyAttribute).GetConstructor(new[] { typeof(Type) }),  new object[] { typeof(JsObjectDebugView).AssemblyQualifiedName }, new CustomAttributeValue[0]);
			typBldr.AddCustomAttribute(typeof(DebuggerDisplayAttribute).GetConstructor(new[] { typeof(string) }), new object[] { "" }, new[] { new CustomAttributeValue(false, "Type", ""), new CustomAttributeValue(false, "Name", "") });
		}
		#endregion compile node

		#region variables/members generating
		VarInfo DefineVar(string varName)
		{
			FunctionInfo funInfo = _functions.Peek();
			if (!funInfo.Vars.TryGetValue(varName, out VarInfo result))
			{
				result=GenerateSimpleProperty(funInfo.TypBldr, varName);
				funInfo.Vars.Add(varName, result);
			}
			return result;
		}

		VarInfo GenerateSimpleProperty(TypeBuilder typBldr, string propName)
		{
			FieldBuilder fieldBldr = typBldr.DefineField($"<{propName}>k__BackingField", _objectType, FieldAttributes.Private);
			PropertyBuilder propBldr = typBldr.DefineProperty(propName, PropertyAttributes.None, _objectType, IType.EmptyTypes);
			MethodBuilder varGetter = typBldr.DefineMethod("get_"+propName, MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, _objectType, IType.EmptyTypes);
			MethodBuilder varSetter = typBldr.DefineMethod("set_"+propName, MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, typeof(void), new IType[] { _objectType });
			varSetter.DefineParameter(1, ParameterAttributes.None, "value");
			propBldr.SetGetMethod(varGetter);
			propBldr.SetSetMethod(varSetter);

			//GenerateVarGetter
			ILGenerator gen = varGetter.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, fieldBldr);
			gen.Emit(ILOpCode.Ret);

			//GenerateVarSetter;
			gen=varSetter.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Stfld, fieldBldr);
			gen.Emit(ILOpCode.Ret);

			return new VarInfo { Field=fieldBldr, Property=propBldr };
		}

		VarInfo GeneratePropertyRefParentOne(FunctionInfo funInfo, PropertyBuilder parProp)
		{
			TypeBuilder typBldr = funInfo.TypBldr;

			//check the prop doesn't exist still

			string propName = parProp.Name;
			PropertyBuilder propBldr = typBldr.DefineProperty(propName, PropertyAttributes.None, _objectType, IType.EmptyTypes);
			MethodBuilder varGetter = typBldr.DefineMethod("get_"+propName, MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, _objectType, IType.EmptyTypes);
			MethodBuilder varSetter = typBldr.DefineMethod("set_"+propName, MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, typeof(void), new IType[] { _objectType });
			varSetter.DefineParameter(1, ParameterAttributes.None, "value");
			propBldr.SetGetMethod(varGetter);
			propBldr.SetSetMethod(varSetter);

			//GenerateVarGetter
			ILGenerator gen = varGetter.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, funInfo.ParentScopeField);
			gen.Emit(ILOpCode.Call, parProp.GetGetMethod());
			gen.Emit(ILOpCode.Ret);

			//GenerateVarSetter;
			gen=varSetter.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, funInfo.ParentScopeField);
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Call, parProp.GetSetMethod());
			gen.Emit(ILOpCode.Ret);

			return new VarInfo { Property=propBldr };
		}

		VarInfo GeneratePropertyRefParentArg(FunctionInfo funInfo, FunctionInfo funInfoPar, string propName)
		{
			TypeBuilder typBldr = funInfo.TypBldr;

			//check the prop doesn't exist still

			PropertyBuilder propBldr = typBldr.DefineProperty(propName, PropertyAttributes.None, _objectType, IType.EmptyTypes);
			MethodBuilder varGetter = typBldr.DefineMethod("get_"+propName, MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, _objectType, IType.EmptyTypes);
			MethodBuilder varSetter = typBldr.DefineMethod("set_"+propName, MethodAttributes.Public|MethodAttributes.HideBySig|MethodAttributes.SpecialName, typeof(void), new IType[] { _objectType });
			varSetter.DefineParameter(1, ParameterAttributes.None, "value");
			propBldr.SetGetMethod(varGetter);
			propBldr.SetSetMethod(varSetter);

			//GenerateVarGetter
			ILGenerator gen = varGetter.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, funInfo.ParentScopeField);
			gen.Emit(ILOpCode.Ldfld, funInfoPar.Vars["arguments"].Field);
			gen.Emit(ILOpCode.Castclass, typeof(object[]));
			gen.Emit(ILOpCode.Ldc_i4, Array.IndexOf(funInfoPar.Args, propName));
			gen.Emit(ILOpCode.Ldelem_ref);
			gen.Emit(ILOpCode.Ret);

			gen=varSetter.GetILGenerator();
			gen.Emit(ILOpCode.Ldarg_0);
			gen.Emit(ILOpCode.Ldfld, funInfo.ParentScopeField);
			gen.Emit(ILOpCode.Ldfld, funInfoPar.Vars["arguments"].Field);
			gen.Emit(ILOpCode.Castclass, typeof(object[]));
			gen.Emit(ILOpCode.Ldc_i4, Array.IndexOf(funInfoPar.Args, propName));
			gen.Emit(ILOpCode.Ldarg_1);
			gen.Emit(ILOpCode.Stelem_ref);
			gen.Emit(ILOpCode.Ret);

			return new VarInfo { Property=propBldr };
		}

		GetVarInfoRes GetVarInfo(FunctionInfo funInfo, string varName, bool canDefine, bool canDefineFromParent)
		{
			int pos = Array.IndexOf(funInfo.Args, varName);
			if (pos>=0)
				return new GetVarInfoRes { Type=GetVarInfoResType.Argument, ArgIndex=pos+1 };

			if (funInfo.Vars.TryGetValue(varName, out VarInfo varInfo))
				return new GetVarInfoRes { Type=GetVarInfoResType.Variable, Variable=varInfo };

			if (canDefineFromParent)
			{
				FunctionInfo funInfoPar = funInfo.Parent;
				if (funInfoPar!=null)
				{
					GetVarInfoRes viPar = GetVarInfo(funInfoPar, varName, false, false);
					if (viPar!=null)
					{
						if (viPar.Type==GetVarInfoResType.Argument)
							return new GetVarInfoRes { Type=GetVarInfoResType.Variable, Variable=GeneratePropertyRefParentArg(funInfo, funInfoPar, varName) };
						else if (viPar.Type==GetVarInfoResType.Variable)
							return new GetVarInfoRes { Type=GetVarInfoResType.Variable, Variable=GeneratePropertyRefParentOne(funInfo, viPar.Variable.Property) };
					}
				}
			}

			return canDefine ? new GetVarInfoRes { Type=GetVarInfoResType.Variable, Variable=DefineVar(varName) } : null;
		}

		class GetVarInfoRes
		{
			internal GetVarInfoResType Type;
			internal int ArgIndex;
			internal VarInfo Variable;
		}
		enum GetVarInfoResType { Argument, Variable }

        enum GetVarNameResType { Direct, Member, Unknown }
        (AstNode Root, string Name, GetVarNameResType Type) GetVarName(AstNode node)
        {
            return
                node is Lookup l ? (null, l.Name, GetVarNameResType.Direct)
                : node is BindingIdentifier b ? (null, b.Name, GetVarNameResType.Direct)
                : node is Member m ? (m.Root, m.Name, GetVarNameResType.Member)
                : (null, null, GetVarNameResType.Unknown);
        }

		void GetVar(AstNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

            (AstNode Root, string Name, GetVarNameResType Type) varNameInfo = GetVarName(node);
            if (varNameInfo.Type==GetVarNameResType.Direct)
			{
				if (_useDynamicJsMembers)
				{
					gen.Emit(ILOpCode.Ldarg_0);
					gen.Emit(ILOpCode.Ldstr, varNameInfo.Name);
					gen.Emit(ILOpCode.Callvirt, GetRuntimeType(typeof(IJsObject)).GetMethod("GetValue"));
				}
				else
				{
					GetVarInfoRes vi = GetVarInfo(funInfo, varNameInfo.Name, true, true);

					if (vi.Type==GetVarInfoResType.Argument)
						gen.Emit(ILOpCode.Ldarg, vi.ArgIndex);
					else
					{
						gen.Emit(ILOpCode.Ldarg_0);
						gen.Emit(ILOpCode.Call, vi.Variable.Property.GetGetMethod());
					}
				}
				funInfo.Stack++;
			}
			else if (varNameInfo.Type == GetVarNameResType.Member)
            {
				/*if ((m.Root is Lookup lf)&&(lf.Name=="Object"))
				{
					//new StaticObject { Type=typeof(Jellequin.Runtime.Object) };
					Type staticObjectType = GetRuntimeType(typeof(StaticObject));
					gen.Emit(ILOpCode.Newobj, staticObjectType.GetConstructors()[0]);
					gen.Emit(ILOpCode.Dup);
					gen.Emit(ILOpCode.Ldtoken, typeof(Jellequin.Runtime.Object));
					gen.Emit(ILOpCode.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
					gen.Emit(ILOpCode.Stfld, staticObjectType.GetField("Type"));
					funInfo.Pushes++;
				}
				else*/
					CompileNode(varNameInfo.Root);
				gen.Emit(ILOpCode.Ldstr, varNameInfo.Name);
				CallRuntimeMethod(nameof(RuntimeMethods.GetMember));
			}
			else
				throw new NotImplementedException();
		}

		void PreSetVar(AstNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

			(AstNode Root, string Name, GetVarNameResType Type) varNameInfo = GetVarName(node);
			if (varNameInfo.Type==GetVarNameResType.Direct)
			{
				if (_useDynamicJsMembers)
				{
					gen.Emit(ILOpCode.Ldarg_0);
					gen.Emit(ILOpCode.Ldstr,varNameInfo.Name);
				}
				else
				{
					GetVarInfoRes vi = GetVarInfo(funInfo,varNameInfo.Name,true,true);
					if (vi.Type!=GetVarInfoResType.Argument)
						gen.Emit(ILOpCode.Ldarg_0);
				}
			}
			else if (varNameInfo.Type==GetVarNameResType.Member)
			{
				CompileNode(varNameInfo.Root);
				gen.Emit(ILOpCode.Ldstr,varNameInfo.Name);
			}
			else
				throw new NotImplementedException();
		}

		void SetVar(AstNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

            (AstNode Root, string Name, GetVarNameResType Type) varNameInfo = GetVarName(node);
            if (varNameInfo.Type==GetVarNameResType.Direct)
			{
				if (_useDynamicJsMembers)
					gen.Emit(ILOpCode.Callvirt, GetRuntimeType(typeof(IJsObject)).GetMethod("SetValue"));
				else
				{
                    GetVarInfoRes vi = GetVarInfo(funInfo, varNameInfo.Name, false, true);

					if (vi.Type==GetVarInfoResType.Argument)
						gen.Emit(ILOpCode.Ldarg, vi.ArgIndex);
					else
						//gen.Emit(ILOpCode.Call, funInfo.Vars[varName].Property.GetSetMethod());
						gen.Emit(ILOpCode.Call, vi.Variable.Property.GetSetMethod());
				}
				funInfo.Stack--;
			}
			else if (varNameInfo.Type==GetVarNameResType.Member)
			{
				CallRuntimeMethod(nameof(RuntimeMethods.SetMember));
				funInfo.Stack-=2;
			}
			else
				throw new NotImplementedException();

		}

		void DeleteVar(AstNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;

            (AstNode Root, string Name, GetVarNameResType Type) varNameInfo = GetVarName(node);
			if (varNameInfo.Type==GetVarNameResType.Direct)
			{
				if (_useDynamicJsMembers)
				{
					gen.Emit(ILOpCode.Ldarg_0);
					gen.Emit(ILOpCode.Ldstr, varNameInfo.Name);
					gen.Emit(ILOpCode.Callvirt, GetRuntimeType(typeof(IJsObject)).GetMethod("DeleteMember"));
				}
				else
					throw new CompilerException(CompilerExceptionReason.DeleteOnStaticObject);
			}
			else if (varNameInfo.Type==GetVarNameResType.Member)
			{
				CompileNode(varNameInfo.Root);
				gen.Emit(ILOpCode.Ldstr, varNameInfo.Name);
				CallRuntimeMethod(nameof(RuntimeMethods.DeleteMember));
				funInfo.Stack--;
			}
			else if (node is CallNode c)
			{
				CompileNode(c.Function);
				CompileNode(c.Arguments[0]);
				CallRuntimeMethod(nameof(RuntimeMethods.DeleteArrayItem));
				funInfo.Stack--;
			}
			else
				throw new NotImplementedException();
		}
		#endregion variables/members generating

		#region helpers
		void FinalizeFunctionPushes(int count)
		{
			FunctionInfo funInfo = _functions.Peek();
			ILGenerator gen = funInfo.Gen;
			for (int a = 0; a<count; a++)
				gen.Emit(ILOpCode.Pop);
			funInfo.Stack-=count;
		}

		void MarkSequencePoint(AstNode node)
		{
			if (!_debug)
				return;
			Context c = node.Context;
			_functions.Peek().Gen.MarkSequencePoint(_symbolWriter, c.StartLineNumber, c.StartColumn+1, c.EndLineNumber, c.EndColumn+1);
		}

		void MarkSequencePointEnd(AstNode node)
		{
			if (!_debug)
				return;
			Context c = node.Context;
			_functions.Peek().Gen.MarkSequencePoint(_symbolWriter, c.EndLineNumber, c.StartColumn+1, c.EndLineNumber, c.EndColumn+1);
		}

		void CallRuntimeMethod(string methodName)
		{
			CallRuntimeMethod(GetRuntimeMethod(methodName));
		}

		void CallRuntimeMethod(MethodInfo rawRuntimeMethod)
		{
			_functions.Peek().Gen.Emit(ILOpCode.Call, GetRuntimeMethod(rawRuntimeMethod));
		}

		MethodInfo GetRuntimeMethod(MethodInfo mi)
		{
			return mi;
			//return _runtimeMethodsUsage==RuntimeMethodsUsage.Call ? mi : _typeCopier.GetMethodCopy(mi);
		}

		MethodInfo GetRuntimeMethod(string methodName)
		{
			return GetRuntimeMethod(typeof(Jellequin.Runtime.RuntimeMethods).GetMethod(methodName, BindingFlags.Public|BindingFlags.Static));
		}

		Type GetRuntimeType(Type type)
		{
			return type;
			//return _runtimeMethodsUsage==RuntimeMethodsUsage.Call ? type : _typeCopier.GetTypeCopy(type);
		}

		class Point : IComparable<Point>
		{
			internal Point(AstNode node)
			{
				EndLine=node.Context.EndLineNumber;
				EndColumn=node.Context.EndColumn;
			}

			internal int EndLine;
			internal int EndColumn;

			public int CompareTo(Point other)
			{
				return EndLine==other.EndLine ? EndColumn.CompareTo(other.EndColumn) : EndLine.CompareTo(other.EndLine);
			}
		}

		#region info classes
		class FunctionInfo
		{
			internal TypeBuilder TypBldr;
			internal ILGenerator Gen;
			int _pushes;
			internal int Stack { get { return _pushes; } set { if (value<0) throw new CompilerException(CompilerExceptionReason.WrongCallStackGeneration); _pushes=value; /*if (_maxStack<value) _maxStack=value;*/ } }
			internal LocalBuilder ThisVar;
			internal FieldBuilder ThisField;
			//int _maxStack;
			//internal int MaxStack => _maxStack+1;
			internal Dictionary<string, VarInfo> Vars = new Dictionary<string, VarInfo>();
			internal string[] Args = new string[0];
			internal Stack<LoopInfo> LoopLabels = new Stack<LoopInfo>();
			internal Label EndLabel;
			internal LocalBuilder ResultValue;
			internal FunctionInfo Parent;
			internal FieldBuilder ParentScopeField;
		}

		class VarInfo
		{
			internal FieldBuilder Field;
			internal PropertyBuilder Property;
		}

		class LoopInfo
		{
			internal Label Next;
			internal Label Finish;
		}
		#endregion info classes

		class StackInfo
		{
			internal FunctionInfo FunInfo;
			internal int Pushes;

			internal StackInfo(FunctionInfo funInfo, int pushes)
			{
				FunInfo=funInfo;
				Pushes=pushes;
			}
		}

		StackInfo GetStackInfo()
		{
			FunctionInfo funInfo = _functions.Peek();
			return new StackInfo(funInfo, funInfo.Stack);
		}

		void PopToStackInfo(StackInfo stackInfo)
		{
			FunctionInfo funInfo2 = _functions.Peek();
			if (funInfo2!=stackInfo.FunInfo)
				throw new CompilerException(CompilerExceptionReason.WrongCallStackGeneration);
			FinalizeFunctionPushes(funInfo2.Stack-stackInfo.Pushes);
		}

		void CompileNodeWithPopToBase(AstNode node)
		{
			FunctionInfo funInfo = _functions.Peek();
			int pushes = funInfo.Stack;
			CompileNode(node);
			FunctionInfo funInfo2 = _functions.Peek();
			if (funInfo2!=funInfo)
				throw new CompilerException(CompilerExceptionReason.WrongCallStackGeneration);
			FinalizeFunctionPushes(funInfo.Stack-pushes);
		}

		void CompileExpression(AstNode node)
		{
			CompileExpression(node, 1);
		}

		void CompileExpression(AstNode node, int delta)
		{
			FunctionInfo funInfo = _functions.Peek();
			int pushes = funInfo.Stack;
			CompileNode(node);
			FunctionInfo funInfo2 = _functions.Peek();
			if ((funInfo2!=funInfo)||(funInfo.Stack!=(pushes+delta)))
				throw new CompilerException(CompilerExceptionReason.WrongCallStackGeneration);
		}
		#endregion helpers
	}

	#region CompilerOptions
	public class CompilerOptions
	{
		public FileKind FileKind { get; set; }
		public RuntimeMethodsUsage RuntimeMethodsUsage { get; set; }
		//public Type BaseClass { get; set; } //mustn't be sealed - really?
		public bool DontUseDynamicJsMembers { get; set; }
		public Stream Icon { get; set; }

		public DebugOptions Debug { get; set; }
	}

	public class DebugOptions
	{
		public bool Debug { get; set; }
		public bool EmbedSourceCode { get; set; }
		public Stream Pdb { get; set; }
	}

	public enum RuntimeMethodsUsage { Call, Copy };
	public enum FileKind { Dll, ConsoleExe }
	#endregion CompilerOptions

	#region CompilerException
	public class CompilerException : Exception
	{
		internal CompilerException(CompilerExceptionReason reason, params object[] data)
		{
			Reason=reason;
			ReasonData=data;
		}

		public CompilerExceptionReason Reason { get; private set; }
		public object[] ReasonData { get; private set; }
	}
	public enum CompilerExceptionReason { InvalidImport, UnknownObjectType /*, InternalErrorCopyRuntime, CantResolveLibrary*/, DeleteOnStaticObject, WrongCallStackGeneration }
	#endregion CompilerException
}

#region class hierarchy
/*
ClassNode
ComprehensionClause
	ComprehensionForClause
	ComprehensionIfClause
CustomNode
Declaration
EmptyStatement
Expression
	ComprehensionNode
	ConstantWrapper
		DirectivePrologue
		ObjectLiteralField
			GetterSetter
	ConstantWrapperPP
ImportExportSpecifier
ImportExportStatement
	ExportNode
	ImportNode
LabeledStatement
WithNode
*/
#endregion class hierarchy

#region using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#endregion using

namespace Jellequin.Runtime
{
/*#if !DebugRuntime
	[DebuggerStepThrough]
#endif*/
	public class Executor
	{
		object _global;
		MethodInfo _mi;
		Action<object[]> _act;
		IJsObject _externalVariables;

		public Executor(Assembly toExecute)
		{
			Type type=toExecute.GetType("Root");
			_mi=type.GetMethod("Main", BindingFlags.Public | BindingFlags.Instance);
			_global=Activator.CreateInstance(type);

			_act = (Action<object[]>)Delegate.CreateDelegate(typeof(Action<object[]>), _global, _mi);

			//_externalVariables=type.GetProperty("ExternalVariables")?.GetValue(_global,new object[0]) as IJsObject;
			_externalVariables = BridgeJsObject(_global);

			Action<ResolveExternalLibraryEventArgs> externalLibraryResolver=new Action<ResolveExternalLibraryEventArgs>(e=> { FireResolveExternalLibrary(this,e); });
			FieldInfo fi=type.GetField("~externalLibraryResolver", BindingFlags.NonPublic | BindingFlags.Instance);
			fi.SetValue(_global, fi.FieldType.GetConstructors()[0].Invoke(new object[] { this, externalLibraryResolver.Method.MethodHandle.GetFunctionPointer() }));
		}

		public static Executor RunExe(Assembly toExecute,string[] args)
		{
			Executor asmExec=new Executor(toExecute);
			asmExec.ResolveExternalLibrary+=DefaultExternalLibraryResolver;
			asmExec.Execute(args);
			return asmExec;
		}

		public static ResolveExternalLibraryEventHandler DefaultExternalLibraryResolver = new ResolveExternalLibraryEventHandler((sender, e) => { e.Assembly = Assembly.Load(e.Predefinition); });

		public IJsObject ExternalVariables => _externalVariables;
		public void JoinScope(IJsObject parentScope)
		{
			_externalVariables=new JoinedJsObject(_externalVariables, parentScope);
		}
		
		public event ResolveExternalLibraryEventHandler ResolveExternalLibrary;
		void FireResolveExternalLibrary(object sender,ResolveExternalLibraryEventArgs args)
		{
			if (ResolveExternalLibrary!=null)
				ResolveExternalLibrary(sender,args);
		}

		/// <summary>
		/// Don't use with RuntimeMethodsUsage.Copy - type incompatibility runtime's IRuntimeMethodsExtender and copied IRuntimeMethodsExtender
		/// </summary>
		/*public IRuntimeMethodsExtender RuntimeMethodsExtender
		{
			get { return RuntimeMethods.RuntimeMethodsExtender; }
			set { RuntimeMethods.RuntimeMethodsExtender=value; }
		}*/

		public void /*object*/ Execute(string[] args)
		{
			_act(args);
		}

		CompiledRuntimeInfo _compiledRuntimeInfo;
		public CompiledRuntimeInfo CompiledRuntimeInfo
		{
			get
			{
				if (_compiledRuntimeInfo==null)
					_compiledRuntimeInfo=new CompiledRuntimeInfo(_mi.DeclaringType.Assembly);
				return _compiledRuntimeInfo;
			}
		}

		public static IJsObject BridgeJsObject(object copiedRuntimeJsObject)
		{
			return (copiedRuntimeJsObject as IJsObject) ?? new BridgeJsObject(copiedRuntimeJsObject);
		}
	}

	class BridgeJsObject : IJsObject
	{
		object _source;
		Type _type;
		internal BridgeJsObject(object source)
		{
			_source = source;
			_type = source.GetType();
		}

		public void DeleteMember(string name)
		{
			_type.GetMethod("DeleteMember").Invoke(_source, new object[] { name });
		}

		public object GetValue(string name)
		{
			return _type.GetMethod("GetValue").Invoke(_source, new object[] { name });
		}

		public bool HasMember(string name)
		{
			return (bool)_type.GetMethod("HasMember").Invoke(_source, new object[] { name });
		}

		public void SetValue(string name, object value)
		{
			_type.GetMethod("SetValue").Invoke(_source, new object[] { name, value });
		}

		public IEnumerable<string> EnumMembers()
		{
			return (IEnumerable<string>)_type.GetMethod("EnumMembers").Invoke(_source, new object[0]);
		}
	}

	class JoinedJsObject : IJsObject
	{
		IJsObject _regularScope;
		IJsObject _parentScope;

		public JoinedJsObject(IJsObject regularScope, IJsObject parentScope)
		{
			_regularScope=regularScope;
			_parentScope=parentScope;
		}

		public void DeleteMember(string name)
		{
			_parentScope.DeleteMember(name);
			_regularScope.DeleteMember(name);
		}

		public object GetValue(string name)
		{
			return _regularScope.HasMember(name)
				? _regularScope.GetValue(name)
				: _parentScope.HasMember(name)
					? _parentScope.GetValue(name)
					: null;
		}

		public bool HasMember(string name)
		{
			return _regularScope.HasMember(name)||_parentScope.HasMember(name);
		}

		public void SetValue(string name, object value)
		{
			if ((!_regularScope.HasMember(name))&&(_parentScope.HasMember(name)))
			{
				_parentScope.SetValue(name, value);
				return;
			}
			_regularScope.SetValue(name, value);

		}

		public IEnumerable<string> EnumMembers()
		{
			return _regularScope.EnumMembers().Union(_parentScope.EnumMembers());
		}
	}

	public class AssemblyRuntimeInfo
	{
		internal Version _version;
		internal Guid _moduleVersionId;
		public Version Version { get { return _version; } }
		public Guid ModuleVersionId { get { return _moduleVersionId; } }

		static AssemblyRuntimeInfo()
		{
			//Assembly asm=typeof(Executor).Assembly;
			Assembly asm=typeof(AssemblyRuntimeInfo).Assembly;
			_current=new AssemblyRuntimeInfo() { _version=asm.GetName().Version,_moduleVersionId=asm.ManifestModule.ModuleVersionId };
		}

		static AssemblyRuntimeInfo _current;
		public static AssemblyRuntimeInfo Current
		{
			get { return _current; }
		}

		public override bool Equals(object obj)
		{
			AssemblyRuntimeInfo other=obj as AssemblyRuntimeInfo;
			return other==null?false:(_version.Equals(other._version)&&_moduleVersionId.Equals(other._moduleVersionId));
		}

		public override int GetHashCode()
		{
			return _version.GetHashCode()^_moduleVersionId.GetHashCode();
		}
	}

	public class CompiledRuntimeInfo:AssemblyRuntimeInfo
	{
		internal CompiledRuntimeInfo(Assembly asm)
		{
			Type type=asm.GetType("CompileRuntimeInfo");
			_moduleVersionId=new Guid(GetFieldRawConstantValue<string>(type,"ModuleVersionId"));
			_version=new Version(GetFieldRawConstantValue<string>(type,"Version"));
			_hasRuntimeCopy=GetFieldRawConstantValue<bool>(type,"RuntimeCopied");
			_equalsCurrent=this.Equals(Current);
		}

		internal bool _hasRuntimeCopy;
		internal bool _equalsCurrent;
		public bool HasRuntimeCopy { get { return _hasRuntimeCopy; } }
		public bool EqualsCurrent { get { return _equalsCurrent; } }

		T GetFieldRawConstantValue<T>(Type type,string fieldName)
		{
			return (T)type.GetField(fieldName).GetRawConstantValue();
		}
	}
}

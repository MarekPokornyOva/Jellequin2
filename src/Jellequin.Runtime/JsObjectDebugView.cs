#region using
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
#endregion using

namespace Jellequin.Runtime.Diagnostics
{
	public class JsObjectDebugView
	{
		IJsObject _container;
		public JsObjectDebugView(IJsObject container)
		{
			_container = container;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePairDebugView[] Items
			=> _container.EnumMembers().OrderBy(x=>x).Select(x => new KeyValuePairDebugView { Key = x, Value = _container.GetValue(x) }).ToArray();

		//http://msdn.microsoft.com/en-us/library/e514eeby.aspx
		[DebuggerDisplay("{ValueToShow,nq}", Name = "{Key,nq}", Type = "{TypeToShow,nq}")]
		public class KeyValuePairDebugView
		{
			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public string Key;
			[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
			public object Value;

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public object ValueToShow
			{
				get
				{
					if (Value == null)
						return "null";
					Type t = Value.GetType();
					return t.IsValueType
						? Value
						: t == typeof(string)
							? "\"" + Value + "\""
							: typeof(Delegate).IsAssignableFrom(t) || typeof(ExternalMethodInfo) == t
								? "{function}"
								: "{object}";
				}
			}

			[DebuggerBrowsable(DebuggerBrowsableState.Never)]
			public string TypeToShow
			{
				get
				{
					if (Value == null)
						return "object";
					Type t = Value.GetType();
					return ((t.IsValueType) || (t == typeof(string))
						? ("object {" + t.Name.ToLowerInvariant() + "}")
						: (typeof(Delegate).IsAssignableFrom(t) || typeof(ExternalMethodInfo) == t
							? "function"
							: "object"));
				}
			}
		}
	}
}

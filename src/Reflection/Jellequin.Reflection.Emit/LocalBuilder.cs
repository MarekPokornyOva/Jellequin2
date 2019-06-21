#region using
using System;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class LocalBuilder
	{
		public Type LocalType { get; }
		public int Index { get; }
		public bool IsPinned { get; }
		internal LocalBuilder(Type type,int index,bool isPinned)
		{
			LocalType=type??throw new ArgumentNullException(nameof(type));
			Index=index;
			IsPinned=isPinned;
		}
	}
}

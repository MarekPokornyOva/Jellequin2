#region using
using System;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class ReflectionException:Exception
	{
		internal ReflectionException(ReflectionExceptionReason reason,params object[] data)
		{
			Reason=reason;
			ReasonData=data;
		}

		public ReflectionExceptionReason Reason { get; private set; }
		public object[] ReasonData { get; private set; }
	}
	public enum ReflectionExceptionReason { WrongCallStackGeneration, UndefinedMethodParameter, InvalidNamedProperties }
}

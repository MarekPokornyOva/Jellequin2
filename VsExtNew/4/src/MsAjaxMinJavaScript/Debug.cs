namespace System.Diagnostics
{
	public static class DebugEx
	{
		public static void Fail(string message)
		{
			Debug.Assert(false, message);
		}
	}
}

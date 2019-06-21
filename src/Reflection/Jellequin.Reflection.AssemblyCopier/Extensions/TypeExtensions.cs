namespace System
{
	static class TypeExtensions
	{
		internal static bool IsGenericTypeParameter(this Type type) => type.DeclaringType!=null;
		internal static bool IsGenericMethodParameter(this Type type) => type.DeclaringMethod!=null;
	}
}

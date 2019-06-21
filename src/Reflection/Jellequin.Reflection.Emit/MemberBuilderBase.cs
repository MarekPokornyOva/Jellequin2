using System.Reflection;

namespace Jellequin.Reflection.Emit
{
	class MemberBuilderBase:CustomAttributesContainer
	{
		internal string Name { get; }
		internal TypeBuilder DeclaringType { get; }

		internal MemberBuilderBase(TypeBuilder declaringType,string name)
		{
			DeclaringType=declaringType;
			Name=name;
		}

		internal Module Module => DeclaringType.Module;
	}
}

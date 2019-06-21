using System;

namespace Jellequin.Reflection.Emit
{
	public interface IMemberBuilderBase:ICustomAttributesContainer
	{
		string Name { get; }
		Type DeclaringType { get; }
	}
}

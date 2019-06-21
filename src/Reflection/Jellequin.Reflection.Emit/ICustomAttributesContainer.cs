#region using
using System.Collections.Generic;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public interface ICustomAttributesContainer
	{
		void SetCustomAttribute(CustomAttributeData attribute);
		object[] GetCustomAttributes(bool inherit);
		IList<CustomAttributeData> GetCustomAttributesData();
	}
}
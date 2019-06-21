using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Jellequin.Reflection.Emit
{
	class CustomAttributesContainer:ICustomAttributesContainer
	{
		readonly List<CustomAttributeData> _custAttributes = new List<CustomAttributeData>();
		public void SetCustomAttribute(CustomAttributeData attribute)
			=> _custAttributes.AddWithReturn(attribute);

		public object[] GetCustomAttributes(bool inherit)
			=> inherit ? throw new NotImplementedException() : _custAttributes.Select(x => (object)x).ToArray();

		public IList<CustomAttributeData> GetCustomAttributesData() => _custAttributes.AsReadOnly();
	}
}

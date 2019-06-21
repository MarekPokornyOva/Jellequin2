#region using
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class ParameterBuilder:ParameterInfo, ICustomAttributesContainer
	{
		readonly CustomAttributesContainer _customAttributesContainer = new CustomAttributesContainer();

		public ParameterBuilder(int position,ParameterAttributes attributes,string name,Type type)
		{
			Position=position;
			Attributes=attributes;
			Name=name??throw new ArgumentNullException(nameof(name));
			ParameterType=type??throw new ArgumentNullException(nameof(type));
		}

		public override int Position { get; }
		public override ParameterAttributes Attributes { get; }
		public override string Name { get; }
		public override Type ParameterType { get; }

		public override object[] GetCustomAttributes(bool inherit) => _customAttributesContainer.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _customAttributesContainer.GetCustomAttributesData();

		public void SetCustomAttribute(CustomAttributeData attribute) => _customAttributesContainer.SetCustomAttribute(attribute);
	}
}

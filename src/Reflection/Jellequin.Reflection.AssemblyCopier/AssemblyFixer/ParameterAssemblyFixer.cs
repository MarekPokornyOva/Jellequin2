using System;
using System.Collections.Generic;
using System.Reflection;

namespace Jellequin.Reflection.Emit.Internal
{
	internal class ParameterAssemblyFixer:ParameterInfo
	{
		readonly ParameterInfo _pi;
		readonly IAssemblyFixer _assemblyFixer;

		public ParameterAssemblyFixer(ParameterInfo parameterInfo,IAssemblyFixer assemblyFixer)
		{
			_pi=parameterInfo;
			_assemblyFixer=assemblyFixer;
		}

		public override ParameterAttributes Attributes => _pi.Attributes;

		public override IEnumerable<CustomAttributeData> CustomAttributes => _pi.CustomAttributes;

		public override object DefaultValue => _pi.DefaultValue;

		public override bool Equals(object obj) => _pi.Equals(obj);

		public override object[] GetCustomAttributes(bool inherit) => _pi.GetCustomAttributes(inherit);

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => _pi.GetCustomAttributes(attributeType,inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _pi.GetCustomAttributesData();

		public override int GetHashCode() => _pi.GetHashCode();

		public override Type[] GetOptionalCustomModifiers() => _pi.GetOptionalCustomModifiers();

		public override Type[] GetRequiredCustomModifiers() => _pi.GetRequiredCustomModifiers();

		public override bool HasDefaultValue => _pi.HasDefaultValue;

		public override bool IsDefined(Type attributeType,bool inherit) => _pi.IsDefined(attributeType,inherit);

		public override MemberInfo Member => _pi.Member;

		public override int MetadataToken => _pi.MetadataToken;

		public override string Name => _pi.Name;

		public override Type ParameterType => _assemblyFixer.FixType(_pi.ParameterType);

		public override int Position => _pi.Position;

		public override object RawDefaultValue => _pi.RawDefaultValue;

		public override string ToString() => _pi.ToString();
	}
}
#region using
using System;
using System.Collections.Generic;
using System.Reflection;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class EventBuilder:EventInfo, IMemberBuilderBase
	{
		readonly MemberBuilderBase _base;
		MethodBuilder _addOnMethod, _removeOnMethod, _raiseMethod;
		List<MethodBuilder> _otherMethods=new List<MethodBuilder>();

		public EventBuilder(TypeBuilder declaringType,string name,EventAttributes attributes,Type handlerType)
		{
			_base=new MemberBuilderBase(declaringType,name);
			Attributes=attributes;
			EventHandlerType=handlerType??throw new ArgumentNullException(nameof(handlerType));
		}

		public override EventAttributes Attributes { get; }

		public override Type DeclaringType => _base.DeclaringType;

		public override string Name => _base.Name;

		public override Type ReflectedType => throw new NotImplementedException();

		public override MethodInfo GetAddMethod(bool nonPublic) => _addOnMethod;

		public override object[] GetCustomAttributes(bool inherit) => _base.GetCustomAttributes(inherit);

		public override IList<CustomAttributeData> GetCustomAttributesData() => _base.GetCustomAttributesData();

		public override object[] GetCustomAttributes(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodInfo GetRaiseMethod(bool nonPublic) => _raiseMethod;

		public override MethodInfo GetRemoveMethod(bool nonPublic) => _removeOnMethod;

		public override bool IsDefined(Type attributeType,bool inherit) => throw new NotImplementedException();

		public override MethodInfo[] GetOtherMethods(bool nonPublic) => nonPublic ? _otherMethods.ToArray() : throw new NotImplementedException();

		public override Type EventHandlerType { get; }

		public override Module Module => _base.Module;

		public void SetAddOnMethod(MethodBuilder addOnMethod) => _addOnMethod=addOnMethod;

		public void SetRemoveOnMethod(MethodBuilder removeOnMethod) => _removeOnMethod=removeOnMethod;

		public void SetRaiseMethod(MethodBuilder raiseMethod) => _raiseMethod=raiseMethod;

		public void SetOtherMethod(MethodBuilder otherMethod) => _otherMethods.Add(otherMethod);

		public void SetCustomAttribute(CustomAttributeData attribute) => _base.SetCustomAttribute(attribute);

		public MethodBuilder AddMethodBuilder => _addOnMethod;
		public MethodBuilder RemoveMethodBuilder => _removeOnMethod;
		public MethodBuilder RaiseMethodBuilder => _raiseMethod;
		public ICollection<MethodBuilder> OtherMethodBuilders => _otherMethods.AsReadOnly();
	}
}

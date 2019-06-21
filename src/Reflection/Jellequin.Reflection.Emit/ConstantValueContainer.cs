namespace Jellequin.Reflection.Emit
{
	class ConstantValueContainer:IConstantValueMember
	{
		object _constantValue;
		public void SetConstant(object value)
		{
			HasRawConstantValue=true;
			_constantValue=value;
		}

		public bool HasRawConstantValue { get; private set; }

		public object GetRawConstantValue() => _constantValue;
	}
}

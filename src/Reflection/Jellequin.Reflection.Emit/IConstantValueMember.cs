namespace Jellequin.Reflection.Emit
{
	public interface IConstantValueMember
	{
		void SetConstant(object value);
		bool HasRawConstantValue { get; }
		object GetRawConstantValue();
	}
}

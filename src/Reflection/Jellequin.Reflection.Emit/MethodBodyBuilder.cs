using System.Reflection;

namespace Jellequin.Reflection.Emit
{
	public class MethodBodyBuilder:MethodBody
	{
		internal MethodBodyBuilder(ILGenerator ilGenerator)
			=> ILGenerator=ilGenerator;

		public ILGenerator ILGenerator { get; }
	}
}

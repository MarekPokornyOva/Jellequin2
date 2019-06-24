#region using
using System;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class ExceptionRegionInfo
	{
		internal ExceptionRegionInfo(ILGenerator iLGenerator)
		{
			ILGenerator=iLGenerator;
			TryStart=iLGenerator.DefineLabel();
			HandleStart=iLGenerator.DefineLabel();
			HandleEnd=iLGenerator.DefineLabel();
		}

		readonly ILGenerator ILGenerator;
		public Label TryStart { get; private set; }
		public Label HandleStart { get; private set; }
		public Label HandleEnd { get; private set; }
		public bool IsCatch => ExceptionType!=null;
		public Type ExceptionType { get; private set; }


		internal void MarkTryStart()
			=> ILGenerator.MarkLabel(TryStart);

		internal void MarkCatchStart(Type exceptionType)
		{
			ExceptionType=exceptionType;
			ILGenerator.MarkLabel(HandleStart);
		}

		internal void MarkFinallyStart()
			=> ILGenerator.MarkLabel(HandleStart);

		internal void MarkHandleEnd()
			=> ILGenerator.MarkLabel(HandleEnd);
	}
}

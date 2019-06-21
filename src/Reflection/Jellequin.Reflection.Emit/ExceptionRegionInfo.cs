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
			MarkTryStart();
		}

		internal ExceptionRegionInfo(ILGenerator iLGenerator,bool isCatch,Type exceptionType):this(iLGenerator)
		{
			IsCatch=isCatch;
			ExceptionType=exceptionType;
		}

		readonly ILGenerator ILGenerator;
		public Label TryStart { get; }
		public Label HandleStart { get; }
		public Label HandleEnd { get; }
		public bool IsCatch { get; private set; }
		public Type ExceptionType { get; private set; }


		internal void MarkTryStart()
			=> ILGenerator.MarkLabel(TryStart);

		internal void MarkCatchStart(Type exceptionType)
		{
			IsCatch=true;
			ExceptionType=exceptionType;
			ILGenerator.MarkLabel(HandleStart);
		}

		internal void MarkFinallyStart()
			=> ILGenerator.MarkLabel(HandleStart);

		internal void MarkHandleEnd()
			=> ILGenerator.MarkLabel(HandleEnd);
	}
}

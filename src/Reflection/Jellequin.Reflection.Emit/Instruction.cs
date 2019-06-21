#region using
using System.Collections.Generic;
using System.Reflection.Metadata;
#endregion using

namespace Jellequin.Reflection.Emit
{
	public class Instruction
	{
		public ILOpCode OpCode { get; }
		public (string DocumentName, int StartLineNumber, ushort StartColumn, int EndLineNumber, ushort EndColumn) SourceLocation { get; set; }
		public List<Label> Labels { get; } = new List<Label>();

		internal Instruction(ILOpCode opCode)
		{
			OpCode=opCode;
		}
	}

	public class Instruction<TData>:Instruction
	{
		public TData Data { get; }

		internal Instruction(ILOpCode opCode,TData data) : base(opCode)
		{
			Data=data;
		}
	}
}

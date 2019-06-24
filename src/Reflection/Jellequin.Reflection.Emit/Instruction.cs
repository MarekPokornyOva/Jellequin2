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
		List<Label> _labels = new List<Label>();
		public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();

		internal Instruction(ILOpCode opCode)
		{
			OpCode=opCode;
		}

		internal void AddLabel(Label label)
			=> _labels.Add(label);
		internal void AddLabels(IEnumerable<Label> labels)
			=> _labels.AddRange(labels);
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

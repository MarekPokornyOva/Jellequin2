namespace Jellequin.Reflection.Emit
{
	public class SymbolWriter
	{
		public string DocumentName { get; }

		internal SymbolWriter(string documentName)
		{
			DocumentName=documentName;
		}
	}
}

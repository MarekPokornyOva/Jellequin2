#region using
using System.IO;
using System.Text;
#endregion using

namespace Jellequin.Compiler
{
	public interface ISource
	{
		string GetText();
		string GetEmbedFilename();
		byte[] GetBytes();
	}

	public class StringSource:ISource
	{
		string _code;
		string _embedFilename;

		internal static string EmbedFilename = "source.jell";

		public StringSource(string code):this(code, EmbedFilename)
		{ }

		public StringSource(string code, string embedFilename)
		{
			_code = code;
			_embedFilename = embedFilename;
		}

		public string GetText() => _code;

		public string GetEmbedFilename() => _embedFilename;

		public byte[] GetBytes() => Encoding.UTF8.GetBytes(_code);
	}

	public class FileSource : ISource
	{
		string _path;
		public FileSource(string path)
		{
			_path = path;
		}

		public byte[] GetBytes() => File.ReadAllBytes(_path);

		public string GetEmbedFilename() => _path;

		public string GetText() => File.ReadAllText(_path);
	}
}

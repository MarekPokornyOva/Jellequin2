#region using
//using Microsoft.CodeAnalysis.Text;
using System;
#endregion using

namespace Jellequin.Compiler
{
	class HashAlgorithmGuids
	{
		public static readonly Guid Md5 = new Guid("406ea660-64cf-4c82-b6f0-42d48172a799");
		public static readonly Guid Sha1 = new Guid("ff1816ec-aa5e-4d10-87f7-6f4963833460");
		public static readonly Guid Sha256 = new Guid("8829d00f-11b8-4213-878b-770e8597ac16");

		/*public static Guid FromSourceHashAlgorithm(SourceHashAlgorithm algorithm)
		{
			switch (algorithm)
			{
				case SourceHashAlgorithm.Sha1:
					return Sha1;

				case SourceHashAlgorithm.Sha256:
					return Sha256;

				default:
					return null;
			}
		}*/
	}
}

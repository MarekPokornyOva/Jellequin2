namespace Microsoft.CodeAnalysis.Text
{
	/// <summary>
	/// Specifies a hash algorithms used for hashing source files.
	/// </summary>
	public enum SourceHashAlgorithm
	{
		/// <summary>
		/// No algorithm specified.
		/// </summary>
		None,
		/// <summary>
		/// Secure Hash Algorithm 1.
		/// </summary>
		Sha1,
		/// <summary>
		/// Secure Hash Algorithm 2 with a hash size of 256 bits.
		/// </summary>
		Sha256
	}
}

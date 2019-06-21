namespace Microsoft.CodeAnalysis
{
	// Microsoft.CodeAnalysis.EmbeddedText
	using Microsoft.Cci;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.Text;
	using Roslyn.Utilities;
	using System;
	using System.Collections.Immutable;
	using System.IO;
	using System.IO.Compression;
	using System.Reflection.Metadata;
	using System.Security.Cryptography;
	using System.Threading;
	using System.Threading.Tasks;

	/// <summary>
	/// Represents text to be embedded in a PDB.
	/// </summary>
	public sealed class EmbeddedText
	{
		private sealed class CountingDeflateStream:DeflateStream
		{
			public int BytesWritten
			{
				get;
				private set;
			}

			public CountingDeflateStream(Stream stream,CompressionLevel compressionLevel,bool leaveOpen)
				: base(stream,compressionLevel,leaveOpen)
			{
			}

			public override void Write(byte[] array,int offset,int count)
			{
				base.Write(array,offset,count);
				checked
				{
					BytesWritten+=count;
				}
			}

			public override void WriteByte(byte value)
			{
				base.WriteByte(value);
				checked
				{
					BytesWritten++;
				}
			}

			public override Task WriteAsync(byte[] buffer,int offset,int count,CancellationToken cancellationToken)
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// The maximum number of bytes in to write out uncompressed.
		///
		/// This prevents wasting resources on compressing tiny files with little to negative gain
		/// in PDB file size.
		///
		/// Chosen as the point at which we start to see &gt; 10% blob size reduction using all
		/// current source files in corefx and roslyn as sample data. 
		/// </summary>
		internal const int CompressionThreshold = 200;

		/// <summary>
		/// The path to the file to embed.
		/// </summary>
		/// <remarks>See remarks of <see cref="P:Microsoft.CodeAnalysis.SyntaxTree.FilePath" /></remarks>
		public string FilePath
		{
			get;
		}

		/// <summary>
		/// Hash algorithm to use to calculate checksum of the text that's saved to PDB.
		/// </summary>
		public SourceHashAlgorithm ChecksumAlgorithm
		{
			get;
		}

		/// <summary>
		/// The <see cref="P:Microsoft.CodeAnalysis.EmbeddedText.ChecksumAlgorithm" /> hash of the uncompressed bytes
		/// that's saved to the PDB.
		/// </summary>
		public ImmutableArray<byte> Checksum
		{
			get;
		}

		/// <summary>
		/// The content that will be written to the PDB.
		/// </summary>
		/// <remarks>
		/// Internal since this is an implementation detail. The only public
		/// contract is that you can pass EmbeddedText instances to Emit.
		/// It just so happened that doing this up-front was most practical
		/// and efficient, but we don't want to be tied to it.
		///
		/// For efficiency, the format of this blob is exactly as it is written
		/// to the PDB,which prevents extra copies being made during emit.
		///
		/// The first 4 bytes (little endian int32) indicate the format:
		///
		///            0: data that follows is uncompressed
		///     Positive: data that follows is deflate compressed and value is original, uncompressed size
		///     Negative: invalid at this time, but reserved to mark a different format in the future.
		/// </remarks>
		internal ImmutableArray<byte> Blob
		{
			get;
		}

		private EmbeddedText(string filePath,ImmutableArray<byte> checksum,SourceHashAlgorithm checksumAlgorithm,ImmutableArray<byte> blob)
		{
			FilePath=filePath;
			Checksum=checksum;
			ChecksumAlgorithm=checksumAlgorithm;
			Blob=blob;
		}

		/// <summary>
		/// Constructs an <see cref="T:Microsoft.CodeAnalysis.EmbeddedText" /> from stream content.
		/// </summary>
		/// <param name="filePath">The file path (pre-normalization) to use in the PDB.</param>
		/// <param name="stream">The stream.</param>
		/// <param name="checksumAlgorithm">Hash algorithm to use to calculate checksum of the text that's saved to PDB.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="filePath" /> is null.
		/// <paramref name="stream" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="filePath" /> is empty.
		/// <paramref name="stream" /> doesn't support reading or seeking.
		/// <paramref name="checksumAlgorithm" /> is not supported.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
		/// <remarks>Reads from the beginning of the stream. Leaves the stream open.</remarks>
		public static EmbeddedText FromStream(string filePath,Stream stream,SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
		{
			ValidateFilePath(filePath);
			if (stream==null)
			{
				throw new ArgumentNullException("stream");
			}
			if (!stream.CanRead||!stream.CanSeek)
			{
				throw new ArgumentException("stream - Stream must support read and seek");
			}
			return new EmbeddedText(filePath,CalculateChecksum(stream,checksumAlgorithm),checksumAlgorithm,CreateBlob(stream));
		}

		static ImmutableArray<byte> CalculateChecksum(Stream stream,SourceHashAlgorithm algorithmId)
		{
			using (HashAlgorithm algorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId))
			{
				if (stream.CanSeek)
				{
					stream.Seek(0L,SeekOrigin.Begin);
				}
				return ImmutableArray.Create(algorithm.ComputeHash(stream));
			}
		}

		static ImmutableArray<byte> CalculateChecksum(byte[] buffer,int offset,int count,SourceHashAlgorithm algorithmId)
		{
			using (HashAlgorithm algorithm = CryptographicHashProvider.TryGetAlgorithm(algorithmId))
			{
				return ImmutableArray.Create(algorithm.ComputeHash(buffer,offset,count));
			}
		}

		static class CryptographicHashProvider
		{
			internal static HashAlgorithm TryGetAlgorithm(object algorithmId)
			{
				switch (algorithmId)
				{
					case SourceHashAlgorithm.Sha1:
						return SHA1.Create();
					case SourceHashAlgorithm.Sha256:
						return SHA256.Create();
					default:
						return null;
				}
			}
		}

		/// <summary>
		/// Constructs an <see cref="T:Microsoft.CodeAnalysis.EmbeddedText" /> from bytes.
		/// </summary>
		/// <param name="filePath">The file path (pre-normalization) to use in the PDB.</param>
		/// <param name="bytes">The bytes.</param>
		/// <param name="checksumAlgorithm">Hash algorithm to use to calculate checksum of the text that's saved to PDB.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// <paramref name="bytes" /> is default-initialized.
		/// <paramref name="filePath" /> is null.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// <paramref name="filePath" /> is empty.
		/// <paramref name="checksumAlgorithm" /> is not supported.
		/// </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
		/// <remarks>Reads from the beginning of the stream. Leaves the stream open.</remarks>
		public static EmbeddedText FromBytes(string filePath,ArraySegment<byte> bytes,SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1)
		{
			ValidateFilePath(filePath);
			if (bytes.Array==null)
			{
				throw new ArgumentNullException("bytes");
			}
			return new EmbeddedText(filePath,CalculateChecksum(bytes.Array,bytes.Offset,bytes.Count,checksumAlgorithm),checksumAlgorithm,CreateBlob(bytes));
		}

		/// <exception cref="T:System.ArgumentNullException"><paramref name="filePath" /> is null.</exception>
		/// <exception cref="T:System.ArgumentException"><paramref name="filePath" /> is empty.</exception>
		private static void ValidateFilePath(string filePath)
		{
			if (filePath==null)
				throw new ArgumentNullException("filePath");
			if (filePath.Length==0)
				throw new ArgumentException("filePath - Argument cannot be empty");
		}

		/// <summary>
		/// Creates the blob to be saved to the PDB.
		/// </summary>
		internal static ImmutableArray<byte> CreateBlob(Stream stream)
		{
			long length2 = stream.Length;
			if (length2>int.MaxValue)
			{
				throw new IOException("Stream is too long");
			}
			stream.Seek(0L,SeekOrigin.Begin);
			int length = (int)length2;
			if (length<200)
			{
				BlobBuilder pooledBlobBuilder = new BlobBuilder();
				pooledBlobBuilder.WriteInt32(0);
				int bytesWritten = pooledBlobBuilder.TryWriteBytes(stream,length);
				if (length!=bytesWritten)
				{
					throw new EndOfStreamException();
				}
				return pooledBlobBuilder.ToImmutableArray();
			}
			using (BlobBuildingStream builder = BlobBuildingStream.GetInstance())
			{
				builder.WriteInt32(length);
				using (CountingDeflateStream deflater = new CountingDeflateStream(builder,CompressionLevel.Optimal,leaveOpen: true))
				{
					stream.CopyTo(deflater);
					if (length!=deflater.BytesWritten)
					{
						throw new EndOfStreamException();
					}
				}
				return builder.ToImmutableArray();
			}
		}

		internal static ImmutableArray<byte> CreateBlob(ArraySegment<byte> bytes)
		{
			if (bytes.Count<200)
			{
				BlobBuilder pooledBlobBuilder = new BlobBuilder();
				pooledBlobBuilder.WriteInt32(0);
				pooledBlobBuilder.WriteBytes(bytes.Array,bytes.Offset,bytes.Count);
				return pooledBlobBuilder.ToImmutableArray();
			}
			using (BlobBuildingStream builder = BlobBuildingStream.GetInstance())
			{
				builder.WriteInt32(bytes.Count);
				using (CountingDeflateStream deflater = new CountingDeflateStream(builder,CompressionLevel.Optimal,leaveOpen: true))
				{
					deflater.Write(bytes.Array,bytes.Offset,bytes.Count);
				}
				return builder.ToImmutableArray();
			}
		}
	}
}

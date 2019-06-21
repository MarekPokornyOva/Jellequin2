namespace Roslyn.Utilities
{
	// Roslyn.Utilities.BlobBuildingStream
	//using Microsoft.CodeAnalysis.PooledObjects;
	//using Roslyn.Utilities;
	using System;
	using System.Collections.Immutable;
	using System.IO;
	using System.Reflection.Metadata;

	/// <summary>
	/// A write-only memory stream backed by a <see cref="T:System.Reflection.Metadata.BlobBuilder" />.
	/// </summary>
	internal sealed class BlobBuildingStream:Stream
	{
		private BlobBuilder _builder;

		/// <summary>
		/// The chunk size to be used by the underlying BlobBuilder.
		/// </summary>
		/// <remarks>
		/// The current single use case for this type is embedded sources in PDBs.
		///
		/// 32 KB is:
		///
		/// * Large enough to handle 99.6% all VB and C# files in Roslyn and CoreFX 
		///   without allocating additional chunks.
		///
		/// * Small enough to avoid the large object heap.
		///
		/// * Large enough to handle the files in the 0.4% case without allocating tons
		///   of small chunks. Very large source files are often generated in build
		///   (e.g. Syntax.xml.Generated.vb is 390KB compressed!) and those are actually
		///   attractive candidates for embedding, so we don't want to discount the large
		///   case too heavily.)
		///
		/// * We pool the outer BlobBuildingStream but only retain the first allocated chunk.
		/// </remarks>
		public const int ChunkSize = 32768;

		public override bool CanWrite => true;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override long Length => _builder.Count;

		public override long Position
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public static BlobBuildingStream GetInstance()
		{
			return new BlobBuildingStream();
		}

		private BlobBuildingStream()
		{
			_builder=new BlobBuilder(32768);
		}

		public override void Write(byte[] buffer,int offset,int count)
		{
			_builder.WriteBytes(buffer,offset,count);
		}

		public override void WriteByte(byte value)
		{
			_builder.WriteByte(value);
		}

		public void WriteInt32(int value)
		{
			_builder.WriteInt32(value);
		}

		public Blob ReserveBytes(int byteCount)
		{
			return _builder.ReserveBytes(byteCount);
		}

		public ImmutableArray<byte> ToImmutableArray()
		{
			return _builder.ToImmutableArray();
		}

		public void Free()
		{
			_builder.Clear();
		}

		public override void Flush()
		{
		}

		protected override void Dispose(bool disposing)
		{
			Free();
		}

		public override long Seek(long offset,SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer,int offset,int count)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}
	}
}

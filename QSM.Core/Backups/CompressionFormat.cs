using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSM.Core.Backups
{
	/// <summary>
	/// Compression algorithms
	/// </summary>
	public enum CompressionFormat
	{
		/// <summary>
		/// Applicable to both zip and tar archive format.
		/// </summary>
		None,
		/// <summary>
		/// For use with both zip and tar archive format.
		/// 
		/// Could also refer to GZip, which is based on the Deflate algorithm.
		/// https://en.wikipedia.org/wiki/Deflate
		/// </summary>
		Deflate,
		/// <summary>
		/// For use with both .zip and tar.bz2 archive format.
		/// 
		/// https://en.wikipedia.org/wiki/Bzip2
		/// </summary>
		BZip2,
		/// <summary>
		/// For use with zip/tar.lzma format.
		/// </summary>
		LZMA,
		/// <summary>
		/// For use with the zip archive format.
		/// 
		/// https://en.wikipedia.org/wiki/Prediction_by_partial_matching
		/// </summary>
		PPMd,
		/// <summary>
		/// For use with both zip and tar archive format.
		/// 
		/// https://en.wikipedia.org/wiki/Zstd
		/// </summary>
		Zstd
	}
}

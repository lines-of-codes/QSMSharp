using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;

namespace QSM.Core.Backups;

public static class ZipArchiver
{
	public static readonly CompressionFormat[] SupportedCompression =
	[
		CompressionFormat.None,
		CompressionFormat.Deflate,
		CompressionFormat.BZip2,
		CompressionFormat.LZMA,
		CompressionFormat.PPMd
	];

	public static void CompressDirectory(string folderPath, string dest, CompressionFormat compressionFormat)
	{
		using ZipArchive archive = ZipArchive.Create();
		archive.AddAllFromDirectory(folderPath);
		archive.SaveTo(dest, new WriterOptions(compressionFormat switch
		{
			CompressionFormat.None => CompressionType.None,
			CompressionFormat.Deflate => CompressionType.Deflate,
			CompressionFormat.BZip2 => CompressionType.BZip2,
			CompressionFormat.LZMA => CompressionType.LZMA,
			CompressionFormat.PPMd => CompressionType.PPMd,
			_ => throw new InvalidOperationException()
		}));
	}
}
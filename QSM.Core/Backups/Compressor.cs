using System.Text;

namespace QSM.Core.Backups;

public class Compressor
{
	public static string GetFileExtension(ArchiveFormat archiveFormat, CompressionFormat compressionFormat)
	{
		switch (archiveFormat)
		{
			case ArchiveFormat.Zip:
				return ".zip";
			case ArchiveFormat.Tar:
				StringBuilder sb = new();

				sb.Append(".tar");
				sb.Append(compressionFormat switch
				{
					CompressionFormat.Deflate => ".gz",
					CompressionFormat.BZip2 => ".bz2",
					CompressionFormat.LZMA => ".lzma",
					CompressionFormat.Zstd => ".zst",
					_ => ""
				});

				return sb.ToString();
			default:
				throw new InvalidOperationException();
		}
	}

	public static async Task CompressFolderAsync(string folderPath, string destinationPath, ArchiveFormat archiveFormat,
		CompressionFormat compressionFormat)
	{
		switch (archiveFormat)
		{
			case ArchiveFormat.Zip:
				await Task.Run(() => ZipArchiver.CompressDirectory(folderPath, destinationPath, compressionFormat));
				break;
			case ArchiveFormat.Tar:
				await TarArchiver.CompressTarAsync(folderPath, destinationPath, compressionFormat);
				break;
			default:
				throw new NotImplementedException();
		}
	}
}
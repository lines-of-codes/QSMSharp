﻿using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Writers;
using System.Formats.Tar;
using System.IO.Pipelines;
using ZstdSharp;
using ZstdSharp.Unsafe;

namespace QSM.Core.Backups;

public static class TarArchiver
{
	public static readonly CompressionFormat[] SupportedCompression =
	[
		CompressionFormat.None,
		CompressionFormat.Deflate,
		CompressionFormat.BZip2,
		CompressionFormat.LZMA,
		CompressionFormat.Zstd
	];

	public static async Task CompressTarAsync(string folderPath, string dest, CompressionFormat compressionFormat)
	{
		switch (compressionFormat)
		{
			case CompressionFormat.None:
				await TarFile.CreateFromDirectoryAsync(folderPath, dest, false);
				break;
			case CompressionFormat.Deflate:
			case CompressionFormat.BZip2:
			case CompressionFormat.LZMA:
				SharpCompressTar(folderPath, dest, compressionFormat);
				break;
			case CompressionFormat.Zstd:
				await ZstdCompressTarAsync(folderPath, dest);
				break;
			default:
				throw new InvalidOperationException();
		}
	}

	public static async Task CreateTarArchiveAsync(string sourceDirectory, PipeWriter writer)
	{
		await TarFile.CreateFromDirectoryAsync(sourceDirectory, writer.AsStream(), false);
		await writer.CompleteAsync();
	}

	public static void SharpCompressTar(string folderPath, string dest, CompressionFormat format)
	{
		using TarArchive archive = TarArchive.Create();
		archive.AddAllFromDirectory(folderPath);
		archive.SaveTo(
			File.Create(dest),
			new WriterOptions(format switch
			{
				CompressionFormat.Deflate => CompressionType.GZip,
				CompressionFormat.BZip2 => CompressionType.BZip2,
				CompressionFormat.LZMA => CompressionType.LZMA,
				_ => throw new InvalidOperationException()
			}) { LeaveStreamOpen = false });
	}

	public static async Task ZstdCompressTarAsync(string sourceDirectory, string outputFilePath,
		byte compressionLevel = 11)
	{
		using FileStream outputFileStream = File.Create(outputFilePath);
		using CompressionStream compressionStream = new(outputFileStream, compressionLevel);
		Pipe pipe = new();

		compressionStream.SetParameter(ZSTD_cParameter.ZSTD_c_nbWorkers, Environment.ProcessorCount);

		Task tarTask = CreateTarArchiveAsync(sourceDirectory, pipe.Writer);
		Task compressTask = pipe.Reader.CopyToAsync(compressionStream);

		await Task.WhenAll(tarTask, compressTask);
	}
}
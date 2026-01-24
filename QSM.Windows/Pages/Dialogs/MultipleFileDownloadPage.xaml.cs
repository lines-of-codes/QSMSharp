using Microsoft.UI.Xaml.Controls;
using QSM.Core;
using QSM.Core.ModPluginSource;
using QSM.Core.Utilities;
using QSM.Windows.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Dialogs;

public struct FileDownloadEntry
{
	public string FileName = string.Empty;
	public double Percentage = 0;
	public long TotalBytes = 0;
	public long TotalBytesRead = 0;
	internal string ProgressText = string.Empty;
	internal bool IsIndeterminate = true;

	public FileDownloadEntry() { }
}

/// <summary>
/// A page designed to display download progress of multiple files.
/// </summary>
public sealed partial class MultipleFileDownloadPage : Page
{
	const byte ConcurrentDownloads = 5;
	private static readonly HttpClient _httpClient = new();
	private readonly ObservableCollection<FileDownloadEntry> Files = [];
	private readonly Queue<byte> _indexQueue = new([0, 1, 2, 3, 4]);

	public MultipleFileDownloadPage()
	{
		InitializeComponent();
	}

	public MultipleFileDownloadPage(Queue<ModPluginDownloadInfo> downloads, string folderPath)
	{
		InitializeComponent();
		DownloadMods(downloads, folderPath);
	}

	public Task DownloadMods(Queue<ModPluginDownloadInfo> mods, string folderPath)
	{
		Queue<FileDownloadRequest> files = new(mods.Select(mod => new FileDownloadRequest()
		{
			Destination = Path.Combine(folderPath, mod.FileName),
			DownloadLocations = [mod.DownloadUri],
			Hash = mod.Hash,
			HashAlgorithm = mod.HashAlgorithm
		}));

		return DownloadFiles(files);
	}

	// I actually don't know how this works
	public Task DownloadFiles(Queue<FileDownloadRequest> downloads)
	{
		List<Task> tasks = [];
		int initialQueueSize = downloads.Count;
		SemaphoreSlim semaphore = new(ConcurrentDownloads > initialQueueSize ? initialQueueSize : ConcurrentDownloads);

		for (byte i = 0; i < initialQueueSize && i < ConcurrentDownloads; i++)
		{
			Files.Add(new FileDownloadEntry());
		}

		while (downloads.Count != 0)
		{
			var download = downloads.Dequeue();
			var count = downloads.Count;

			tasks.Add(Task.Run(async () =>
			{
				await semaphore.WaitAsync();
				byte index;

				lock (_indexQueue)
				{
					index = _indexQueue.Dequeue();
				}

				try
				{
					foreach (var uri in download.DownloadLocations)
					{
						var downloadResult = await DownloadFileAsync(uri, download.Destination, index);

						if (!downloadResult)
							continue;

						var localHash = Hasher.GetFileHash(download.HashAlgorithm, download.Destination);

						// If the hash of the downloaded file doesn't match the hash provided from file provider(s)...
						if (localHash != download.Hash)
						{
							Log.Error($"File {Path.GetFileName(download.Destination)} hash checking failed. Expected {download.HashAlgorithm} \"{download.Hash}\", Got \"{localHash}\"");
							File.Delete(download.Destination);
							continue;
						}

						break;
					}
				}
				finally
				{
					lock (_indexQueue)
					{
						_indexQueue.Enqueue(index);
					}
					semaphore.Release();
				}
			}));
		}

		return Task.WhenAll(tasks);
	}

	/// <summary>
	/// Downloads a file and report progress to the interface.
	/// </summary>
	/// <param name="fileUrl">The URL to download the file</param>
	/// <param name="dest">The destination where the file will be saved to</param>
	/// <param name="index">The index in the download queue of this function.</param>
	/// <returns>Returns true if download is successful.</returns>
	private async Task<bool> DownloadFileAsync(string fileUrl, string dest, byte index)
	{
		using HttpResponseMessage response = await _httpClient.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);

		if (!response.IsSuccessStatusCode)
		{
			return false;
		}

		long totalBytes = response.Content.Headers.ContentLength ?? -1L;

		using var contentStream = await response.Content.ReadAsStreamAsync();
		using var fileStream = File.Create(dest);

		var buffer = new byte[8192];
		long totalBytesRead = 0;
		int bytesRead;
		double percentage = 0;

		var entry = Files[index];

		entry.FileName = Path.GetFileName(dest);

		if (totalBytes != -1)
		{
			entry.IsIndeterminate = false;
		}

		DispatcherQueue.TryEnqueue(() => Files[index] = entry);

		while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
		{
			await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
			totalBytesRead += bytesRead;

			if (totalBytes != -1)
			{
				percentage = (double)totalBytesRead / totalBytes * 100;

				entry.Percentage = percentage;
				entry.TotalBytesRead = totalBytesRead;
				entry.TotalBytes = totalBytes;
				entry.ProgressText = $"Downloaded {SizeUnitConversion.bytesToAppropriateUnit(totalBytesRead)} of {SizeUnitConversion.bytesToAppropriateUnit(totalBytes)} ({percentage:0.00}%)";

				DispatcherQueue.TryEnqueue(() => Files[index] = entry);
			}
		}

		return true;
	}
}

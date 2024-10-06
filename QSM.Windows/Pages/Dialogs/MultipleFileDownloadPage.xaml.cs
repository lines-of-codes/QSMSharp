using Microsoft.UI.Xaml.Controls;
using QSM.Core.ModPluginSource;
using QSM.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MultipleFileDownloadPage : Page
{
    const byte ConcurrentDownloads = 5;
    private ObservableCollection<FileDownloadEntry> Files = 
    [
        new FileDownloadEntry(),
        new FileDownloadEntry(),
        new FileDownloadEntry(),
        new FileDownloadEntry(),
        new FileDownloadEntry()
    ];

    public MultipleFileDownloadPage()
    {
        InitializeComponent();
    }

    public MultipleFileDownloadPage(Queue<ModPluginDownloadInfo> downloads, string folderPath)
    {
        InitializeComponent();
        DownloadMods(downloads, folderPath);
    }

    // I actually don't know how this works
    public Task DownloadMods(Queue<ModPluginDownloadInfo> downloads, string folderPath)
    {
        List<Task> tasks = [];
        SemaphoreSlim semaphore = new(ConcurrentDownloads);
        var indexQueue = new Queue<byte>(new byte[] { 0, 1, 2, 3, 4 });

        int initialQueueSize = downloads.Count;
        for (byte i = 0; i < initialQueueSize; i++)
        {
            var download = downloads.Dequeue();

            tasks.Add(Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    byte index;
                    lock (indexQueue)
                    {
                        index = indexQueue.Dequeue();
                    }
                    await DownloadFileAsync(download.DownloadUri, Path.Combine(folderPath, download.FileName), i);
                }
                finally
                {
                    lock (indexQueue)
                    {
                        indexQueue.Enqueue((byte)(tasks.Count % 5));
                    }
                    semaphore.Release();
                }
            }));
        }

        return Task.WhenAll(tasks);
    }

    private async Task DownloadFileAsync(string fileUrl, string dest, byte index)
    {
        using HttpClient client = new();

        using HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

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

        Files[index] = entry;

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

                Files[index] = entry;
            }
        }

        Files.RemoveAt(index);
    }
}

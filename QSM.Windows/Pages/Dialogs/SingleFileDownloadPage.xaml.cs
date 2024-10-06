using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Windows.Utilities;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SingleFileDownloadPage : Page
{
    public SingleFileDownloadPage()
    {
        this.InitializeComponent();
    }

    public void SetIsIndeterminate(bool indeterminate)
    {
        DownloadProgressBar.IsIndeterminate = indeterminate;
    }

	public void SetOperation(string operation)
	{
		DownloadProgressText.Text = operation;
	}

    public void UpdateProgress(double percentage, long totalBytesRead, long totalBytes)
    {
        DownloadProgressBar.Value = percentage;
        DownloadProgressText.Text = $"Downloaded {SizeUnitConversion.bytesToAppropriateUnit(totalBytesRead)} of {SizeUnitConversion.bytesToAppropriateUnit(totalBytes)} ({percentage:0.00}%)";
    }

    public void DownloadComplete()
    {
        DownloadProgressText.Text = "Download complete!";
    }

	public async Task DownloadFileAsync(string fileUrl, string dest)
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

		if (totalBytes != -1)
		{
			SetIsIndeterminate(false);
		}

		while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
		{
			await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
			totalBytesRead += bytesRead;

			if (totalBytes != -1)
			{
				percentage = (double)totalBytesRead / totalBytes * 100;
				UpdateProgress(percentage, totalBytesRead, totalBytes);
			}
		}

		DownloadComplete();
	}

	public ContentDialog CreateDialog(Page page)
	{
		ContentDialog dialog = new()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "Downloading a file...",
			IsPrimaryButtonEnabled = false,
			IsSecondaryButtonEnabled = false,
			DefaultButton = ContentDialogButton.Primary,
			Content = this
		};

		return dialog;
	}
}

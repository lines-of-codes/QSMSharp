using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using QSM.Core.JavaProvider;
using QSM.Core.Utilities;
using QSM.Windows.Pages.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class JavaDownloadPage : Page
{
	private static readonly Dictionary<string, IJavaProvider> Providers = new()
	{
		{ "Azul Zulu", Program.Hoster.Services.GetService<AzulProvider>() },
		{ "Eclipse Temurin", Program.Hoster.Services.GetService<AdoptiumProvider>() }
	};

	string _selectedProviderName = "Azul Zulu";
	Dictionary<string, int> _majorReleaseIntMapping = [];
	ExtendedObservableCollection<string> _majorReleases = [];
	ExtendedObservableCollection<string> _jreReleases = [];

	public JavaDownloadPage()
	{
		this.InitializeComponent();
	}

	private async void JavaProviderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		_selectedProviderName = (string)JavaProviderSelector.SelectedItem;
		var provider = Providers[_selectedProviderName];

		_majorReleaseIntMapping = await provider.GetAvailableReleasesAsync();
		_majorReleases.Clear();
		_majorReleases.AddRange(_majorReleaseIntMapping.Keys);
		MajorReleaseSelector.SelectedIndex = 0;

		if (provider.Terms == null)
			TermsText.Visibility = Visibility.Collapsed;

		if (TermsText.Inlines.Count > 1)
			TermsText.Inlines.RemoveAt(TermsText.Inlines.Count - 1);

		Hyperlink hyperlink = new();
		Run run = new();

		run.Text = provider.Terms;
		hyperlink.NavigateUri = new Uri(provider.Terms);

		hyperlink.Inlines.Add(run);
		TermsText.Inlines.Add(hyperlink);
	}

	private async void MajorReleaseSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (MajorReleaseSelector.SelectedItem == null)
			return;

		_jreReleases.Clear();
		_jreReleases.AddRange(await Providers[_selectedProviderName].ListJREAsync(_majorReleaseIntMapping[(string)MajorReleaseSelector.SelectedItem]));
		JreReleaseSelector.SelectedIndex = 0;
	}

	private async void DownloadButton_Click(object sender, RoutedEventArgs e)
	{
		var downloadPage = new SingleFileDownloadPage();

		var dialog = downloadPage.CreateDialog(this);

		_ = dialog.ShowAsync();

		var selectedRelease = (string)JreReleaseSelector.SelectedItem;
		var javaZip = Path.Combine(ApplicationData.DownloadFolderPath, $"{_selectedProviderName}_{selectedRelease}.zip");

		var downloadInfo = await Providers[_selectedProviderName].GetDownloadUrlAsync(selectedRelease);
		await downloadPage.DownloadFileAsync(downloadInfo.Url, javaZip);

		downloadPage.SetOperation("Verifying file...");
		downloadPage.SetIsIndeterminate(true);

		var localHash = Hasher.GetFileHash(downloadInfo.HashAlgorithm, javaZip);
		if (localHash != downloadInfo.Hash)
		{
			Log.Warning("Hash \"{LocalHash}\" does not match \"{DownloadHash}\" for file \"{JavaZip}\"", localHash, downloadInfo.Hash, javaZip);
			File.Delete(javaZip);
			dialog.Hide();
			var infoDialog = new InfoDialog("The integrity of the file cannot be verified, The expected hash doesn't match the actual hash.");
			await infoDialog.CreateDialog("Verification Failed", this).ShowAsync();
			return;
		}

		downloadPage.SetOperation("Extracting...");

		string javaFolderName = string.Empty;
		using (ZipArchive archive = await ZipFile.OpenReadAsync(javaZip))
		{
			// likely the folder containing everything
			javaFolderName = archive.Entries[0].FullName;
		}

		var extractedFolder = Path.Combine(ApplicationData.JavaInstallsPath, javaFolderName);

		if (Directory.Exists(extractedFolder))
		{
			dialog.Hide();
			File.Delete(javaZip);
			Log.Warning("Attempted to install Java, Folder \"{ExtractedFolder}\" already exists.", extractedFolder);
			var infoDialog = new InfoDialog("The Java installation already exists, Please delete that first. (Or perhaps you meant to import?)");
			await infoDialog.CreateDialog("Folder already exists", this).ShowAsync();
			return;
		}

		await ZipFile.ExtractToDirectoryAsync(javaZip, ApplicationData.JavaInstallsPath);

		if (!Directory.Exists(extractedFolder))
			throw new FileNotFoundException();

		ApplicationData.Configuration.JavaInstallations.Add(extractedFolder);
		ApplicationData.SaveConfiguration();

		File.Delete(javaZip);

		dialog.Hide();
	}
}

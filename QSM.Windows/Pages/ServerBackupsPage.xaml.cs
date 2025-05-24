using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.Backups;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages.Dialogs;
using QSM.Windows.Utilities;
using System;
using System.IO;
using Visus.Cuid;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerBackupsPage : Page
{
	int _metadataIndex;
	ServerMetadata _metadata;
	private readonly ExtendedObservableCollection<BackupItem> _backups = [];

	public ServerBackupsPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		_metadata = ApplicationData.Configuration.Servers[_metadataIndex];
		_backups.AddRange(ApplicationData.ServerSettings[_metadata.Guid].Backups);

		base.OnNavigatedTo(e);
	}

	// skipcq: CS-R1005
	private async void NewBackupButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		var createPage = new BackupCreationPage();

		var dialog = createPage.CreateDialog(this);

		var result = await dialog.ShowAsync();

		if (result != ContentDialogResult.Primary)
			return;

		string name = createPage.BackupName;
		ArchiveFormat archive = createPage.SelectedArchiveFormat;
		CompressionFormat compression = createPage.SelectedCompression;

		Cuid2 cuid = new(BackupItem.DefaultCuidLength);

		string backupFileName =
			Path.Combine(
				ApplicationData.BackupsFolderPath,
				StringUtility.TurnIntoValidFileName($"{_metadata.Name}_{cuid}_{name}")
				+ Compressor.GetFileExtension(archive, compression));

		var progressPage = new SingleFileDownloadPage();

		dialog = progressPage.CreateDialog(this, "Compressing files...");

		_ = dialog.ShowAsync();

		progressPage.SetOperation("Compressing files...");

		await Compressor.CompressFolderAsync(
			_metadata.ServerPath,
			backupFileName,
			archive,
			compression);

		dialog.Hide();

		var backupItem = new BackupItem(
			cuid,
			name,
			new Uri(backupFileName));

		ApplicationData.ServerSettings[_metadata.Guid].Backups.Add(backupItem);
		await ApplicationData.ServerSettings[_metadata.Guid].SaveJsonAsync(_metadata.QsmConfigFile);
		_backups.Add(backupItem);
	}

	// skipcq: CS-R1005
	private async void DeleteBackupButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		if (BackupList.SelectedItem == null) return;

		var selected = (BackupItem)BackupList.SelectedItem;

		if (selected.Uri.Scheme == "file")
		{
			ApplicationData.ServerSettings[_metadata.Guid].Backups.Remove(selected);
			await ApplicationData.ServerSettings[_metadata.Guid].SaveJsonAsync(_metadata.QsmConfigFile);
			_backups.Remove(selected);
			File.Delete(selected.Uri.LocalPath);
		}
	}
}

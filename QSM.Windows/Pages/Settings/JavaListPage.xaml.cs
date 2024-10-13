using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Windows.Pages.Dialogs;
using QSM.Windows.Utilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class JavaListPage : Page
{
	ExtendedObservableCollection<JavaInstallation> JavaInstallations = [];

	public JavaListPage()
	{
		this.InitializeComponent();
		foreach (string path in ApplicationData.Configuration.JavaInstallations)
		{
			if (JavaCheck.CheckJavaInstallation(path, out var install))
				JavaInstallations.Add(install);
		}
	}

	private async void PickJavaZip_Click(object sender, RoutedEventArgs e)
	{
		var openPicker = new FileOpenPicker();
		var window = SettingsPage.JavaWindow;
		var hWnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(openPicker, hWnd);

		openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
		openPicker.FileTypeFilter.Add(".zip");

		var file = await openPicker.PickSingleFileAsync();
		if (file == null)
			return;

		string javaFolderName = string.Empty;
		using (ZipArchive archive = ZipFile.OpenRead(file.Path))
		{
			// likely the folder containing everything
			javaFolderName = archive.Entries.First().FullName;
		}
		ZipFile.ExtractToDirectory(file.Path, ApplicationData.JavaInstallsPath);

		var extractedFolder = Path.Combine(ApplicationData.JavaInstallsPath, javaFolderName);
		if (!Directory.Exists(extractedFolder))
			throw new FileNotFoundException();

		ApplicationData.Configuration.JavaInstallations.Add(extractedFolder);

		if (JavaCheck.CheckJavaInstallation(extractedFolder, out var install))
			JavaInstallations.Add(install);

		ApplicationData.SaveConfiguration();
	}

	private async void PickJavaFolder_Click(object sender, RoutedEventArgs e)
	{
		var openPicker = new FolderPicker();
		var window = SettingsPage.JavaWindow;
		var hWnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(openPicker, hWnd);

		openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
		openPicker.FileTypeFilter.Add("*");

		var folder = await openPicker.PickSingleFolderAsync();
		if (folder == null)
			return;

		ApplicationData.Configuration.JavaInstallations.Add(folder.Path);

		if (JavaCheck.CheckJavaInstallation(folder.Path, out var install))
			JavaInstallations.Add(install);

		ApplicationData.SaveConfiguration();
	}

	private async void DeleteJavaButton_Click(object sender, RoutedEventArgs e)
	{
		var selectedInstall = (JavaInstallation)JavaInstallationList.SelectedItem;
		var deletable = selectedInstall.Path.StartsWith(ApplicationData.JavaInstallsPath);

		var removalConfirm = new RemovalConfirmationPage(deletable);

		var dialog = removalConfirm.CreateDialog(this);

		ContentDialogResult result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			if (removalConfirm.DeleteFile)
			{
				Directory.Delete(selectedInstall.Path, true);
			}

			RemoveJavaInstall();
		}
	}

	void RemoveJavaInstall()
	{
		var install = (JavaInstallation)JavaInstallationList.SelectedItem;
		ApplicationData.Configuration.JavaInstallations.Remove(install.Path);
		ApplicationData.SaveConfiguration();
		JavaInstallations.Remove(install);
	}
}

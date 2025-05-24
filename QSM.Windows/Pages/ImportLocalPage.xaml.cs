using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Core.ModPluginSource.Modrinth;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using QSM.Windows.Utilities;
using Serilog;
using System;
using System.IO;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ImportLocalPage : Page
{
	public ImportLocalPage()
	{
		this.InitializeComponent();
	}

	private async void ImportBtn_Click(object sender, RoutedEventArgs e)
	{
		var selected = URIInput.Text;

		if (!selected.EndsWith(".mrpack"))
		{
			Log.Error("File extension of the modpack is not equal to the expected .mrpack file.");
			return;
		}

		var downloadPage = new SingleFileDownloadPage();

		var dialog = downloadPage.CreateDialog(this);

		_ = dialog.ShowAsync();

		string modpackFileName = StringUtility.TurnIntoValidFileName(Path.GetFileNameWithoutExtension(selected));
		string tempDir = FileSystemUtility.GetTemporaryDirectory();
		string serverDir = Path.Combine(
			ApplicationData.ServersFolderPath,
			modpackFileName);

		if (Directory.Exists(serverDir))
		{
			serverDir = Path.Combine(
				ApplicationData.ServersFolderPath,
				StringUtility.TurnIntoValidFileName($"{modpackFileName}_{Path.GetRandomFileName()}"));
		}

		Directory.CreateDirectory(serverDir);

		var extractResult = await MrpackExtractor.ExtractAsync(selected, tempDir);
		var downloader = MrpackExtractor.DownloadMods(extractResult.Index, serverDir);

		await foreach (var operation in downloader)
		{
			downloadPage.SetOperation(operation.Operation);

			if (operation.Progress == null)
			{
				downloadPage.SetIsIndeterminate(true);
			}
			else
			{
				downloadPage.SetIsIndeterminate(false);
				downloadPage.UpdateProgress((double)operation.Progress);
			}
		}

		var copyOperation = MrpackExtractor.CopyOverrides(extractResult.ExtractLocation, serverDir);

		downloadPage.SetIsIndeterminate(true);
		foreach (var operation in copyOperation)
		{
			downloadPage.SetOperation(operation.Operation);
		}

		Directory.Delete(extractResult.ExtractLocation, true);
		Directory.Delete(tempDir, true);

		var metadata = new ServerMetadata(
			Path.GetFileName(serverDir),
			extractResult.Index.MinecraftServerSoftware,
			extractResult.Index.MinecraftVersion,
			extractResult.Index.MinecraftSoftwareVersion,
			serverDir);

		InfoFetcher api = extractResult.Index.MinecraftServerSoftware switch
		{
			ServerSoftwares.Fabric => new FabricFetcher(),
			ServerSoftwares.NeoForge => new NeoForgeFetcher(),
			_ => throw new InvalidOperationException("Unsupported Minecraft server software.")
		};
		string url = await api.GetDownloadUrlAsync(extractResult.Index.MinecraftVersion, extractResult.Index.MinecraftSoftwareVersion);
		await downloadPage.DownloadFileAsync(url, Path.Join(serverDir, "server.jar"));

		ServerSettings settings = new();

		await settings.SaveJsonAsync(metadata.QsmConfigFile);

		ApplicationData.ServerSettings[metadata.Guid] = settings;

		AppEvents.AddNewServer(metadata);

		dialog.Hide();
	}

	private async void BrowseBtn_Click(object sender, RoutedEventArgs e)
	{
		var openPicker = new FileOpenPicker();
		var window = App.MainWindow;
		var hWnd = WindowNative.GetWindowHandle(window);
		InitializeWithWindow.Initialize(openPicker, hWnd);

		openPicker.SuggestedStartLocation = PickerLocationId.Downloads;
		openPicker.FileTypeFilter.Add(".mrpack");

		var file = await openPicker.PickSingleFileAsync();

		if (file == null)
			return;

		URIInput.Text = file.Path;
	}
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using QSM.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CreateServerPage : Page
{
	string _defaultServersLocation;

	ServerSoftware[] _serverSoftwares = [
		new() {
			Name = "Paper",
			Icon = "/Assets/ServerSoftware/papermc-logomark.png",
			InfoFetcher = new PaperMCFetcher("paper")
		},
		new () {
			Name = "Purpur",
			Icon = "/Assets/ServerSoftware/purpur.svg",
			InfoFetcher = new PurpurFetcher()
		},
		new() {
			Name = "Vanilla",
			Icon = "/Assets/ServerSoftware/minecraft_logo.svg",
			InfoFetcher = new VanillaFetcher()
		},
		new() {
			Name = "Fabric",
			Icon = "/Assets/ServerSoftware/Fabric.png",
			InfoFetcher = new FabricFetcher()
		},
		new () {
			Name = "NeoForge",
			Icon = "/Assets/ServerSoftware/NeoForged.png",
			InfoFetcher = new NeoForgeFetcher()
		},
		new() {
			Name = "Velocity",
			Icon = "/Assets/ServerSoftware/velocity-blue.svg",
			InfoFetcher = new PaperMCFetcher("velocity")
		},
		new() {
			Name = "Forge",
			Icon = "/Assets/ServerSoftware/forge.png",
			InfoFetcher = new ForgeFetcher()
		}
	];

	static Dictionary<string, ServerSoftwares> s_softwareDisplayNameToEnumMapping = new()
	{
		{ "Paper", ServerSoftwares.Paper },
		{ "Purpur", ServerSoftwares.Purpur },
		{ "Vanilla", ServerSoftwares.Vanilla },
		{ "Fabric", ServerSoftwares.Fabric },
		{ "NeoForge", ServerSoftwares.NeoForge },
		{ "Velocity", ServerSoftwares.Velocity },
		{ "Forge", ServerSoftwares.Forge },
	};

	ExtendedObservableCollection<string> MinecraftVersions { get; set; } = [];
	ExtendedObservableCollection<string> AvailableBuilds { get; set; } = [];

	public CreateServerPage()
	{
		this.InitializeComponent();
		_defaultServersLocation = ApplicationData.ServersFolderPath;
		serverFolderPathInput.Text = _defaultServersLocation;
		serverSoftware.SelectedIndex = 0;
	}

	async Task FetchAvailableMinecraftVersions()
	{
		MinecraftVersions.Clear();
		string[] versions = await ((ServerSoftware)serverSoftware.SelectedItem).InfoFetcher.FetchAvailableMinecraftVersionsAsync();
		MinecraftVersions.AddRange(versions);
		minecraftVersionList.SelectedIndex = 0;
	}

	async Task FetchAvailableBuilds()
	{
		AvailableBuilds.Clear();
		if (minecraftVersionList.SelectedItem == null) return;
		string[] builds = await ((ServerSoftware)serverSoftware.SelectedItem).InfoFetcher.FetchAvailableBuildsAsync((string)minecraftVersionList.SelectedItem);
		AvailableBuilds.AddRange(builds);
		serverBuildList.SelectedIndex = 0;
	}

	private async void serverSoftware_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await FetchAvailableMinecraftVersions();
	}

	private async void serverCreateBtn_Click(object sender, RoutedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(serverNameInput.Text))
		{
			requiredNameInfoBar.IsOpen = true;
			return;
		}

		DirectoryInfo serverDirectory = Directory.CreateDirectory(serverFolderPathInput.Text);

		string downloadUrl = await ((ServerSoftware)serverSoftware.SelectedItem).InfoFetcher.GetDownloadUrlAsync((string)minecraftVersionList.SelectedItem, (string)serverBuildList.SelectedItem);

		var dialogContent = new SingleFileDownloadPage();

		ContentDialog dialog = dialogContent.CreateDialog(this);

		_ = dialog.ShowAsync();

		await dialogContent.DownloadFileAsync(
			downloadUrl,
			$"{serverDirectory.FullName}/server.jar");

		var metadata = new ServerMetadata(
			serverNameInput.Text,
			s_softwareDisplayNameToEnumMapping[((ServerSoftware)serverSoftware.SelectedItem).Name],
			(string)minecraftVersionList.SelectedItem,
			(string)serverBuildList.SelectedItem,
			serverDirectory.FullName
		);

		ServerSettings settings = new();

		await settings.SaveJsonAsync(metadata.QsmConfigFile);

		ApplicationData.ServerSettings[metadata.Guid] = settings;

		AppEvents.AddNewServer(metadata);

		dialog.Hide();
	}

	private async void minecraftVersionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await FetchAvailableBuilds();
	}

	private async void serverFolderBrowseBtn_Click(object sender, RoutedEventArgs e)
	{
		FolderPicker folderPicker = new();

		var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

		WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

		folderPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		folderPicker.FileTypeFilter.Add("*");

		StorageFolder folder = await folderPicker.PickSingleFolderAsync();
		if (folder != null)
		{
			serverFolderPathInput.Text = folder.Path;
		}
	}

	private void serverNameInput_TextChanged(object sender, TextChangedEventArgs e)
	{
		// Don't change the server folder path if the user has already set a custom one
		if (!serverFolderPathInput.Text.StartsWith(_defaultServersLocation)) return;

		serverFolderPathInput.Text = $"{_defaultServersLocation}\\{StringUtility.TurnIntoValidFileName(serverNameInput.Text)}";
	}
}

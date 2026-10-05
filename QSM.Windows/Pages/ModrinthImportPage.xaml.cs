using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ModPluginSource;
using QSM.Core.ModPluginSource.Modrinth;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using QSM.Core.Utilities;
using QSM.Windows.Pages.Dialogs;
using QSM.Windows.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ModrinthImportPage : Page
{
	ModrinthProvider _modrinth;
	readonly ExtendedObservableCollection<ModPluginInfo> _searchResults = [];
	readonly ExtendedObservableCollection<ModPluginDownloadInfo> _availableVersions = [];
	readonly ExtendedObservableCollection<Category> _categories = [];

	ModPluginInfo _selectedMod;

	public ModrinthImportPage()
	{
		this.InitializeComponent();
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		_modrinth = Program.Hoster.Services.GetService<ModrinthProvider>();

		var searchResults = (await _modrinth.SearchAsync(projectType: ModrinthProvider.ProjectType.Modpack))
			.Select(modpack =>
			{
				if (string.IsNullOrWhiteSpace(modpack.IconUrl))
				{
					modpack.IconUrl = "ms-appx://Square44x44Logo.scale-200.png";
				}
				return modpack;
			});

		_searchResults.AddRange(searchResults);

		IEnumerable<Category> categories = await _modrinth.ListCategories();

		categories = categories
			.Where(cat => cat.ProjectType == "modpack")
			.Select(cat => cat with { Name = StringUtility.KebabCaseToText(cat.Name) });

		_categories.AddRange(categories);

		base.OnNavigatedTo(e);
	}

	private async void ModpackSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
	{
		await FilteredSearch();
	}

	private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		ConfirmButton.IsEnabled = false;

		var selected = (ModPluginDownloadInfo)VersionSelector.SelectedItem;

		if (!selected.FileName.EndsWith(".mrpack"))
		{
			Log.Error("File extension of the modpack is not equal to the expected .mrpack file.");
			return;
		}

		var downloadPage = new SingleFileDownloadPage();

		var dialog = downloadPage.CreateDialog(this);

		_ = dialog.ShowAsync();

		string packPath = Path.Combine(ApplicationData.DownloadFolderPath, StringUtility.TurnIntoValidFileName(selected.FileName));

		if (!File.Exists(packPath))
			await downloadPage.DownloadFileAsync(selected.DownloadUri, packPath);

		using var sha512 = SHA512.Create();

		if (selected.Hash != sha512.GetFileHashAsString(packPath))
		{
			Log.Error("The file \"{PackPath}\" seems to be corrupted and its integrity cannot be verified.", Path.GetFileName(packPath));
			Log.Verbose("The application won't try to install the modpack.");
			dialog.Hide();
			return;
		}

		var selectedModpack = (ModPluginInfo)ModList.SelectedItem;

		string tempDir = FileSystemUtility.GetTemporaryDirectory();
		string serverDir = Path.Combine(
			ApplicationData.ServersFolderPath,
			StringUtility.TurnIntoValidFileName(selectedModpack.Name));

		if (Directory.Exists(serverDir))
		{
			serverDir = Path.Combine(
				ApplicationData.ServersFolderPath,
				StringUtility.TurnIntoValidFileName($"{selectedModpack.Name}_{Path.GetRandomFileName()}"));
		}

		Directory.CreateDirectory(serverDir);

		downloadPage.SetOperation("Extracting .mrpack file...");
		var extractResult = await MrpackExtractor.ExtractAsync(packPath, tempDir);

		var copyOperation = MrpackExtractor.CopyOverrides(extractResult.ExtractLocation, serverDir);

		downloadPage.SetIsIndeterminate(true);
		foreach (var operation in copyOperation)
		{
			downloadPage.SetOperation(operation.Operation);
		}

		IHttpClientFactory clientFactory = Program.Hoster.Services.GetRequiredService<IHttpClientFactory>();
		InfoFetcher api = extractResult.Index.MinecraftServerSoftware switch
		{
			ServerSoftwares.Fabric => new FabricFetcher(clientFactory),
			ServerSoftwares.Quilt => new QuiltFetcher(clientFactory),
			ServerSoftwares.NeoForge => new NeoForgeFetcher(clientFactory),
			ServerSoftwares.Forge => new ForgeFetcher(clientFactory),
			_ => throw new InvalidOperationException("Unsupported Minecraft server software.")
		};
		string url = await api.GetDownloadUrlAsync(extractResult.Index.MinecraftVersion, extractResult.Index.MinecraftSoftwareVersion);
		await downloadPage.DownloadFileAsync(url, Path.Join(serverDir, "server.jar"));

		var downloadList = MrpackExtractor.GetModList(extractResult.Index, serverDir);

		var concurrentDownloadPage = new MultipleFileDownloadPage();
		dialog.Content = concurrentDownloadPage;

		await concurrentDownloadPage.DownloadFiles(downloadList);

		Directory.Delete(extractResult.ExtractLocation, true);
		Directory.Delete(tempDir, true);

		var metadata = new ServerMetadata(
			Path.GetFileName(serverDir),
			extractResult.Index.MinecraftServerSoftware,
			extractResult.Index.MinecraftVersion,
			extractResult.Index.MinecraftSoftwareVersion,
			serverDir);

		ServerSettings settings = new();

		await settings.SaveJsonAsync(metadata.QsmConfigFile);

		ApplicationData.ServerSettings[metadata.Guid] = settings;

		AppEvents.AddNewServer(metadata);

		dialog.Hide();
	}

	private async void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count == 0) return;

		_selectedMod = (ModPluginInfo)e.AddedItems[0];

		_selectedMod = await _modrinth.GetDetailedInfoAsync(_selectedMod);

		ModIcon.Source = new BitmapImage(new Uri(_selectedMod.IconUrl));
		ModName.Text = _selectedMod.Name;
		OwnerLabel.Text = _selectedMod.Owner;

		if (string.IsNullOrEmpty(_selectedMod.LicenseUrl))
		{
			ModLicense.Text = $"License: {_selectedMod.License}";
		}
		else
		{
			ModLicense.Text = "License: ";

			Hyperlink hyperlink = new()
			{
				NavigateUri = new Uri(_selectedMod.LicenseUrl)
			};

			Run run = new()
			{
				Text = _selectedMod.License
			};

			hyperlink.Inlines.Add(run);
			ModLicense.Inlines.Add(hyperlink);
		}

		ModDownloadCount.Text = $"{_selectedMod.DownloadCount:n0} Downloads";
		ModDescription.Text = _selectedMod.LongDescription;

		VersionSelector.IsEnabled = true;

		ModPluginDownloadInfo[] versions = await _modrinth.GetVersionsAsync(_selectedMod.Slug);

		_availableVersions.Clear();
		_availableVersions.AddRange(versions);
		VersionSelector.SelectedIndex = 0;

		ConfirmButton.IsEnabled = true;
		OpenBrowser.Visibility = Visibility.Visible;
	}

	async Task FilteredSearch()
	{
		var modpacks = (await _modrinth.SearchAsync(
			ServerMetadata.Selected,
			ModpackSearchBox.Text,
			ModrinthProvider.ProjectType.Modpack,
			FilterCategorySelector.SelectedItems.Select(cat =>
			{
				return StringUtility.ToKebabCase(((Category)cat).Name);
			}).Concat(ModLoaderSelector.SelectedItems.Select(loader =>
			{
				return ((string)loader).ToLowerInvariant();
			})))).Select(modpack =>
			{
				if (string.IsNullOrWhiteSpace(modpack.IconUrl))
				{
					modpack.IconUrl = "ms-appx://Square44x44Logo.scale-200.png";
				}
				return modpack;
			});

		_searchResults.Clear();
		_searchResults.AddRange(modpacks);
	}

	private async void FilterCategorySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await FilteredSearch();
	}

	private void FilterButton_Click(object sender, RoutedEventArgs e)
	{
		FilterPane.IsPaneOpen = true;
	}

	private async void ModLoaderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await FilteredSearch();
	}

	private void OpenBrowser_Click(object sender, RoutedEventArgs e)
	{
		Process.Start(@"C:\Windows\explorer.exe", _selectedMod.Url);
	}
}

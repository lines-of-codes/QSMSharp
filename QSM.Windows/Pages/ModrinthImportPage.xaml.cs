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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ModrinthImportPage : Page
{
	ModrinthProvider _modrinth;
	readonly ExtendedObservableCollection<ModPluginInfo> SearchResults = [];
	readonly ExtendedObservableCollection<ModPluginDownloadInfo> AvailableVersions = [];
	readonly ExtendedObservableCollection<Category> _categories = [];

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

		SearchResults.AddRange(searchResults);

		IEnumerable<Category> categories = await _modrinth.ListCategories();

		categories = categories
			.Where(cat => cat.project_type == "modpack")
			.Select(cat => cat with { name = StringUtility.KebabCaseToText(cat.name) });

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
			Log.Error($"The file {Path.GetFileName(packPath)} seems to be corrupted and its integrity cannot be verified.");
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

		//var downloader = MrpackExtractor.DownloadMods(index, serverDir);

		//await foreach (var operation in downloader)
		//{
		//	downloadPage.SetOperation(operation.Operation);

		//	if (operation.Progress == null)
		//	{
		//		downloadPage.SetIsIndeterminate(true);
		//	}
		//	else
		//	{
		//		downloadPage.SetIsIndeterminate(false);
		//		downloadPage.UpdateProgress((double)operation.Progress);
		//	}
		//}

		InfoFetcher api = extractResult.Index.MinecraftServerSoftware switch
		{
			ServerSoftwares.Fabric => new FabricFetcher(),
			ServerSoftwares.NeoForge => new NeoForgeFetcher(),
			ServerSoftwares.Forge => new ForgeFetcher(),
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

		var mod = (ModPluginInfo)e.AddedItems.First();

		mod = await _modrinth.GetDetailedInfoAsync(mod);

		ModIcon.Source = new BitmapImage(new Uri(mod.IconUrl));
		ModName.Text = mod.Name;
		OwnerLabel.Text = mod.Owner;

		if (string.IsNullOrEmpty(mod.LicenseUrl))
		{
			ModLicense.Text = $"License: {mod.License}";
		}
		else
		{
			ModLicense.Text = "License: ";

			Hyperlink hyperlink = new()
			{
				NavigateUri = new Uri(mod.LicenseUrl)
			};

			Run run = new()
			{
				Text = mod.License
			};

			hyperlink.Inlines.Add(run);
			ModLicense.Inlines.Add(hyperlink);
		}

		ModDownloadCount.Text = $"{mod.DownloadCount:n0} Downloads";
		ModDescription.Text = mod.LongDescription;

		VersionSelector.IsEnabled = true;

		ModPluginDownloadInfo[] versions = await _modrinth.GetVersionsAsync(mod.Slug);

		AvailableVersions.Clear();
		AvailableVersions.AddRange(versions);
		VersionSelector.SelectedIndex = 0;

		ConfirmButton.IsEnabled = true;
	}

	async Task FilteredSearch()
	{
		var modpacks = (await _modrinth.SearchAsync(
			ModpackSearchBox.Text,
			ModrinthProvider.ProjectType.Modpack,
			FilterCategorySelector.SelectedItems.Select(cat =>
			{
				return StringUtility.ToKebabCase(((Category)cat).name);
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

		SearchResults.Clear();
		SearchResults.AddRange(modpacks);
	}

	private async void FilterCategorySelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await FilteredSearch();
	}

	private void FilterButton_Checked(object sender, RoutedEventArgs e)
	{
		FilterPane.IsPaneOpen = true;
	}

	private void FilterButton_Unchecked(object sender, RoutedEventArgs e)
	{
		FilterPane.IsPaneOpen = false;
	}

	private async void ModLoaderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		await FilteredSearch();
	}
}

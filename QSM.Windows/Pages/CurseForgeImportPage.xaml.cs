using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ModPluginSource;
using QSM.Core.ModPluginSource.CurseForge;
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
public sealed partial class CurseForgeImportPage : Page
{
	CurseForgeProvider _curseforge;
	readonly ExtendedObservableCollection<ModPluginInfo> _searchResults = [];
	readonly ExtendedObservableCollection<ModPluginDownloadInfo> _availableVersions = [];
	readonly ExtendedObservableCollection<CurseCategory> _categories = [];
	CurseCategory _modpackCat;

	public CurseForgeImportPage()
	{
		InitializeComponent();
	}

	protected override async void OnNavigatedTo(NavigationEventArgs e)
	{
		_curseforge = Program.Hoster.Services.GetService<CurseForgeProvider>();

		IEnumerable<CurseCategory> categories = await _curseforge.ListCategories();

		_modpackCat = categories.First(c => c.Name == "Modpacks");

		categories = categories.Where(cat => cat.ClassId == _modpackCat.Id);

		_categories.AddRange(categories);

		var searchResults = (await _curseforge.SearchAsync(classId: _modpackCat.Id))
			.Select(modpack =>
			{
				if (string.IsNullOrWhiteSpace(modpack.IconUrl))
				{
					modpack.IconUrl = "ms-appx://Square44x44Logo.scale-200.png";
				}
				return modpack;
			});

		_searchResults.AddRange(searchResults);

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

		if (!selected.FileName.EndsWith(".zip"))
		{
			Log.Error("File extension of the modpack is not equal to the expected .zip file.");
			return;
		}

		var downloadPage = new SingleFileDownloadPage();

		var progressDialog = downloadPage.CreateDialog(this);

		_ = progressDialog.ShowAsync();

		string packPath = Path.Combine(ApplicationData.DownloadFolderPath, StringUtility.TurnIntoValidFileName(selected.FileName));

		if (!File.Exists(packPath))
			await downloadPage.DownloadFileAsync(selected.DownloadUri, packPath);

		if (selected.Hash != null)
		{
			using var sha1 = SHA1.Create();

			if (selected.Hash != sha1.GetFileHashAsString(packPath))
			{
				Log.Error($"The file {Path.GetFileName(packPath)} seems to be corrupted and its integrity cannot be verified.");
				Log.Verbose("The application won't try to install the modpack.");
				progressDialog.Hide();
				return;
			}
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

		downloadPage.SetOperation("Extracting .zip file...");

		var extractor = new CursePackExtractor();

		var manifest = await extractor.ExtractAsync(packPath, tempDir);

		extractor.CopyOverrides(serverDir);

		downloadPage.SetIsIndeterminate(true);

		InfoFetcher api = manifest.Minecraft.PrimaryLoader.Software switch
		{
			ServerSoftwares.Fabric => new FabricFetcher(),
			ServerSoftwares.NeoForge => new NeoForgeFetcher(),
			ServerSoftwares.Forge => new ForgeFetcher(),
			_ => throw new InvalidOperationException("Unsupported Minecraft server software.")
		};
		string url = await api.GetDownloadUrlAsync(manifest.Minecraft.Version, manifest.Minecraft.PrimaryLoader.Version);
		await downloadPage.DownloadFileAsync(url, Path.Join(serverDir, "server.jar"));

		var downloadList = await _curseforge.GetDownloadQueueFromManifest(manifest, serverDir);

		var concurrentDownloadPage = new MultipleFileDownloadPage();
		progressDialog.Content = concurrentDownloadPage;

		await concurrentDownloadPage.DownloadFiles(downloadList.Queue);

		Directory.Delete(extractor.ExtractLocation, true);
		Directory.Delete(tempDir, true);

		progressDialog.Hide();

		if (downloadList.Skipped.Length > 0)
		{
			var extraDownloads = new CurseExtraDownloads(downloadList.Skipped, Path.Join(serverDir, "mods"));
			var missingDialog = extraDownloads.CreateDialog(this);

			await missingDialog.ShowAsync();
		}

		var metadata = new ServerMetadata(
			Path.GetFileName(serverDir),
			manifest.Minecraft.PrimaryLoader.Software,
			manifest.Minecraft.Version,
			manifest.Minecraft.PrimaryLoader.Version,
			serverDir);

		ServerSettings settings = new();

		await settings.SaveJsonAsync(metadata.QsmConfigFile);

		ApplicationData.ServerSettings[metadata.Guid] = settings;

		AppEvents.AddNewServer(metadata);
	}

	private async void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count == 0) return;

		var mod = (ModPluginInfo)e.AddedItems.First();

		mod = await _curseforge.GetDetailedInfoAsync(mod);

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

		ModPluginDownloadInfo[] versions = await _curseforge.GetVersionsAsync(mod.Id.ToString());

		_availableVersions.Clear();
		_availableVersions.AddRange(versions);
		VersionSelector.SelectedIndex = 0;

		ConfirmButton.IsEnabled = true;
	}

	async Task FilteredSearch()
	{
		var modpacks = await _curseforge.SearchAsync(
			ModpackSearchBox.Text,
			_modpackCat.Id,
			FilterCategorySelector.SelectedItems.Select(c => ((CurseCategory)c).Id),
			ModLoaderSelector.SelectedItems.Select(l => (string)l switch
			{
				"Forge" => CurseForgeProvider.ModLoaderType.Forge,
				"NeoForge" => CurseForgeProvider.ModLoaderType.NeoForge,
				"Fabric" => CurseForgeProvider.ModLoaderType.Fabric,
				_ => throw new InvalidOperationException("Unsupported server software")
			})
			);

		_searchResults.Clear();
		_searchResults.AddRange(modpacks);
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

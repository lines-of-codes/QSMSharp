using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ModPluginSource;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages.Dialogs;
using ReverseMarkdown;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ModSearchPage : Page
{
	int _metadataIndex;
	ModPluginInfo _selectedMod;
	ServerMetadata _metadata;
	ProviderInfo _currentProvider;
	readonly List<ModPluginDownloadInfo> _selectedMods = [];
	readonly ObservableCollection<ProviderInfo> _providers = [];
	readonly ExtendedObservableCollection<ModPluginInfo> _searchResults = [];
	readonly ExtendedObservableCollection<ModPluginDownloadInfo> _availableVersions = [];
	readonly Converter _htmlConverter = new(new Config
	{
		Tags = { Unknown = Config.UnknownTagsOption.Drop }
	});

	public ModSearchPage()
	{
		InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		_metadata = ApplicationData.Configuration.Servers[_metadataIndex];

		if (_metadata.IsPluginSupported)
		{
			_providers.Add(new()
			{
				Icon = "/Assets/ModPluginProvider/hangar-logo.svg",
				ProviderName = "PaperMC Hangar",
				Provider = Program.Hoster.Services.GetService<PaperMCHangarProvider>()
			});
		}

		_providers.Add(new()
		{
			Icon = "/Assets/ModPluginProvider/modrinth-logo.svg",
			ProviderName = "Modrinth",
			Provider = Program.Hoster.Services.GetService<ModrinthProvider>()
		});

		_providers.Add(new()
		{
			Icon = "/Assets/ModPluginProvider/curseforge-orange-logo.svg",
			ProviderName = "CurseForge",
			Provider = Program.Hoster.Services.GetService<CurseForgeProvider>()
		});

		ProviderSelector.SelectedIndex = 0;

		base.OnNavigatedTo(e);
	}

	private async void ProviderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var provider = (ProviderInfo)e.AddedItems[0];
		_currentProvider = provider;

		ModPluginInfo[] mods;
		try
		{
			mods = await provider.Provider.SearchAsync();
		}
		catch (HttpRequestException ex)
		{
			Log.Error(ex, "An error occurred while requesting mod information.");
			return;
		}

		_searchResults.Clear();
		_searchResults.AddRange(mods);
	}

	private async void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count == 0) return;

		_selectedMod = (ModPluginInfo)e.AddedItems[0];

		_selectedMod = await _currentProvider.Provider.GetDetailedInfoAsync(_selectedMod);

		if (_currentProvider.ProviderName == "CurseForge")
		{
			_selectedMod.LongDescription = _htmlConverter.Convert(_selectedMod.LongDescription);
		}

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

			Hyperlink hyperlink = new();
			Run run = new()
			{
				Text = _selectedMod.License
			};
			hyperlink.NavigateUri = new Uri(_selectedMod.LicenseUrl);

			hyperlink.Inlines.Add(run);
			ModLicense.Inlines.Add(hyperlink);
		}

		ModDownloadCount.Text = $"{_selectedMod.DownloadCount:n0} Downloads";
		ModDescription.Text = _selectedMod.LongDescription;

		VersionSelector.IsEnabled = true;
		OpenBrowser.Visibility = Visibility.Visible;

		try
		{
			ModPluginDownloadInfo[] versions = await _currentProvider.Provider.GetVersionsAsync(_currentProvider.ProviderName == "CurseForge" ? _selectedMod.Id.ToString() : _selectedMod.Slug, ServerMetadata.Selected);

			_availableVersions.Clear();
			_availableVersions.AddRange(versions);
			VersionSelector.SelectedIndex = 0;

			if (versions.Length == 0)
			{
				SelectButton.IsEnabled = false;
				return;
			}

			SelectButton.IsEnabled = true;
			ConfirmButton.IsEnabled = true;
		}
		catch (HttpRequestException reqEx)
		{
			SelectButton.IsEnabled = false;
			Log.Error(reqEx, "[GetVersionsAsync] An error occurred while fetching versions");
		}
	}

	private async void ModSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
	{
		IEnumerable<ModPluginInfo> mods = await ((ProviderInfo)ProviderSelector.SelectedItem).Provider.SearchAsync(args.QueryText);

		mods = mods.Select(mod =>
		{
			if (string.IsNullOrWhiteSpace(mod.IconUrl))
			{
				mod.IconUrl = "ms-appx://Square44x44Logo.scale-200.png";
			}
			return mod;
		});

		_searchResults.Clear();
		_searchResults.AddRange(mods);
	}

	private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
	{
		var confirmPage = new ModDownloadsConfirmPage([.. _selectedMods]);

		ContentDialog dialog = new()
		{
			XamlRoot = XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "Mod Download Confirmation",
			PrimaryButtonText = "Continue",
			CloseButtonText = "Cancel",
			IsSecondaryButtonEnabled = false,
			DefaultButton = ContentDialogButton.Primary,
			Content = confirmPage
		};

		var result = await dialog.ShowAsync();

		string modsFolderPath = string.Empty;

		if (_metadata.IsModSupported)
			modsFolderPath = Path.Combine(_metadata.ServerPath, "mods");
		if (_metadata.IsPluginSupported)
			modsFolderPath = Path.Combine(_metadata.ServerPath, "plugins");

		if (string.IsNullOrEmpty(modsFolderPath))
		{
			Log.Error("Unable to identify if the software uses the mod or plugins folder.");
			return;
		}

		if (result is ContentDialogResult.Primary)
		{
			var downloadPage = new MultipleFileDownloadPage();

			dialog = new()
			{
				XamlRoot = XamlRoot,
				Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
				Title = "Downloading files...",
				IsPrimaryButtonEnabled = false,
				IsSecondaryButtonEnabled = false,
				DefaultButton = ContentDialogButton.Primary,
				Content = downloadPage
			};

			_ = dialog.ShowAsync();

			await downloadPage.DownloadMods(
				new Queue<ModPluginDownloadInfo>(confirmPage.DownloadList.Where(entry => entry.Download)),
				modsFolderPath);

			dialog.Hide();
		}
	}

	private void SelectButton_Checked(object sender, RoutedEventArgs e)
	{
		var selectedVersion = (ModPluginDownloadInfo)VersionSelector.SelectedItem;

		if (_selectedMods.Contains(selectedVersion))
			return;

		_selectedMods.Add(selectedVersion);
	}

	private void SelectButton_Unchecked(object sender, RoutedEventArgs e)
	{
		var selectedVersion = (ModPluginDownloadInfo)VersionSelector.SelectedItem;
		_selectedMods.Remove(selectedVersion);
	}

	private void VersionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (e.AddedItems.Count == 0) return;

		var selectedVersion = (ModPluginDownloadInfo)e.AddedItems[0];
		SelectButton.IsChecked = _selectedMods.Contains(selectedVersion);
	}

	private void ModDescription_LinkClicked(object sender, CommunityToolkit.WinUI.UI.Controls.LinkClickedEventArgs e)
	{
		Process.Start(@"C:\Windows\explorer.exe", e.Link);
	}

	private void OpenBrowser_Click(object sender, RoutedEventArgs e)
	{
		Process.Start(@"C:\Windows\explorer.exe", _selectedMod.Url);
	}
}

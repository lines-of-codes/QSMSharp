using LiveChartsCore.Drawing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ModPluginSource;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModSearchPage : Page
    {
        int MetadataIndex;
        ServerMetadata Metadata;
        ModPluginInfo SelectedMod;
        ProviderInfo CurrentProvider;
        List<ModPluginDownloadInfo> SelectedMods = [];
        ObservableCollection<ProviderInfo> Providers = [];
        ExtendedObservableCollection<ModPluginInfo> SearchResults = [];
        ExtendedObservableCollection<ModPluginDownloadInfo> AvailableVersions = [];

        public ModSearchPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MetadataIndex = (int)e.Parameter;
            Metadata = ApplicationData.Configuration.Servers[MetadataIndex];

            if (Metadata.Software == ServerSoftwares.Paper || Metadata.Software == ServerSoftwares.Velocity)
            {
                Providers.Add(new()
                {
                    Icon = "/Assets/ModPluginProvider/hangar-logo.svg",
                    ProviderName = "PaperMC Hangar",
                    Provider = new PaperMCHangarProvider(Metadata)
                });
            }

            Providers.Add(new()
            {
                Icon = "/Assets/ModPluginProvider/modrinth-logo.svg",
                ProviderName = "Modrinth",
                Provider = new ModrinthProvider(Metadata)
            });

            ProviderSelector.SelectedIndex = 0;

            base.OnNavigatedTo(e);
        }

        private async void ProviderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var provider = (ProviderInfo)e.AddedItems.First();
            CurrentProvider = provider;

            var mods = await provider.Provider.SearchAsync();

            SearchResults.Clear();
            SearchResults.AddRange(mods);
        }

        private async void ModList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var mod = (ModPluginInfo)e.AddedItems.First();

            mod = await CurrentProvider.Provider.GetDetailedInfo(mod);

            SelectedMod = mod;

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

                Hyperlink hyperlink = new();
                Run run = new();

                run.Text = mod.License;
                hyperlink.NavigateUri = new Uri(mod.LicenseUrl);

                hyperlink.Inlines.Add(run);
                ModLicense.Inlines.Add(hyperlink);
            }

            ModDownloadCount.Text = $"{mod.DownloadCount:n0} Downloads";
            ModDescription.Text = mod.LongDescription;
            
            VersionSelector.IsEnabled = true;

            ModPluginDownloadInfo[] versions = await CurrentProvider.Provider.GetVersionsAsync(mod.Slug);

            AvailableVersions.Clear();
            AvailableVersions.AddRange(versions);
            VersionSelector.SelectedIndex = 0;

            SelectButton.IsEnabled = true;
            ConfirmButton.IsEnabled = true;
        }

        private async void ModSearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

            var mods = await ((ProviderInfo)ProviderSelector.SelectedItem).Provider.SearchAsync(args.QueryText);

            SearchResults.Clear();
            SearchResults.AddRange(mods);
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            var confirmPage = new ModDownloadsConfirmPage(SelectedMods.ToArray());

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

            var modsFolder = Path.Combine(Metadata.ServerPath, "/mods/");

            if (result is ContentDialogResult.Primary)
            {
                dialog = new()
                {
                    XamlRoot = XamlRoot,
                    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                    Title = "Downloading files...",
                    IsPrimaryButtonEnabled = false,
                    IsSecondaryButtonEnabled = false,
                    DefaultButton = ContentDialogButton.Primary,
                    Content = new MultipleFileDownloadPage(
                        new Queue<ModPluginDownloadInfo>(confirmPage.DownloadList.Where(entry => entry.Download)), 
                        modsFolder
                    )
                };
            }
        }

        private void SelectButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedVersion = (ModPluginDownloadInfo)VersionSelector.SelectedItem;

            if (SelectedMods.Contains(selectedVersion))
                return;

            SelectedMods.Add(selectedVersion);
        }

        private void SelectButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var selectedVersion = (ModPluginDownloadInfo)VersionSelector.SelectedItem;
            SelectedMods.Remove(selectedVersion);
        }

        private void VersionSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            var selectedVersion = (ModPluginDownloadInfo)e.AddedItems.First();
            SelectButton.IsChecked = SelectedMods.Contains(selectedVersion);
        }
    }
}

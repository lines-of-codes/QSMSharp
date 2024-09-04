using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Core.ServerSoftware;
using QSM.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateServerPage : Page
    {
        string defaultServersLocation;

        ServerSoftware[] serverSoftwares = [
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
            }
        ];

        static Dictionary<string, ServerSoftwares> SoftwareDisplayNameToEnumMapping = new()
        {
            { "Paper", ServerSoftwares.Paper },
            { "Purpur", ServerSoftwares.Purpur },
            { "Vanilla", ServerSoftwares.Vanilla },
            { "Fabric", ServerSoftwares.Fabric },
            { "NeoForge", ServerSoftwares.NeoForge },
            { "Velocity", ServerSoftwares.Velocity }
        };

        ExtendedObservableCollection<string> minecraftVersions { get; set; } = [];
        ExtendedObservableCollection<string> availableBuilds { get; set; } = [];

        public CreateServerPage()
        {
            this.InitializeComponent();
            defaultServersLocation = ApplicationData.ApplicationDataPath + "\\Servers";
            serverFolderPathInput.Text = defaultServersLocation;
            serverSoftware.SelectedIndex = 0;
        }

        async Task FetchAvailableMinecraftVersions()
        {
            minecraftVersions.Clear();
            string[] versions = await ((ServerSoftware)serverSoftware.SelectedItem).InfoFetcher.FetchAvailableMinecraftVersions();
            minecraftVersions.AddRange(versions);
            minecraftVersionList.SelectedIndex = 0;
        }

        async Task FetchAvailableBuilds()
        {
            availableBuilds.Clear();
            if (minecraftVersionList.SelectedItem == null) return;
            string[] builds = await ((ServerSoftware)serverSoftware.SelectedItem).InfoFetcher.FetchAvailableBuilds((string)minecraftVersionList.SelectedItem);
            availableBuilds.AddRange(builds);
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

            string downloadUrl = await ((ServerSoftware)serverSoftware.SelectedItem).InfoFetcher.GetDownloadUrl((string)minecraftVersionList.SelectedItem, (string)serverBuildList.SelectedItem);

            ContentDialog dialog = new();

            dialog.XamlRoot = this.XamlRoot;
            dialog.Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style;
            dialog.Title = "Downloading a file...";
            dialog.IsPrimaryButtonEnabled = false;
            dialog.IsSecondaryButtonEnabled = false;
            dialog.DefaultButton = ContentDialogButton.Primary;
            dialog.Content = new SingleFileDownloadDialogContent();

            _ = dialog.ShowAsync();

            await DownloadFileAsync(
                downloadUrl, 
                $"{serverDirectory.FullName}/server.jar", 
                (SingleFileDownloadDialogContent)dialog.Content);

            var metadata = new ServerMetadata(
                serverNameInput.Text,
                SoftwareDisplayNameToEnumMapping[((ServerSoftware)serverSoftware.SelectedItem).Name],
                (string)minecraftVersionList.SelectedItem,
                (string)serverBuildList.SelectedItem,
                serverDirectory.FullName
            );

            AppEvents.AddNewServer(metadata);

            dialog.Hide();
        }

        private async Task DownloadFileAsync(string fileUrl, string dest, SingleFileDownloadDialogContent dialogContent)
        {
            using HttpClient client = new();

            using HttpResponseMessage response = await client.GetAsync(fileUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = File.Create(dest);

            var buffer = new byte[8192];
            long totalBytesRead = 0;
            int bytesRead;
            double percentage = 0;

            if (totalBytes != -1)
            {
                dialogContent.SetIsIndeterminate(false);
            }

            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;

                if (totalBytes != -1)
                {
                    percentage = (double)totalBytesRead / totalBytes * 100;
                    dialogContent.UpdateProgress(percentage, totalBytesRead, totalBytes);
                }
            }

            dialogContent.DownloadComplete();
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
                StorageApplicationPermissions.FutureAccessList.Add(folder);
                serverFolderPathInput.Text = folder.Path;
            }
        }

        private void serverNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Don't change the server folder path if the user has already set a custom one
            if (!serverFolderPathInput.Text.StartsWith(defaultServersLocation)) return;

            serverFolderPathInput.Text = $"{defaultServersLocation}\\{StringUtility.TurnIntoValidFileName(serverNameInput.Text)}";
        }
    }
}

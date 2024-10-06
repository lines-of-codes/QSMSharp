using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.JavaProvider;
using Microsoft.UI.Xaml.Documents;
using System.IO.Compression;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Settings
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JavaDownloadPage : Page
    {
        private static readonly Dictionary<string, JavaProvider> Providers = new()
        {
            { "Azul Zulu", new AzulProvider() },
            { "Eclipse Temurin", new AdoptiumProvider() }
        };

        string _selectedProviderName = "Azul Zulu";
		Dictionary<string, int> _majorReleaseIntMapping = [];
        ExtendedObservableCollection<string> _majorReleases = [];
        ExtendedObservableCollection<string> _jreReleases = [];

        public JavaDownloadPage()
        {
            this.InitializeComponent();
        }

		private async void JavaProviderSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            _selectedProviderName = (string)JavaProviderSelector.SelectedItem;
            var provider = Providers[_selectedProviderName];

			_majorReleaseIntMapping = await provider.GetAvailableReleasesAsync();
            _majorReleases.Clear();
            _majorReleases.AddRange(_majorReleaseIntMapping.Keys);
            MajorReleaseSelector.SelectedIndex = 0;

            if (provider.Terms == null)
                TermsText.Visibility = Visibility.Collapsed;

            if (TermsText.Inlines.Count > 1)
                TermsText.Inlines.RemoveAt(TermsText.Inlines.Count - 1);

            Hyperlink hyperlink = new();
            Run run = new();

            run.Text = provider.Terms;
            hyperlink.NavigateUri = new Uri(provider.Terms);

            hyperlink.Inlines.Add(run);
            TermsText.Inlines.Add(hyperlink);
		}

		private async void MajorReleaseSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            if (MajorReleaseSelector.SelectedItem == null)
                return;

            _jreReleases.Clear();
            _jreReleases.AddRange(await Providers[_selectedProviderName].ListJREAsync(_majorReleaseIntMapping[(string)MajorReleaseSelector.SelectedItem]));
            JreReleaseSelector.SelectedIndex = 0;
		}

		private async void DownloadButton_Click(object sender, RoutedEventArgs e)
		{
            var downloadPage = new SingleFileDownloadPage();

            var dialog = downloadPage.CreateDialog(this);

            _ = dialog.ShowAsync();

            var selectedRelease = (string)JreReleaseSelector.SelectedItem;
            var javaZip = Path.Combine(ApplicationData.ApplicationDataPath, $"{_selectedProviderName}_{selectedRelease}.zip");

			await downloadPage.DownloadFileAsync(await Providers[_selectedProviderName].GetDownloadUrlAsync(selectedRelease), javaZip);

            downloadPage.SetOperation("Extracting...");
            downloadPage.SetIsIndeterminate(true);

			string javaFolderName = string.Empty;
			using (ZipArchive archive = ZipFile.OpenRead(javaZip))
			{
				// likely the folder containing everything
				javaFolderName = archive.Entries.First().FullName;
			}
			ZipFile.ExtractToDirectory(javaZip, ApplicationData.JavaInstallsPath);

			var extractedFolder = Path.Combine(ApplicationData.JavaInstallsPath, javaFolderName);

			if (!Directory.Exists(extractedFolder))
				throw new FileNotFoundException();

			ApplicationData.Configuration.JavaInstallations.Add(extractedFolder);
			ApplicationData.SaveConfiguration();

            File.Delete(javaZip);

            dialog.Hide();
		}
	}
}

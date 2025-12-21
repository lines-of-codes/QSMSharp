using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages.Dialogs;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerSummaryPage : Page
{
	int _metadataIndex;
	ServerMetadata _metadata;
	bool ProcessExitWatched;

	public ServerSummaryPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		_metadata = ApplicationData.Configuration.Servers[_metadataIndex];

		ServerNameTitle.Text = _metadata.Name;
		ServerSoftwareInfo.Text = $"{_metadata.Software} {_metadata.MinecraftVersion} ({_metadata.ServerVersion})";

		if (ServerProcessManager.Instance.Processes.TryGetValue(_metadata.Guid, out var process))
		{
			StartButton.IsEnabled = process.HasExited;
			StopButton.IsEnabled = !process.HasExited;
			ServerActiveStatus.Text = process.HasExited ? "Inactive" : "Active";

			if (!process.HasExited)
			{
				process.Exited += OnServerProcessExited;
				ProcessExitWatched = true;
			}
		}

		base.OnNavigatedTo(e);
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		if (ProcessExitWatched)
		{
			ServerProcessManager.Instance.Processes[_metadata.Guid].Exited -= OnServerProcessExited;
		}

		base.OnNavigatedFrom(e);
	}

	void OnServerProcessExited(object sender, EventArgs e)
	{
		StartButton.IsEnabled = true;
		StopButton.IsEnabled = false;
		ServerActiveStatus.Text = "Inactive";
	}

	// skipcq: CS-R1005
	private async void StartButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		if (string.IsNullOrWhiteSpace(ApplicationData.ServerSettings[_metadata.Guid].Java.JavaHome))
		{
			await InfoDialog.CreateDialog("Java Not Selected", "A Java installation hasn't been selected for this server instance. Please select one in the Configuration › Java menu.", this).ShowAsync();
			Log.Error($"Java install not set for server instance. (Server \"{_metadata.Name}\")");
			return;
		}

		StartButton.IsEnabled = false;
		StopButton.IsEnabled = true;

		var loadingPage = new SingleFileDownloadPage();

		var dialog = loadingPage.CreateDialog(this, "Starting server...");

		_ = dialog.ShowAsync();

		var settings = ApplicationData.ServerSettings[_metadata.Guid];

		if (settings.FirstRun)
		{
			if (_metadata.Software == ServerSoftwares.NeoForge) {
				await new NeoForgeFetcher().InitializeOnFirstRun(
					_metadata,
					settings,
					(obj, e) => DispatcherQueue.TryEnqueue(() => loadingPage.SetOperation(e.Data ?? string.Empty)));
			} else if (_metadata.Software == ServerSoftwares.Forge) {
				await new ForgeFetcher().InitializeOnFirstRun(
					_metadata,
					settings,
					(obj, e) => DispatcherQueue.TryEnqueue(() => loadingPage.SetOperation(e.Data ?? string.Empty)));
			}

			settings.FirstRun = false;
			await settings.SaveJsonAsync(_metadata.QsmConfigFile);
		}

		var process = ServerProcessManager.Instance.StartServer(_metadataIndex, _metadata.Guid);

		dialog.Hide();

		ServerActiveStatus.Text = "Active";

		process.Exited += OnServerProcessExited;
		ProcessExitWatched = true;
	}

	// skipcq: CS-R1005
	private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		var deletePage = new RemovalConfirmationPage(true);

		var dialog = deletePage.CreateDialog(this);

		var result = await dialog.ShowAsync();

		if (result == ContentDialogResult.Primary)
		{
			AppEvents.RemoveServer(_metadata);

			if (deletePage.DeleteFile)
			{
				Directory.Delete(_metadata.ServerPath, true);
			}
		}
	}

	private void StopButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		if (!ServerProcessManager.Instance.Processes[_metadata.Guid].HasExited)
		{
			ServerProcessManager.Instance.Processes[_metadata.Guid].StandardInput.WriteLine("stop");
		}
		StopButton.IsEnabled = false;
	}

	private void OpenServerFolderButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		Process.Start("explorer.exe", _metadata.ServerPath);
	}

	private void ChangeVersionButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{

	}
}

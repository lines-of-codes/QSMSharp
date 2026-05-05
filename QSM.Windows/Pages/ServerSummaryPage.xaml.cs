using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.ApplicationModel.Resources;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages;
using QSM.Windows.Pages.Dialogs;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerSummaryPage : Page
{
	ResourceLoader _resourceLoader;
	int _metadataIndex;
	ServerMetadata _metadata;
	bool ProcessExitWatched;

	public ServerSummaryPage()
	{
		InitializeComponent();
		_resourceLoader = new ResourceLoader("QSM.Windows.pri", "Server");
		ServerActiveStatus.Text = _resourceLoader.GetString("Inactive");
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		_metadata = ApplicationData.Configuration.Servers[_metadataIndex];

		ServerNameTitle.Text = _metadata.Name;
		ServerSoftwareInfo.Text = $"{_metadata.Software} {_metadata.MinecraftVersion}";

		if (!string.IsNullOrEmpty(_metadata.ServerVersion))
		{
			ServerSoftwareInfo.Text += $" ({_metadata.ServerVersion})";
		}

		if (ServerProcessManager.Instance.Processes.TryGetValue(_metadata.Guid, out var process))
		{
			StartButton.IsEnabled = process.HasExited;
			StopButton.IsEnabled = !process.HasExited;
			ServerActiveStatus.Text = process.HasExited ? _resourceLoader.GetString("Inactive") : _resourceLoader.GetString("Active");

			if (!process.HasExited)
			{
				process.Exited += OnServerProcessExited;
				ProcessExitWatched = true;
			}
		}

		ServerListPage.EulaAccept += ServerListPage_EulaAccept;

		base.OnNavigatedTo(e);
	}

	private void ServerListPage_EulaAccept(int obj)
	{
		if (ServerProcessManager.Instance.Processes.TryGetValue(_metadata.Guid, out var process))
		{
			StartButton.IsEnabled = process.HasExited;
			StopButton.IsEnabled = !process.HasExited;
			ServerActiveStatus.Text = process.HasExited ? _resourceLoader.GetString("Inactive") : _resourceLoader.GetString("Active");

			if (!process.HasExited)
			{
				process.Exited += OnServerProcessExited;
				ProcessExitWatched = true;
			}
		}
	}

	protected override void OnNavigatedFrom(NavigationEventArgs e)
	{
		if (ProcessExitWatched)
		{
			ServerProcessManager.Instance.Processes[_metadata.Guid].Exited -= OnServerProcessExited;
		}

		ServerListPage.EulaAccept -= ServerListPage_EulaAccept;

		base.OnNavigatedFrom(e);
	}

	void OnServerProcessExited(object sender, EventArgs e)
	{
		StartButton.IsEnabled = true;
		StopButton.IsEnabled = false;
		ServerActiveStatus.Text = _resourceLoader.GetString("Inactive");
	}

	private async void StartButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		var settings = ApplicationData.ServerSettings[_metadata.Guid];

		if (string.IsNullOrWhiteSpace(settings.Java.JavaHome))
		{
			var defaultJava = ApplicationData.Configuration.JavaInstallations.FirstOrDefault();

			if (defaultJava == null)
			{
				await InfoDialog.CreateDialog("Java Not Selected", "No registered Java installation. Please add one in the Settings › Manage Java menu.", this).ShowAsync();
				Log.Error("No registed Java installation (Server \"{ServerName}\")", _metadata.Name);
				return;
			}

			settings.Java.JavaHome = defaultJava;
			await settings.SaveJsonAsync(_metadata.QsmConfigFile);
		}

		StartButton.IsEnabled = false;
		StopButton.IsEnabled = true;

		var loadingPage = new SingleFileDownloadPage();

		var dialog = loadingPage.CreateDialog(this, "Starting server...");

		_ = dialog.ShowAsync();

		if (settings.FirstRun)
		{
			switch (_metadata.Software)
			{
				case ServerSoftwares.NeoForge:
					{
						await new NeoForgeFetcher(null!).InitializeOnFirstRun(
							_metadata,
							settings,
							(obj, e) => DispatcherQueue.TryEnqueue(() => loadingPage.SetOperation(e.Data ?? string.Empty)));
						break;
					}

				case ServerSoftwares.Forge:
					{
						await new ForgeFetcher(null!).InitializeOnFirstRun(
							_metadata,
							settings,
							(obj, e) => DispatcherQueue.TryEnqueue(() => loadingPage.SetOperation(e.Data ?? string.Empty)));
						break;
					}
				case ServerSoftwares.Quilt:
					{
						await QuiltFetcher.InitializeOnFirstRun(
							_metadata,
							settings,
							(obj, e) => DispatcherQueue.TryEnqueue(() => loadingPage.SetOperation(e.Data ?? string.Empty)));
						break;
					}
			}

			settings.FirstRun = false;
			await settings.SaveJsonAsync(_metadata.QsmConfigFile);
		}

		var process = ServerProcessManager.Instance.StartServer(_metadataIndex, _metadata.Guid);

		dialog.Hide();

		ServerActiveStatus.Text = _resourceLoader.GetString("Active");

		process.Exited += OnServerProcessExited;
		ProcessExitWatched = true;
	}

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
		Process.Start(@"C:\Windows\explorer.exe", _metadata.ServerPath);
	}

	private void ChangeVersionButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		// TODO: Implement this
	}
}

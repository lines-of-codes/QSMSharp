using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace QSM.Windows.Pages;

public sealed partial class ServerListPage : Page
{
	public static event Action<int> EulaAccept;

	private readonly ObservableCollection<WinServerInfo> _serverList = [];
	private static readonly Dictionary<ServerSoftwares, string> s_softwareIconPaths = new()
	{
		{ ServerSoftwares.Paper, "ms-appx:///Assets/ServerSoftware/papermc-logomark.png" },
		{ ServerSoftwares.Folia, "ms-appx:///Assets/ServerSoftware/folia.png" },
		{ ServerSoftwares.Purpur, "ms-appx:///Assets/ServerSoftware/purpur.svg" },
		{ ServerSoftwares.Vanilla, "ms-appx:///Assets/ServerSoftware/minecraft_logo.svg" },
		{ ServerSoftwares.Fabric, "ms-appx:///Assets/ServerSoftware/Fabric.png" },
		{ ServerSoftwares.NeoForge, "ms-appx:///Assets/ServerSoftware/NeoForged.png" },
		{ ServerSoftwares.Velocity, "ms-appx:///Assets/ServerSoftware/velocity-blue.svg" },
		{ ServerSoftwares.Forge, "ms-appx:///Assets/ServerSoftware/forge.png" },
		{ ServerSoftwares.Quilt, "ms-appx:///Assets/ServerSoftware/quilt-logo.png" }
	};

	public ServerListPage()
	{
		foreach (ServerMetadata metadata in ApplicationData.Configuration.Servers)
		{
			_serverList.Add(new WinServerInfo(new(s_softwareIconPaths[metadata.Software]), metadata));
		}

		InitializeComponent();

		AppEvents.NewServerAdded += AppEvents_NewServerAdded;
		AppEvents.ServerRemoved += AppEvents_ServerRemoved;
		ServerProcessManager.EulaPrompt += ServerProcessManager_EulaPrompt;
	}

	private void ServerProcessManager_EulaPrompt(ServerMetadata meta)
	{
		DispatcherQueue.TryEnqueue(async () =>
		{
			EulaPromptPage promptPage = new();
			ContentDialog dialog = promptPage.CreateDialog(this, meta.Name);
			var result = await dialog.ShowAsync();

			if (result != ContentDialogResult.Primary)
			{
				return;
			}

			ServerProperties props = new(Path.Join(meta.ServerPath, "eula.txt"));
			props.Load();
			props.Properties["eula"] = "true";
			props.Save();

			var metaIndex = ApplicationData.Configuration.Servers.FindIndex(item => item.Guid == meta.Guid);
			ServerProcessManager.Instance.StartServer(metaIndex, meta.Guid);
			EulaAccept?.Invoke(metaIndex);
		});
	}

	private void AppEvents_ServerRemoved(ServerMetadata obj)
	{
		WinServerInfo serverEntry = _serverList.First(entry => entry.Metadata == obj);

		if (serverListView.SelectedItem == serverEntry)
		{
			serverListView.SelectedItem = CreateNewServer;
		}

		_serverList.Remove(serverEntry);
	}

	private void AppEvents_NewServerAdded(ServerMetadata obj)
	{
		WinServerInfo serverEntry = new(new(s_softwareIconPaths[obj.Software]), obj);
		_serverList.Add(serverEntry);
		serverListView.SelectedItem = serverEntry;
	}

	private void serverListView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		ServerMetadata.Selected = null;

		if (args.IsSettingsSelected)
		{
			contentFrame.Navigate(typeof(SettingsPage));
		}
		else if (args.SelectedItem is NavItem navItem && navItem.Text == CreateNewServer.Text)
		{
			contentFrame.Navigate(typeof(NewServerPage));
		}
		else
		{
			var metadata = ((WinServerInfo)args.SelectedItem).Metadata;
			ServerMetadata.Selected = metadata;
			contentFrame.Navigate(typeof(ServerManagementPage), ApplicationData.Configuration.Servers.IndexOf(metadata));
		}
	}
}

public class NavItem
{
	public string Text { get; set; }
	public IconElement Icon { get; set; }
}

[ContentProperty(Name = "ItemTemplate")]
partial class MenuItemTemplateSelector : DataTemplateSelector
{
	public DataTemplate ItemTemplate { get; set; }
	public DataTemplate NavItemTemplate { get; set; }

	protected override DataTemplate SelectTemplateCore(object item)
	{
		if (item is NavItem)
		{
			return NavItemTemplate;
		}

		return ItemTemplate;
	}
}

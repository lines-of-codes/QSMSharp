using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using QSM.Core.ServerSoftware;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerListPage : Page
{
	private readonly ObservableCollection<WinServerInfo> ServerList = new();
	private static readonly Dictionary<ServerSoftwares, string> SoftwareIconPaths = new()
	{
		{ ServerSoftwares.Paper, "ms-appx:///Assets/ServerSoftware/papermc-logomark.png" },
		{ ServerSoftwares.Purpur, "ms-appx:///Assets/ServerSoftware/purpur.svg" },
		{ ServerSoftwares.Vanilla, "ms-appx:///Assets/ServerSoftware/minecraft_logo.svg" },
		{ ServerSoftwares.Fabric, "ms-appx:///Assets/ServerSoftware/Fabric.png" },
		{ ServerSoftwares.NeoForge, "ms-appx:///Assets/ServerSoftware/NeoForged.png" },
		{ ServerSoftwares.Velocity, "ms-appx:///Assets/ServerSoftware/velocity-blue.svg" },
		{ ServerSoftwares.Forge, "ms-appx:///Assets/ServerSoftware/forge.png" }
	};

	public ServerListPage()
	{
		foreach (ServerMetadata metadata in ApplicationData.Configuration.Servers)
		{
			ServerList.Add(new WinServerInfo(new(SoftwareIconPaths[metadata.Software]), metadata));
		}

		this.InitializeComponent();

		AppEvents.NewServerAdded += AppEvents_NewServerAdded;
		AppEvents.ServerRemoved += AppEvents_ServerRemoved;
	}

	private void AppEvents_ServerRemoved(ServerMetadata obj)
	{
		WinServerInfo serverEntry = ServerList.First(entry => entry.Metadata == obj);

		if (serverListView.SelectedItem == serverEntry)
		{
			serverListView.SelectedItem = CreateNewServer;
		}

		ServerList.Remove(serverEntry);
	}

	private void AppEvents_NewServerAdded(ServerMetadata obj)
	{
		WinServerInfo serverEntry = new(new(SoftwareIconPaths[obj.Software]), obj);
		ServerList.Add(serverEntry);
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

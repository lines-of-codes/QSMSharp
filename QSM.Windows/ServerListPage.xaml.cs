using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using QSM.Core.ServerSoftware;
using System.Collections.Generic;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerListPage : Page
    {
        private ObservableCollection<WinServerInfo> ServerList = new();
        private static Dictionary<ServerSoftwares, string> SoftwareIconPaths = new()
        {
            { ServerSoftwares.Paper, "ms-appx:///Assets/ServerSoftware/papermc-logomark.png" },
            { ServerSoftwares.Purpur, "ms-appx:///Assets/ServerSoftware/purpur.svg" },
            { ServerSoftwares.Vanilla, "ms-appx:///Assets/ServerSoftware/minecraft_logo.svg" },
            { ServerSoftwares.Fabric, "ms-appx:///Assets/ServerSoftware/Fabric.png" },
            { ServerSoftwares.NeoForge, "ms-appx:///Assets/ServerSoftware/NeoForged.png" },
            { ServerSoftwares.Velocity, "ms-appx:///Assets/ServerSoftware/velocity-blue.svg" }
        };

        public ServerListPage()
        {
            ServerList.Add(new WinServerInfo(new(Symbol.Add), new ServerMetadata()
            {
                Name = "Create server"
            }));

            foreach(ServerMetadata metadata in ApplicationData.Configuration.Servers)
            {
                ServerList.Add(new WinServerInfo(new(SoftwareIconPaths[metadata.Software]), metadata));
            }

            this.InitializeComponent();

            AppEvents.NewServerAdded += AppEvents_NewServerAdded;
        }

        private void AppEvents_NewServerAdded(ServerMetadata obj)
        {
            WinServerInfo serverEntry = new(new(SoftwareIconPaths[obj.Software]), obj);
            ServerList.Add(serverEntry);
            serverListView.SelectedItem = serverEntry;
        }

        private void serverListView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else if (((WinServerInfo)args.SelectedItem).Metadata.Name == "Create server")
            {
                contentFrame.Navigate(typeof(CreateServerPage));
            }
            else
            {
                contentFrame.Navigate(typeof(ServerManagementPage), ApplicationData.Configuration.Servers.IndexOf(((WinServerInfo)args.SelectedItem).Metadata));
            }
        }
    }

    [ContentProperty(Name = "ItemTemplate")]
    class MenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ItemTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item)
        {
            return ItemTemplate;
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
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

        public ServerListPage()
        {
            ServerList.Add(new WinServerInfo("Create server", new(Symbol.Add), ""));
            this.InitializeComponent();
        }

        private void serverListView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                contentFrame.Navigate(typeof(SettingsPage));
            }
            else if (((WinServerInfo)args.SelectedItem).Name == "Create server")
            {
                contentFrame.Navigate(typeof(CreateServerPage));
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

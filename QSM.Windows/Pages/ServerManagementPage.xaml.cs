using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerManagementPage : Page
    {
        int _metadataIndex;

        public ServerManagementPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _metadataIndex = (int)e.Parameter;

            ConfigurationNavigationView.SelectedItem = SummaryTab;

            base.OnNavigatedTo(e);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            navOptions.IsNavigationStackEnabled = false;
            Type targetPage = null;
            NavigationViewItem viewItem = (NavigationViewItem)args.SelectedItem;

            switch (viewItem.Name)
            {
                case "SummaryTab":
                    targetPage = typeof(ServerSummaryPage);
                    break;
                case "BackupsTab":
                    targetPage = typeof(ServerBackupsPage);
                    break;
                case "ConfigurationTab":
                    targetPage = typeof(ServerConfigurationPage);
                    break;
                case "ModsPluginsTab":
                    targetPage = typeof(ModManagementPage);
                    break;
                case "ConsoleTab":
                    targetPage = typeof(ServerConsolePage);
                    break;
                default:
                    throw new ArgumentException("An unexpected NavigationViewItem has been encountered!");
            }

            ConfigurationFrame.NavigateToType(targetPage, _metadataIndex, navOptions);
        }
    }
}

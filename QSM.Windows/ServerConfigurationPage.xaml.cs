using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using QSM.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerConfigurationPage : Page
    {
        int MetadataIndex;
        ServerMetadata Metadata;

        public ServerConfigurationPage()
        {
            this.InitializeComponent();
            ConfigurationNavigationView.SelectedItem = WorldGenTab;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MetadataIndex = (int)e.Parameter;
            Metadata = ApplicationData.Configuration.Servers[MetadataIndex];

            base.OnNavigatedTo(e);
        }

        private void ConfigurationNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            navOptions.IsNavigationStackEnabled = false;
            Type targetPage = null;
            NavigationViewItem viewItem = (NavigationViewItem)args.SelectedItem;

            switch(viewItem.Name)
            {
                case "WorldGenTab":
                    targetPage = typeof(WorldGenConfigPage);
                    break;
                case "GameplayTab":
                    targetPage = typeof(GameplayConfigPage);
                    break;
                case "PerformanceTab":
                    targetPage = typeof(ServerPerformanceConfigPage);
                    break;
                case "NetworkTab":
                    targetPage = typeof(NetworkConfigPage);
                    break;
                case "PlayerInteractTab":
                    targetPage = typeof(PlayerInteractConfigPage);
                    break;
                case "SecurityTab":
                    targetPage = typeof(ServerSecurityConfigPage);
                    break;
                case "DebugTab":
                    targetPage = typeof(ServerDebugConfigPage);
                    break;
                case "JavaTab":
                    targetPage = typeof(ServerJavaConfigPage);
                    break;
                case "MiscTab":
                    targetPage = typeof(ServerMiscConfigPage);
                    break;
            }

            ConfigurationFrame.NavigateToType(targetPage, MetadataIndex, navOptions);
        }
    }
}

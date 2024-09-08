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

            if (viewItem == WorldGenTab)
            {
                targetPage = typeof(WorldGenConfigPage);
            }
            else if (viewItem == GameplayTab)
            {
                targetPage = typeof(GameplayConfigPage);
            }
            else if (viewItem == PerformanceTab)
            {
                targetPage = typeof(ServerPerformanceConfigPage);
            }
            else if (viewItem == NetworkTab)
            {
                targetPage = typeof(NetworkConfigPage);
            }
            else if (viewItem == PlayerInteractTab)
            {
                targetPage = typeof(PlayerInteractConfigPage);
            }
            else if (viewItem == SecurityTab)
            {
                targetPage = typeof(ServerSecurityConfigPage);
            }
            else if (viewItem == DebugTab)
            {
                targetPage = typeof(ServerDebugConfigPage);
            }
            else if (viewItem == JavaTab)
            {
                targetPage = typeof(ServerJavaConfigPage);
            }
            else if (viewItem == MiscTab)
            {
                targetPage = typeof(ServerMiscConfigPage);
            }

            ConfigurationFrame.NavigateToType(targetPage, MetadataIndex, navOptions);
        }
    }
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
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
        ServerMetadata Metadata;
        int MetadataIndex;

        public ServerManagementPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MetadataIndex = (int)e.Parameter;
            Metadata = ApplicationData.Configuration.Servers[MetadataIndex];

            ConfigurationNavigationView.SelectedItem = SummaryTab;

            base.OnNavigatedTo(e);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            navOptions.IsNavigationStackEnabled = false;
            Type targetPage = null;

            if ((NavigationViewItem)args.SelectedItem == SummaryTab)
            {
                targetPage = typeof(ServerSummaryPage);
            }
            else if ((NavigationViewItem)args.SelectedItem == BackupsTab)
            {
                targetPage = typeof(ServerBackupsPage);
            }

            ConfigurationFrame.NavigateToType(targetPage, MetadataIndex, navOptions);
        }
    }
}

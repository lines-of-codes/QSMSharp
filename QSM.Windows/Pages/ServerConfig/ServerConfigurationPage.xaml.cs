using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerConfigurationPage : Page
{
    int _metadataIndex;
    ServerMetadata _metadata;

    public ServerConfigurationPage()
    {
        this.InitializeComponent();
        ConfigurationNavigationView.SelectedItem = WorldGenTab;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _metadataIndex = (int)e.Parameter;
        _metadata = ApplicationData.Configuration.Servers[_metadataIndex];

        base.OnNavigatedTo(e);
    }

    private void ConfigurationNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
		FrameNavigationOptions navOptions = new()
		{
			TransitionInfoOverride = args.RecommendedNavigationTransitionInfo,
			IsNavigationStackEnabled = false
		};
        NavigationViewItem viewItem = (NavigationViewItem)args.SelectedItem;

		Type targetPage = viewItem.Name switch
		{
			"WorldGenTab" => typeof(WorldGenConfigPage),
			"GameplayTab" => typeof(GameplayConfigPage),
			"PerformanceTab" => typeof(ServerPerformanceConfigPage),
			"NetworkTab" => typeof(NetworkConfigPage),
			"PlayerInteractTab" => typeof(PlayerInteractConfigPage),
			"SecurityTab" => typeof(ServerSecurityConfigPage),
			"DebugTab" => typeof(ServerDebugConfigPage),
			"JavaTab" => typeof(ServerJavaConfigPage),
			"MiscTab" => typeof(ServerMiscConfigPage),
			_ => throw new ArgumentException("An unexpected NavigationViewItem has been encountered!"),
		};

		ConfigurationFrame.NavigateToType(targetPage, _metadataIndex, navOptions);
    }
}

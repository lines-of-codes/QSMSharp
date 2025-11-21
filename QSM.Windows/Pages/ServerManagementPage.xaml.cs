using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages;
using QSM.Windows.Pages.ServerConfig;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerManagementPage : Page
{
	int _metadataIndex;
	ServerMetadata _metadata;

	public ServerManagementPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		_metadata = ApplicationData.Configuration.Servers[_metadataIndex];

		if (ServerSettings.TryLoadJson(_metadata.QsmConfigFile, out var settings))
			ApplicationData.ServerSettings[_metadata.Guid] = settings;

		ConfigurationNavigationView.SelectedItem = SummaryTab;

		base.OnNavigatedTo(e);
	}

	private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		FrameNavigationOptions navOptions = new()
		{
			TransitionInfoOverride = args.RecommendedNavigationTransitionInfo,
			IsNavigationStackEnabled = false
		};
		NavigationViewItem viewItem = (NavigationViewItem)args.SelectedItem;

		Type targetPage = viewItem.Name switch
		{
			"SummaryTab" => typeof(ServerSummaryPage),
			"BackupsTab" => typeof(ServerBackupsPage),
			"ConfigurationTab" => typeof(ServerConfigurationPage),
			"ModsPluginsTab" => typeof(ModManagementPage),
			"ConsoleTab" => typeof(ServerConsolePage),
			_ => throw new ArgumentException("An unexpected NavigationViewItem has been encountered!"),
		};

		ConfigurationFrame.NavigateToType(targetPage, _metadataIndex, navOptions);
	}
}

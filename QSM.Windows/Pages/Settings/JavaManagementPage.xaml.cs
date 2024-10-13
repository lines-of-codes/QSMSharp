using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Serilog;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Settings;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class JavaManagementPage : Page
{
	public JavaManagementPage()
	{
		this.InitializeComponent();
		ManagementNavigationView.SelectedItem = ManageTab;
	}

	private void ManagementNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		FrameNavigationOptions navOptions = new();
		navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
		navOptions.IsNavigationStackEnabled = false;
		Type targetPage = null;
		var viewItem = (NavigationViewItem)ManagementNavigationView.SelectedItem;

		switch (viewItem.Name)
		{
			case "ManageTab":
				targetPage = typeof(JavaListPage);
				break;
			case "DownloadTab":
				targetPage = typeof(JavaDownloadPage);
				break;
			default:
				Log.Warning("An unknown Java management tab has been accessed.");
				break;
		}

		ContentFrame.NavigateToType(targetPage, null, navOptions);
	}
}

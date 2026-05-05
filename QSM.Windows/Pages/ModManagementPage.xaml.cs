using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ModPluginSource;
using QSM.Windows.Pages;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

internal class ProviderInfo
{
	public string Icon;
	public string ProviderName;
	public ModPluginProvider Provider;
}

/// <summary>
/// Mods/Plugins management page.
/// </summary>
public sealed partial class ModManagementPage : Page
{
	int _metadataIndex;

	public ModManagementPage()
	{
		InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		NavigationView.SelectedItem = ManageTab;

		base.OnNavigatedTo(e);
	}

	private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		FrameNavigationOptions navOptions = new()
		{
			TransitionInfoOverride = args.RecommendedNavigationTransitionInfo,
			IsNavigationStackEnabled = false
		};
		Type targetPage = null;
		NavigationViewItem viewItem = (NavigationViewItem)args.SelectedItem;

		switch (viewItem.Name)
		{
			case "ManageTab":
				targetPage = typeof(ModListPage);
				break;
			case "SearchTab":
				targetPage = typeof(ModSearchPage);
				break;
			default:
				throw new ArgumentException("An unexpected NavigationViewItem has been encountered!");
		}

		ContentFrame.NavigateToType(targetPage, _metadataIndex, navOptions);
	}
}

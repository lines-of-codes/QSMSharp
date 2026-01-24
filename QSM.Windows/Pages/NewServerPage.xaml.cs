using Microsoft.UI.Xaml.Controls;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NewServerPage : Page
{
	public NewServerPage()
	{
		this.InitializeComponent();
		NewNavigationView.SelectedItem = CreateTab;
	}

	private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		Type page = null;

		switch (((NavigationViewItem)sender.SelectedItem).Name)
		{
			case "CreateTab":
				page = typeof(CreateServerPage);
				break;
			case "ModrinthTab":
				page = typeof(ModrinthImportPage);
				break;
			case "CurseForgeTab":
				page = typeof(CurseForgeImportPage);
				break;
			case "ManualImportTab":
				page = typeof(ImportLocalPage);
				break;
		}

		ContentFrame.Navigate(page);
	}
}

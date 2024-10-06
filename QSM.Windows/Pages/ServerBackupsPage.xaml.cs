using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerBackupsPage : Page
{
    int MetadataIndex;
	private readonly ExtendedObservableCollection<BackupItem> _backups = [];

    public ServerBackupsPage()
    {
        this.InitializeComponent();
    }

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		MetadataIndex = (int)e.Parameter;
		_backups.AddRange(ApplicationData.ServerSettings[ApplicationData.Configuration.Servers[MetadataIndex].Guid].Backups);

		base.OnNavigatedTo(e);
	}
}
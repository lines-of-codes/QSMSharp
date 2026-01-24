using Microsoft.UI.Xaml.Controls;
using QSM.Core.ModPluginSource;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Dialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ModDownloadsConfirmPage : Page
{
	public ModPluginDownloadInfo[] DownloadList;

	public ModDownloadsConfirmPage(ModPluginDownloadInfo[] downloadList)
	{
		DownloadList = downloadList;
		InitializeComponent();
	}
}

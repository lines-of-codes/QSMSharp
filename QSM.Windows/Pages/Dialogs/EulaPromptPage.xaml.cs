using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Dialogs;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class EulaPromptPage : Page
{
	public EulaPromptPage()
	{
		InitializeComponent();
	}

	public ContentDialog CreateDialog(Page page, string serverName)
	{
		var common = new ResourceLoader("QSM.Windows.pri", "Common");
		var server = new ResourceLoader("QSM.Windows.pri", "Server");

		ContentDialog dialog = new()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = $"{server.GetString("/EulaNoticeTitle")} ({serverName})",
			PrimaryButtonText = common.GetString("/AgreeTerms"),
			SecondaryButtonText = common.GetString("/DisagreeTerms"),
			IsSecondaryButtonEnabled = true,
			DefaultButton = ContentDialogButton.Primary,
			Content = this
		};

		return dialog;
	}
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Dialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class InfoDialog : Page
{
	public InfoDialog(string message)
	{
		this.InitializeComponent();
		InfoText.Text = message;
	}

	public ContentDialog CreateDialog(string title, Page page)
	{
		ContentDialog dialog = new()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = title,
			PrimaryButtonText = "OK",
			IsSecondaryButtonEnabled = false,
			DefaultButton = ContentDialogButton.Primary,
			Content = this
		};

		return dialog;
	}

	public static ContentDialog CreateDialog(string title, string message, Page page)
	{
		return new InfoDialog(message).CreateDialog(title, page);
	}
}

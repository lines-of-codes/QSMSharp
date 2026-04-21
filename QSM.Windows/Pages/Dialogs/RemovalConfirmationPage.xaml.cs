using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;

namespace QSM.Windows.Pages.Dialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class RemovalConfirmationPage : Page
{
	public bool DeleteFile => DeleteFileCheckbox.IsChecked ?? false;

	public RemovalConfirmationPage(bool displayDeleteFileCheckbox)
	{
		this.InitializeComponent();
		if (!displayDeleteFileCheckbox)
		{
			DeleteFileCheckbox.Visibility = Visibility.Collapsed;
		}
		else
		{
			var loader = new ResourceLoader("QSM.Windows.pri", "Dialogs");
			
			Description.Text += loader.GetString("/RemovalExtDescription");
		}
	}

	public ContentDialog CreateDialog(Page page)
	{
		var common = new ResourceLoader("QSM.Windows.pri", "Common");
		var loader = new ResourceLoader("QSM.Windows.pri", "Dialogs");

		return new ContentDialog()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = loader.GetString("/RemovalConfirmTitle"),
			PrimaryButtonText = common.GetString("/Remove"),
			CloseButtonText = common.GetString("/Cancel"),
			IsSecondaryButtonEnabled = false,
			DefaultButton = ContentDialogButton.Close,
			Content = this
		};
	}
}

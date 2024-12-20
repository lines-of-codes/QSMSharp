using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
			DeleteFileCheckbox.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
		}
		else
		{
			Description.Text += " If the checkbox below is left unchecked, Only the registered entry will be removed and the files on your system will be kept.";
		}
	}

	public ContentDialog CreateDialog(Page page)
	{
		return new ContentDialog()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "Are you sure?",
			PrimaryButtonText = "Remove",
			CloseButtonText = "Cancel",
			IsSecondaryButtonEnabled = false,
			DefaultButton = ContentDialogButton.Close,
			Content = this
		};
	}
}

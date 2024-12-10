using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Core.Backups;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Dialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class BackupCreationPage : Page
{
	public string BackupName => BackupNameInput.Text;
	public ArchiveFormat SelectedArchiveFormat => (ArchiveFormat)ArchiveFormatSelector.SelectedItem;
	public CompressionFormat SelectedCompression => (CompressionFormat)CompressionSelector.SelectedItem;

	private readonly ArchiveFormat[] _archiveFormats =
	[
		ArchiveFormat.Tar,
		ArchiveFormat.Zip
	];
	private readonly ExtendedObservableCollection<CompressionFormat> _compressionFormats = [];

	public BackupCreationPage()
	{
		this.InitializeComponent();
		ArchiveFormatSelector.SelectedIndex = 0;
	}

	private void ArchiveFormatSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		_compressionFormats.Clear();
		switch(SelectedArchiveFormat)
		{
			case ArchiveFormat.Tar:
				_compressionFormats.AddRange(TarArchiver.SupportedCompression);
				break;
			case ArchiveFormat.Zip:
				_compressionFormats.AddRange(ZipArchiver.SupportedCompression);
				break;
			default:
				throw new InvalidOperationException();
		}
		CompressionSelector.SelectedIndex = 0;
	}

	public ContentDialog CreateDialog(Page page)
	{
		ContentDialog dialog = new()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = "Create backup",
			PrimaryButtonText = "Create",
			SecondaryButtonText = "Cancel",
			IsPrimaryButtonEnabled = true,
			IsSecondaryButtonEnabled = true,
			DefaultButton = ContentDialogButton.Primary,
			Content = this
		};

		return dialog;
	}
}

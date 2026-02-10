using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.ApplicationModel.Resources;
using QSM.Core.ModPluginSource.CurseForge;
using System.Diagnostics;

namespace QSM.Windows.Pages.Dialogs;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CurseExtraDownloads : Page
{
	public CurseMissingMod[] FileList;
	string _modsFolder;

	public CurseExtraDownloads(CurseMissingMod[] files, string modsFolder)
	{
		InitializeComponent();
		FileList = files;
		_modsFolder = modsFolder;
	}

	public ContentDialog CreateDialog(Page page, string title = "Missing Mod(s)")
	{
		var loader = new ResourceLoader("QSM.Windows.pri", "Common");

		ContentDialog dialog = new()
		{
			XamlRoot = page.XamlRoot,
			Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
			Title = title,
			PrimaryButtonText = loader.GetString("/Okay"),
			DefaultButton = ContentDialogButton.Primary,
			Content = this
		};

		return dialog;
	}

	private void OpenModsFolderBtn_Click(object sender, RoutedEventArgs e)
	{
		Process.Start("explorer.exe", _modsFolder);
	}
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.IO;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ModListPage : Page
{
	int _metadataIndex;
	string _modsFolderPath;
	readonly ExtendedObservableCollection<string> _mods = [];

	public ModListPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
		var metadata = ApplicationData.Configuration.Servers[_metadataIndex];

		if (metadata.IsModSupported)
			_modsFolderPath = Path.Combine(metadata.ServerPath, "mods");
		if (metadata.IsPluginSupported)
			_modsFolderPath = Path.Combine(metadata.ServerPath, "plugins");

		Directory.CreateDirectory(_modsFolderPath);

		var fsEntries = Directory.EnumerateFiles(_modsFolderPath);

		foreach (var fsEntry in fsEntries)
		{
			_mods.Add(Path.GetFileName(fsEntry));
		}

		Directory.CreateDirectory(_modsFolderPath);

		base.OnNavigatedTo(e);
	}

	private void RemoveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
	{
		var selected = (string)ModList.SelectedItem;

		File.Delete(Path.Combine(_modsFolderPath, selected));

		_mods.Remove(selected);
	}
}

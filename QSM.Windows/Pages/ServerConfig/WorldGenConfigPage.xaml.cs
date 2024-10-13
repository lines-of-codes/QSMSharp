using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class WorldGenConfigPage : Page
{
	ServerMetadata Metadata;
	ExtendedObservableCollection<string> levelTypes =
	[
		"normal",
		"flat",
		"large_biomes",
		"amplified",
		"single_biome_surface"
	];

	public WorldGenConfigPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		int MetadataIndex = (int)e.Parameter;
		Metadata = ApplicationData.Configuration.Servers[MetadataIndex];
		Version minecraftVersion = new(Metadata.MinecraftVersion);

		// If Minecraft server version is 1.15 or below
		if (minecraftVersion.Minor <= 15)
		{
			levelTypes.AddRange(["buffet", "default_1_1", "customized"]);
		}

		base.OnNavigatedTo(e);
	}
}

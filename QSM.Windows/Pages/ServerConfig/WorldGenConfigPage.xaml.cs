using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;
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
	class WorldSettings : PropertyModificationModel
	{
		[ServerProperty("level-name")]
		public string LevelName { get; set; } = "world";

		[ServerProperty("level-seed")]
		public string LevelSeed { get; set; } = string.Empty;

		[ServerProperty("generator-settings")]
		public string GeneratorSettings { get; set; } = "{}";

		[ServerProperty("max-world-size")]
		public uint MaxWorldSize { get; set; } = 29999984;

		[ServerProperty("level-type")]
		public string LevelType { get; set; } = "minecraft:normal";

		[ServerProperty("generate-structures")]
		public bool GenerateStructures { get; set; } = true;
	}

	WorldSettings _worldSettings = new();
	ServerProperties _serverProps;
	ServerMetadata _metadata;
	ExtendedObservableCollection<string> _levelTypes =
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
		int metadataIndex = (int)e.Parameter;
		_metadata = ApplicationData.Configuration.Servers[metadataIndex];
		Version minecraftVersion = new(_metadata.MinecraftVersion);

		// If Minecraft server version is 1.15 or below
		if (minecraftVersion.Minor <= 15)
		{
			_levelTypes.AddRange(["buffet", "default_1_1", "customized"]);
		}

		_serverProps = new ServerProperties(_metadata.ServerPropertiesFile);
		_serverProps.Load();
		_worldSettings.Load(_serverProps);

		if (_worldSettings.LevelType.StartsWith("minecraft\\:"))
		{
			_worldSettings.LevelType = _worldSettings.LevelType["minecraft\\:".Length..];
		}

		base.OnNavigatedTo(e);
	}

	protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
	{
		_worldSettings.Apply(_serverProps);
		_serverProps.Save();

		base.OnNavigatingFrom(e);
	}
}

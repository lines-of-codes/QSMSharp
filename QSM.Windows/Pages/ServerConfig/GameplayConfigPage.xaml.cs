using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;
using QSM.Windows.Utilities;
using System;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class GameplayConfigPage : Page
{
	private class Gameplay : PropertyModificationModel
	{
		[ServerProperty("gamemode")]
		public string Gamemode { get; set; } = "survival";

		[ServerProperty("difficulty")]
		public string Difficulty { get; set; } = "easy";


		[ServerProperty("force-gamemode")]
		public bool ForceGameMode { get; set; }

		[ServerProperty("hardcore")]
		public bool Hardcore { get; set; }

		[ServerProperty("pvp")]
		public bool PvP { get; set; } = true;

		[ServerProperty("allow-flight")]
		public bool AllowFlight { get; set; }

		[ServerProperty("allow-nether")]
		public bool AllowNether { get; set; } = true;
	}

	private Gameplay _settingsData = new();
	private ServerProperties _serverProps;

	public GameplayConfigPage()
	{
		this.InitializeComponent();
	}
	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		var metadataIndex = (int)e.Parameter;
		var server = ApplicationData.Configuration.Servers[metadataIndex];
		_serverProps = new ServerProperties(server.ServerPropertiesFile);
		_serverProps.Load();
		_settingsData.Load(_serverProps);

		var minecraftVersion = new Version(server.MinecraftVersion);
		var affectedVersion = new Version("1.21.9");

		// Minecraft 1.21.9 removed these two properties
		if (minecraftVersion >= affectedVersion)
		{
			PvP.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
			AllowNether.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
		}

		base.OnNavigatedTo(e);
	}

	protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
	{
		_settingsData.Apply(_serverProps);
		_serverProps.Save();

		base.OnNavigatingFrom(e);
	}
}

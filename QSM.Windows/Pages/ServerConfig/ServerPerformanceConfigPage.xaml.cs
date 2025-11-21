using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Org.BouncyCastle.Asn1.Cms;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerPerformanceConfigPage : Page
{
	class PerformanceSettings : PropertyModificationModel
	{
		[ServerProperty("max-players")]
		public int MaxPlayers { get; set; } = 20;

		[ServerProperty("view-distance")]
		public int ViewDistance { get; set; } = 10;

		[ServerProperty("simulation-distance")]
		public int SimulationDistance { get; set; } = 10;

		[ServerProperty("entity-broadcast-range-percentage")]
		public int EntityBroadcastRangePercentage { get; set; } = 100;

		[ServerProperty("max-tick-time")]
		public int MaxTickTime { get; set; } = 60_000;
	}

	PerformanceSettings _settings = new();
	ServerProperties _serverProps;

	public ServerPerformanceConfigPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		int metadataIndex = (int)e.Parameter;
		var _metadata = ApplicationData.Configuration.Servers[metadataIndex];

		_serverProps = new ServerProperties(_metadata.ServerPropertiesFile);
		_serverProps.Load();
		_settings.Load(_serverProps);
		
		base.OnNavigatedTo(e);
	}

	protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
	{
		_settings.Apply(_serverProps);
		_serverProps.Save();

		base.OnNavigatingFrom(e);
	}
}

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class NetworkConfigPage : Page
{
	class NetworkSettings : PropertyModificationModel
	{
		[ServerProperty("server-ip")]
		public string ServerIp { get; set; } = string.Empty;

		[ServerProperty("server-port")]
		public int ServerPort { get; set; } = 25565;

		[ServerProperty("query.port")]
		public int QueryPort { get; set; } = 25565;

		[ServerProperty("network-compression-threshold")]
		public int NetworkCompressionThreshold { get; set; } = 256;

		[ServerProperty("enable-status")]
		public bool EnableStatus { get; set; } = true;

		[ServerProperty("enable-query")]
		public bool EnableQuery { get; set; }

		[ServerProperty("use-native-transport")]
		public bool UseNativeTransport { get; set; } = true;
	}

	NetworkSettings _settings = new();
	ServerProperties _serverProps;

	public NetworkConfigPage()
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

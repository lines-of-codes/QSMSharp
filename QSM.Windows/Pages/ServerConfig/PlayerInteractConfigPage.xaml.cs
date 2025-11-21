using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class PlayerInteractConfigPage : Page
{
	class InteractionSettings : PropertyModificationModel
	{
		[ServerProperty("op-permission-level")]
		public int OpPermissionLevel { get; set; } = 4;

		[ServerProperty("spawn-protection")]
		public int SpawnProtection { get; set; } = 16;

		[ServerProperty("white-list")]
		public bool Whitelist { get; set; }

		[ServerProperty("enforce-whitelist")]
		public bool EnforceWhitelist { get; set; }
	}

	InteractionSettings _settings = new();
	ServerProperties _serverProps;

	public PlayerInteractConfigPage()
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

using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerDebugConfigPage : Page
{
	class DebugSettings : PropertyModificationModel
	{
		[ServerProperty("debug")]
		public bool Debug { get; set; }

		[ServerProperty("enable-jmx-monitoring")]
		public bool EnableJmxMonitoring { get; set; }
	}

	DebugSettings _settings = new();
	ServerProperties _serverProps;

	public ServerDebugConfigPage()
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

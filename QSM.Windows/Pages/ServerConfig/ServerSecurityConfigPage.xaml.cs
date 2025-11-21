using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSettings;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.ServerConfig;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerSecurityConfigPage : Page
{
	class SecurityOptions : PropertyModificationModel
	{
		[ServerProperty("online-mode")]
		public bool OnlineMode { get; set; } = true;

		[ServerProperty("prevent-proxy-connections")]
		public bool PreventProxyConnections { get; set; }

		[ServerProperty("enable-command-block")]
		public bool EnableCommandBlocks { get; set; }
	}

	SecurityOptions _settings = new();
	ServerProperties _serverProps;

	static readonly Version s_changedVersion = new("1.21.9");

	public ServerSecurityConfigPage()
	{
		this.InitializeComponent();
	}

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		int metadataIndex = (int)e.Parameter;
		var _metadata = ApplicationData.Configuration.Servers[metadataIndex];
		var minecraftVersion = new Version(_metadata.MinecraftVersion);

		_serverProps = new ServerProperties(_metadata.ServerPropertiesFile);
		_serverProps.Load();
		_settings.Load(_serverProps);

		if (minecraftVersion >= s_changedVersion)
		{
			EnableCommandBlocks.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
		}

		base.OnNavigatedTo(e);
	}

	protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
	{
		_settings.Apply(_serverProps);
		_serverProps.Save();

		base.OnNavigatingFrom(e);
	}
}

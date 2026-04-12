using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Serilog;
using SkiaSharp;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
	/// <summary>
	/// Initializes the singleton application object.  This is the first line of authored code
	/// executed, and as such is the logical equivalent of main() or WinMain().
	/// </summary>
	public App()
	{
		this.InitializeComponent();

		UnhandledException += App_UnhandledException;

		ApplicationData.LoadConfiguration();
	}

	private static void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		Log.Fatal(e.Exception, e.Message);
	}

	/// <summary>
	/// Invoked when the application is launched.
	/// </summary>
	/// <param name="args">Details about the launch request and process.</param>
	protected override void OnLaunched(LaunchActivatedEventArgs args)
	{
		MainWindow = new MainWindow();
		MainWindow.Activate();

		LiveCharts.Configure(config =>
		{
			if (RequestedTheme == ApplicationTheme.Light)
			{
				config.AddLightTheme();
			}
			else
			{
				config.AddDarkTheme();
			}

			config.HasTextSettings(new TextSettings()
			{
				DefaultTypeface = SKFontManager.Default.MatchCharacter('ก')
			});
		});

		MainWindow.AppWindow.Closing += OnClosing;
	}

	private static async void OnClosing(object sender, AppWindowClosingEventArgs e)
	{
		foreach (var process in ServerProcessManager.Instance.Processes.Where(process => !process.Value.HasExited))
		{
			process.Value.Kill();
		}

		await Log.CloseAndFlushAsync();
	}

	public static Window MainWindow { get; private set; }
}

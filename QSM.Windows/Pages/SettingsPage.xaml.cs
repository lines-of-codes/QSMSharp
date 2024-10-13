using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using QSM.Windows.Pages.Settings;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class SettingsPage : Page
{
	public static Window JavaWindow { get; private set; }
	private static readonly string[] s_systemFonts = CanvasTextFormat.GetSystemFontFamilies();

	public SettingsPage()
	{
		this.InitializeComponent();
		MonospaceFontSelector.Text = ApplicationData.Configuration.MonospaceFont;
	}

	private void OpenJavaWindowButton_Click(object sender, RoutedEventArgs e)
	{
		JavaWindow = new Window
		{
			Title = "Manage Java Installations",
			Content = new JavaManagementPage()
		};
		JavaWindow.Activate();
	}

	private void MonospaceFontSelector_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
	{
		if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
		{
			var suitableItems = new List<string>();
			var splitText = sender.Text.ToLower().Split(" ");

			foreach (var font in s_systemFonts)
			{
				var found = splitText.All((key) => font.Contains(key, System.StringComparison.CurrentCultureIgnoreCase));

				if (found)
				{
					suitableItems.Add(font);
				}
			}

			if (suitableItems.Count == 0)
			{
				suitableItems.Add("No results found");
			}
			sender.ItemsSource = suitableItems;
		}
	}

	private void MonospaceFontSelector_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
	{
		ApplicationData.Configuration.MonospaceFont = args.SelectedItem.ToString();
		ApplicationData.SaveConfiguration();
	}

	private void ResetConfigButton_Click(object sender, RoutedEventArgs e)
	{
		Log.Information("Resetting configuration file...");
		File.Delete(ApplicationData.ConfigFile);
		ApplicationData.SaveConfiguration();
		Log.Information("Configuration resetting finished.");
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using QSM.Windows.Pages.Settings;
using QSM.Windows.Utilities;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JavaPickerPage : Page
    {
		ExtendedObservableCollection<JavaInstallation> JavaInstallations = new();
		public JavaInstallation SelectedInstallation => (JavaInstallation)JavaInstallationList.SelectedItem;

		public JavaPickerPage()
        {
            this.InitializeComponent();
			foreach (string path in ApplicationData.Configuration.JavaInstallations)
			{
				if (JavaCheck.CheckJavaInstallation(path, out var install))
					JavaInstallations.Add(install);
			}
			JavaInstallationList.SelectedIndex = 0;
		}

		public ContentDialog CreateDialog(Page page)
		{
			ContentDialog dialog = new()
			{
				XamlRoot = page.XamlRoot,
				Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
				Title = "Pick a Java installation",
				PrimaryButtonText = "Select",
				IsSecondaryButtonEnabled = false,
				DefaultButton = ContentDialogButton.Primary,
				Content = this
			};

			return dialog;
		}
	}
}

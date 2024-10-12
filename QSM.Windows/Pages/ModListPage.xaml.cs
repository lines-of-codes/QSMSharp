using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModListPage : Page
    {
        int _metadataIndex;
		string _modsFolderPath;
		readonly ExtendedObservableCollection<string> _mods = [];

        public ModListPage()
        {
            this.InitializeComponent();
        }

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			_metadataIndex = (int)e.Parameter;
            var metadata = ApplicationData.Configuration.Servers[_metadataIndex];

            if (metadata.IsModSupported)
                _modsFolderPath = Path.Combine(metadata.ServerPath, "mods");
            if (metadata.IsPluginSupported)
                _modsFolderPath = Path.Combine(metadata.ServerPath, "plugins");

            Directory.CreateDirectory(_modsFolderPath);

            var fsEntries = Directory.EnumerateFiles(_modsFolderPath);

            foreach (var fsEntry in fsEntries)
            {
                _mods.Add(Path.GetFileName(fsEntry));
            }

            Directory.CreateDirectory(_modsFolderPath);

			base.OnNavigatedTo(e);
		}
	}
}

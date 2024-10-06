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
using QSM.Core.ServerSoftware;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WorldGenConfigPage : Page
    {
        ServerMetadata Metadata;
        ExtendedObservableCollection<string> levelTypes = 
        [
            "normal",
            "flat",
            "large_biomes",
            "amplified",
            "single_biome_surface"
        ];

        public WorldGenConfigPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int MetadataIndex = (int)e.Parameter;
            Metadata = ApplicationData.Configuration.Servers[MetadataIndex];
            Version minecraftVersion = new(Metadata.MinecraftVersion);

            // If Minecraft server version is 1.15 or below
            if (minecraftVersion.Minor <= 15)
            {
                levelTypes.AddRange(["buffet", "default_1_1", "customized"]);
            }

            base.OnNavigatedTo(e);
        }
    }
}

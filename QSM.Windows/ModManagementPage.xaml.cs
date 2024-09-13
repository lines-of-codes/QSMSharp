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
using System.Collections.ObjectModel;
using QSM.Core.ModPluginSource;
using QSM.Core.ServerSoftware;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    internal class ProviderInfo
    {
        public string Icon;
        public string ProviderName;
        public ModPluginProvider Provider;
    }

    /// <summary>
    /// Mods/Plugins management page.
    /// </summary>
    public sealed partial class ModManagementPage : Page
    {
        int MetadataIndex;

        public ModManagementPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MetadataIndex = (int)e.Parameter;
            NavigationView.SelectedItem = SearchTab;

            base.OnNavigatedTo(e);
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            FrameNavigationOptions navOptions = new();
            navOptions.TransitionInfoOverride = args.RecommendedNavigationTransitionInfo;
            navOptions.IsNavigationStackEnabled = false;
            Type targetPage = null;
            NavigationViewItem viewItem = (NavigationViewItem)args.SelectedItem;

            switch (viewItem.Name)
            {
                case "SearchTab":
                    targetPage = typeof(ModSearchPage);
                    break;
            }

            ContentFrame.NavigateToType(targetPage, MetadataIndex, navOptions);
        }
    }
}

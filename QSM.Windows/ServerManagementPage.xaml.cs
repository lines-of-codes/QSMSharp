using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using ScottPlot;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerManagementPage : Page
    {
        ServerMetadata Metadata;

        public ServerManagementPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Metadata = ApplicationData.Configuration.Servers[(int)e.Parameter];

            ServerNameTitle.Text = Metadata.Name;

            ServerStats.Plot.Add.Signal(Generate.Sin(51));
            ServerStats.Plot.Add.Signal(Generate.Cos(51));
            ServerStats.Refresh();

            base.OnNavigatedTo(e);
        }
    }
}

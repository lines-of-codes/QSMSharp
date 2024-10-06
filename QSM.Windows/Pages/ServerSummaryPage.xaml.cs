using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using QSM.Core.ServerSoftware;
using QSM.Windows.Pages.Dialogs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerSummaryPage : Page
    {
        ServerMetadata _metadata;

        public ServerSummaryPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _metadata = ApplicationData.Configuration.Servers[(int)e.Parameter];

            ServerNameTitle.Text = _metadata.Name;
            ServerSoftwareInfo.Text = $"{_metadata.Software.ToString()} {_metadata.MinecraftVersion} ({_metadata.ServerVersion})";

            base.OnNavigatedTo(e);
        }

        private void StartButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }

		private void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
            var deletePage = new RemovalConfirmationPage(true);


        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using QSM.Windows.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SingleFileDownloadDialogContent : Page
    {
        public SingleFileDownloadDialogContent()
        {
            this.InitializeComponent();
        }

        public void SetIsIndeterminate(bool indeterminate)
        {
            DownloadProgressBar.IsIndeterminate = indeterminate;
        }

        public void UpdateProgress(double percentage, long totalBytesRead, long totalBytes)
        {
            DownloadProgressBar.Value = percentage;
            DownloadProgressText.Text = $"Downloaded {SizeUnitConversion.bytesToAppropriateUnit(totalBytesRead)} of {SizeUnitConversion.bytesToAppropriateUnit(totalBytes)} ({percentage:0.00}%)";
        }

        public void DownloadComplete()
        {
            DownloadProgressText.Text = "Download complete!";
        }
    }
}

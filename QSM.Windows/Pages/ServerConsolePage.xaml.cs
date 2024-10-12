using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ServerConsolePage : Page
{
    int _metadataIndex;
    Guid _serverGuid;

    public ServerConsolePage()
    {
        this.InitializeComponent();

        // Configured font + some fallback fonts
        var monospaceFont = new FontFamily($"{ApplicationData.Configuration.MonospaceFont},Cascadia Code,Consolas");
        ProcessOutput.FontFamily = monospaceFont;
        CommandInput.FontFamily = monospaceFont;
    }

	protected override void OnNavigatedTo(NavigationEventArgs e)
	{
		_metadataIndex = (int)e.Parameter;
        var metadata = ApplicationData.Configuration.Servers[_metadataIndex];

        _serverGuid = metadata.Guid;

        if (!ServerProcessManager.Instance.Processes.TryGetValue(_serverGuid, out var process))
            return;

        var outputs = ServerProcessManager.Instance.ProcessOutputs[_serverGuid];

        foreach (var output in outputs)
        {
            switch(output.Type)
            {
                case ServerProcessManager.OutputType.Normal:
                    ProcessOutput.Text += output.Message + Environment.NewLine;
                    break;
                case ServerProcessManager.OutputType.Error:
                    AppendError(output.Message);
                    break;
            }
        }

        ScrollToEnd();

        process.OutputDataReceived += (sender, args) =>
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                ProcessOutput.Text += args.Data + Environment.NewLine;
                ScrollToEnd();
            });
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            AppendError(args.Data);
        };

		base.OnNavigatedTo(e);
	}

    void AppendError(string data)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
			Run run = new()
			{
				Foreground = new SolidColorBrush(Colors.Red),
				Text = data + Environment.NewLine
			};
			ProcessOutput.Inlines.Add(run);
            ScrollToEnd();
		});
	}

    void ScrollToEnd()
    {
		OutputScrollView.ScrollToVerticalOffset(OutputScrollView.ScrollableHeight);
	}

	private async void CommandInput_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
        if (e.Key == VirtualKey.Enter)
        {
            if (!ServerProcessManager.Instance.Processes.TryGetValue(_serverGuid, out var process) && process.HasExited)
                return;

            await process.StandardInput.WriteLineAsync(CommandInput.Text);
            CommandInput.Text = "";
        }
	}
}

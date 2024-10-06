using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Runtime.InteropServices;
using QSM.Windows.Utilities;
using QSM.Windows.Pages.Dialogs;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace QSM.Windows
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ServerJavaConfigPage : Page
    {
        static readonly Dictionary<string, string> s_jvmArgPresets = new()
        {
            { "None", "" },
            { "Aikar's Flags", "-XX:+UseG1GC -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=200 -XX:+UnlockExperimentalVMOptions -XX:+DisableExplicitGC -XX:+AlwaysPreTouch -XX:G1NewSizePercent=30 -XX:G1MaxNewSizePercent=40 -XX:G1HeapRegionSize=8M -XX:G1ReservePercent=20 -XX:G1HeapWastePercent=5 -XX:G1MixedGCCountTarget=4 -XX:InitiatingHeapOccupancyPercent=15 -XX:G1MixedGCLiveThresholdPercent=90 -XX:G1RSetUpdatingPauseTimePercent=5 -XX:SurvivorRatio=32 -XX:+PerfDisableSharedMem -XX:MaxTenuringThreshold=1 -Dusing.aikars.flags=https://mcflags.emc.gs -Daikars.new.flags=true" },
            { "Aikar's Flags (12GB+)", "-XX:+UseG1GC -XX:+ParallelRefProcEnabled -XX:MaxGCPauseMillis=200 -XX:+UnlockExperimentalVMOptions -XX:+DisableExplicitGC -XX:+AlwaysPreTouch -XX:G1NewSizePercent=40 -XX:G1MaxNewSizePercent=50 -XX:G1HeapRegionSize=16M -XX:G1ReservePercent=15 -XX:G1HeapWastePercent=5 -XX:G1MixedGCCountTarget=4 -XX:InitiatingHeapOccupancyPercent=20 -XX:G1MixedGCLiveThresholdPercent=90 -XX:G1RSetUpdatingPauseTimePercent=5 -XX:SurvivorRatio=32 -XX:+PerfDisableSharedMem -XX:MaxTenuringThreshold=1 -Dusing.aikars.flags=https://mcflags.emc.gs -Daikars.new.flags=true" },
            { "Shenandoah GC", "-XX:+UnlockExperimentalVMOptions -XX:+UseShenandoahGC" },
            { "ZGC", "-XX:+UnlockExperimentalVMOptions -XX:+UseZGC" }
        };
        static readonly Dictionary<string, string> s_jvmArgPresetInverse = s_jvmArgPresets.ToDictionary(x => x.Value, x => x.Key);

        /// <summary>
        /// Offset the maximum memory amount you can set by n GB(s)
        /// Help prevent users from freezing their OS by giving Minecraft all memory.
        /// (And also the fact that the JVM can have some overhead, allocating more 
        /// memory than specified in the specified Xmx argument)
        /// </summary>
        const byte MaxMemoryOffset = 2;

        [LibraryImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        public ServerJavaConfigPage()
        {
            this.InitializeComponent();

            GetPhysicallyInstalledSystemMemory(out long totalMem);
            double maxMemGb = Math.Round(SizeUnitConversion.kilobytesToGigabytes(totalMem) * 2, MidpointRounding.AwayFromZero) / 2 - MaxMemoryOffset;

            InitMemorySizeSlider.Maximum = maxMemGb;
            MaxMemorySizeSlider.Maximum = maxMemGb;
        }

        private void JvmArgPresetSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (s_jvmArgPresets.TryGetValue((string)JvmArgPresetSelector.SelectedItem, out string args))
            {
                if (JvmArgsInput == null) return;
                JvmArgsInput.Text = args;
            }
        }

        private void JvmArgsInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (s_jvmArgPresetInverse.TryGetValue(JvmArgsInput.Text, out string presetName))
            {
                JvmArgPresetSelector.SelectedItem = presetName;
                return;
            }

            JvmArgPresetSelector.SelectedItem = "Custom";
        }

		private async void JavaSelectButton_Click(object sender, RoutedEventArgs e)
		{
            var picker = new JavaPickerPage();
            var dialog = picker.CreateDialog(this);
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.None)
                return;

            Debug.WriteLine($"{picker.SelectedInstallation.Vendor} {picker.SelectedInstallation.Version}");
        }
    }
}
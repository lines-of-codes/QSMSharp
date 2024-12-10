using QSM.Core.ServerSoftware;

namespace QSM.Core.ModPluginSource;

public abstract class ModPluginProvider
{
    protected ServerMetadata? ServerMetadata => ServerMetadata.Selected;

	public abstract Task<ModPluginInfo[]> SearchAsync(string query = "");

    public abstract Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin);

    /// <summary>
    /// Get available versions for 
    /// </summary>
    /// <param name="slug">The project's slug</param>
    /// <returns>Returns a dictionary in a Version:DownloadUrl format</returns>
    public abstract Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug);

    public abstract Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod);

    /// <summary>
    /// Check for mod/plugin updates.
    /// This function is on-hold, will implement later. (But contributions are still welcome)
    /// </summary>
    /// <param name="modFiles">The path to the mod files.</param>
    /// <returns>The download information for available updates.</returns>
    public abstract Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles);
}

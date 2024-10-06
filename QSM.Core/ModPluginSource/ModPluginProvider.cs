using QSM.Core.ServerSoftware;

namespace QSM.Core.ModPluginSource;

public abstract class ModPluginProvider
{
    protected ServerMetadata ServerMetadata;

    protected ModPluginProvider(ServerMetadata serverMetadata)
    {
        ServerMetadata = serverMetadata;
    }

    public abstract Task<ModPluginInfo[]> SearchAsync(string query = "");

    public abstract Task<ModPluginInfo> GetDetailedInfo(ModPluginInfo modPlugin);

    /// <summary>
    /// Get available versions for 
    /// </summary>
    /// <param name="slug">The project's slug</param>
    /// <returns>Returns a dictionary in a Version:DownloadUrl format</returns>
    public abstract Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug);

    public abstract Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod);
}

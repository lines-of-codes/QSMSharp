namespace QSM.Core.ModPluginSource;

public class ModPluginDownloadInfo
{
    public class Dependency
    {
        /// <summary>
        /// A unique name/identity to the dependency.
        /// </summary>
        public string Slug = string.Empty;
        public required string Name;
        public Uri? DownloadUri;
        public string? ExternalPageUrl;
        public bool Required;
    }

    /// <summary>
    /// A boolean specifying whether or not this mod should be downloaded
    /// </summary>
    public bool Download = true;
    public string DisplayName = string.Empty;
    public string FileName = string.Empty;
    public string? DownloadUri;
    public string? ExternalPageUrl;
    public string? Hash;
    public Dependency[] Dependencies = [];

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (obj.GetType() != typeof(ModPluginDownloadInfo))
            return false;

        var info = (ModPluginDownloadInfo)obj;

        return DisplayName == info.DisplayName && FileName == info.FileName && Hash == info.Hash;
    }

    public override int GetHashCode()
    {
        return DisplayName.GetHashCode() + FileName.GetHashCode() + Hash?.GetHashCode() ?? 0;
    }
}

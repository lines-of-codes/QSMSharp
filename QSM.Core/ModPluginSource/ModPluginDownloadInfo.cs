using QSM.Core.Utilities;

namespace QSM.Core.ModPluginSource;

public class ModPluginDownloadInfo
{
	public Dependency[] Dependencies = [];
	public string VersionId = string.Empty;
	public string DisplayName = string.Empty;

	/// <summary>
	///     A boolean specifying whether or not this mod should be downloaded
	/// </summary>
	public bool Download = true;

	public string? DownloadUri;
	public string? ExternalPageUrl;
	public string FileName = string.Empty;
	public string? Hash;
	public HashAlgorithm HashAlgorithm = HashAlgorithm.None;

	public override bool Equals(object? obj)
	{
		if (obj == null)
		{
			return false;
		}

		if (obj.GetType() != typeof(ModPluginDownloadInfo))
		{
			return false;
		}

		ModPluginDownloadInfo info = (ModPluginDownloadInfo)obj;

		return DisplayName == info.DisplayName && FileName == info.FileName && Hash == info.Hash;
	}

	public override int GetHashCode()
	{
		return DisplayName.GetHashCode() + FileName.GetHashCode() + Hash?.GetHashCode() ?? 0;
	}

	public class Dependency
	{
		public Uri? DownloadUri;
		public string? ExternalPageUrl;
		public required string Name;
		public bool Required;

		/// <summary>
		///     A unique name/identity to the dependency.
		/// </summary>
		public string Slug = string.Empty;
	}
}
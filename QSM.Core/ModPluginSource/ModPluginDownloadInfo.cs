using QSM.Core.Utilities;

namespace QSM.Core.ModPluginSource;

public class ModPluginDownloadInfo(string versionId)
{
	public class Dependency
	{
		public Uri? DownloadUri { get; init; }
		public string? ExternalPageUrl { get; init; }
		public required string Name { get; init; }
		public bool Required { get; init; }

		/// <summary>
		///     A unique name/identity to the dependency.
		/// </summary>
		public string Slug { get; init; } = string.Empty;
	}

	public Dependency[] Dependencies { get; set; } = [];
	public readonly string VersionId = versionId;
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>
	///     A boolean specifying whether this mod should be downloaded
	/// </summary>
	public bool Download { get; set; } = true;

	public string? DownloadUri { get; set; }
	public string? ExternalPageUrl { get; set; }
	public string FileName { get; set; } = string.Empty;
	public string? Hash { get; set; }
	public HashAlgorithm HashAlgorithm { get; set; } = HashAlgorithm.None;

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

	public override int GetHashCode() => VersionId.GetHashCode();
}
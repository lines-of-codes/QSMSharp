using System.Text.Json.Serialization;

namespace QSM.Core.ModPluginSource.Modrinth;

public class MrpackFile
{
	[JsonPropertyName("path")] public string Path { get; set; } = string.Empty;

	/// <summary>
	///     A dictionary with hashes. Contains the required keys
	///     of "sha1" and "sha512".
	/// </summary>
	[JsonPropertyName("hashes")]
	public Dictionary<string, string> Hashes { get; set; } = [];

	/// <summary>
	///     A dictionary containing a "client" and "server" key
	///     with the possible values being "required", "optional", and "unsupported"
	/// </summary>
	[JsonPropertyName("env")]
	public Dictionary<string, string> Env { get; set; } = [];

	[JsonPropertyName("downloads")] public List<string> Downloads { get; set; } = [];

	/// <summary>
	///     File size (in bytes)
	/// </summary>
	[JsonPropertyName("fileSize")]
	public long FileSize { get; set; }
}
using Serilog;
using System.Text.Json;

namespace QSM.Core.ServerSettings;

/// <summary>
/// Contains a more in-depth information of a server.
/// </summary>
public partial class ServerSettings
{
	public List<BackupItem> Backups { get; set; } = [];

	public JavaSettings Java { get; set; } = new();

	public Task SaveJson(string filePath, JsonSerializerOptions? options = null)
	{
		options ??= JsonSerializerOptions.Default;

		return File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(this, options));
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="filePath"></param>
	/// <param name="settings"></param>
	/// <returns>`true` on load success, `false` on load fail</returns>
	public static bool TryLoadJson(string filePath, out ServerSettings? settings, JsonSerializerOptions? options = null)
	{
		settings = null;

		if (!File.Exists(filePath))
			return false;

		using var stream = File.OpenRead(filePath);
		
		try
		{
			settings = JsonSerializer.Deserialize<ServerSettings>(stream, options);
		}
		catch (Exception e)
		{
			Log.Error(e, $"Failed to deserialize JSON for QSM Server Settings: {e.Message}");
		}

		return true;
	}
}

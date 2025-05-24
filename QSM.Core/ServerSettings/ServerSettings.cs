using QSM.Core.Backups;
using Serilog;
using System.Text.Json;

namespace QSM.Core.ServerSettings;

/// <summary>
/// Contains a more in-depth information of a server.
/// </summary>
public partial class ServerSettings
{
	public bool FirstRun { get; set; } = true;

	public List<BackupItem> Backups { get; set; } = [];

	public JavaSettings Java { get; set; } = new();

	public Task SaveJsonAsync(string filePath)
	{
		return File.WriteAllTextAsync(
			filePath,
			JsonSerializer.Serialize(
				this,
				ServerSettingsContext.Default.ServerSettings));
	}

	public void SaveJson(string filePath)
	{
		File.WriteAllText(
			filePath,
			JsonSerializer.Serialize(
				this,
				ServerSettingsContext.Default.ServerSettings));
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
			settings = JsonSerializer.Deserialize(stream, ServerSettingsContext.Default.ServerSettings);
		}
		catch (Exception e)
		{
			Log.Error(e, $"Failed to deserialize JSON for QSM Server Settings: {e.Message}");
		}

		return true;
	}
}

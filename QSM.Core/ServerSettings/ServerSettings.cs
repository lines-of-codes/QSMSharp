using QSM.Core.Backups;
using Serilog;
using System.Text.Json;

namespace QSM.Core.ServerSettings;

/// <summary>
///     Contains a more in-depth information of a server.
/// </summary>
public class ServerSettings
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
	/// Try and load a server settings file
	/// </summary>
	/// <param name="filePath">The path to the configuration file</param>
	/// <param name="settings">Output of the JSON loading. Will be null if failed to load.</param>
	/// <returns>`true` on load success, `false` on load fail</returns>
	public static bool TryLoadJson(string filePath, out ServerSettings? settings)
	{
		settings = null;

		if (!File.Exists(filePath))
		{
			return false;
		}

		using FileStream stream = File.OpenRead(filePath);

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
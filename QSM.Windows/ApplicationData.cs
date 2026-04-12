using QSM.Core.ServerSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace QSM.Windows;

internal static class ApplicationData
{
	public static readonly string ApplicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "QSM");
	public static readonly string DownloadFolderPath = Path.Combine(ApplicationDataPath, "Downloads");
	public static readonly string ServersFolderPath = Path.Combine(ApplicationDataPath, "Servers");
	public static readonly string BackupsFolderPath = Path.Combine(ApplicationDataPath, "Backups");
	public static readonly string JavaInstallsPath = Path.Combine(ApplicationDataPath, "Java");
	public static readonly string LogsFolderPath = Path.Combine(ApplicationDataPath, "Logs");
	public static readonly string ConfigFile = Path.Combine(ApplicationDataPath, "config.json");
	public static ApplicationConfiguration Configuration { get; set; } = new();
	public static Dictionary<Guid, ServerSettings> ServerSettings { get; set; } = [];
	public static readonly JsonSerializerOptions SerializerOptions = new()
	{
		IgnoreReadOnlyProperties = true
	};

	public static void EnsureDataFolderExists()
	{
		Directory.CreateDirectory(DownloadFolderPath);
		Directory.CreateDirectory(BackupsFolderPath);
		Directory.CreateDirectory(JavaInstallsPath);
		Directory.CreateDirectory(LogsFolderPath);
	}

	public static void LoadConfiguration()
	{
		if (!File.Exists(ConfigFile)) return;
		Configuration = JsonSerializer.Deserialize(
			File.ReadAllText(ConfigFile),
			ApplicationConfigurationContext.Default.ApplicationConfiguration);
	}

	public static void SaveConfiguration()
	{
		string jsonStr = JsonSerializer.Serialize(Configuration, ApplicationConfigurationContext.Default.ApplicationConfiguration);
		File.WriteAllText(ConfigFile, jsonStr);
	}
}

using System.Runtime.InteropServices;
using System.Text.Json;

namespace QSM.Web.Data;

public class ApplicationConfig
{
	public List<string> JavaInstalls { get; set; } = [];
	public string? DefaultJavaInstall { get; set; }
	
	private static string GetDefaultAppDataFolder()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "/var/lib/qsm-web/";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return "/usr/local/etc/qsm-web/";

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return "C:\\ProgramData\\QSMWeb\\";
		// TODO: Add default path for macOS

		return string.Empty;
	}

	public static ApplicationConfig? LoadConfig()
	{
		var path = Path.Combine(GetDefaultAppDataFolder(), "config.json");

		if (!File.Exists(path)) return null;
		
		using var stream = File.OpenRead(path);
		return (ApplicationConfig?)JsonSerializer.Deserialize(stream, typeof(ApplicationConfig), ApplicationConfigContext.Default);
	}

	public void SaveConfig()
	{
		string path = Path.Combine(GetDefaultAppDataFolder(), "config.json");

		using var stream = File.OpenWrite(path);
		JsonSerializer.Serialize(stream, this, typeof(ApplicationConfig), ApplicationConfigContext.Default);
	}
}
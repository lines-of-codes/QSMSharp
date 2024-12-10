using QSM.Core.ServerSettings;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace QSM.Core.ServerSoftware;

public partial class NeoForgeFetcher : InfoFetcher
{
    internal record class NeoForgeVersions(bool? IsSnapshot = null, string[]? Versions = null);

    string[] availableVersionsCache = [];
    Regex majorMinorVersionMatch = MajorMinorVersionMatch();
    HttpClient httpClient;

    public NeoForgeFetcher()
    {
        httpClient = new();
    }

    public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
    {
        if (minecraftVersionsCache.Length != 0) return minecraftVersionsCache;

        NeoForgeVersions? response = await httpClient.GetFromJsonAsync<NeoForgeVersions>("https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/neoforge");

        availableVersionsCache = response!.Versions!;

        List<string> supportedVersions = [];

        foreach (var version in availableVersionsCache)
        {
            var matched = majorMinorVersionMatch.Match(version).Value;
            var minecraftVersion = $"1.{matched}";

            if (!supportedVersions.Contains(minecraftVersion))
                supportedVersions.Add(minecraftVersion);
        }

        minecraftVersionsCache = supportedVersions.ToArray();
        Array.Reverse(minecraftVersionsCache);

        return minecraftVersionsCache;
    }

    public override Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
    {
        if (buildInfoCache.TryGetValue(minecraftVersion, out var buildInfo))
            return Task.FromResult(buildInfo);

        var majorMinorVersion = minecraftVersion.Substring(2);
        List<string> versions = [];

        foreach (var version in availableVersionsCache)
        {
            if (version.StartsWith(majorMinorVersion))
            {
                versions.Add(version);
            }
        }

        versions.Reverse();

        buildInfoCache[minecraftVersion] = versions.ToArray();

        return Task.FromResult(buildInfoCache[minecraftVersion]);
    }

    [GeneratedRegex("[\\d]+\\.[\\d]")]
    private static partial Regex MajorMinorVersionMatch();

    public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
    {
        return Task.FromResult($"https://maven.neoforged.net/releases/net/neoforged/neoforge/{build}/neoforge-{build}-installer.jar");
    }

	public override string FirstRunArgs => "--install-server";

    public async Task InitializeOnFirstRun(ServerMetadata metadata, ServerSettings.ServerSettings settings, DataReceivedEventHandler outputReceived)
    {
        string serverJar = Path.Combine(metadata.ServerPath, "server.jar");
        string installerJar = Path.Combine(metadata.ServerPath, "installer.jar");

        // If installer jar doesn't exist, assume server.jar is the installer jar.
		if (!File.Exists(installerJar))
            File.Move(serverJar, installerJar);

		var startInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
			FileName = Path.Combine(settings.Java.JavaHome, "bin", "javaw.exe"),
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"{settings.Java.JvmArgs} -jar \"{Path.Combine(metadata.ServerPath, "installer.jar")}\" {FirstRunArgs}",
			WorkingDirectory = metadata.ServerPath
		};

		var process = new Process
		{
			StartInfo = startInfo
		};

        process.OutputDataReceived += outputReceived;
        process.ErrorDataReceived += outputReceived;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
	}
}

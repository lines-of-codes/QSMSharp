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
}

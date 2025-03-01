using QSM.Core.ServerSoftware;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace QSM.Core.ModPluginSource;

public class PaperMCHangarProvider : ModPluginProvider
{
    internal record class PaginationInfo(
        int? Limit = null,
        int? Offset = null,
        int? Count = null);

    internal record class HangarNamespace(
        string? Owner = null,
        string? Slug = null);
    
    internal record class HangarStats(
        int? Views = null,
        int? Downloads = null,
        int? RecentViews = null,
        int? RecentDownloads = null,
        int? Stars = null,
        int? Watchers = null);

    internal record class HangarLicense(
        string? Name = null,
        string? Url = null,
        string? Type = null);

    internal record class HangarSettings(
        string[]? Tags = null,
        HangarLicense? License = null,
        string[]? Keywords = null,
        string? Sponsors = null);

    internal record class HangarProject(
        DateTime? CreatedAt = null,
        string? Name = null,
        HangarNamespace? Namespace = null,
        HangarStats? Stats = null,
        string? Category = null,
        DateTime? LastUpdated = null,
        string? Visibility = null,
        string? AvatarUrl = null,
        string? Description = null,
        HangarSettings? Settings = null);

    internal record class ProjectReleaseChannel(
        string? Name = null);

    internal record class HangarFileInfo(
        string? Name = null,
        int? SizeBytes = null,
        string? Sha256Hash = null);

    internal record class HangarDownloadEntry(
        HangarFileInfo? FileInfo = null,
        string? ExternalUrl = null,
        string? DownloadUrl = null);

    internal record class Dependency(
        string? Name = null,
        bool? Required = null,
        string? ExternalUrl = null,
        string? Platform = null);

    internal record class ProjectVersionEntry(
        DateTime? CreatedAt = null,
        string? Name = null,
        ProjectReleaseChannel? Channel = null,
        Dictionary<string, HangarDownloadEntry>? Downloads = null,
        Dictionary<string, Dependency[]>? PluginDependencies = null);

    internal record class SearchRequest(
        PaginationInfo? Pagination = null,
        HangarProject[]? Result = null);

    internal record class VersionRequest(
        PaginationInfo? Pagination = null,
        ProjectVersionEntry[]? Result = null);

	public const string HttpClientName = "PaperMCHangarApi";
	public const string BaseAddress = "https://hangar.papermc.io/api/v1/";

	/// <summary>
	/// Time until the rate limit is being reset in milliseconds.
	/// </summary>
	const ushort RateLimitResetTime = 5000;
    
	readonly IHttpClientFactory _httpClientFactory;

	public PaperMCHangarProvider(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public override async Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug)
    {
        if (ServerMetadata is null) return [];

		using HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

		VersionRequest response = await client.GetFromJsonAsync<VersionRequest>(
            $"projects/{slug}/versions?platform={ServerMetadata.Software}&platformVersion={ServerMetadata.MinecraftVersion}")
            ?? throw new NetworkResourceUnavailableException();

        List<ModPluginDownloadInfo> versions = [];

        foreach (ProjectVersionEntry version in response.Result!)
        {
            string platform = ServerMetadata.Software.ToString().ToUpperInvariant();
            HangarDownloadEntry downloadEntry = version.Downloads![platform];
            _ = version.PluginDependencies!.TryGetValue(platform, out var dependencies);

            dependencies ??= [];

            IEnumerable<ModPluginDownloadInfo.Dependency> genericInfo = dependencies.Select(dependency =>
                new ModPluginDownloadInfo.Dependency()
                {
                    Name = dependency.Name!,
                    DownloadUri = null,
                    ExternalPageUrl = dependency.ExternalUrl,
                    Required = (bool)dependency.Required!
                }
            );

            versions.Add(new()
            {
                DisplayName = $"{version.Name!} ({version.Channel!.Name})",
                FileName = downloadEntry.FileInfo!.Name!,
                DownloadUri = downloadEntry.DownloadUrl,
                ExternalPageUrl = downloadEntry.ExternalUrl,
                Dependencies = genericInfo.ToArray(),
                Hash = downloadEntry.FileInfo.Sha256Hash
            });
        }

		return [.. versions];
    }

    public override async Task<ModPluginInfo[]> SearchAsync(string query = "")
    {
        string route = "projects";

        if (ServerMetadata is not null)
        {
            route += $"?platform={ServerMetadata.Software}&version={ServerMetadata.MinecraftVersion}";
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            route += route.Contains('?') ? '&' : '?';
            route += $"query={WebUtility.UrlEncode(query)}";
		}

        using HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

		SearchRequest response = await client.GetFromJsonAsync<SearchRequest>(route) 
            ?? throw new NetworkResourceUnavailableException();

        List<ModPluginInfo> plugins = [];

        foreach (var project in response.Result!)
        {
            plugins.Add(new()
            {
                Name = project.Name!,
                IconUrl = project.AvatarUrl!,
                License = project.Settings!.License!.Name!,
                LicenseUrl = project.Settings.License.Url!,
                Owner = project.Namespace!.Owner!,
                Slug = project.Namespace.Slug!,
                DownloadCount = (uint)project.Stats!.Downloads!,
                Description = project.Description!
            });
        }

		return [.. plugins];
    }

    public override async Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod)
    {
        var resolvedDependencies = await Task.WhenAll(mod.Dependencies.Select(async dependency =>
        {
            Uri? downloadUri = null;

            if (dependency.ExternalPageUrl == null)
            {
                downloadUri = new Uri((await GetVersionsAsync(dependency.Name!)).First().DownloadUri!);
            }

            return new ModPluginDownloadInfo.Dependency()
            {
                Name = dependency.Name!,
                DownloadUri = downloadUri,
                ExternalPageUrl = dependency.ExternalPageUrl,
                Required = dependency.Required!
            };
        }));

        mod.Dependencies = [.. resolvedDependencies];

        return mod;
    }

    public override async Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin)
    {
		using HttpClient client = _httpClientFactory.CreateClient(HttpClientName);
		modPlugin.LongDescription = await client.GetStringAsync($"https://hangar.papermc.io/api/v1/pages/main/{modPlugin.Slug}");

        return modPlugin;
    }

    async Task<HangarProject?> GetProjectFromHash(string hash)
    {
		using HttpClient client = _httpClientFactory.CreateClient(HttpClientName);

		try
		{
			HangarProject? project = await client.GetFromJsonAsync<HangarProject>($"versions/hash/{hash}");
            return project;
		}
		catch (HttpRequestException ex)
		{
			if (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
			{
				await Task.Delay(RateLimitResetTime);
                return await GetProjectFromHash(hash);
			}

			throw;
		}
	}

	public override async Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles)
	{
        List<ModPluginDownloadInfo> updates = [];

		foreach (var fileName in modFiles)
		{
			using var file = File.OpenRead(fileName);
			using var hasher = SHA256.Create();
			var hashed = hasher.ComputeHash(file);
			StringBuilder sb = new();

			foreach (byte b in hashed)
				sb.Append(b.ToString("x2"));

            await GetProjectFromHash(sb.ToString());
		}

		return [.. updates];
	}
}

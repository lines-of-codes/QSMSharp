using JetBrains.Annotations;
using QSM.Core.ServerSoftware;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using HashAlgorithm = QSM.Core.Utilities.HashAlgorithm;

namespace QSM.Core.ModPluginSource;

[PublicAPI]
public partial class PaperMCHangarProvider(IHttpClientFactory httpClientFactory) : ModPluginProvider
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
	[JsonSerializable(typeof(VersionRequest))]
	[JsonSerializable(typeof(SearchRequest))]
	[JsonSerializable(typeof(HangarProject))]
	internal sealed partial class HangarContext : JsonSerializerContext { }

	public const string HttpClientName = "PaperMCHangarApi";
	public const string BaseAddress = "https://hangar.papermc.io/api/v1/";

	/// <summary>
	///     Time until the rate limit is being reset in milliseconds.
	/// </summary>
	private const ushort RateLimitResetTime = 5000;

	public override async Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug, ServerMetadata? serverMetadata = null)
	{
		if (serverMetadata is null)
		{
			return [];
		}

		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

		VersionRequest response = await client.GetFromJsonAsync(
									  $"projects/{slug}/versions?platform={serverMetadata.Software}&platformVersion={serverMetadata.MinecraftVersion}",
									  HangarContext.Default.VersionRequest)
								  ?? throw new NetworkResourceUnavailableException();

		List<ModPluginDownloadInfo> versions = [];

		foreach (ProjectVersionEntry version in response.Result)
		{
			string platform = serverMetadata.Software.ToString().ToUpperInvariant();
			HangarDownloadEntry downloadEntry = version.Downloads![platform];
			_ = version.PluginDependencies!.TryGetValue(platform, out Dependency[]? dependencies);

			dependencies ??= [];

			if (downloadEntry.FileInfo == null)
				continue;

			IEnumerable<ModPluginDownloadInfo.Dependency> genericInfo = dependencies.Select(dependency =>
				new ModPluginDownloadInfo.Dependency
				{
					Name = dependency.Name!,
					DownloadUri = null,
					ExternalPageUrl = dependency.ExternalUrl,
					Required = (bool)dependency.Required!
				}
			);

			versions.Add(new ModPluginDownloadInfo(version.Id.ToString())
			{
				DisplayName = $"{version.Name!} ({version.Channel!.Name})",
				FileName = downloadEntry.FileInfo.Name!,
				DownloadUri = downloadEntry.DownloadUrl,
				ExternalPageUrl = downloadEntry.ExternalUrl,
				Dependencies = [.. genericInfo],
				Hash = downloadEntry.FileInfo.Sha256Hash,
				HashAlgorithm = HashAlgorithm.Sha256
			});
		}

		return [.. versions];
	}

	public override async Task<ModPluginInfo[]> SearchAsync(string query = "", ServerMetadata? serverMetadata = null)
	{
		string route = "projects";

		if (serverMetadata is not null)
		{
			route += $"?platform={serverMetadata.Software}&version={serverMetadata.MinecraftVersion}";
		}

		if (!string.IsNullOrWhiteSpace(query))
		{
			route += route.Contains('?') ? '&' : '?';
			route += $"query={WebUtility.UrlEncode(query)}";
		}

		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

		SearchRequest response = await client.GetFromJsonAsync(route, HangarContext.Default.SearchRequest)
								 ?? throw new NetworkResourceUnavailableException();

		List<ModPluginInfo> plugins = [];

		foreach (HangarProject project in response.Result)
		{
			plugins.Add(new ModPluginInfo
			{
				Name = project.Name,
				IconUrl = project.AvatarUrl,
				License = project.Settings.License.Name ?? project.Settings.License.Type ?? string.Empty,
				LicenseUrl = project.Settings.License.Url!,
				Owner = project.Namespace.Owner!,
				Slug = project.Namespace.Slug!,
				Id = project.Namespace.Slug,
				DownloadCount = (uint)project.Stats.Downloads!,
				Description = project.Description,
				Url = $"https://hangar.papermc.io/{project.Namespace.Owner}/{project.Namespace.Slug}"
			});
		}

		return [.. plugins];
	}

	public override async Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod)
	{
		ModPluginDownloadInfo.Dependency[] resolvedDependencies = await Task.WhenAll(
			mod.Dependencies.Select(async dependency =>
			{
				Uri? downloadUri = null;

				if (dependency.ExternalPageUrl == null)
				{
					downloadUri = new Uri((await GetVersionsAsync(dependency.Name))[0].DownloadUri!);
				}

				return new ModPluginDownloadInfo.Dependency
				{
					Name = dependency.Name,
					DownloadUri = downloadUri,
					ExternalPageUrl = dependency.ExternalPageUrl,
					Required = dependency.Required
				};
			}));

		mod.Dependencies = [.. resolvedDependencies];

		return mod;
	}

	public override async Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		modPlugin.LongDescription =
			await client.GetStringAsync($"https://hangar.papermc.io/api/v1/pages/main/{modPlugin.Slug}");

		return modPlugin;
	}

	private async Task<HangarProject?> GetProjectFromHash(string hash)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

		try
		{
			HangarProject? project = await client.GetFromJsonAsync($"versions/hash/{hash}", HangarContext.Default.HangarProject);
			return project;
		}
		catch (HttpRequestException ex)
		{
			if (ex.StatusCode == HttpStatusCode.TooManyRequests)
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

		foreach (string fileName in modFiles)
		{
			await using FileStream file = File.OpenRead(fileName);
			using SHA256 hasher = SHA256.Create();
			byte[] hashed = await hasher.ComputeHashAsync(file);
			StringBuilder sb = new();

			foreach (byte b in hashed)
			{
				sb.Append(b.ToString("x2"));
			}

			await GetProjectFromHash(sb.ToString());
		}

		return [.. updates];
	}

	internal sealed record PaginationInfo(
		int? Limit = null,
		int? Offset = null,
		int? Count = null);

	internal sealed record HangarNamespace(
		string? Owner = null,
		string? Slug = null);

	internal sealed record HangarStats(
		int? Views = null,
		int? Downloads = null,
		int? RecentViews = null,
		int? RecentDownloads = null,
		int? Stars = null,
		int? Watchers = null);

	internal sealed record HangarLicense(
		string? Name = null,
		string? Url = null,
		string? Type = null);

	internal sealed record HangarSettings(
		string[] Tags,
		HangarLicense License,
		string[] Keywords,
		string Sponsors);

	internal sealed record HangarProject(
		DateTime CreatedAt,
		string Name,
		HangarNamespace Namespace,
		HangarStats Stats,
		string Category,
		DateTime LastUpdated,
		string Visibility,
		string AvatarUrl,
		string Description,
		HangarSettings Settings);

	internal sealed record ProjectReleaseChannel(
		string? Name = null);

	internal sealed record HangarFileInfo(
		string? Name = null,
		int? SizeBytes = null,
		string? Sha256Hash = null);

	internal sealed record HangarDownloadEntry(
		HangarFileInfo? FileInfo = null,
		string? ExternalUrl = null,
		string? DownloadUrl = null);

	internal sealed record Dependency(
		string? Name = null,
		bool? Required = null,
		string? ExternalUrl = null,
		string? Platform = null);

	internal sealed record ProjectVersionEntry(
		int Id,
		DateTime? CreatedAt = null,
		string? Name = null,
		ProjectReleaseChannel? Channel = null,
		Dictionary<string, HangarDownloadEntry>? Downloads = null,
		Dictionary<string, Dependency[]>? PluginDependencies = null);

	internal sealed record SearchRequest(
		PaginationInfo Pagination,
		HangarProject[] Result);

	internal sealed record VersionRequest(
		PaginationInfo Pagination,
		ProjectVersionEntry[] Result);
}
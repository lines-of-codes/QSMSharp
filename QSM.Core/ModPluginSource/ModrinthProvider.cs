using QSM.Core.ModPluginSource.Modrinth;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using HashAlgorithm = QSM.Core.Utilities.HashAlgorithm;

namespace QSM.Core.ModPluginSource;

public class ModrinthProvider(IHttpClientFactory httpClientFactory) : ModPluginProvider
{
	public enum ProjectType
	{
		Mod,
		Plugin,
		Modpack
	}

	public const string HttpClientName = "ModrinthApi";
	public const string BaseAddress = "https://api.modrinth.com/v2/";
	private static readonly string[] s_ignoredDependencyType = ["embedded", "incompatible"];

	public override async Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

		string queryString = $"project/{slug}/version?include_changelog=false";

		if (ServerMetadata != null)
		{
			queryString += "&loaders=";
			queryString += WebUtility.UrlEncode($"[\"{ServerMetadata.Software.ToString().ToLowerInvariant()}\"]");
			queryString += "&game_versions=";
			queryString += WebUtility.UrlEncode($"[\"{ServerMetadata.MinecraftVersion}\"]");
		}

		VersionInfo[] response = await client.GetFromJsonAsync<VersionInfo[]>(queryString)
		                         ?? throw new NetworkResourceUnavailableException();

		List<ModPluginDownloadInfo> versions = [];

		foreach (VersionInfo info in response)
		{
			IEnumerable<ModPluginDownloadInfo.Dependency> dependencies = info.dependencies!.Select(dependency =>
				new ModPluginDownloadInfo.Dependency
				{
					Slug = dependency.version_id ?? string.Empty,
					Name = dependency.file_name ?? string.Empty,
					DownloadUri = null,
					ExternalPageUrl = dependency.dependency_type,
					Required = dependency.dependency_type == "required"
				});

			VersionFile primaryFile = info.files!.FirstOrDefault(file => (bool)file.primary!, info.files![0]);

			versions.Add(new ModPluginDownloadInfo
			{
				VersionId = info.id ?? string.Empty,
				DisplayName = $"{info.name} ({info.version_type})",
				FileName = primaryFile.filename!,
				Dependencies = dependencies.ToArray(),
				DownloadUri = primaryFile.url,
				ExternalPageUrl = null,
				Hash = primaryFile.hashes!.sha512,
				HashAlgorithm = HashAlgorithm.SHA512
			});
		}

		return versions.ToArray();
	}

	public async Task<ModPluginDownloadInfo> GetVersionAsync(string id)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		VersionInfo version = await client.GetFromJsonAsync<VersionInfo>("version/" + id)
		                       ?? throw new NetworkResourceUnavailableException();
		
		IEnumerable<ModPluginDownloadInfo.Dependency> dependencies = version.dependencies!.Select(dependency =>
			new ModPluginDownloadInfo.Dependency
			{
				Slug = dependency.version_id ?? string.Empty,
				Name = dependency.file_name ?? string.Empty,
				DownloadUri = null,
				ExternalPageUrl = dependency.dependency_type,
				Required = dependency.dependency_type == "required"
			});

		
		VersionFile primaryFile = version.files!.FirstOrDefault(file => (bool)file.primary!, version.files![0]);

		return new ModPluginDownloadInfo
		{
			VersionId = version.id ?? string.Empty,
			DisplayName = $"{version.name} ({version.version_type})",
			FileName = primaryFile.filename!,
			Dependencies = dependencies.ToArray(),
			DownloadUri = primaryFile.url,
			ExternalPageUrl = null,
			Hash = primaryFile.hashes!.sha512,
			HashAlgorithm = HashAlgorithm.SHA512
		};
	}
	
	public override Task<ModPluginInfo[]> SearchAsync(string query = "")
	{
		return SearchAsync(query);
	}

	public async Task<ModPluginInfo[]> SearchAsync(string query = "", ProjectType projectType = ProjectType.Mod,
		IEnumerable<string>? categories = null)
	{
		string queryString = "search";

		List<List<string>> facets = [];

		List<string> projectTypes = [];
		if (ServerMetadata?.IsModSupported ?? projectType == ProjectType.Mod)
		{
			projectTypes.Add("project_type:mod");
		}

		if (ServerMetadata?.IsPluginSupported ?? projectType == ProjectType.Plugin)
		{
			projectTypes.Add("project_type:plugin");
		}

		if (projectType == ProjectType.Modpack)
		{
			projectTypes.Add("project_type:modpack");
		}

		facets.Add(projectTypes);

		if (ServerMetadata != null)
		{
			facets.Add([$"versions:{ServerMetadata.MinecraftVersion}"]);
			facets.Add([$"categories:{ServerMetadata.Software.ToString().ToLowerInvariant()}"]);
		}

		categories ??= [];

		foreach (string category in categories)
		{
			facets.Add([$"categories:{category}"]);
		}

		facets.Add(["server_side!=unsupported"]);

		StringBuilder sb = new();

		sb.Append('[');
		foreach (List<string> facet in facets)
		{
			sb.Append('[');
			foreach (string entry in facet)
			{
				sb.Append($"\"{entry}\"");
			}

			sb.Append("],");
		}

		sb.Remove(sb.Length - 1, 1); // Remove the last trailing comma
		sb.Append(']');

		queryString += "?facets=";
		queryString += WebUtility.UrlEncode(sb.ToString());

		if (!string.IsNullOrWhiteSpace(query))
		{
			queryString += "&query=";
			queryString += WebUtility.UrlEncode(query);
		}

		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		SearchRequest response = await client.GetFromJsonAsync<SearchRequest>(queryString)
		                         ?? throw new NetworkResourceUnavailableException();

		List<ModPluginInfo> modPlugins = [];

		foreach (ProjectResult project in response.Hits!)
		{
			modPlugins.Add(new ModPluginInfo
			{
				IconUrl = project.icon_url,
				License = project.license!,
				Name = project.title!,
				Owner = project.author!,
				Slug = project.slug!,
				DownloadCount = (uint)project.downloads!,
				LicenseUrl =
					project.license!.StartsWith("LicenseRef")
						? string.Empty
						: $"https://spdx.org/licenses/{project.license}",
				Description = project.description!
			});
		}

		return modPlugins.ToArray();
	}

	public override async Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod)
	{
		ModPluginDownloadInfo.Dependency[] resolvedDependencies = await Task.WhenAll(
			mod.Dependencies.Select(async dependency =>
			{
				Uri? downloadUri = null;

				if (!s_ignoredDependencyType.Contains(dependency.ExternalPageUrl))
				{
					using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
					VersionInfo response = await client.GetFromJsonAsync<VersionInfo>($"version/{dependency.Slug}")
					                       ?? throw new NetworkResourceUnavailableException();

					downloadUri = new Uri(response.files!.First(file => (bool)file.primary!).url!);
				}

				return new ModPluginDownloadInfo.Dependency
				{
					Slug = dependency.Slug,
					Name = dependency.Name,
					DownloadUri = downloadUri,
					ExternalPageUrl = null,
					Required = dependency.Required
				};
			}));

		mod.Dependencies = resolvedDependencies;

		return mod;
	}

	public override async Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		DetailedProjectResult? project =
			await client.GetFromJsonAsync<DetailedProjectResult>($"project/{modPlugin.Slug}");

		if (string.IsNullOrEmpty(modPlugin.LicenseUrl))
		{
			modPlugin.LicenseUrl = project!.license!.url!;
		}

		modPlugin.LongDescription = project!.body!;

		return modPlugin;
	}

	// TODO: Finish this thing
	public override Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles)
	{
		List<string> hashes = [];

		foreach (string fileName in modFiles)
		{
			using FileStream file = File.OpenRead(fileName);
			using SHA512 hasher = SHA512.Create();
			byte[] hashed = hasher.ComputeHash(file);
			StringBuilder sb = new();

			foreach (byte b in hashed)
			{
				sb.Append(b.ToString("x2"));
			}

			hashes.Add(sb.ToString());
		}

		return Task.FromResult(Array.Empty<ModPluginDownloadInfo>());
	}

	public async Task<Category[]> ListCategories()
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		return (await client.GetFromJsonAsync<Category[]>("tag/category"))!;
	}

	// ReSharper disable InconsistentNaming
	internal record VersionDependency(
		string? version_id = null,
		string? project_id = null,
		string? file_name = null,
		string? dependency_type = null);

	internal record VersionFileHashes(
		string? sha512 = null,
		string? sha1 = null);

	internal record VersionFile(
		VersionFileHashes? hashes = null,
		string? url = null,
		string? filename = null,
		bool? primary = null,
		int? size = null,
		string? file_type = null);

	internal record VersionInfo(
		string? id = null,
		string? name = null,
		string? version_number = null,
		string? changelog = null,
		VersionDependency[]? dependencies = null,
		string? version_type = null,
		bool? featured = null,
		VersionFile[]? files = null);

	internal record ProjectResult(
		string? slug = null,
		string? title = null,
		string? description = null,
		string[]? categories = null,
		string? client_side = null,
		string? server_side = null,
		string? project_type = null,
		int? downloads = null,
		string? icon_url = null,
		string? project_id = null,
		string? author = null,
		string[]? display_categories = null,
		string[]? versions = null,
		int? follows = null,
		DateTime? date_created = null,
		DateTime? date_modified = null,
		string? latest_version = null,
		string? license = null,
		string[]? gallery = null,
		string? featured_gallery = null);

	internal record LicenseDetails(
		string? id = null,
		string? name = null,
		string? url = null);

	internal record GalleryImage(
		string? url = null,
		bool? featured = null,
		string? title = null,
		string? description = null,
		DateTime? created = null,
		int? ordering = null);

	internal record DetailedProjectResult(
		string? slug = null,
		string? title = null,
		string? description = null,
		string[]? categories = null,
		string? client_side = null,
		string? server_side = null,
		string? body = null,
		string? project_type = null,
		int? downloads = null,
		string? icon_url = null,
		string? project_id = null,
		string? author = null,
		string[]? display_categories = null,
		string[]? versions = null,
		int? follows = null,
		DateTime? date_created = null,
		DateTime? date_modified = null,
		string? latest_version = null,
		LicenseDetails? license = null,
		GalleryImage[]? gallery = null,
		string? featured_gallery = null);

	internal record SearchRequest(
		ProjectResult[]? hits = null,
		int? offset = null,
		int? limit = null,
		int? total_hits = null)
	{
		public ProjectResult[]? Hits = hits;
		public int? Limit = limit;
		public int? Offset = offset;
		public int? TotalHits = total_hits;
	}
	// ReSharper restore InconsistentNaming
}
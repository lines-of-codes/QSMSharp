using QSM.Core.ModPluginSource.Modrinth;
using QSM.Core.ServerSoftware;
using QSM.Core.Utilities;
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

	public override async Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug, ServerMetadata? serverMetadata = null)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

		string queryString = $"project/{slug}/version?include_changelog=false";

		if (serverMetadata != null)
		{
			queryString += "&loaders=";
			queryString += WebUtility.UrlEncode($"[\"{serverMetadata.Software.ToString().ToLowerInvariant()}\"]");
			queryString += "&game_versions=";
			queryString += WebUtility.UrlEncode($"[\"{serverMetadata.MinecraftVersion}\"]");
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

			VersionFile primaryFile = info.files.FirstOrDefault(file => file.primary, info.files[0]);

			versions.Add(new ModPluginDownloadInfo(info.id)
			{
				DisplayName = $"{info.name} ({info.version_type})",
				FileName = primaryFile.filename,
				Dependencies = [.. dependencies],
				DownloadUri = primaryFile.url,
				ExternalPageUrl = null,
				Hash = primaryFile.hashes.sha512,
				HashAlgorithm = HashAlgorithm.Sha512,
				Size = primaryFile.size
			});
		}

		return [.. versions];
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

		VersionFile primaryFile = version.files.FirstOrDefault(file => file.primary, version.files[0]);

		return new ModPluginDownloadInfo(version.id)
		{
			DisplayName = $"{version.name} ({version.version_type})",
			FileName = primaryFile.filename,
			Dependencies = [.. dependencies],
			DownloadUri = primaryFile.url,
			ExternalPageUrl = null,
			Hash = primaryFile.hashes.sha512,
			HashAlgorithm = HashAlgorithm.Sha512,
			Size = primaryFile.size
		};
	}

	public override Task<ModPluginInfo[]> SearchAsync(string query = "", ServerMetadata? serverMetadata = null)
	{
		return SearchAsync(serverMetadata, query);
	}

	public async Task<ModPluginInfo[]> SearchAsync(ServerMetadata? serverMetadata = null, string query = "",
		ProjectType projectType = ProjectType.Mod, IEnumerable<string>? categories = null)
	{
		string queryString = "search";

		List<List<string>> facets = [];

		List<string> projectTypes = [];
		if (serverMetadata?.IsModSupported ?? projectType == ProjectType.Mod)
		{
			projectTypes.Add("project_type:mod");
		}

		if (serverMetadata?.IsPluginSupported ?? projectType == ProjectType.Plugin)
		{
			projectTypes.Add("project_type:plugin");
		}

		if (projectType == ProjectType.Modpack)
		{
			projectTypes.Add("project_type:modpack");
		}

		facets.Add(projectTypes);


		if (serverMetadata != null)
		{
			facets.Add([$"versions:{serverMetadata.MinecraftVersion}"]);
			facets.Add([$"categories:{serverMetadata.Software.ToString().ToLowerInvariant()}"]);
		}

		categories ??= [];

		facets.AddRange(categories.Select(category => (List<string>)[$"categories:{category}"]));

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

		return
		[
			.. response.Hits!.Select(project => new ModPluginInfo
			{
				IconUrl = project.icon_url,
				License = project.license,
				Name = project.title!,
				Owner = project.author,
				Slug = project.slug!,
				Id = project.project_id,
				DownloadCount = (uint)project.downloads,
				LicenseUrl = project.license.StartsWith("LicenseRef")
					? string.Empty
					: $"https://spdx.org/licenses/{project.license}",
				Description = project.description!
			})
		];
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

					downloadUri = new Uri(response.files.First(file => file.primary).url);
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

	public override async Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles)
	{
		using SHA512 hasher = SHA512.Create();
		List<string> hashes = [.. modFiles.Select(hasher.GetFileHashAsString)];
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		HttpResponseMessage response = await client.PostAsJsonAsync("version_files/update", hashes);
		Dictionary<string, VersionInfo>? updates = await response.Content.ReadFromJsonAsync<Dictionary<string, VersionInfo>>();

		if (updates is null) return [];

		return
		[
			.. updates.Values.Select(version =>
			{
				VersionFile primaryFile = version.files.FirstOrDefault(file => file.primary, version.files[0]);
				
				return new ModPluginDownloadInfo(version.id)
				{
					DownloadUri = primaryFile.url,
					DisplayName = version.name ?? string.Empty,
					FileName = primaryFile.filename,
					Hash = primaryFile.hashes.sha512,
					HashAlgorithm = HashAlgorithm.Sha512,
					Size = primaryFile.size
				};
			})
		];
	}

	public async Task<Category[]> ListCategories()
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		return (await client.GetFromJsonAsync<Category[]>("tag/category"))!;
	}

	// ReSharper disable InconsistentNaming
	// ReSharper disable ClassNeverInstantiated.Global
	internal record VersionDependency(
		string? version_id = null,
		string? project_id = null,
		string? file_name = null,
		string? dependency_type = null);

	internal record VersionFileHashes(
		string? sha512 = null,
		string? sha1 = null);

	internal record VersionFile(
		VersionFileHashes hashes,
		string url,
		string filename,
		bool primary,
		long size,
		string? file_type = null);

	internal record ProjectResult(
		string project_id,
		string project_type,
		int downloads,
		string author,
		int follows,
		DateTime date_created,
		DateTime date_modified,
		string license,
		string? slug = null,
		string? title = null,
		string? description = null,
		string[]? categories = null,
		string? icon_url = null,
		string[]? display_categories = null,
		string[]? versions = null,
		string? latest_version = null,
		string[]? gallery = null,
		string? featured_gallery = null);

	internal record VersionInfo(
		string id = "",
		string? name = null,
		string? version_number = null,
		string? changelog = null,
		VersionDependency[]? dependencies = null,
		string? version_type = null,
		bool? featured = null,
		VersionFile[] files = null!);

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
	// ReSharper restore ClassNeverInstantiated.Global
	// ReSharper restore InconsistentNaming
}
using QSM.Core.ModPluginSource.Modrinth;
using QSM.Core.ServerSoftware;
using QSM.Core.Utilities;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using HashAlgorithm = QSM.Core.Utilities.HashAlgorithm;

namespace QSM.Core.ModPluginSource;

public partial class ModrinthProvider(IHttpClientFactory httpClientFactory) : ModPluginProvider
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
	[JsonSerializable(typeof(VersionInfo[]))]
	[JsonSerializable(typeof(SearchRequest))]
	[JsonSerializable(typeof(DetailedProjectResult))]
	[JsonSerializable(typeof(List<string>))]
	[JsonSerializable(typeof(Dictionary<string, VersionInfo>))]
	[JsonSerializable(typeof(Category[]))]
	private sealed partial class ModrinthContext : JsonSerializerContext { }

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

		VersionInfo[] response = await client.GetFromJsonAsync(queryString, ModrinthContext.Default.VersionInfoArray)
								 ?? throw new NetworkResourceUnavailableException();

		List<ModPluginDownloadInfo> versions = [];

		foreach (VersionInfo info in response)
		{
			IEnumerable<ModPluginDownloadInfo.Dependency> dependencies = info.Dependencies!.Select(dependency =>
				new ModPluginDownloadInfo.Dependency
				{
					Slug = dependency.VersionId ?? string.Empty,
					Name = dependency.FileName ?? string.Empty,
					DownloadUri = null,
					ExternalPageUrl = dependency.DependencyType,
					Required = dependency.DependencyType == "required"
				});

			VersionFile primaryFile = info.Files.FirstOrDefault(file => file.Primary, info.Files[0]);

			versions.Add(new ModPluginDownloadInfo(info.Id)
			{
				DisplayName = $"{info.Name} ({info.VersionType})",
				FileName = primaryFile.Filename,
				Dependencies = [.. dependencies],
				DownloadUri = primaryFile.Url,
				ExternalPageUrl = null,
				Hash = primaryFile.Hashes.SHA512,
				HashAlgorithm = HashAlgorithm.Sha512,
				Size = primaryFile.Size
			});
		}

		return [.. versions];
	}

	public async Task<ModPluginDownloadInfo> GetVersionAsync(string id)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		VersionInfo version = await client.GetFromJsonAsync("version/" + id, ModrinthContext.Default.VersionInfo)
							   ?? throw new NetworkResourceUnavailableException();

		IEnumerable<ModPluginDownloadInfo.Dependency> dependencies = version.Dependencies!.Select(dependency =>
			new ModPluginDownloadInfo.Dependency
			{
				Slug = dependency.VersionId ?? string.Empty,
				Name = dependency.FileName ?? string.Empty,
				DownloadUri = null,
				ExternalPageUrl = dependency.DependencyType,
				Required = dependency.DependencyType == "required"
			});

		VersionFile primaryFile = version.Files.FirstOrDefault(file => file.Primary, version.Files[0]);

		return new ModPluginDownloadInfo(version.Id)
		{
			DisplayName = $"{version.Name} ({version.VersionType})",
			FileName = primaryFile.Filename,
			Dependencies = [.. dependencies],
			DownloadUri = primaryFile.Url,
			ExternalPageUrl = null,
			Hash = primaryFile.Hashes.SHA512,
			HashAlgorithm = HashAlgorithm.Sha512,
			Size = primaryFile.Size
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
		SearchRequest response = await client.GetFromJsonAsync(queryString, ModrinthContext.Default.SearchRequest)
								 ?? throw new NetworkResourceUnavailableException();

		return
		[
			.. response.Hits!.Select(project => new ModPluginInfo
			{
				IconUrl = project.IconUrl,
				License = project.License,
				Name = project.Title!,
				Owner = project.Author,
				Slug = project.Slug!,
				Id = project.ProjectId,
				DownloadCount = (uint)project.Downloads,
				LicenseUrl = project.License.StartsWith("LicenseRef")
					? string.Empty
					: $"https://spdx.org/licenses/{project.License}",
				Description = project.Description!,
				Url = $"https://modrinth.com/mod/{project.ProjectId}"
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
					VersionInfo response = await client.GetFromJsonAsync($"version/{dependency.Slug}", ModrinthContext.Default.VersionInfo)
										   ?? throw new NetworkResourceUnavailableException();

					downloadUri = new Uri(response.Files.First(file => file.Primary).Url);
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
			await client.GetFromJsonAsync($"project/{modPlugin.Slug}", ModrinthContext.Default.DetailedProjectResult);

		if (string.IsNullOrEmpty(modPlugin.LicenseUrl))
		{
			modPlugin.LicenseUrl = project!.License!.Url!;
		}

		modPlugin.LongDescription = project!.Body!;

		return modPlugin;
	}

	public override async Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles)
	{
		using SHA512 hasher = SHA512.Create();
		List<string> hashes = [.. modFiles.Select(hasher.GetFileHashAsString)];
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		HttpResponseMessage response = await client.PostAsJsonAsync("version_files/update", hashes, ModrinthContext.Default.ListString);
		Dictionary<string, VersionInfo>? updates = await response.Content.ReadFromJsonAsync(ModrinthContext.Default.DictionaryStringVersionInfo);

		if (updates is null) return [];

		return
		[
			.. updates.Values.Select(version =>
			{
				VersionFile primaryFile = version.Files.FirstOrDefault(file => file.Primary, version.Files[0]);

				return new ModPluginDownloadInfo(version.Id)
				{
					DownloadUri = primaryFile.Url,
					DisplayName = version.Name ?? string.Empty,
					FileName = primaryFile.Filename,
					Hash = primaryFile.Hashes.SHA512,
					HashAlgorithm = HashAlgorithm.Sha512,
					Size = primaryFile.Size
				};
			})
		];
	}

	public async Task<Category[]> ListCategories()
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		return await client.GetFromJsonAsync("tag/category", ModrinthContext.Default.CategoryArray) ?? [];
	}

	// ReSharper disable ClassNeverInstantiated.Global
	internal record VersionDependency(
		string? VersionId = null,
		string? ProjectId = null,
		string? FileName = null,
		string? DependencyType = null);

	internal record VersionFileHashes(
		string? SHA512 = null,
		string? SHA1 = null);

	internal record VersionFile(
		VersionFileHashes Hashes,
		string Url,
		string Filename,
		bool Primary,
		long Size,
		string? FileType = null);

	internal record ProjectResult(
		string ProjectId,
		string ProjectType,
		int Downloads,
		string Author,
		int Follows,
		DateTime DateCreated,
		DateTime DateModified,
		string License,
		string? Slug = null,
		string? Title = null,
		string? Description = null,
		string[]? Categories = null,
		string? IconUrl = null,
		string[]? DisplayCategories = null,
		string[]? Versions = null,
		string? LatestVersion = null,
		string[]? Gallery = null,
		string? FeaturedGallery = null);

	internal record VersionInfo(
		string Id = "",
		string? Name = null,
		string? VersionNumber = null,
		string? Changelog = null,
		VersionDependency[]? Dependencies = null,
		string? VersionType = null,
		bool? Featured = null,
		VersionFile[] Files = null!);

	internal record LicenseDetails(
		string? Id = null,
		string? Name = null,
		string? Url = null);

	internal record GalleryImage(
		string? Url = null,
		bool? Featured = null,
		string? Title = null,
		string? Description = null,
		DateTime? Created = null,
		int? Ordering = null);

	internal record DetailedProjectResult(
		string? Slug = null,
		string? Title = null,
		string? Description = null,
		string[]? Categories = null,
		string? ClientSide = null,
		string? ServerSide = null,
		string? Body = null,
		string? ProjectType = null,
		int? Downloads = null,
		string? IconUrl = null,
		string? ProjectId = null,
		string? Author = null,
		string[]? DisplayCategories = null,
		string[]? Versions = null,
		int? Follows = null,
		DateTime? DateCreated = null,
		DateTime? DateModified = null,
		string? LatestVersion = null,
		LicenseDetails? License = null,
		GalleryImage[]? Gallery = null,
		string? FeaturedGallery = null);

	internal record SearchRequest(
		ProjectResult[]? Hits = null,
		int? Offset = null,
		int? Limit = null,
		int? TotalHits = null);
	// ReSharper restore ClassNeverInstantiated.Global
}
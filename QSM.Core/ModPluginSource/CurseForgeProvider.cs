using JetBrains.Annotations;
using QSM.Core.ModPluginSource.CurseForge;
using QSM.Core.ServerSoftware;
using QSM.Core.Utilities;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace QSM.Core.ModPluginSource;

public class CurseForgeProvider(IHttpClientFactory httpClientFactory) : ModPluginProvider
{
	public const string HttpClientName = "CurseForgeApi";
	public const string BaseAddress = "https://api.curseforge.com/v1/";
	public const string CurseKey = "$2a$10$kz4OPSZlWNbJLaJImOgTIOwfx3bnMshplbA1F5L2WiMuL5oq63o.q";

	private const ushort MinecraftId = 432;
	public async Task<Category[]> ListCategories()
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		return (await client.GetFromJsonAsync<GetCategoriesResponse>($"categories?gameId={MinecraftId}"))!.Data;
	}

	public override Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles)
	{
		throw new NotImplementedException();
	}

	public override async Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin)
	{
		if (modPlugin.Id is null) throw new ArgumentNullException(nameof(modPlugin));
		
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		StringResponse desc = await client.GetFromJsonAsync<StringResponse>(
			                      $"mods/{(uint)modPlugin.Id}/description?raw=true")
		                      ?? throw new NetworkResourceUnavailableException();

		modPlugin.LongDescription = desc.Data;

		return modPlugin;
	}

	///  <summary>
	/// 	Fetch the list of files for a mod from CurseForge
	///  </summary>
	///  <param name="modId">String version of the Mod ID integer</param>
	///  <param name="serverMetadata"></param>
	///  <returns>List of files</returns>
	public override async Task<ModPluginDownloadInfo[]> GetVersionsAsync(string modId, ServerMetadata? serverMetadata = null)
	{
		string path = $"mods/{modId}/files";
		
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		GetModFilesResponse response = await client.GetFromJsonAsync<GetModFilesResponse>(path)
		                               ?? throw new NetworkResourceUnavailableException();

		return response.Data.Select(file =>
		{
			StringBuilder name = new(file.DisplayName);

			switch (file.ReleaseType)
			{
				case FileReleaseType.Alpha:
					name.Append(" (alpha)");
					break;
				case FileReleaseType.Beta:
					name.Append(" (beta)");
					break;
				case FileReleaseType.Release:
				default:
					break;
			}
			
			file.GameVersions.Sort();

			name.Append(" (");
			name.Append(string.Join(' ', file.GameVersions.Reverse()));
			name.Append(')');

			IEnumerable<ModPluginDownloadInfo.Dependency> dependencies =
				file.Dependencies
					.Where(dep =>
						dep.RelationType is FileRelationType.RequiredDependency
							or FileRelationType.OptionalDependency)
					.Select(dep => new ModPluginDownloadInfo.Dependency
					{
						Name = string.Empty,
						Required = dep.RelationType == FileRelationType.RequiredDependency,
						Slug = dep.ModId.ToString()
					});

			var hash = file.Hashes.FirstOrDefault(data => data.Algo == CfHashAlgorithm.Sha1)?.Value;
			
			return new ModPluginDownloadInfo
			{
				DisplayName = name.ToString(),
				DownloadUri = file.DownloadUrl,
				FileName = file.FileName,
				Hash = hash,
				HashAlgorithm = hash == null ? HashAlgorithm.None : HashAlgorithm.Sha1,
				VersionId = file.Id.ToString(),
				Dependencies = dependencies.ToArray()
			};
		}).ToArray();
	}

	public override Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod)
	{
		throw new NotImplementedException();
	}

	public override Task<ModPluginInfo[]> SearchAsync(string query = "", ServerMetadata? serverMetadata = null)
	{
		return SearchAsync(query);
	}

	public async Task<ModPluginInfo[]> SearchAsync(string searchFilter = "", uint? classId = null, IEnumerable<uint>? categories = null, IEnumerable<ModLoaderType>? modLoaderTypes = null)
	{
		StringBuilder pathString = new($"mods/search?gameId={MinecraftId}&sortOrder=1");

		if (!string.IsNullOrWhiteSpace(searchFilter))
		{
			pathString.Append("&searchFilter=");
			pathString.Append(WebUtility.UrlEncode(searchFilter));
		}

		if (classId != null)
		{
			pathString.Append("&classId=")
					  .Append(classId);
		}

		if (categories != null)
		{
			pathString.Append("&categoryIds=");

			StringBuilder catList = new("[");
			catList.Append(string.Join(',', categories));
			catList.Append(']');

			pathString.Append(WebUtility.UrlEncode(catList.ToString()));
		}

		if (modLoaderTypes != null)
		{
			pathString.Append("&modLoaderTypes=");
			StringBuilder typeList = new("[");
			typeList.Append(string.Join(',', modLoaderTypes));
			typeList.Append(']');
			pathString.Append(WebUtility.UrlEncode(typeList.ToString()));
		}

		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		SearchModsResponse response = await client.GetFromJsonAsync<SearchModsResponse>(pathString.ToString()) 
		                              ?? throw new NetworkResourceUnavailableException();

		IEnumerable<ModPluginInfo> modPlugins = from mod in response.Data
			where mod.AllowModDistribution is not (null or false)
			select new ModPluginInfo
			{
				Id = mod.Id,
				Name = mod.Name,
				Description = mod.Summary,
				DownloadCount = mod.DownloadCount,
				License = string.Empty,
				Owner = string.Join(", ", mod.Authors.Select(author => author.Name)),
				Slug = mod.Slug,
				IconUrl = mod.Logo.Url,
				Url = mod.Links.WebsiteUrl
			};

		return modPlugins.ToArray();
	}
	
	public async Task<Queue<FileDownloadRequest>> GetDownloadQueueFromManifest(CursePackManifest manifest, string dest)
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

		HttpResponseMessage message = await client.PostAsJsonAsync(
			"mods/files", 
			new GetModFilesRequestBody(
				manifest.Files.Select(f => f.FileId).ToList()));
		message.EnsureSuccessStatusCode();
		
		File[] response = (await message.Content.ReadFromJsonAsync<GetFilesResponse>())?.Data ?? throw new NetworkResourceUnavailableException();
		string modsFolder = Path.Join(dest, "mods");

		Directory.CreateDirectory(modsFolder);
		
		return new Queue<FileDownloadRequest>(response.Select(file => new FileDownloadRequest()
		{
			Destination = Path.Join(modsFolder, file.FileName),
			DownloadLocations = [file.DownloadUrl],
			Hash = file.Hashes.First(h => h.Algo == CfHashAlgorithm.Sha1).Value,
			HashAlgorithm = HashAlgorithm.Sha1
		}));
	}

	private record GetModFilesRequestBody([UsedImplicitly] List<uint> fileIds);

	public enum ModLoaderType
	{
		Any,
		Forge,
		Cauldron,
		LiteLoader,
		Fabric,
		Quilt,
		NeoForge
	}

	public record Category(
		uint Id,
		uint GameId,
		string Name,
		string Slug,
		string Url,
		string IconUrl,
		string DateModified,
		bool? IsClass,
		uint? ClassId,
		uint? ParentCategoryId,
		int? DisplayIndex);

	private record GetCategoriesResponse(
		Category[] Data);

	private record Pagination(
		uint Index,
		uint PageSize,
		uint ResultCount,
		uint TotalCount);

	[UsedImplicitly]
	private record ModAuthor(
		uint Id,
		string Name,
		string Url);

	[UsedImplicitly]
	private record ModLinks(
		string WebsiteUrl,
		string? WikiUrl,
		string? IssuesUrl,
		string? SourceUrl);

	[UsedImplicitly]
	private record ModAsset(
		uint Id,
		string Url);

	[UsedImplicitly]
	private record Mod(
		uint Id,
		string Name,
		string Slug,
		ModLinks Links,
		string Summary,
		ulong DownloadCount,
		Category[] Categories,
		ModAuthor[] Authors,
		ModAsset Logo,
		bool? AllowModDistribution);

	private enum CfHashAlgorithm
	{
		Sha1 = 1,
		Md5
	}

	[UsedImplicitly]
	private record FileHash(
		string Value,
		CfHashAlgorithm Algo);

	private enum FileReleaseType
	{
		Release = 1,
		Beta,
		Alpha
	}

	private enum FileRelationType
	{
		EmbeddedLibrary = 1,
		OptionalDependency,
		RequiredDependency,
		Tool,
		Incompatible,
		Include
	}

	[UsedImplicitly]
	private record FileDependency(
		uint ModId,
		FileRelationType RelationType);

	[UsedImplicitly]
	private record File(
		uint Id,
		string DisplayName,
		string FileName,
		FileReleaseType ReleaseType,
		FileHash[] Hashes,
		string DownloadUrl,
		string[] GameVersions,
		FileDependency[] Dependencies);

	private record GetFilesResponse(File[] Data);

	private record GetModFilesResponse(
		File[] Data,
		Pagination Pagination);

	private record SearchModsResponse(
		Mod[] Data,
		Pagination Pagination);

	private record StringResponse(
		string Data);
}

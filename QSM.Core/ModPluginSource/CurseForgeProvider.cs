using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace QSM.Core.ModPluginSource;

public class CurseForgeProvider(IHttpClientFactory httpClientFactory) : ModPluginProvider
{
	public const string HttpClientName = "CurseForgeApi";
	public const string BaseAddress = "https://api.curseforge.com/v1/";
	public const string CurseKey = "$2a$10$kz4OPSZlWNbJLaJImOgTIOwfx3bnMshplbA1F5L2WiMuL5oq63o.q";

	const ushort MinecraftId = 432;
	public async Task<Category[]> ListCategories()
	{
		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);
		return (await client.GetFromJsonAsync<GetCategoriesResponse>("categories"))!.Data;
	}

	public override Task<ModPluginDownloadInfo[]> CheckForUpdatesAsync(IEnumerable<string> modFiles)
	{
		throw new NotImplementedException();
	}

	public override Task<ModPluginInfo> GetDetailedInfoAsync(ModPluginInfo modPlugin)
	{
		throw new NotImplementedException();
	}

	public override Task<ModPluginDownloadInfo[]> GetVersionsAsync(string slug)
	{
		throw new NotImplementedException();
	}

	public override Task<ModPluginDownloadInfo> ResolveDependenciesAsync(ModPluginDownloadInfo mod)
	{
		throw new NotImplementedException();
	}

	public override Task<ModPluginInfo[]> SearchAsync(string query = "")
	{
		return SearchAsync(query);
	}

	public async Task<ModPluginInfo[]> SearchAsync(string searchFilter = "", uint? classId = null, IEnumerable<uint>? categories = null, IEnumerable<ModLoaderType>? modLoaderTypes = null)
	{
		StringBuilder pathString = new($"mods/search?gameId={MinecraftId}");

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

			StringBuilder catList = new('[');
			catList.Append(string.Join(',', categories));
			catList.Append(']');

			pathString.Append(WebUtility.UrlEncode(catList.ToString()));
		}

		if (modLoaderTypes != null)
		{
			pathString.Append("&modLoaderTypes=");
			StringBuilder typeList = new('[');
			typeList.Append(string.Join(',', modLoaderTypes));
			typeList.Append(']');
			pathString.Append(WebUtility.UrlEncode(typeList.ToString()));
		}

		using HttpClient client = httpClientFactory.CreateClient(HttpClientName);

	}

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
		uint? DisplayIndex);

	record GetCategoriesResponse(
		Category[] Data);

	record ModLinks();

	record Mod(
		uint Id,
		string Name,
		ModLinks Links,
		string Summary,
		long DownloadCount);

	record SearchModsResponse();
}

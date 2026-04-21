using QSM.Core.Utilities;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace QSM.Core.JavaProvider;

public class AdoptiumProvider(IHttpClientFactory factory) : IJavaProvider, IHttpConsumer
{
	public string HttpClientName => "AdoptiumFetcher";
	public string HttpBaseAddress => "https://api.adoptium.net/v3/";

	private static string ProcessArchitecture => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.X86 => "x32",
		Architecture.X64 => "x64",
		Architecture.Arm => "arm",
		Architecture.Arm64 => "aarch64",
		Architecture.S390x => "s390x",
		Architecture.Ppc64le => "ppc64le",
		_ => throw new NotSupportedException("Invalid CPU architecture")
	};

	// ReSharper disable once InconsistentNaming
	internal static string OS
	{
		get
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return "windows";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return "linux";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return "mac";

			throw new NotSupportedException("Unidentified platform.");
		}
	}

	public string Terms => "https://adoptium.net/about/#_licenses";

	public async Task<Dictionary<string, int>> GetAvailableReleasesAsync()
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		AvailableReleasesRequest? response =
			await client.GetFromJsonAsync<AvailableReleasesRequest>("info/available_releases");

		return response!.available_releases!.Reverse().ToDictionary(version =>
			response.available_lts_releases!.Contains(version) ? $"Java {version} (LTS)" : $"Java {version}");
	}

	public async Task<JavaDownloadInfo> GetDownloadUrlAsync(string releaseName)
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		string response = await client.GetStringAsync($"https://api.adoptium.net/v3/checksum/version/{releaseName}/{OS}/{ProcessArchitecture}/jdk/hotspot/normal/eclipse?project=jdk");

		return new JavaDownloadInfo(
			$"{HttpBaseAddress}binary/version/{releaseName}/{OS}/{ProcessArchitecture}/jre/hotspot/normal/eclipse",
			response.Split(' ')[0], HashAlgorithm.Sha256);
	}

	public async Task<string[]> ListJREAsync(int javaMajorRelease)
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		ReleaseNamesRequest? response = await client.GetFromJsonAsync<ReleaseNamesRequest>(
			$"info/release_names?architecture={ProcessArchitecture}&heap_size=normal&image_type=jre&os={OS}&page=0&page_size=10&project=jdk&release_type=ga&semver=false&sort_method=DEFAULT&sort_order=DESC&vendor=eclipse&version=%5B{javaMajorRelease}%2C{javaMajorRelease + 1}%5D");

		return response!.releases!;
	}

	internal sealed record AvailableReleasesRequest(
		int[]? available_lts_releases = null,
		int[]? available_releases = null,
		int? most_recent_feature_release = null,
		int? most_recent_feature_version = null,
		int? most_recent_lts = null,
		int? tip_version = null);

	internal sealed record ReleaseNamesRequest(
		string[]? releases = null);
}
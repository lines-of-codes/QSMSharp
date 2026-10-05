using QSM.Core.Utilities;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace QSM.Core.JavaProvider;

public partial class AdoptiumProvider(IHttpClientFactory factory) : IJavaProvider, IHttpConsumer
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
	[JsonSerializable(typeof(ReleaseNamesRequest))]
	[JsonSerializable(typeof(AvailableReleasesRequest))]
	internal partial class AdoptiumContext : JsonSerializerContext { }

	public string HttpClientName => "AdoptiumFetcher";
	public string HttpBaseAddress => "https://api.adoptium.net/v3/";

	internal static string ProcessArchitecture => RuntimeInformation.ProcessArchitecture switch
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

			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "mac" : throw new NotSupportedException("Unidentified platform.");
		}
	}

	public string Terms => "https://adoptium.net/about/#_licenses";

	public async Task<Dictionary<string, int>> GetAvailableReleasesAsync()
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		AvailableReleasesRequest? response =
			await client.GetFromJsonAsync("info/available_releases", AdoptiumContext.Default.AvailableReleasesRequest);

		return response!.AvailableReleases!.Reverse().ToDictionary(version =>
			response.AvailableLTSReleases!.Contains(version) ? $"Java {version} (LTS)" : $"Java {version}");
	}

	public async Task<JavaDownloadInfo> GetDownloadUrlAsync(string releaseName)
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		string response = await client.GetStringAsync($"checksum/version/{releaseName}/{OS}/{ProcessArchitecture}/jre/hotspot/normal/eclipse?project=jdk");

		return new JavaDownloadInfo(
			$"{HttpBaseAddress}binary/version/{releaseName}/{OS}/{ProcessArchitecture}/jre/hotspot/normal/eclipse",
			response.Split(' ')[0], HashAlgorithm.Sha256);
	}

	public async Task<string[]> ListJREAsync(int javaMajorRelease)
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		ReleaseNamesRequest? response = await client.GetFromJsonAsync(
			$"info/release_names?architecture={ProcessArchitecture}&heap_size=normal&image_type=jre&os={OS}&page=0&page_size=10&project=jdk&release_type=ga&semver=false&sort_method=DEFAULT&sort_order=DESC&vendor=eclipse&version=%5B{javaMajorRelease}%2C{javaMajorRelease + 1}%5D",
			AdoptiumContext.Default.ReleaseNamesRequest);

		return response!.Releases!;
	}

	internal sealed record AvailableReleasesRequest(
		int[]? AvailableLTSReleases = null,
		int[]? AvailableReleases = null,
		int? MostRecentFeatureRelease = null,
		int? MostRecentFeatureVersion = null,
		int? MostRecentLTS = null,
		int? TipVersion = null);

	internal sealed record ReleaseNamesRequest(
		string[]? Releases = null);
}
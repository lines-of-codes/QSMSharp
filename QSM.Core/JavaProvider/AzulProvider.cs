using QSM.Core.Utilities;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace QSM.Core.JavaProvider;

public partial class AzulProvider(IHttpClientFactory factory) : IJavaProvider, IHttpConsumer
{
	[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
	[JsonSerializable(typeof(ZuluJvmData[]))]
	private sealed partial class AzulContext : JsonSerializerContext { }

	public string HttpClientName => "AzulFetcher";
	public string HttpBaseAddress => "https://api.azul.com/metadata/v1/zulu/";

	private readonly Dictionary<int, Dictionary<string, JavaDownloadInfo>> _downloadUrlCache = [];

	private static string ProcessArchitecture => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.X86 => "i686",
		Architecture.X64 => "amd64",
		Architecture.Arm => "aarch32",
		Architecture.Arm64 => "aarch64",
		_ => throw new NotSupportedException("Invalid CPU architecture")
	};

	// ReSharper disable once InconsistentNaming
	private static string OS
	{
		get
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return "windows";

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return "linux";

			return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : throw new NotSupportedException("Unidentified platform.");
		}
	}

	private static string ArchiveType
	{
		get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "zip" : "tar.gz";
	}

	public string Terms => "https://www.azul.com/products/core/openjdk-terms-of-use/";

	public async Task<Dictionary<string, int>> GetAvailableReleasesAsync()
	{
		Dictionary<int, string> availableMajorReleases = [];

		HttpClient client = factory.CreateClient(HttpClientName);
		ZuluJvmData[]? response = await client.GetFromJsonAsync(
			$"packages?latest=true&arch={ProcessArchitecture}&os={OS}&java_package_type=jre&javafx_bundled=false&archive_type={ArchiveType}&include_fields=support_term",
			AzulContext.Default.ZuluJvmDataArray);

		foreach (ZuluJvmData runtime in response!)
		{
			int majorVersion = runtime.JavaVersion![0];

			if (availableMajorReleases.ContainsKey(majorVersion))
			{
				continue;
			}

			availableMajorReleases.Add(majorVersion,
				runtime.SupportTerm == "lts" ? $"Java {majorVersion} (LTS)" : $"Java {majorVersion}");
		}

		return availableMajorReleases.ToDictionary(x => x.Value, x => x.Key);
	}

	public Task<JavaDownloadInfo> GetDownloadUrlAsync(string releaseName)
	{
		if (!int.TryParse(releaseName.Split('.')[0], out int major))
		{
			throw new FormatException();
		}

		return Task.FromResult(_downloadUrlCache[major][releaseName]);
	}

	public async Task<string[]> ListJREAsync(int javaMajorRelease)
	{
		HttpClient client = factory.CreateClient(HttpClientName);
		ZuluJvmData[]? response = await client.GetFromJsonAsync(
			$"packages?java_version={javaMajorRelease}&arch={ProcessArchitecture}&os={OS}&java_package_type=jre&javafx_bundled=false&archive_type={ArchiveType}&include_fields=lib_c_type&include_fields=sha256_hash",
			AzulContext.Default.ZuluJvmDataArray);

		List<string> jres = [];
		Dictionary<string, JavaDownloadInfo> downloadUrlCache = [];

		foreach (ZuluJvmData runtime in response!)
		{
			string version =
				$"{runtime.JavaVersion![0]}.{runtime.JavaVersion[1]}.{runtime.JavaVersion[2]}+{runtime.OpenjdkBuildNumber}";
			jres.Add(version);
			_ = downloadUrlCache.TryAdd(version, new JavaDownloadInfo(runtime.DownloadUrl!, runtime.SHA256Hash!, HashAlgorithm.Sha256));
		}

		_downloadUrlCache.TryAdd(javaMajorRelease, downloadUrlCache);

		return jres.ToArray();
	}

	private sealed record ZuluJvmData(
		string? AvailabilityType = null,
		int[]? DistroVersion = null,
		string? DownloadUrl = null,
		int[]? JavaVersion = null,
		bool? Latest = null,
		string? LibCType = null,
		string? Name = null,
		int? OpenjdkBuildNumber = null,
		string? PackageUuid = null,
		string? Product = "zulu",
		string? SHA256Hash = null,
		string? SupportTerm = null);
}
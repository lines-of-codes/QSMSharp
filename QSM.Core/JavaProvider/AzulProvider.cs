using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace QSM.Core.JavaProvider;

public class AzulProvider : IJavaProvider
{
	private readonly Dictionary<int, Dictionary<string, string>> _downloadUrlCache = [];

	private readonly HttpClient HttpClient;

	public AzulProvider()
	{
		HttpClient = new HttpClient { BaseAddress = new Uri("https://api.azul.com/metadata/v1/zulu/") };
	}

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
			{
				return "windows";
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				return "linux";
			}

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				return "macos";
			}

			throw new NotSupportedException("Unidentified platform.");
		}
	}

	private static string ArchiveType
	{
		get => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "zip" : "tar.gz";
	}

	public string Terms => "https://www.azul.com/products/core/openjdk-terms-of-use/";

	public async Task<Dictionary<string, int>> GetAvailableReleasesAsync()
	{
		Dictionary<int, string> availableMajorReleases = new();

		ZuluJvmData[]? response = await HttpClient.GetFromJsonAsync<ZuluJvmData[]>(
			$"packages?latest=true&arch={ProcessArchitecture}&os={OS}&java_package_type=jre&javafx_bundled=false&archive_type={ArchiveType}&include_fields=support_term");

		foreach (ZuluJvmData runtime in response!)
		{
			int majorVersion = runtime.java_version![0];

			if (availableMajorReleases.ContainsKey(majorVersion))
			{
				continue;
			}

			availableMajorReleases.Add(majorVersion,
				runtime.support_term == "lts" ? $"Java {majorVersion} (LTS)" : $"Java {majorVersion}");
		}

		return availableMajorReleases.ToDictionary(x => x.Value, x => x.Key);
	}

	public Task<string> GetDownloadUrlAsync(string releaseName)
	{
		if (!int.TryParse(releaseName.Split('.')[0], out int major))
		{
			throw new FormatException();
		}

		return Task.FromResult(_downloadUrlCache[major][releaseName]);
	}

	public async Task<string[]> ListJREAsync(int javaMajorRelease)
	{
		ZuluJvmData[]? response = await HttpClient.GetFromJsonAsync<ZuluJvmData[]>(
			$"packages?java_version={javaMajorRelease}&arch={ProcessArchitecture}&os={OS}&java_package_type=jre&javafx_bundled=false&archive_type={ArchiveType}&include_fields=lib_c_type");

		List<string> jres = new();
		Dictionary<string, string> downloadUrlCache = new();

		foreach (ZuluJvmData runtime in response!)
		{
			string version =
				$"{runtime.java_version![0]}.{runtime.java_version[1]}.{runtime.java_version[2]}+{runtime.openjdk_build_number}";
			jres.Add(version);
			_ = downloadUrlCache.TryAdd(version, runtime.download_url!);
		}

		_downloadUrlCache.TryAdd(javaMajorRelease, downloadUrlCache);

		return jres.ToArray();
	}

	private sealed record ZuluJvmData(
		string? availability_type = null,
		int[]? distro_version = null,
		string? download_url = null,
		int[]? java_version = null,
		bool? latest = null,
		string? lib_c_type = null,
		string? name = null,
		int? openjdk_build_number = null,
		string? package_uuid = null,
		string? product = "zulu",
		string? support_term = null);
}
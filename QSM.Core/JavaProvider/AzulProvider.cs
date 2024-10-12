using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace QSM.Core.JavaProvider;

public class AzulProvider : JavaProvider
{
	internal record class ZuluJVMData(
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

	HttpClient HttpClient;
	private string _processArchitecture => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.X86 => "i686",
		Architecture.X64 => "amd64",
		Architecture.Arm => "aarch32",
		Architecture.Arm64 => "aarch64",
		_ => throw new NotSupportedException("Invalid CPU architecture")
	};
	private string _os
	{
		get
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return "windows";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				return "linux";
			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				return "macos";

			throw new NotSupportedException("Unidentified platform.");
		}
	}
	private string _archiveType
	{
		get
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				return "zip";

			return "tar.gz";
		}
	}

	public override string Terms => "https://www.azul.com/products/core/openjdk-terms-of-use/";

	Dictionary<int, Dictionary<string, string>> DownloadURLCache = [];

	public AzulProvider()
	{
		HttpClient = new()
		{
			BaseAddress = new Uri("https://api.azul.com/metadata/v1/zulu/")
		};
	}

	public override async Task<Dictionary<string, int>> GetAvailableReleasesAsync()
	{
		Dictionary<int, string> availableMajorReleases = new();

		var response = await HttpClient.GetFromJsonAsync<ZuluJVMData[]>($"packages?latest=true&arch={_processArchitecture}&os={_os}&java_package_type=jre&javafx_bundled=false&archive_type={_archiveType}&include_fields=support_term");

		foreach (var runtime in response!)
		{
			var majorVersion = runtime.java_version![0];

			if (availableMajorReleases.ContainsKey(majorVersion))
				continue;

			availableMajorReleases.Add(majorVersion, runtime.support_term == "lts" ? $"Java {majorVersion} (LTS)" : $"Java {majorVersion}");
		}

		return availableMajorReleases.ToDictionary(x => x.Value, x => x.Key);
	}

	public override Task<string> GetDownloadUrlAsync(string releaseName)
	{
		if (!int.TryParse(releaseName.Split('.')[0], out int major))
			throw new FormatException();

		return Task.FromResult(DownloadURLCache[major][releaseName]);
	}

	public override async Task<string[]> ListJREAsync(int javaMajorRelease)
	{
		var response = await HttpClient.GetFromJsonAsync<ZuluJVMData[]>($"packages?java_version={javaMajorRelease}&arch={_processArchitecture}&os={_os}&java_package_type=jre&javafx_bundled=false&archive_type={_archiveType}&include_fields=lib_c_type");

		List<string> jres = new();
		Dictionary<string, string> downloadUrlCache = new();

		foreach (var runtime in response!)
		{
			var version = $"{runtime.java_version![0]}.{runtime.java_version[1]}.{runtime.java_version[2]}+{runtime.openjdk_build_number}";
			jres.Add(version);
			_ = downloadUrlCache.TryAdd(version, runtime.download_url!);
		}

		if (!DownloadURLCache.ContainsKey(javaMajorRelease))
			DownloadURLCache.Add(javaMajorRelease, downloadUrlCache);

		return jres.ToArray();
	}
}

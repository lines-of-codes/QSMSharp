using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace QSM.Core.JavaProvider;

public class AdoptiumProvider : IJavaProvider
{
	internal record class AvailableReleasesRequest(
	int[]? available_lts_releases = null,
	int[]? available_releases = null,
	int? most_recent_feature_release = null,
	int? most_recent_feature_version = null,
	int? most_recent_lts = null,
	int? tip_version = null);

	internal record class ReleaseNamesRequest(
		string[]? releases = null);

	HttpClient HttpClient;

	public string Terms => "https://adoptium.net/about/#_licenses";

	private string _processArchitecture => RuntimeInformation.ProcessArchitecture switch
	{
		Architecture.X86 => "x32",
		Architecture.X64 => "x64",
		Architecture.Arm => "arm",
		Architecture.Arm64 => "aarch64",
		Architecture.S390x => "s390x",
		Architecture.Ppc64le => "ppc64le",
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
				return "mac";

			throw new NotSupportedException("Unidentified platform.");
		}
	}

	public AdoptiumProvider()
	{
		HttpClient = new()
		{
			BaseAddress = new Uri("https://api.adoptium.net/v3/")
		};
	}

	public async Task<Dictionary<string, int>> GetAvailableReleasesAsync()
	{
		var response = await HttpClient.GetFromJsonAsync<AvailableReleasesRequest>("info/available_releases");

		var versions = new Dictionary<string, int>();

		foreach (var version in response!.available_releases!)
		{
			versions.Add(response.available_lts_releases!.Contains(version) ? $"Java {version} (LTS)" : $"Java {version}", version);
		}

		return versions;
	}

	public Task<string> GetDownloadUrlAsync(string releaseName)
	{
		return Task.FromResult($"{HttpClient.BaseAddress}binary/version/{releaseName}/{_os}/{_processArchitecture}/jre/hotspot/normal/eclipse");
	}

	public async Task<string[]> ListJREAsync(int javaMajorRelease)
	{
		var response = await HttpClient.GetFromJsonAsync<ReleaseNamesRequest>($"info/release_names?architecture={_processArchitecture}&heap_size=normal&image_type=jre&os={_os}&page=0&page_size=10&project=jdk&release_type=ga&semver=false&sort_method=DEFAULT&sort_order=DESC&vendor=eclipse&version=%5B{javaMajorRelease}%2C{javaMajorRelease + 1}%5D");

		return response!.releases!;
	}
}

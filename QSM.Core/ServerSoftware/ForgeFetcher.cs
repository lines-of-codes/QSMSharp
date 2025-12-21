using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace QSM.Core.ServerSoftware;

public partial class ForgeFetcher : InfoFetcher
{
	private readonly HttpClient httpClient;
	private readonly Regex majorMinorVersionMatch = MajorMinorVersionMatch();

	private string[] availableVersionsCache = [];

	public ForgeFetcher()
	{
		httpClient = new HttpClient();
	}

	public override string FirstRunArgs => "--install-server";

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (minecraftVersionsCache.Length != 0)
		{
			return minecraftVersionsCache;
		}

		ForgeVersions? response =
			await httpClient.GetFromJsonAsync<ForgeVersions>(
				"https://maven.minecraftforge.net/api/maven/versions/releases/net/minecraftforge/forge");

		availableVersionsCache = response!.Versions!;

		List<string> supportedVersions = [];

		foreach (string version in availableVersionsCache)
		{
			string minecraftVersion = majorMinorVersionMatch.Match(version).Value;
			minecraftVersion = minecraftVersion.Substring(0, minecraftVersion.Length - 1);

			if (!supportedVersions.Contains(minecraftVersion))
			{
				supportedVersions.Add(minecraftVersion);
			}
		}

		minecraftVersionsCache = supportedVersions.ToArray();
		Array.Reverse(minecraftVersionsCache);

		return minecraftVersionsCache;
	}

	public override Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (buildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return Task.FromResult(buildInfo);
		}

		string prefix = minecraftVersion + "-";
		List<string> versions = [];

		foreach (string version in availableVersionsCache)
		{
			if (version.StartsWith(prefix))
			{
				versions.Add(version.Substring(prefix.Length));
			}
		}

		versions.Reverse();

		buildInfoCache[minecraftVersion] = versions.ToArray();

		return Task.FromResult(buildInfoCache[minecraftVersion]);
	}

	[GeneratedRegex("1\\..+-")]
	private static partial Regex MajorMinorVersionMatch();

	public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		return Task.FromResult(
			$"https://maven.minecraftforge.net/net/minecraftforge/forge/{minecraftVersion}-{build}/forge-{minecraftVersion}-{build}-installer.jar");
	}

	public async Task InitializeOnFirstRun(ServerMetadata metadata, ServerSettings.ServerSettings settings,
		DataReceivedEventHandler outputReceived)
	{
		string serverJar = Path.Combine(metadata.ServerPath, "server.jar");
		string installerJar = Path.Combine(metadata.ServerPath, "installer.jar");

		// If installer jar doesn't exist, assume server.jar is the installer jar.
		if (!File.Exists(installerJar))
		{
			File.Move(serverJar, installerJar);
		}

		ProcessStartInfo startInfo = new()
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
			FileName = Path.Combine(settings.Java.JavaHome, "bin", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "javaw.exe" : "java"),
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments =
				$"{settings.Java.JvmArgs} -jar \"{Path.Combine(metadata.ServerPath, "installer.jar")}\" {FirstRunArgs}",
			WorkingDirectory = metadata.ServerPath
		};

		Process process = new() { StartInfo = startInfo };

		process.OutputDataReceived += outputReceived;
		process.ErrorDataReceived += outputReceived;

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		await process.WaitForExitAsync();
	}

	internal record class ForgeVersions(bool? IsSnapshot = null, string[]? Versions = null);
}
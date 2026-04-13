using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace QSM.Core.ServerSoftware;

public partial class ForgeFetcher(IHttpClientFactory factory) : InfoFetcher
{
	private readonly Regex _majorMinorVersionMatch = MajorMinorVersionMatch();

	private string[] _availableVersionsCache = [];

	public override string FirstRunArgs => "--installServer";

	public override string HttpClientName => "ForgeFetcher";

	public override string HttpBaseAddress => "";

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		if (MinecraftVersionsCache.Length != 0)
		{
			return MinecraftVersionsCache;
		}

		HttpClient client = factory.CreateClient(HttpClientName);
		ForgeVersions? response =
			await client.GetFromJsonAsync<ForgeVersions>(
				"https://maven.minecraftforge.net/api/maven/versions/releases/net/minecraftforge/forge");

		_availableVersionsCache = response!.Versions!;

		List<string> supportedVersions = [];

		foreach (string version in _availableVersionsCache)
		{
			string minecraftVersion = _majorMinorVersionMatch.Match(version).Value;
			minecraftVersion = minecraftVersion[..^1];

			if (!supportedVersions.Contains(minecraftVersion))
			{
				supportedVersions.Add(minecraftVersion);
			}
		}
		
		MinecraftVersionsCache = supportedVersions.ToArray();
		Array.Reverse(MinecraftVersionsCache);

		return MinecraftVersionsCache;
	}

	public override Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (BuildInfoCache.TryGetValue(minecraftVersion, out string[]? buildInfo))
		{
			return Task.FromResult(buildInfo);
		}

		string prefix = minecraftVersion + "-";
		List<string> versions = (from version in _availableVersionsCache
			where version.StartsWith(prefix)
			select version[prefix.Length..]).ToList();
		
		versions.Reverse();

		BuildInfoCache[minecraftVersion] = versions.ToArray();

		return Task.FromResult(BuildInfoCache[minecraftVersion]);
	}

	[GeneratedRegex(".+-")]
	private static partial Regex MajorMinorVersionMatch();

	public override Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		return Task.FromResult(
			$"https://maven.minecraftforge.net/net/minecraftforge/forge/{minecraftVersion}-{build}/forge-{minecraftVersion}-{build}-installer.jar");
	}

	public async Task InitializeOnFirstRun(ServerMetadata metadata, ServerSettings.ServerSettings settings,
		DataReceivedEventHandler outputReceived)
	{
		string serverJar = Path.Join(metadata.ServerPath, "server.jar");
		string installerJar = Path.Join(metadata.ServerPath, "installer.jar");

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

		string newServerFile = Path.Join(metadata.ServerPath, $"minecraft_server.{metadata.MinecraftVersion}.jar");
		if (File.Exists(newServerFile))
		{
			File.Move(newServerFile, serverJar);
		}
	}

	internal record ForgeVersions(bool? IsSnapshot = null, string[]? Versions = null);
}
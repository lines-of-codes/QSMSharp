using JetBrains.Annotations;
using QSM.Core.Utilities;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Runtime.InteropServices;

namespace QSM.Core.ServerSoftware;

public class QuiltFetcher(IHttpClientFactory factory) : InfoFetcher
{
	public override string HttpClientName => "QuiltFetcher";
	public override string HttpBaseAddress => "https://meta.quiltmc.org/v3/";

	private readonly SimpleCache<string[]> _versionListCache = new(TimeSpan.FromMinutes(30));

	public override async Task<string[]> FetchAvailableMinecraftVersionsAsync()
	{
		using HttpClient client = factory.CreateClient(HttpClientName);
		GameVersion[]? versions = await client.GetFromJsonAsync<GameVersion[]>("versions/game");
		return versions?.Where(v => v.Stable).Select(v => v.Version).ToArray() ?? [];
	}

	public override async Task<string[]> FetchAvailableBuildsAsync(string minecraftVersion)
	{
		if (_versionListCache.Value != null) return _versionListCache.Value;

		using HttpClient client = factory.CreateClient(HttpClientName);
		LoaderVersion[]? versions = await client.GetFromJsonAsync<LoaderVersion[]>("versions/loader");
		_versionListCache.Value = versions?.Select(v => v.Version).ToArray();
		return _versionListCache.Value ?? [];
	}

	public override async Task<string> GetDownloadUrlAsync(string minecraftVersion, string build)
	{
		using HttpClient client = factory.CreateClient(HttpClientName);
		InstallerVersion[]? versions = await client.GetFromJsonAsync<InstallerVersion[]>("versions/installer");
		return versions?[0].Url ?? string.Empty;
	}

	public static async Task InitializeOnFirstRun(ServerMetadata metadata, ServerSettings.ServerSettings settings,
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
				$"{settings.Java.JvmArgs} -jar \"{Path.Combine(metadata.ServerPath, "installer.jar")}\" install server {metadata.MinecraftVersion} {metadata.ServerVersion} --install-dir=. --download-server",
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

	[UsedImplicitly]
	private sealed record GameVersion(string Version, bool Stable);

	[UsedImplicitly]
	private sealed record LoaderVersion(
		string Separator,
		byte Build,
		string Maven,
		string Version);

	[UsedImplicitly]
	private sealed record InstallerVersion(
		string Url,
		string Maven,
		string Version);
}
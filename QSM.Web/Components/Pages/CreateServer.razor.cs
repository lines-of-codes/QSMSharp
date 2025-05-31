using JetBrains.Annotations;
using Microsoft.AspNetCore.Components;
using QSM.Core.ServerSoftware;
using QSM.Web.Data;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace QSM.Web.Components.Pages;

[UsedImplicitly]
public partial class CreateServer : ComponentBase
{
	private sealed class NewServerModel
	{
		[Required] public string? Name { get; set; }

		[Required] public ServerSoftwares Software { get; set; } = ServerSoftwares.Paper;

		public string? MinecraftVersion { get; set; }

		public string? ServerBuild { get; set; }

		[Required] public string? Path { get; set; }
	}

	[SupplyParameterFromForm] private NewServerModel Model { get; set; } = new();

	private string[] MinecraftVersions { get; set; } = [];
	private string[] AvailableBuilds { get; set; } = [];
	private string _targetFolderPreview = string.Empty;
	private string _processingMessage = string.Empty;
	private bool _isProcessing;

	private readonly Dictionary<ServerSoftwares, InfoFetcher> _infoFetchers = new()
	{
		{ ServerSoftwares.Paper, new PaperMCFetcher("paper") },
		{ ServerSoftwares.Purpur, new PurpurFetcher() },
		{ ServerSoftwares.Vanilla, new VanillaFetcher() },
		{ ServerSoftwares.Fabric, new FabricFetcher() },
		{ ServerSoftwares.NeoForge, new NeoForgeFetcher() },
		{ ServerSoftwares.Velocity, new PaperMCFetcher("velocity") }
	};

	protected override void OnInitialized()
	{
		Model.Path = GetDefaultServerPath();
		_targetFolderPreview = Model.Path;
	}

	string GetDefaultServerPath()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			return "/var/lib/qsm-web/servers/";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
			return "/usr/local/etc/qsm-web/servers/";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			return "C:\\ProgramData\\QSMWeb\\Servers\\";
		// TODO: Add default path for macOS

		return string.Empty;
	}

	protected override async Task OnInitializedAsync()
	{
		await FetchMinecraftVersions();
		await FetchAvailableBuilds(MinecraftVersions[0]);
	}

	private async Task OnValidSubmit()
	{
		_isProcessing = true;

		_processingMessage = "Creating folder...";
		Directory.CreateDirectory(_targetFolderPreview);

		_processingMessage = "Downloading server file...";
		var downloadUrl = await _infoFetchers[Model.Software]
			.GetDownloadUrlAsync(Model.MinecraftVersion ?? MinecraftVersions[0],
				Model.ServerBuild ?? AvailableBuilds[0]);

		using (HttpClient client = new())
		{
			await using Stream stream = await client.GetStreamAsync(downloadUrl);
			await using FileStream fs = new(Path.Join(_targetFolderPreview, "server.jar"), FileMode.OpenOrCreate);

			await stream.CopyToAsync(fs);
		}

		_processingMessage = "Registering server...";
		await using (ApplicationDbContext ctx = await DbFactory.CreateDbContextAsync())
		{
			var newServer = new ServerInstance()
			{
				MinecraftVersion = Model.MinecraftVersion ?? MinecraftVersions[0],
				Software = Model.Software,
				Name = Model.Name,
				Running = false,
				ServerPath = _targetFolderPreview,
				ServerVersion = Model.ServerBuild ?? AvailableBuilds[0],
			};
			ctx.Servers.Add(newServer);
			await ctx.SaveChangesAsync();
		}

		_isProcessing = false;
		NavigationManager.NavigateTo("/");
	}

	private async Task FetchMinecraftVersions(ServerSoftwares software = ServerSoftwares.Paper)
	{
		MinecraftVersions = await _infoFetchers[software].FetchAvailableMinecraftVersionsAsync();
	}

	private async Task FetchAvailableBuilds(string minecraftVersion)
	{
		var software = Model.Software;

		if (software == ServerSoftwares.Custom) return;

		AvailableBuilds = await _infoFetchers[software].FetchAvailableBuildsAsync(minecraftVersion);
	}

	private async Task SoftwareSelectionChanged(ChangeEventArgs args)
	{
		var parseResult = Enum.TryParse(typeof(ServerSoftwares), (string)args.Value!, false, out var software);

		if (!parseResult || (ServerSoftwares)software! == ServerSoftwares.Custom) return;

		await FetchMinecraftVersions((ServerSoftwares)software);
		await FetchAvailableBuilds(MinecraftVersions[0]);
	}

	private async Task MinecraftVersionChanged(ChangeEventArgs args)
	{
		var version = (string?)args.Value;

		if (version == null) return;

		await FetchAvailableBuilds(version);
	}

	private void SetTargetFolderPreview(string path, string folderName)
	{
		var invalidChars = Path.GetInvalidFileNameChars();

		if (folderName.IndexOfAny(invalidChars) != -1)
		{
			folderName = string.Join("", folderName.Split(invalidChars));

			if (folderName.Length == 0)
			{
				folderName = Path.GetRandomFileName();
			}
		}

		_targetFolderPreview = Path.Join(path, folderName);
	}

	private void NameInputChanged(ChangeEventArgs args)
	{
		var newName = (string?)args.Value;

		if (newName == null) return;

		SetTargetFolderPreview(Model.Path!, newName);
	}

	private void PathInputChanged(ChangeEventArgs args)
	{
		var newPath = (string?)args.Value;

		if (newPath == null) return;

		SetTargetFolderPreview(newPath, Model.Name!);
	}
}
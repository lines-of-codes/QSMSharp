using Microsoft.EntityFrameworkCore;
using QSM.Core.ServerSettings;
using QSM.Core.ServerSoftware;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static QSM.Web.Program;

namespace QSM.Web.Data;

public class ProcessManager
{
	public enum OutputType
	{
		Normal,
		Error
	}

	public class OutputCache(OutputType type, string message)
	{
		public OutputType Type = type;
		public string Message = message;
	}

	private readonly Dictionary<int, Process> _processes = [];
	public IReadOnlyDictionary<int, Process> Processes => _processes;

	private readonly Dictionary<int, List<OutputCache>> _serverOutput = [];

	public IReadOnlyDictionary<int, List<OutputCache>> ServerOutput => _serverOutput;

	public event Action<int, OutputCache>? OutputReceived;

	/// <summary>
	/// Runs a server from the provided information.
	/// </summary>
	/// <param name="server">Server information</param>
	/// <exception cref="Exception">Failure to load the server's configuration</exception>
	public async Task RunAsync(ServerInstance server)
	{
		if (Processes.TryGetValue(server.Id, out var process) && !process.HasExited)
		{
			await StopAsync(server.Id);
		}

		if (!ServerSettings.TryLoadJson(server.ConfigPath, out ServerSettings? settings) || settings == null)
		{
			settings = new ServerSettings();
			await settings.SaveJsonAsync(server.ConfigPath);
		}

		_serverOutput[server.Id] = [];

		if (settings.Java.JavaHome == string.Empty)
		{
			ApplicationConfig applicationConfig = App.Services.GetRequiredService<ApplicationConfig>();
			settings.Java.JavaHome = applicationConfig.DefaultJavaInstall ?? string.Empty;
		}

		if (settings.FirstRun)
		{
			if (server.Software == ServerSoftwares.NeoForge)
			{
				await new NeoForgeFetcher().InitializeOnFirstRun(server, settings, (_, e) =>
				{
					OutputCache output = new(OutputType.Normal, e.Data ?? string.Empty);
					_serverOutput[server.Id].Add(output);
					OutputReceived?.Invoke(server.Id, output);
				});
			} else if (server.Software == ServerSoftwares.Forge)
			{
				await new ForgeFetcher().InitializeOnFirstRun(server, settings, (_, e) =>
				{
					OutputCache output = new(OutputType.Normal, e.Data ?? string.Empty);
					_serverOutput[server.Id].Add(output);
					OutputReceived?.Invoke(server.Id, output);
				});
			}
			settings.FirstRun = false;
			await settings.SaveJsonAsync(server.ConfigPath);
		}

		string args = string.Empty;

		if (settings.Java.InitMemoryPoolSize > 0)
			args += $"-Xms{settings.Java.InitMemoryPoolSize}G ";
		
		if (settings.Java.MaxMemoryPoolSize > 0)
			args += $"-Xmx{settings.Java.MaxMemoryPoolSize}G ";

		args +=
			$"{settings.Java.JvmArgs} -jar \"{Path.Combine(server.ServerPath!, "server.jar")}\" {settings.Java.ProgramArgs}";

		if (server.Software == ServerSoftwares.NeoForge)
		{
			string argsFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win_args.txt" : "unix_args.txt";
			args =
				$"{settings.Java.JvmArgs} @libraries/net/neoforged/neoforge/{server.ServerVersion}/{argsFile} {settings.Java.ProgramArgs}";
		} else if (server.Software == ServerSoftwares.Forge)
		{
			string argsFile = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "win_args.txt" : "unix_args.txt";
			args = $"{settings.Java.JvmArgs} @libraries/net/minecraftforge/forge/{server.MinecraftVersion}-{server.ServerVersion}/{argsFile} {settings.Java.ProgramArgs}";
		}

		var startInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			Arguments = args,
			RedirectStandardInput = true,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
			FileName = Path.Combine(settings.Java.JavaHome, "bin",
				RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "java.exe" : "java"),
			WindowStyle = ProcessWindowStyle.Hidden,
			WorkingDirectory = server.ServerPath
		};

		process = new Process { StartInfo = startInfo };

		process.OutputDataReceived += (_, e) =>
		{
			OutputCache output = new(OutputType.Normal, e.Data ?? string.Empty);
			_serverOutput[server.Id].Add(output);
			OutputReceived?.Invoke(server.Id, output);
		};

		process.ErrorDataReceived += (_, e) =>
		{
			OutputCache output = new(OutputType.Error, e.Data ?? string.Empty);
			_serverOutput[server.Id].Add(output);
			OutputReceived?.Invoke(server.Id, output);
		};
		
		process.Exited += (_, _) =>
		{
			if (!server.Running) return;
			
			var dbFactory = App.Services.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
			using var ctx = dbFactory.CreateDbContext();
			server.Running = false;
			ctx.Update(server);
			ctx.SaveChanges();
		};

		OutputCache startMessage = new(OutputType.Normal, $"Starting server with arguments: {args}");
		_serverOutput[server.Id].Add(startMessage);
		OutputReceived?.Invoke(server.Id, startMessage);

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		_processes[server.Id] = process;
	}

	public async Task StopAsync(int id)
	{
		bool processExist = Processes.TryGetValue(id, out var process);

		if (!processExist || process!.HasExited) return;
		
		await process.StandardInput.WriteLineAsync("stop");
		await process.WaitForExitAsync();
	}
}
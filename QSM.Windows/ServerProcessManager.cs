using QSM.Core.ServerSoftware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace QSM.Windows;

public class ServerProcessManager
{
	public enum OutputType
	{
		Normal,
		Error
	}

	public class OutputCache(ServerProcessManager.OutputType type, string message)
	{
		public OutputType Type = type;
		public string Message = message;
	}

	private static ServerProcessManager s_instance = null;
	public static ServerProcessManager Instance
	{
		get
		{
			s_instance ??= new();
			return s_instance;
		}
	}

	public Dictionary<Guid, Process> Processes { get; private set; } = [];
	public Dictionary<Guid, List<OutputCache>> ProcessOutputs { get; private set; } = [];

	public Process StartServer(int metadataIndex, Guid serverGuid)
	{
		if (Processes.TryGetValue(serverGuid, out var process) && !process.HasExited)
		{
			throw new NotSupportedException("Process already running for this server instance.");
		}

		if (serverGuid == Guid.Empty)
		{
			throw new ArgumentException("Server GUID is empty.");
		}

		var settings = ApplicationData.ServerSettings[serverGuid];
		var metadata = ApplicationData.Configuration.Servers[metadataIndex];
		string programArgs = settings.Java.ProgramArgs;

		ProcessOutputs[serverGuid] = [];

		var serverJar = Path.Join(metadata.ServerPath, "server.jar");
		string args = $"{settings.Java.JvmArgs} -jar \"{serverJar}\" {programArgs}";

		if (metadata.Software == ServerSoftwares.NeoForge)
		{
			args = $"{settings.Java.JvmArgs} @libraries/net/neoforged/neoforge/{metadata.ServerVersion}/win_args.txt {programArgs}";
		}
		else if (metadata.Software == ServerSoftwares.Forge && !File.Exists(serverJar))
		{
			args = $"{settings.Java.JvmArgs} @libraries/net/minecraftforge/forge/{metadata.MinecraftVersion}-{metadata.ServerVersion}/win_args.txt {programArgs}";
		}

		var startInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardInput = true,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
			FileName = Path.Combine(settings.Java.JavaHome, "bin", "javaw.exe"),
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = args,
			WorkingDirectory = metadata.ServerPath
		};

		process = new Process
		{
			StartInfo = startInfo
		};

		ProcessOutputs[serverGuid].Add(new(OutputType.Normal, $"Starting server with arguments: {args}"));

		process.OutputDataReceived += (sender, e) => ProcessOutputs[serverGuid].Add(new(OutputType.Normal, e.Data));
		process.ErrorDataReceived += (sender, e) => ProcessOutputs[serverGuid].Add(new(OutputType.Error, e.Data));

		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		Processes[serverGuid] = process;

		return process;
	}
}
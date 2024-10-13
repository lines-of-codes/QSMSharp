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

	public class OutputCache
	{
		public OutputType Type;
		public string Message;

		public OutputCache(OutputType type, string message)
		{
			Type = type;
			Message = message;
		}
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

	public Dictionary<Guid, Process> Processes { get; private set; } = new();
	public Dictionary<Guid, List<OutputCache>> ProcessOutputs { get; private set; } = new();

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

		var startInfo = new ProcessStartInfo
		{
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardInput = true,
			RedirectStandardError = true,
			RedirectStandardOutput = true,
			FileName = Path.Combine(settings.Java.JavaHome, "bin", "javaw.exe"),
			WindowStyle = ProcessWindowStyle.Hidden,
			Arguments = $"{settings.Java.JvmArgs} -jar \"{Path.Combine(metadata.ServerPath, "server.jar")}\" nogui",
			WorkingDirectory = metadata.ServerPath
		};

		process = new Process
		{
			StartInfo = startInfo
		};

		ProcessOutputs[serverGuid] = new();

		process.OutputDataReceived += (sender, e) => ProcessOutputs[serverGuid].Add(new(OutputType.Normal, e.Data));

		process.ErrorDataReceived += (sender, e) => ProcessOutputs[serverGuid].Add(new(OutputType.Error, e.Data));


		process.Start();
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		Processes[serverGuid] = process;

		return process;
	}
}

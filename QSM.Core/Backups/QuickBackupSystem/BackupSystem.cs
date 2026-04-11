using System.Diagnostics;

namespace QSM.Core.Backups.QuickBackupSystem;

public class BackupSystem(string path)
{
	/// <summary>
	/// Path to the qbsgo binary
	/// </summary>
	private string BinaryPath { get; } = path;

	public static async Task<bool> CheckSystemdAsync()
	{
		Process proc = Process.Start("/usr/bin/systemctl", "--version");
		await proc.WaitForExitAsync();
		return proc.ExitCode == 0;
	}

	public Process InstallService(IEnumerable<string> targets)
	{
		return new Process
		{
			StartInfo = new ProcessStartInfo(BinaryPath, [
				"-install",
				"-targets",
				string.Join(',', targets),
				"-dontask"
			])
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				WorkingDirectory = Path.GetDirectoryName(BinaryPath)
			}
		};
	}

	public Process BackupProcess(IEnumerable<string> targets)
	{
		return new Process
		{
			StartInfo = new ProcessStartInfo(BinaryPath, [
				"-backup",
				"-targets",
				string.Join(',', targets)
			])
			{
				RedirectStandardOutput = true,
				WorkingDirectory = Path.GetDirectoryName(BinaryPath)
			}
		};
	}

	public async Task<(bool success, string? message)> BackupAsync(IEnumerable<string> targets)
	{
		Process proc = BackupProcess(targets);
		proc.Start();
		
		await proc.WaitForExitAsync();
		return (proc.ExitCode == 0, await proc.StandardOutput.ReadToEndAsync());
	}
}
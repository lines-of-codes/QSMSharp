namespace QSM.Core.Backups.QuickBackupSystem;

public class BackupTarget
{
	public string Path { get; set; } = string.Empty;
	public string Remote { get; set; } = string.Empty;
	public string Interval { get; set; } = "weekly";
}
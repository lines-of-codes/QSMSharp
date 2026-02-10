using System.Runtime.Serialization;

namespace QSM.Core.Backups.QuickBackupSystem;

public class BackupRemote
{
	public string Type { get; set; } = string.Empty;
	public string Root { get; set; } = string.Empty;
	
	[DataMember(Name = "destDir")]
	public string DestDir { get; set; } = string.Empty;

	public string User { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public string Script { get; set; } = string.Empty;
}
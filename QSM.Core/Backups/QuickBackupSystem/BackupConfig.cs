using System.Runtime.Serialization;

namespace QSM.Core.Backups.QuickBackupSystem;

public class BackupConfig
{
	public string Archive { get; set; } = "tar";
	public string ArchiveDir { get; set; } = "/tmp/";
	public string Compression { get; set; } = "gzip";
	public int CompressionLevel { get; set; } = 9;
	public bool DeleteAfterUpload { get; set; } = true;
	public BackupList? BackupList { get; set; }

	public Dictionary<string, BackupRemote> Remotes { get; set; } = [];
	
	public Dictionary<string, BackupTarget> Targets { get; set; } = [];
}

public class BackupList
{
	public bool Enabled { get; set; } = true;
	public bool CleanEntries { get; set; } = true;
	public string OlderThan { get; set; } = "1m";
}
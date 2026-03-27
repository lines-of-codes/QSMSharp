using System.Text.Json.Serialization;
using Tomlyn.Serialization;

namespace QSM.Core.Backups.QuickBackupSystem;


[TomlSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[TomlSerializable(typeof(BackupConfig))]
#pragma warning disable SYSLIB1224
public partial class BackupConfigContext : TomlSerializerContext
#pragma warning restore SYSLIB1224
{
	
}
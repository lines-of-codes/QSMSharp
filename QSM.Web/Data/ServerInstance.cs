using QSM.Core.ServerSoftware;
using System.ComponentModel.DataAnnotations;

namespace QSM.Web.Data;

/// <summary>
/// A database-friendly version of the <see cref="QSM.Core.ServerSoftware.ServerMetadata"/>
/// class.
/// </summary>
public class ServerInstance
{
	public int Id { get; set; }
	
	public bool Running { get; set; }
	
	[MaxLength(255)]
	public string? Name { get; set; }
	
	[MaxLength(127)]
	public string? MinecraftVersion { get; set; }
	
	[MaxLength(127)]
	public string? ServerVersion { get; set; }
	
	[MaxLength(4096)]
	public string? ServerPath { get; set; }
	public ServerSoftwares Software { get; set; }
}
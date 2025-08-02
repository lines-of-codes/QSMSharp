using System.ComponentModel.DataAnnotations;

namespace QSM.Web.Data;

public class JavaInstall
{
	public int Id { get; init; }
	
	[MaxLength(4096)]
	public string InstallPath { get; set; } = string.Empty;
	
	public bool Default { get; set; }
}
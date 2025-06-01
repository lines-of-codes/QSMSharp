namespace QSM.Core.ModPluginSource.Modrinth;

public record class Category(
	string? icon = null,
	string? name = null,
	string? project_type = null,
	string? header = null);
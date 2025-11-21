namespace QSM.Core.ServerSettings;

[AttributeUsage(AttributeTargets.Property)]
public class ServerPropertyAttribute(string name) : Attribute
{
	/// <summary>
	/// Minecraft (JE) server.properties Property Name
	/// </summary>
	public string Name { get; set; } = name;
}
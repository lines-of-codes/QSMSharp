namespace QSM.Core.ServerSettings;

public class JavaSettings
{
	public string JavaHome { get; set; } = string.Empty;

	/// <summary>
	/// The initial memory allocation pool size 
	/// of the Java instance. (-Xms)
	/// </summary>
	public int InitMemoryPoolSize { get; set; }

	/// <summary>
	/// The maximum memory allocation pool size
	/// of the Java instance. (-Xmx)
	/// </summary>
	public int MaxMemoryPoolSize { get; set; }

	public string JvmArgs { get; set; } = string.Empty;
}

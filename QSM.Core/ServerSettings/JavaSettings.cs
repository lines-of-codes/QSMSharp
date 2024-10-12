namespace QSM.Core.ServerSettings;

public class JavaSettings
{
	public string JavaHome { get; set; } = string.Empty;

	/// <summary>
	/// The initial memory allocation pool size 
	/// of the Java instance in GiB. (-Xms)
	/// </summary>
	public double InitMemoryPoolSize { get; set; }

	/// <summary>
	/// The maximum memory allocation pool size
	/// of the Java instance in GiB. (-Xmx)
	/// </summary>
	public double MaxMemoryPoolSize { get; set; }

	public string JvmArgs { get; set; } = string.Empty;
}

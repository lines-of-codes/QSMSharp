namespace QSM.Core.ModPluginSource.Modrinth;

public class MrpackOperation(string operation, bool error = false)
{
	public bool Error { get; } = error;
	public string Operation { get; } = operation;
	public double? Progress { get; set; }

	public MrpackOperation(string operation, double? progress) : this(operation)
	{
		Progress = progress;
	}
}
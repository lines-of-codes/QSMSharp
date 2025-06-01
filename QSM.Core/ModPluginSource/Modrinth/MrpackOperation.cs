namespace QSM.Core.ModPluginSource.Modrinth;

public class MrpackOperation
{
	public bool Error;
	public string Operation = string.Empty;
	public double? Progress;

	public MrpackOperation(string operation)
	{
		Operation = operation;
	}

	public MrpackOperation(string operation, bool error)
	{
		Operation = operation;
		Error = error;
	}

	public MrpackOperation(string operation, double? progress) : this(operation)
	{
		Progress = progress;
	}
}
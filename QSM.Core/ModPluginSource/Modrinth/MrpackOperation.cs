namespace QSM.Core.ModPluginSource.Modrinth
{
	public class MrpackOperation
	{
		public string Operation = string.Empty;
		public double? Progress = null;
		public bool Error;

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
}

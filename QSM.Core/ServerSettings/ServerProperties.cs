namespace QSM.Core.ServerSettings;

public class ServerProperties(string path)
{
	public Dictionary<string, string> Properties { get; } = new();
	
	/// <summary>
	/// A block of comments at the top of the file,
	/// the only comments which will be preserved
	/// </summary>
	private readonly List<string> _headers = [];

	public void Load()
	{
		if (!File.Exists(path))
			return;

		bool endOfHeader = false;

		foreach (string line in File.ReadAllLines(path))
		{
			var trimmedLine = line.Trim();
			
			var comment = trimmedLine.IndexOf('#');

			if (comment == 0 && !endOfHeader)
			{
				_headers.Add(trimmedLine);
				continue;
			}
			endOfHeader = true;

			if (comment != -1)
				trimmedLine = line[..comment];

			if (string.IsNullOrWhiteSpace(trimmedLine))
				continue;

			var pair = trimmedLine.Split('=', 2, StringSplitOptions.TrimEntries);
			Properties[pair[0]] = pair[1];
		}
	}

	public void Save()
	{
		using var writer = new StreamWriter(path, false);
		
		_headers.ForEach(writer.WriteLine);

		foreach (var pair in Properties)
		{
			writer.WriteLine($"{pair.Key}={pair.Value}");
		}
	}
}
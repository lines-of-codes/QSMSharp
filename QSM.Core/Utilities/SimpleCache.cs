namespace QSM.Core.Utilities;

/// <summary>
/// Simple in-memory value cache
/// </summary>
public class SimpleCache<T>(TimeSpan cacheTime)
{
	public T? Value
	{
		get => DateTime.UtcNow - _cachedAt >= cacheTime ? default : field;
		set
		{
			field = value;
			_cachedAt = DateTime.UtcNow;
		}
	}

	private DateTime _cachedAt;
}
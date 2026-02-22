namespace QSM.Core.Utilities;

/// <summary>
/// Simple in-memory value cache
/// </summary>
public class SimpleCache<T>(TimeSpan cacheTime)
{
	public T? Value
	{
		get
		{
			return DateTime.Now - _cachedAt >= cacheTime ? default : field;
		}
		set
		{
			field = value;
			_cachedAt = DateTime.Now;
		}
	}

	private DateTime _cachedAt;
}
namespace QSM.Core.JavaProvider;

public interface IJavaProvider
{
	public abstract string Terms { get; }

	public abstract Task<Dictionary<string, int>> GetAvailableReleasesAsync();

	public abstract Task<string[]> ListJREAsync(int javaMajorRelease);

	public abstract Task<string> GetDownloadUrlAsync(string releaseName);
}

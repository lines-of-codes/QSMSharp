namespace QSM.Core.JavaProvider;

public interface IJavaProvider
{
	public string Terms { get; }

	public Task<Dictionary<string, int>> GetAvailableReleasesAsync();

	public Task<string[]> ListJREAsync(int javaMajorRelease);

	public Task<string> GetDownloadUrlAsync(string releaseName);
}
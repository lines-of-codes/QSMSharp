namespace QSM.Core.JavaProvider;

public abstract class JavaProvider
{
    public abstract string Terms { get; }

    public abstract Task<Dictionary<string, int>> GetAvailableReleasesAsync();

    public abstract Task<string[]> ListJREAsync(int javaMajorRelease);

    public abstract Task<string> GetDownloadUrlAsync(string releaseName);
}

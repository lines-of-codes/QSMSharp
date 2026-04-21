using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("QSM.Core.Tests")]
namespace QSM.Core.JavaProvider;

public interface IJavaProvider
{
	public string Terms { get; }

	public Task<Dictionary<string, int>> GetAvailableReleasesAsync();

	public Task<string[]> ListJREAsync(int javaMajorRelease);

	public Task<JavaDownloadInfo> GetDownloadUrlAsync(string releaseName);
}
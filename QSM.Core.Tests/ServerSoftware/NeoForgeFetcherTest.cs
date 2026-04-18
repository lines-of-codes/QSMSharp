using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class NeoForgeFetcherTest : FetcherTestBase<NeoForgeFetcher>
{
	private static MockHttpMessageHandler CreateMock()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/neoforge")
			.Respond("application/json", @"{
	""isSnapshot"": false,
	""versions"": [
		""21.11.41-beta"",""21.11.42"",
		""26.1.2.12-beta"", ""26.1.2.13-beta""
	]
}");
		return mockHttp;
	}	
	
	[Fact]
	public async Task FetchMinecraftVersionsAsync() {
		NeoForgeFetcher fetcher = CreateFetcher(CreateMock());
		string[] result = await fetcher.FetchAvailableMinecraftVersionsAsync();

		Assert.Equal(["26.1", "1.21.11"], result);
	}

	[Fact]
	public async Task FetchAvailableBuildsAsync()
	{
		MockHttpMessageHandler mockHttp = CreateMock();
		NeoForgeFetcher fetcher = CreateFetcher(mockHttp);
		await fetcher.FetchAvailableMinecraftVersionsAsync();
		await fetcher.FetchAvailableBuildsAsync("26.1");

		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task GetDownloadUrlAsync()
	{
		NeoForgeFetcher fetcher = CreateFetcher(null!);
		Assert.Equal("https://maven.neoforged.net/releases/net/neoforged/neoforge/26.1.2.13-beta/neoforge-26.1.2.13-beta-installer.jar", await fetcher.GetDownloadUrlAsync("26.1", "26.1.2.13-beta"));
	}
}

using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class ForgeFetcherTest : FetcherTestBase<ForgeFetcher>
{
	private static MockHttpMessageHandler CreateMock()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("https://maven.minecraftforge.net/api/maven/versions/releases/net/minecraftforge/forge")
			.Respond("application/json", @"{
	""isSnapshot"": false,
	""versions"": [
		""1.21.11-61.1.4"",""1.21.11-61.1.5"",
		""26.1-62.0.8"", ""26.1-62.0.9""
	]
}");
		return mockHttp;
	}	
	
	[Fact]
	public async Task FetchMinecraftVersionsAsync() {
		ForgeFetcher fetcher = CreateFetcher(CreateMock());
		string[] result = await fetcher.FetchAvailableMinecraftVersionsAsync();

		Assert.Equal(["26.1", "1.21.11"], result);
	}
	
	[Fact]
	public async Task FetchMinecraftVersionsAsync_CacheTest() {
		MockHttpMessageHandler mockHttp = CreateMock();
		ForgeFetcher fetcher = CreateFetcher(mockHttp);
		
		await fetcher.FetchAvailableMinecraftVersionsAsync();
		await fetcher.FetchAvailableMinecraftVersionsAsync();
		
		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task FetchAvailableBuildsAsync()
	{
		MockHttpMessageHandler mockHttp = CreateMock();
		ForgeFetcher fetcher = CreateFetcher(mockHttp);
		await fetcher.FetchAvailableMinecraftVersionsAsync();
		string[] result1 = await fetcher.FetchAvailableBuildsAsync("26.1");
		Assert.Equal(["62.0.9", "62.0.8"], result1);
		
		string[] result2 = await fetcher.FetchAvailableBuildsAsync("26.1");
		Assert.Equal(["62.0.9", "62.0.8"], result2);

		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task GetDownloadUrlAsync()
	{
		ForgeFetcher fetcher = CreateFetcher(null!);
		Assert.Equal("https://maven.minecraftforge.net/net/minecraftforge/forge/26.1-62.0.9/forge-26.1-62.0.9-installer.jar", await fetcher.GetDownloadUrlAsync("26.1", "62.0.9"));
	}
}

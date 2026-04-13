using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class FabricFetcherTest : FetcherTestBase<FabricFetcher>
{
	[Fact]
	public async Task FetchMinecraftVersionsAsync_OnlyStable() {
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("/v2/versions/game").Respond("application/json", @"[
	{
		""version"": ""26.1.2"",
		""stable"": true
	},
	{
		""version"": ""26.1.2-rc-1"",
		""stable"": false
	},
	{
		""version"": ""26.1.1"",
		""stable"": true
	}
]");

		var fetcher = CreateFetcher(mockHttp);
		var result = await fetcher.FetchAvailableMinecraftVersionsAsync();

		Assert.Equal(["26.1.2", "26.1.1"], result);
	}

	[Fact]
	public async Task FetchAvailableBuildsAsync_CacheTest()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp
			.Expect("/v2/versions/loader/26.1.2")
			.Respond("application/json", @"[
	{
		""loader"": {
			""version"": ""0.19.1""
		}
	},
	{
		""loader"": {
			""version"": ""0.19.0""
		}
	}
]");

		var fetcher = CreateFetcher(mockHttp);
		await fetcher.FetchAvailableBuildsAsync("26.1.2");
		await fetcher.FetchAvailableBuildsAsync("26.1.2");

		mockHttp.VerifyNoOutstandingExpectation();
	}
}

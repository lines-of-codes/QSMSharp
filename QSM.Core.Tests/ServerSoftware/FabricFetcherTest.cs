using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class FabricFetcherTest : FetcherTestBase<FabricFetcher>
{
	[Fact]
	public async Task FetchMinecraftVersionsAsync_OnlyStable() {
		MockHttpMessageHandler mockHttp = new();
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
		MockHttpMessageHandler mockHttp = new();
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

		FabricFetcher fetcher = CreateFetcher(mockHttp);
		await fetcher.FetchAvailableBuildsAsync("26.1.2");
		await fetcher.FetchAvailableBuildsAsync("26.1.2");

		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task GetDownloadUrlAsync()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.When("/v2/versions/installer")
			.Respond("application/json", @"[
	{
		""url"": ""https://maven.fabricmc.net/net/fabricmc/fabric-installer/1.1.1/fabric-installer-1.1.1.jar"",
		""maven"": ""net.fabricmc:fabric-installer:1.1.1"",
		""version"": ""1.1.1"",
		""stable"": true
	},
	{
		""url"": ""https://maven.fabricmc.net/net/fabricmc/fabric-installer/1.1.0/fabric-installer-1.1.0.jar"",
		""maven"": ""net.fabricmc:fabric-installer:1.1.0"",
		""version"": ""1.1.0"",
		""stable"": false
	}
]");
		
		FabricFetcher fetcher = CreateFetcher(mockHttp);
		Assert.Equal("https://meta.fabricmc.net/v2/versions/loader/26.1.2/0.19.1/1.1.1/server/jar", await fetcher.GetDownloadUrlAsync("26.1.2", "0.19.1"));
	}
}

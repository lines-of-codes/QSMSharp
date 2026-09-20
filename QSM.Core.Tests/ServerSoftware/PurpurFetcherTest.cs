using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class PurpurFetcherTest : FetcherTestBase<PurpurFetcher>
{
	[Fact]
	public async Task NewestVersionsFirst()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("/v2/purpur/")
			.Respond(
				"application/json",
				"""
				{
				  "project": "purpur",
				  "metadata": {
				    "current": "26.2"
				  },
				  "versions": [
				    "1.20",
				    "1.20.1",
				    "1.20.2",
				    "1.20.4",
				    "1.20.6"
				  ]
				}
				""");
		
		PurpurFetcher fetcher = CreateFetcher(mockHttp);
		string[] versions = await fetcher.FetchAvailableMinecraftVersionsAsync();
		
		Assert.Equal(["1.20.6", "1.20.4", "1.20.2", "1.20.1", "1.20"], versions);
		mockHttp.VerifyNoOutstandingExpectation();
	}
	
	[Fact]
	public async Task NewestBuildsFirst()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("/v2/purpur/26.3")
			.Respond(
				"application/json",
				"""
				{
				  "project": "purpur",
				  "version": "26.3",
				  "builds": {
				    "latest": "2637",
				    "all": [
				      "2634",
				      "2635",
				      "2636",
				      "2637",
				      "69"
				    ]
				  }
				}
				""");

		PurpurFetcher fetcher = CreateFetcher(mockHttp);
		string[] result = await fetcher.FetchAvailableBuildsAsync("26.3");

		Assert.Equal(["2637", "2636", "2635", "2634", "69"], result);
		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task GetDownloadUrl()
	{
		MockHttpMessageHandler mockHttp = new();
		PurpurFetcher fetcher = CreateFetcher(mockHttp);
		string url = await fetcher.GetDownloadUrlAsync("26.3", "69");

		Assert.Equal("https://api.purpurmc.org/v2/purpur/26.3/69/download", url);
	}
}
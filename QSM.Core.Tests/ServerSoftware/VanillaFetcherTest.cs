using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class VanillaFetcherTest : FetcherTestBase<VanillaFetcher>
{
	private static MockHttpMessageHandler CreateMock()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.When("/mc/game/version_manifest_v2.json").Respond("application/json", @"{
	""latest"": { ""release"": ""26.1.2"", ""snapshot"": ""26.2-snapshot-3"" },
	""versions"": [
		{
			""id"": ""26.2-snapshot-3"",
			""type"": ""snapshot"",
			""url"": ""https://piston-meta.mojang.com/v1/packages/f8ce00355c7535323ad546f1f702e3aaaa48a99a/26.2-snapshot-3.json"",
			""time"": ""2026-04-14T11:36:13+00:00"",
			""releaseTime"": ""2026-04-14T11:30:34+00:00"",
			""sha1"": ""f8ce00355c7535323ad546f1f702e3aaaa48a99a"",
			""complianceLevel"": 1
	    },
		{
			""id"": ""26.1.2"",
			""type"": ""release"",
			""url"": ""https://piston-meta.mojang.com/v1/packages/e769ea3cbacbb342fcb6311842b3351a1d374506/26.1.2.json"",
			""time"": ""2026-04-14T06:37:07+00:00"",
			""releaseTime"": ""2026-04-09T10:12:23+00:00"",
			""sha1"": ""e769ea3cbacbb342fcb6311842b3351a1d374506"",
			""complianceLevel"": 1
		},
		{
			""id"": ""1.21.11"",
			""type"": ""release"",
			""url"": ""https://piston-meta.mojang.com/v1/packages/fd5b005d89b4c9ce856c45f8d6b7ee992f371e2a/1.21.11.json"",
			""time"": ""2026-04-14T06:35:53+00:00"",
			""releaseTime"": ""2025-12-09T12:23:30+00:00"",
			""sha1"": ""fd5b005d89b4c9ce856c45f8d6b7ee992f371e2a"",
			""complianceLevel"": 1
		}
	]
}");
		return mockHttp;
	}
	
	[Fact]
	public async Task FetchMinecraftVersionsAsync_OnlyStable() {
		VanillaFetcher fetcher = CreateFetcher(CreateMock());
		string[] result = await fetcher.FetchAvailableMinecraftVersionsAsync();

		Assert.Equal(["26.1.2", "1.21.11"], result);
	}

	[Fact]
	public async Task FetchAvailableBuildsAsync()
	{
		VanillaFetcher fetcher = CreateFetcher(new MockHttpMessageHandler());
		Assert.Equal([""], await fetcher.FetchAvailableBuildsAsync("26.1.2"));
	}

	[Fact]
	public async Task GetDownloadUrlAsync()
	{
		MockHttpMessageHandler mockHttp = CreateMock();
		mockHttp.When("/v1/packages/e769ea3cbacbb342fcb6311842b3351a1d374506/26.1.2.json").Respond("application/json",
			@"{
	""downloads"": {
	    ""client"": {
			""sha1"": ""4e618f09a0c649dde3fdf829df443ce0b8831e65"",
			""size"": 38113927,
			""url"": ""https://piston-data.mojang.com/v1/objects/4e618f09a0c649dde3fdf829df443ce0b8831e65/client.jar""
	    },
	    ""server"": {
			""sha1"": ""97ccd4c0ed3f81bbb7bfacddd1090b0c56f9bc51"",
			""size"": 60417480,
			""url"": ""https://piston-data.mojang.com/v1/objects/97ccd4c0ed3f81bbb7bfacddd1090b0c56f9bc51/server.jar""
	    }
	}
}");
		
		VanillaFetcher fetcher = CreateFetcher(mockHttp);
		await fetcher.FetchAvailableMinecraftVersionsAsync();
		Assert.Equal("https://piston-data.mojang.com/v1/objects/97ccd4c0ed3f81bbb7bfacddd1090b0c56f9bc51/server.jar", await fetcher.GetDownloadUrlAsync("26.1.2", ""));
	}
}

using QSM.Core.JavaProvider;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.JavaProvider;

public class AdoptiumProviderTest : FetcherTestBase<AdoptiumProvider>
{
	[Fact]
	public async Task GetAvailableReleasesAsync() {
		MockHttpMessageHandler mockHttp = new();
		mockHttp.When("/v3/info/available_releases").Respond("application/json", @"{
    ""available_lts_releases"": [
        8,
        11,
        17
    ],
    ""available_releases"": [
        8,
        11,
        16,
        17
    ]
}");

		var fetcher = CreateFetcher(mockHttp);
		Assert.Equal(new Dictionary<string, int>{
			{ "Java 8 (LTS)", 8 },
			{ "Java 11 (LTS)", 11 },
			{ "Java 16", 16 },
			{ "Java 17 (LTS)", 17 }
		}, await fetcher.GetAvailableReleasesAsync());
	}

	[Fact]
	public async Task ListJreAsync() {
		MockHttpMessageHandler mockHttp = new();
		mockHttp.When("/v3/info/release_names")
				.WithQueryString("version", "[25,26]")
				.Respond("application/json", @"{
    ""releases"": [
        ""jdk-25.0.2+10"",
        ""jdk-25.0.1+8"",
        ""jdk-25+36""
    ]
}");

		AdoptiumProvider fetcher = CreateFetcher(mockHttp);
		Assert.Equal(["jdk-25.0.2+10", "jdk-25.0.1+8", "jdk-25+36"], await fetcher.ListJREAsync(25));
	}

	[Fact]
	public async Task GetDownloadUrlAsync()
	{
		AdoptiumProvider fetcher = new(null!);
		Assert.Equal($"https://api.adoptium.net/v3/binary/version/jdk-25.0.2+10/{AdoptiumProvider.OS}/x64/jre/hotspot/normal/eclipse", await fetcher.GetDownloadUrlAsync("jdk-25.0.2+10"));
	}
}

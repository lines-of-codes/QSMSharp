using QSM.Core.ServerSoftware;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.ServerSoftware;

public class QuiltFetcherTest : FetcherTestBase<QuiltFetcher>
{
	[Fact]
	public async Task OnlyStableMinecraftVersions()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("/v3/versions/game")
			.Respond(
				"application/json", 
				"""
				[
					{
					  "version": "26.3",
					  "stable": true
					},
					{
					  "version": "26.3-snapshot-1",
					  "stable": false
					},
					{
					  "version": "26.2",
					  "stable": true
					},
					{
					  "version": "26.2-pre-1",
					  "stable": false
					},
					{
					  "version": "26.2-snapshot-2",
					  "stable": false
					},
					{
					  "version": "26.1.2",
					  "stable": true
					}
				]
				""");

		QuiltFetcher fetcher = CreateFetcher(mockHttp);
		string[] versions = await fetcher.FetchAvailableMinecraftVersionsAsync();
		
		Assert.Equal(["26.3", "26.2", "26.1.2"], versions);
		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task LoaderVersions()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("/v3/versions/loader")
			.Respond(
				"application/json",
				"""
				[
					{
						"version": "0.30.2-beta.1"
					},
					{
						"version": "0.30.1"
					}
				]
				""");
		
		QuiltFetcher fetcher = CreateFetcher(mockHttp);
		string[] versions = await fetcher.FetchAvailableBuildsAsync(string.Empty);
		
		Assert.Equal(["0.30.2-beta.1", "0.30.1"], versions);
		mockHttp.VerifyNoOutstandingExpectation();
	}

	[Fact]
	public async Task GetDownloadUrl()
	{
		MockHttpMessageHandler mockHttp = new();
		mockHttp.Expect("/v3/versions/installer")
			.Respond(
				"application/json",
				"""
				[
					{
						"version": "0.15.1",
						"url": "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-installer/0.15.1/quilt-installer-0.15.1.jar"
					},
					{
						"version": "0.15.0",
						"url": "https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-installer/0.15.0/quilt-installer-0.15.0.jar"
					}
				]
				""");
		
		QuiltFetcher fetcher = CreateFetcher(mockHttp);
		string url = await fetcher.GetDownloadUrlAsync(string.Empty, string.Empty);
		
		Assert.Equal("https://maven.quiltmc.org/repository/release/org/quiltmc/quilt-installer/0.15.1/quilt-installer-0.15.1.jar", url);
		mockHttp.VerifyNoOutstandingExpectation();
	}
}
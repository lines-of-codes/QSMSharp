using QSM.Core.JavaProvider;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests.JavaProvider;

public class AzulProviderTest : FetcherTestBase<AzulProvider>
{
	[Fact]
	public async Task GetAvailableReleasesAsync() {
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("/metadata/v1/zulu/packages").Respond("application/json", @"[
	{
		""availability_type"": ""CA"",
		""distro_version"": [
			26,
			28,
			59,
			0
		],
		""download_url"": ""https://cdn.azul.com/zulu/bin/zulu26.28.59-ca-jre26.0.0-win_x64.zip"",
		""java_version"": [
			26,
			0,
			0
		],
		""latest"": true,
		""name"": ""zulu26.28.59-ca-jre26.0.0-win_x64.zip"",
		""openjdk_build_number"": 35,
		""package_uuid"": ""62f9413f-5900-4068-bb2b-e57a6214e33c"",
		""product"": ""zulu"",
		""support_term"": ""sts""
	},
	{
		""availability_type"": ""CA"",
		""distro_version"": [
			25,
			32,
			21,
			0
		],
		""download_url"": ""https://cdn.azul.com/zulu/bin/zulu25.32.21-ca-jre25.0.2-win_x64.zip"",
		""java_version"": [
			25,
			0,
			2
		],
		""latest"": true,
		""name"": ""zulu25.32.21-ca-jre25.0.2-win_x64.zip"",
		""openjdk_build_number"": 10,
		""package_uuid"": ""9f7fbe1e-4ff9-4d8e-b506-6710b7a0e682"",
		""product"": ""zulu"",
		""support_term"": ""lts""
	}
]");

		var fetcher = CreateFetcher(mockHttp);
		Assert.Equal(new Dictionary<string, int>{
			{ "Java 26", 26 },
			{ "Java 25 (LTS)", 25 },
		}, await fetcher.GetAvailableReleasesAsync());
	}

	[Fact]
	public async Task ListJREAsync() {
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("/metadata/v1/zulu/packages")
				.WithQueryString("java_version", "25")
				.Respond("application/json", @"[
	{
		""availability_type"": ""CA"",
		""distro_version"": [
			25,
			32,
			21,
			0
		],
		""download_url"": ""https://cdn.azul.com/zulu/bin/zulu25.32.21-ca-jre25.0.2-win_x64.zip"",
		""java_version"": [
			25,
			0,
			2
		],
		""latest"": true,
		""lib_c_type"": null,
		""name"": ""zulu25.32.21-ca-jre25.0.2-win_x64.zip"",
		""openjdk_build_number"": 10,
		""package_uuid"": ""9f7fbe1e-4ff9-4d8e-b506-6710b7a0e682"",
		""product"": ""zulu""
	},
	{
		""availability_type"": ""CA"",
		""distro_version"": [
			25,
			28,
			85,
			0
		],
		""download_url"": ""https://cdn.azul.com/zulu/bin/zulu25.28.85-ca-jre25.0.0-win_x64.zip"",
		""java_version"": [
			25,
			0,
			0
		],
		""latest"": true,
		""lib_c_type"": null,
		""name"": ""zulu25.28.85-ca-jre25.0.0-win_x64.zip"",
		""openjdk_build_number"": 36,
		""package_uuid"": ""d51db4cb-ee5b-4cfa-a19a-43767463e2d5"",
		""product"": ""zulu""
	}		
]");

		var fetcher = CreateFetcher(mockHttp);
		Assert.Equal(["25.0.2+10", "25.0.0+36"], await fetcher.ListJREAsync(25));
	}

	[Fact]
	public async Task GetDownloadUrlAsync()
	{
		var mockHttp = new MockHttpMessageHandler();
		mockHttp.When("/metadata/v1/zulu/packages")
				.WithQueryString("java_version", "25")
				.Respond("application/json", @"[
	{
		""availability_type"": ""CA"",
		""distro_version"": [
			25,
			32,
			21,
			0
		],
		""download_url"": ""https://cdn.azul.com/zulu/bin/zulu25.32.21-ca-jre25.0.2-win_x64.zip"",
		""java_version"": [
			25,
			0,
			2
		],
		""latest"": true,
		""lib_c_type"": null,
		""name"": ""zulu25.32.21-ca-jre25.0.2-win_x64.zip"",
		""openjdk_build_number"": 10,
		""package_uuid"": ""9f7fbe1e-4ff9-4d8e-b506-6710b7a0e682"",
		""product"": ""zulu""
	},
	{
		""availability_type"": ""CA"",
		""distro_version"": [
			25,
			28,
			85,
			0
		],
		""download_url"": ""https://cdn.azul.com/zulu/bin/zulu25.28.85-ca-jre25.0.0-win_x64.zip"",
		""java_version"": [
			25,
			0,
			0
		],
		""latest"": true,
		""lib_c_type"": null,
		""name"": ""zulu25.28.85-ca-jre25.0.0-win_x64.zip"",
		""openjdk_build_number"": 36,
		""package_uuid"": ""d51db4cb-ee5b-4cfa-a19a-43767463e2d5"",
		""product"": ""zulu""
	}		
]");

		var fetcher = CreateFetcher(mockHttp);
		await fetcher.ListJREAsync(25);
		Assert.Equal("https://cdn.azul.com/zulu/bin/zulu25.32.21-ca-jre25.0.2-win_x64.zip", await fetcher.GetDownloadUrlAsync("25.0.2+10"));
	}
}

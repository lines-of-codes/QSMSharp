using Microsoft.Extensions.DependencyInjection;
using QSM.Core.Utilities;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests;

public class FetcherTestBase<T> where T : IHttpConsumer
{
	protected static T CreateFetcher(MockHttpMessageHandler mockHttp)
	{
		var temp = (T)Activator.CreateInstance(typeof(T), (IHttpClientFactory?)null)!;

		var services = new ServiceCollection();
		services.AddHttpClient(temp.HttpClientName, client => {
			client.BaseAddress = new Uri(temp.HttpBaseAddress);
		}).ConfigurePrimaryHttpMessageHandler(() => mockHttp);

		var factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
		return (T)Activator.CreateInstance(typeof(T), factory)!;
	}
}

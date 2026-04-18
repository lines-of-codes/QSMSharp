using Microsoft.Extensions.DependencyInjection;
using QSM.Core.Utilities;
using RichardSzalay.MockHttp;

namespace QSM.Core.Tests;

public class FetcherTestBase<T> where T : IHttpConsumer
{
	protected static T CreateFetcher(MockHttpMessageHandler mockHttp)
	{
		T temp = (T)Activator.CreateInstance(typeof(T), (IHttpClientFactory?)null)!;

		ServiceCollection services = new();
		services.AddHttpClient(temp.HttpClientName, client => {
			if (!string.IsNullOrEmpty(temp.HttpBaseAddress))
			{
				client.BaseAddress = new Uri(temp.HttpBaseAddress);
			}
		}).ConfigurePrimaryHttpMessageHandler(() => mockHttp);

		IHttpClientFactory factory = services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
		return (T)Activator.CreateInstance(typeof(T), factory)!;
	}
}

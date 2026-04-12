using HappyCoding.Hosting.Desktop.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QSM.Core.JavaProvider;
using QSM.Core.ModPluginSource;
using QSM.Core.ServerSoftware;
using QSM.Core.Utilities;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace QSM.Windows;

public static class Program
{
	public static IHost Hoster { get; private set; }

	public static void Main(string[] args)
	{
		ApplicationData.EnsureDataFolderExists();

		Log.Logger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.File(
				Path.Combine(ApplicationData.LogsFolderPath, "WinQSM.txt"),
				rollingInterval: RollingInterval.Day)
			.CreateLogger();

		var builder = Host.CreateApplicationBuilder(args);

		builder.Services.AddSerilog();

		builder.Services.AddHttpClient(ModrinthProvider.HttpClientName, client =>
		{
			client.BaseAddress = new Uri(ModrinthProvider.BaseAddress);
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"lines-of-codes/QSMSharp/{Assembly.GetEntryAssembly()!.GetName().Version} (linesofcodes@dailitation.xyz)");
		});

		builder.Services.AddHttpClient(PaperMCHangarProvider.HttpClientName, client =>
		{
			client.BaseAddress = new Uri(PaperMCHangarProvider.BaseAddress);
		});

		builder.Services.AddHttpClient(CurseForgeProvider.HttpClientName, client =>
		{
			client.BaseAddress = new Uri(CurseForgeProvider.BaseAddress);
			client.DefaultRequestHeaders.Add("x-api-key", CurseForgeProvider.CurseKey);
		});

		IHttpConsumer[] fetchers =
		[
			new FabricFetcher(null!),
			new QuiltFetcher(null!),
			new PaperMCFetcher("paper", null!),
			new PaperMCFetcher("velocity", null!),
			new PaperMCFetcher("folia", null!),
			new PurpurFetcher(null!),
			new AdoptiumProvider(null!),
			new AzulProvider(null!),
		];

		foreach (IHttpConsumer fetcher in fetchers)
		{
			builder.Services.AddHttpClient(fetcher.HttpClientName, client =>
			{
				client.BaseAddress = new Uri(fetcher.HttpBaseAddress);
			});
		}

		builder.Services.AddTransient<ModrinthProvider>();
		builder.Services.AddTransient<PaperMCHangarProvider>();
		builder.Services.AddTransient<CurseForgeProvider>();

		builder.Services.AddTransient<AdoptiumProvider>();
		builder.Services.AddTransient<AzulProvider>();

		((IHostApplicationBuilder)builder).Properties.Add(
			key: HostingExtensions.HostingContextKey,
			value: new HostingContext() { IsLifetimeLinked = true });

		builder.ConfigureWinUI<App>();

		Hoster = builder.Build();

		Hoster.Run();
	}
}

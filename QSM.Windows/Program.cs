using CommunityToolkit.WinUI.UI.Controls.TextToolbarSymbols;
using HappyCoding.Hosting.Desktop.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QSM.Core.ModPluginSource;
using QSM.Core.ServerSoftware;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QSM.Windows
{
	public static class Program
	{
		public static IHost Hoster;

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
				client.BaseAddress = new System.Uri(ModrinthProvider.BaseAddress);
				client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", $"lines-of-codes/WinQSM/{Assembly.GetEntryAssembly()!.GetName().Version} (linesofcodes@dailitation.xyz)");
			});

			builder.Services.AddHttpClient(PaperMCHangarProvider.HttpClientName, client =>
			{
				client.BaseAddress = new System.Uri(PaperMCHangarProvider.BaseAddress);
			});

			builder.Services.AddTransient<ModrinthProvider>();
			builder.Services.AddTransient<PaperMCHangarProvider>();

			((IHostApplicationBuilder)builder).Properties.Add(
				key: HostingExtensions.HostingContextKey,
				value: new HostingContext() { IsLifetimeLinked = true });

			builder.ConfigureWinUI<App>();

			Hoster = builder.Build();

			Hoster.Run();
		}
	}
}

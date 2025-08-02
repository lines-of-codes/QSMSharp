using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QSM.Core.ModPluginSource;
using QSM.Core.ServerSettings;
using QSM.Web.Components;
using QSM.Web.Components.Account;
using QSM.Web.Data;
using System.Reflection;

namespace QSM.Web;

internal class Program
{
	public static WebApplication App { get; private set; } = null!;

	public static async Task Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		// Add services to the container.
		builder.Services.AddRazorComponents()
			.AddInteractiveServerComponents();

		builder.Services.AddCascadingAuthenticationState();
		builder.Services.AddScoped<IdentityUserAccessor>();
		builder.Services.AddScoped<IdentityRedirectManager>();
		builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

		builder.Services.AddAuthentication(options =>
			{
				options.DefaultScheme = IdentityConstants.ApplicationScheme;
				options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
			})
			.AddIdentityCookies();

		string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
		                          throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
		builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
		{
			options.UseSqlite(connectionString);
			options.ConfigureWarnings((config) => config.Log(RelationalEventId.PendingModelChangesWarning));
		});
		builder.Services.AddDatabaseDeveloperPageExceptionFilter();

		builder.Services.AddIdentityCore<ApplicationUser>()
			.AddRoles<IdentityRole>()
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddSignInManager()
			.AddDefaultTokenProviders();

		// HTTP Clients

		builder.Services.AddHttpClient(ModrinthProvider.HttpClientName, client =>
		{
			client.BaseAddress = new Uri(ModrinthProvider.BaseAddress);
			client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
				$"lines-of-codes/QSMSharp/{Assembly.GetEntryAssembly()!.GetName().Version}-Web (linesofcodes@dailitation.xyz)");
		});

		builder.Services.AddHttpClient(PaperMCHangarProvider.HttpClientName, client =>
		{
			client.BaseAddress = new Uri(PaperMCHangarProvider.BaseAddress);
		});

		builder.Services.AddTransient<ModrinthProvider>();
		builder.Services.AddTransient<PaperMCHangarProvider>();

		builder.Services.AddSingleton(ApplicationConfig.LoadConfig() ?? new ApplicationConfig());
		builder.Services.AddSingleton<ProcessManager>();

		App = builder.Build();

		using (IServiceScope scope = App.Services.CreateScope())
		{
			ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
			await ctx.Database.MigrateAsync();
		}

		// Configure the HTTP request pipeline.
		if (App.Environment.IsDevelopment())
		{
			App.UseMigrationsEndPoint();
		}
		else
		{
			App.UseExceptionHandler("/Error", true);
			// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
			App.UseHsts();
		}

		App.UseHttpsRedirection();


		App.UseAntiforgery();

		App.MapStaticAssets();
		App.MapRazorComponents<App>()
			.AddInteractiveServerRenderMode();

		// Add additional endpoints required by the Identity /Account Razor components.
		App.MapAdditionalIdentityEndpoints();

		await App.RunAsync();
	}
}
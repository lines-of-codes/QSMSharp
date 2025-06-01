using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QSM.Core.ModPluginSource;
using QSM.Web.Components;
using QSM.Web.Components.Account;
using QSM.Web.Data;
using System.Reflection;

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
	options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>()
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

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
	ApplicationDbContext ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	ctx.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Error", true);
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
	.AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
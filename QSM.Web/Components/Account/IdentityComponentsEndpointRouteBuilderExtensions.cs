using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QSM.Web.Components.Account.Pages.Manage;
using QSM.Web.Data;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;

namespace Microsoft.AspNetCore.Routing;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
	// These endpoints are required by the Identity Razor components defined in the /Components/Account/Pages directory of this project.
	public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
	{
		ArgumentNullException.ThrowIfNull(endpoints);

		RouteGroupBuilder accountGroup = endpoints.MapGroup("/Account");

		accountGroup.MapPost("/Logout", async (
			ClaimsPrincipal user,
			[FromServices] SignInManager<ApplicationUser> signInManager,
			[FromForm] string returnUrl) =>
		{
			await signInManager.SignOutAsync();
			return TypedResults.LocalRedirect($"~/{returnUrl}");
		});

		RouteGroupBuilder manageGroup = accountGroup.MapGroup("/Manage").RequireAuthorization();

		manageGroup.MapPost("/LinkExternalLogin", async (
			HttpContext context,
			[FromServices] SignInManager<ApplicationUser> signInManager,
			[FromForm] string provider) =>
		{
			// Clear the existing external cookie to ensure a clean login process
			await context.SignOutAsync(IdentityConstants.ExternalScheme);

			string redirectUrl = UriHelper.BuildRelative(
				context.Request.PathBase,
				"/Account/Manage/ExternalLogins",
				QueryString.Create("Action", ExternalLogins.LinkLoginCallbackAction));

			AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(provider,
				redirectUrl,
				signInManager.UserManager.GetUserId(context.User));
			return TypedResults.Challenge(properties, [provider]);
		});

		ILoggerFactory loggerFactory = endpoints.ServiceProvider.GetRequiredService<ILoggerFactory>();
		ILogger downloadLogger = loggerFactory.CreateLogger("DownloadPersonalData");

		manageGroup.MapPost("/DownloadPersonalData", async (
			HttpContext context,
			[FromServices] UserManager<ApplicationUser> userManager,
			[FromServices] AuthenticationStateProvider authenticationStateProvider) =>
		{
			ApplicationUser? user = await userManager.GetUserAsync(context.User);
			if (user is null)
			{
				return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
			}

			string userId = await userManager.GetUserIdAsync(user);
			downloadLogger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

			// Only include personal data for download
			Dictionary<string, string> personalData = new();
			IEnumerable<PropertyInfo> personalDataProps = typeof(ApplicationUser).GetProperties()
				.Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
			foreach (PropertyInfo p in personalDataProps)
			{
				personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
			}

			IList<UserLoginInfo> logins = await userManager.GetLoginsAsync(user);
			foreach (UserLoginInfo l in logins)
			{
				personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
			}

			personalData.Add("Authenticator Key", (await userManager.GetAuthenticatorKeyAsync(user))!);
			byte[] fileBytes = JsonSerializer.SerializeToUtf8Bytes(personalData);

			context.Response.Headers.TryAdd("Content-Disposition", "attachment; filename=PersonalData.json");
			return TypedResults.File(fileBytes, "application/json", "PersonalData.json");
		});

		return accountGroup;
	}
}
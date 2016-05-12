using Owin;
using System.Configuration;
using System.Globalization;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System.Web.Helpers;
using System.Security.Claims;

namespace JsPlc.Ssc.PetrolPricing.Portal
{
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        public static readonly string Authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

			app.UseKentorOwinCookieSaver();

            var timeout = int.Parse(ConfigurationManager.AppSettings["SessionTimeout"]);

			app.UseCookieAuthentication(
				new CookieAuthenticationOptions
				{
					AuthenticationType = CookieAuthenticationDefaults.AuthenticationType,
					CookieSecure = CookieSecureOption.SameAsRequest,
                    ExpireTimeSpan = System.TimeSpan.FromMinutes(timeout),
                    SlidingExpiration= true,
					Provider = new CookieAuthenticationProvider
					{
						OnResponseSignedIn = (context) =>
						{
							if (context.Identity.IsAuthenticated)
							{
								var facade = new ServiceFacade();
								var array = context.Identity.Name.Split(new[] { '#' });
								var userName = string.Empty;
								if (array == null || array.Length == 0)
								{
									userName = context.Identity.Name;
								}
								else
								{
									if (array.Length == 1)
									{
										userName = context.Identity.Name.Split(new[] { '#' })[0];
									}
									else
									{
										userName = context.Identity.Name.Split(new[] { '#' })[1];
									}
								}
								facade.RegisterUser(userName);
							}
						}
					}
				});

			app.UseOpenIdConnectAuthentication(
				new OpenIdConnectAuthenticationOptions
				{
					SignInAsAuthenticationType = OpenIdConnectAuthenticationDefaults.AuthenticationType,
					ClientId = clientId,
					Authority = Authority,
					PostLogoutRedirectUri = postLogoutRedirectUri,
					RedirectUri = postLogoutRedirectUri,                 
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,                       
                    }
				});

			AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;
        }
    }
}

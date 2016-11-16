using Owin;
using System.Configuration;
using System.Globalization;
using Microsoft.Owin.Security.OpenIdConnect;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using JsPlc.Ssc.PetrolPricing.Portal.Facade;
using System.Web.Helpers;
using System.Security.Claims;
using Microsoft.AspNet.Identity.Owin;
using System.Linq;
using JsPlc.Ssc.PetrolPricing.Portal.Models;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using Microsoft.Owin.Security.Notifications;
using JsPlc.Ssc.PetrolPricing.Portal.Helper.Extensions;
using System.Web;
using System.Web.Security;
using JsPlc.Ssc.PetrolPricing.Core;


namespace JsPlc.Ssc.PetrolPricing.Portal
{


    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        public static readonly string Authority = string.Format(CultureInfo.InvariantCulture, aadInstance, tenant);
        private static long loginTimeTicks;

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
                    ExpireTimeSpan = new TimeSpan(0, timeout, 0),
                    SlidingExpiration = false,
                    Provider = new CookieAuthenticationProvider
                    {

                        OnResponseSignedIn = (context) =>
                        {
                            if (context.Identity.IsAuthenticated)
                            {
                                var facade = new ServiceFacade(new PetrolPricingLogger());
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
                    UseTokenLifetime = false,
                    BackchannelTimeout = new TimeSpan(0, timeout, 0),
                    RefreshOnIssuerKeyNotFound = false,

                    Notifications = new OpenIdConnectAuthenticationNotifications()
                    {
                        RedirectToIdentityProvider = (context) =>
                        {
                            loginTimeTicks = DateTime.Now.Ticks;
                            var user = context.OwinContext.Authentication.User;

                            return Task.FromResult(0);

                        },
                        MessageReceived = (context) =>
                        {
                            long MessageReceivedTimeTicks = DateTime.Now.Ticks;
                            long inSec = (MessageReceivedTimeTicks - loginTimeTicks) / (60 * 60 * 60);

                            if (inSec > 50) //when user enter input
                            {
                                UiHelper.CreateAuthCookie1();
                                UiHelper.CreateAuthCookie2();
                            }
                            UiHelper.bIsFirstStartupAuthCalled = true;
                            return Task.FromResult(0);
                        },
                        AuthorizationCodeReceived = (context) =>
                        {
                            var code = context.Code;
                            JwtSecurityToken tokenold = context.JwtSecurityToken;

                            context.AuthenticationTicket.Properties.IssuedUtc = DateTime.Now;
                            context.AuthenticationTicket.Properties.ExpiresUtc = DateTime.Now.AddMinutes(timeout);

                            var lst = tokenold.Audiences;
                            context.JwtSecurityToken = new JwtSecurityToken(tokenold.Issuer,
                               lst.FirstOrDefault(),
                               tokenold.Claims,
                               DateTime.Now,
                               DateTime.Now.AddMinutes(timeout),
                               tokenold.SigningCredentials);
                            return Task.FromResult(0);
                        }
                    },
                    TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        RequireExpirationTime = true,
                        ValidateLifetime = false
                    }

                });



            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;


           
          
            
        }
    }
}

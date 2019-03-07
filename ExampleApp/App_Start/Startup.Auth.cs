using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;

using IntelligentPlant.Authentication;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;

using Owin;

namespace ExampleApp {
    /// <summary>
    /// OWIN startup class.
    /// </summary>
    public partial class Startup {

        /// <summary>
        /// Base URL for the App Store.
        /// </summary>
        internal static readonly string AppStoreBaseUrl = ConfigurationManager.AppSettings["appStore:baseUrl"];

        /// <summary>
        /// Holds the session NBF (not before) date i.e. the minimum allowed issue time for our application session 
        /// cookies.
        /// </summary>
        private static DateTimeOffset _sessionNbf;


        /// <summary>
        /// Configures App Store authentication for the application.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        private void ConfigureAppStoreAuthentication(IAppBuilder app) {
            ConfigureSessionNbf();

            s_log.Debug("Configuring OAuth token store.");
            // TODO: *** create an IOAuthTokenStore implementation that persists tokens ***
            app.CreatePerOwinContext<IOAuthTokenStore>(() => new InMemoryOAuthTokenStore());

            s_log.Debug("Configuring application cookie authentication.");
            // Allow authentication via an application cookie that gets set when the user authenticates with the App 
            // Store using OAuth.
            app.UseCookieAuthentication(new CookieAuthenticationOptions {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieName = ".ExampleApp.Session",
                // Logins handled by login provider middleware configured below.
                LoginPath = new PathString(LoginProviderOptions.DefaultLoginPath),
                // Logouts handled by login provider middleware configured below.
                LogoutPath = new PathString(LoginProviderOptions.DefaultLogoutPath),
                // When we receive a valid session cookie, we'll make sure that it 
                // was issued after the session NBF (not before) date currently 
                // configured for the application.
                Provider = new CookieAuthenticationProvider() {
                    OnValidateIdentity = context => {
                        if (context.Properties.IssuedUtc.HasValue && context.Properties.IssuedUtc < _sessionNbf) {
                            context.RejectIdentity();
                            context.OwinContext.ExpireSession();
                        }
                        return Task.FromResult(0);
                    }
                }
            });

            s_log.Debug("Configuring external cookie authentication.");
            // Allow temporary storage of information received from an external service (e.g. App Store) in a cookie.
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            s_log.Debug("Configuring App Store authentication.");
            // App Store authentication options.
            var appStoreAuthenticationOptions = new AppStoreAuthenticationOptions() {
                BaseUrl = AppStoreBaseUrl,
                ClientId = ConfigurationManager.AppSettings["appStore:clientId"],
                ClientSecret = ConfigurationManager.AppSettings["appStore:clientSecret"],
                Scope = new List<string>() {
                    "UserInfo", // Request access to App Store user profile.
                    "DataRead", // Request access to App Store Connect data; user can control which sources the application can access.
                    "AccountDebit" // Request ability to bill for usage.
                },
                Prompt = "consent", // Always display the consent screen; remove this property to enable automatic re-consent.
                Provider = new AppStoreAuthenticationProvider() {
                    // Persist the token when the user logs in.
                    OnAuthenticated = context => context.OwinContext.Get<IOAuthTokenStore>().SetTokenDetails(context.Id, context.AccessTokenDetails, context.OwinContext.Request.CallCancelled)
                }
            };
            app.UseAppStoreAuthentication(appStoreAuthenticationOptions);

            s_log.Debug("Configuring login provider.");
            // We are not using ASP.NET Identity user management, so use the login provider middleware to manage 
            // login and logout requests for us.
            app.UseLoginProvider(new LoginProviderOptions() {
                ChallengeAuthenticationType = AppStoreAuthenticationOptions.DefaultAuthenticationType,
                SignInAsAuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                OnBeforeSignIn = async context => {
                    var token = await context.OwinContext
                                             .Get<IOAuthTokenStore>()
                                             .GetTokenDetails(context.Identity.GetUserId(), context.OwinContext.Request.CallCancelled)
                                             .ConfigureAwait(false);

                    if (token?.UtcAccessTokenExpiryTime != null) {
                        context.Properties.ExpiresUtc = token.UtcAccessTokenExpiryTime.Value;
                        context.Properties.IsPersistent = true;
                    }
                    else {
                        context.Properties.IsPersistent = false;
                    }
                }
            });

            s_log.Debug("Configuring Data Core client to use OAuth authentication.");
            // Middleware that will run after authentication has finished and will retrieve the OAuth token for the 
            // caller and set an appropriate Authenticate delegate on the HttpConnectionSettings for the caller.
            app.UseOAuthTokenWithDataCore();

            app.UseOAuthTokenWithAppStore();
        }


        /// <summary>
        /// Configures the session NBF (not before) value.
        /// </summary>
        private static void ConfigureSessionNbf() {
            var nbf = ConfigurationManager.AppSettings["application:sessionNbf"];
            DateTimeOffset val;
            if (String.IsNullOrWhiteSpace(nbf) || !DateTimeOffset.TryParse(nbf, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out val)) {
                val = DateTimeOffset.MinValue;
            }

            _sessionNbf = val;
        }

    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;

using IntelligentPlant.Authentication;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;

using Owin;

namespace ExampleApp {
    /// <summary>
    /// OWIN-related extension methods.
    /// </summary>
    internal static class OwinExtensions {

        /// <summary>
        /// Data Core endpoint URL.
        /// </summary>
        private static readonly Uri DataCoreEndpoint = new Uri(ConfigurationManager.AppSettings["dataCore:endpoint"]);

        /// <summary>
        /// App Store endpoint URL.
        /// </summary>
        private static readonly Uri AppStoreEndpoint = new Uri(ConfigurationManager.AppSettings["appStore:baseUrl"]);

        /// <summary>
        /// Path to the session expired URL.
        /// </summary>
        private static readonly PathString SessionExpiredPath = new PathString("/account/sessionexpired");

        /// <summary>
        /// Adds an OWIN middleware that sets the Data Core connection settings in each request.  Use 
        /// <see cref="GetDataCoreConnectionSettings"/> to retrieve the settings from the OWIN context.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        /// <param name="credentials">The network credentials to use, or <see langword="null"/> to use the App Pool identity.</param>
        /// <returns>
        /// The OWIN application, to allow chaining.
        /// </returns>
        public static IAppBuilder UseDataCoreConnectionSettings(this IAppBuilder app, ICredentials credentials = null) {
            app.Use((context, next) => {
                context.Set("dataCore:connectionSettings", new DataCore.Client.HttpConnectionSettings(DataCoreEndpoint, credentials));
                return next();
            });

            return app;
        }

        /// <summary>
        /// Adds an OWIN middleware that sets the AppStore connection settings in each request.  Use 
        /// <see cref="GetDataCoreConnectionSettings"/> to retrieve the settings from the OWIN context.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        /// <param name="credentials">The network credentials to use, or <see langword="null"/> to use the App Pool identity.</param>
        /// <returns>
        /// The OWIN application, to allow chaining.
        /// </returns>
        public static IAppBuilder UseAppStoreConnectionSettings(this IAppBuilder app, ICredentials credentials = null) {
            app.Use((context, next) => {
                context.Set("appstore:connectionSettings", new DataCore.Client.HttpConnectionSettings(AppStoreEndpoint, credentials));
                return next();
            });

            return app;
        }

        /// <summary>
        /// Adds an OWIN middleware that updates the Data Core connection settings for an OWIN request to use the 
        /// OAuth token for the calling user for authentication.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        /// <returns>
        /// The OWIN application, to allow chaining.
        /// </returns>
        public static IAppBuilder UseOAuthTokenWithDataCore(this IAppBuilder app) {
            app.Use(async (context, next) => {
                var isAuthenticated = context.Authentication.User?.Identity.IsAuthenticated ?? false;
                if (isAuthenticated) {
                    var tokenStore = context.Get<IOAuthTokenStore>();
                    var dataCoreConnectionSettings = context.GetDataCoreConnectionSettings();
                    var userId = context.Authentication.User.Identity.GetUserId();

                    // If we can't get an OAuth token for the caller from the store, we need them to 
                    // sign in again so that we can get a new access token.
                    var token = await tokenStore.GetTokenDetails(userId, context.Request.CallCancelled).ConfigureAwait(false);
                    if (token == null) {
                        context.ExpireSession();
                        return;
                    }

                    dataCoreConnectionSettings.Authenticate = async ct => {
                        var accessToken = await tokenStore.GetTokenDetails(userId, ct).ConfigureAwait(false);
                        return accessToken == null
                                   ? null
                                   : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
                    };
                }
                await next().ConfigureAwait(false);
            });
            app.UseStageMarker(PipelineStage.PostAuthenticate);

            return app;
        }

        /// <summary>
        /// Adds an OWIN middleware that updates App Store connection settings for an OWIN request to use the 
        /// OAuth token for the calling user for authentication.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        /// <returns>
        /// The OWIN application, to allow chaining.
        /// </returns>
        public static IAppBuilder UseOAuthTokenWithAppStore(this IAppBuilder app) {
            app.Use(async (context, next) => {
                var isAuthenticated = context.Authentication.User?.Identity.IsAuthenticated ?? false;
                if (isAuthenticated) {
                    var tokenStore = context.Get<IOAuthTokenStore>();
                    var appstoreConnectionSettings = context.GetAppStoreConnectionSettings();
                    var userId = context.Authentication.User.Identity.GetUserId();

                    // If we can't get an OAuth token for the caller from the store, we need them to sign in again.
                    var token = await tokenStore.GetTokenDetails(userId, context.Request.CallCancelled).ConfigureAwait(false);
                    if (token == null) {
                        // Remove the session cookie.
                        context.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        // Issue a new challenge.
                        context.Authentication.Challenge(AppStoreAuthenticationOptions.DefaultAuthenticationType);
                        return;
                    }

                    appstoreConnectionSettings.Authenticate = async ct => {
                        var accessToken = await tokenStore.GetTokenDetails(userId, ct).ConfigureAwait(false);
                        return accessToken == null
                                   ? null
                                   : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken.AccessToken);
                    };
                }
                await next().ConfigureAwait(false);
            });
            app.UseStageMarker(PipelineStage.PostAuthenticate);

            return app;
        }

        /// <summary>
        /// Gets the Data Core connection settings object from the OWIN environment.
        /// </summary>
        /// <param name="context">The OWIN context for the request.</param>
        /// <returns>
        /// The Data Core connection settings to use.
        /// </returns>
        public static DataCore.Client.HttpConnectionSettings GetDataCoreConnectionSettings(this IOwinContext context) {
            return context.Get<DataCore.Client.HttpConnectionSettings>("dataCore:connectionSettings");
        }

        /// <summary>
        /// Gets the Data Core connection settings object from the OWIN environment.
        /// </summary>
        /// <param name="owinEnvironment">The OWIN environment for the request.</param>
        /// <returns>
        /// The Data Core connection settings to use.
        /// </returns>
        public static DataCore.Client.HttpConnectionSettings GetDataCoreConnectionSettings(IDictionary<string, object> owinEnvironment) {
            object settings;
            if (owinEnvironment == null || !owinEnvironment.TryGetValue("dataCore:connectionSettings", out settings)) {
                return null;
            }

            return settings as DataCore.Client.HttpConnectionSettings;
        }

        /// <summary>
        /// Gets the AppStore connection settings object from the OWIN environment.
        /// </summary>
        /// <param name="context">The OWIN context for the request.</param>
        /// <returns>
        /// The Data Core connection settings to use.
        /// </returns>
        public static DataCore.Client.HttpConnectionSettings GetAppStoreConnectionSettings(this IOwinContext context) {
            return context.Get<DataCore.Client.HttpConnectionSettings>("appstore:connectionSettings");
        }

        /// <summary>
        /// Gets the AppStore connection settings object from the OWIN environment.
        /// </summary>
        /// <param name="context">The OWIN context for the request.</param>
        /// <returns>
        /// The Data Core connection settings to use.
        /// </returns>
        public static DataCore.Client.HttpConnectionSettings GetAppStoreConnectionSettings(IDictionary<string, object> owinEnvironment) {
            object settings;
            if (owinEnvironment == null || !owinEnvironment.TryGetValue("appstore:connectionSettings", out settings)) {
                return null;
            }

            return settings as DataCore.Client.HttpConnectionSettings;
        }

        /// <summary>
        /// Expires the session cookie associated with the current OWIN context and redirects the user agent to the Session Expired page.
        /// </summary>
        /// <param name="context">The OWIN context.</param>
        internal static void ExpireSession(this IOwinContext context) {
            if (context == null) {
                return;
            }

            // Remove the session cookie.
            context.Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            // Redirect to session expired URL.
            var baseUri = String.Concat(context.Request.Scheme, Uri.SchemeDelimiter, context.Request.Host, context.Request.PathBase, SessionExpiredPath);
            context.Response.Redirect(baseUri); // 302
        }

    }
}
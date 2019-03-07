using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using IntelligentPlant.Authentication;

namespace ExampleApp {
    /// <summary>
    /// <see cref="IOAuthTokenStore"/> that stores tokens in memory.
    /// </summary>
    internal class InMemoryOAuthTokenStore : IOAuthTokenStore {

        /// <summary>
        /// Holds tokens.
        /// </summary>
        private static readonly ConcurrentDictionary<string, AccessTokenDetails> Tokens = new ConcurrentDictionary<string, AccessTokenDetails>();

        /// <summary>
        /// HTTP client for refreshing expired tokens.
        /// </summary>
        private static readonly System.Net.Http.HttpClient BackchannelClient = new System.Net.Http.HttpClient();


        /// <summary>
        /// Gets the App Store OAuth token for the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// A task that will either return the token, or return <see langword="null"/> if no valid token was found.
        /// </returns>
        public async Task<AccessTokenDetails> GetTokenDetails(string userId, CancellationToken cancellationToken) {
            AccessTokenDetails result = null;
            if (!String.IsNullOrWhiteSpace(userId)) {
                Tokens.TryGetValue(userId, out result);
            }

            if (result != null && result.UtcAccessTokenExpiryTime.HasValue && result.UtcAccessTokenExpiryTime.Value <= DateTime.UtcNow) {
                if (String.IsNullOrWhiteSpace(result.RefreshToken)) {
                    // The token has expired and we can't refresh it.
                    return null;
                }
                await result.Refresh(BackchannelClient, new Uri(ConfigurationManager.AppSettings["appStore:baseUrl"]), ConfigurationManager.AppSettings["appStore:clientId"], ConfigurationManager.AppSettings["appStore:clientSecret"], cancellationToken).ConfigureAwait(false);
            }

            return result;
        }


        /// <summary>
        /// Stores the App Store OAuth token for the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// A task that will store the token.
        /// </returns>
        public Task SetTokenDetails(string userId, AccessTokenDetails token, CancellationToken cancellationToken) {
            if (!String.IsNullOrWhiteSpace(userId) && token != null) {
                Tokens[userId] = token;
            }

            return Task.FromResult(0);
        }


        /// <summary>
        /// Releases managed resources.
        /// </summary>
        public void Dispose() {
            // Do nothing - we hold tokens in memory until the application shuts down.
        }

    }
}
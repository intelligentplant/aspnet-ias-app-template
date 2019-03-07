using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using IntelligentPlant.Authentication;

namespace ExampleApp {
    /// <summary>
    /// Defines functions that can be used to store and retrieve App Store OAuth access tokens.
    /// </summary>
    /// <remarks>
    /// An example implementation is provided in the <see cref="MemoryOAuthTokenStore"/> class.
    /// </remarks>
    internal interface IOAuthTokenStore : IDisposable {

        /// <summary>
        /// When implemented in a derived type, saves the access token for the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// A task that will save the token details.
        /// </returns>
        Task SetTokenDetails(string userId, AccessTokenDetails token, CancellationToken cancellationToken);

        /// <summary>
        /// When implemented in a derived type, gets the access token for the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// A task that will return the token details.
        /// </returns>
        Task<AccessTokenDetails> GetTokenDetails(string userId, CancellationToken cancellationToken);

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleApp.Http {

    /// <summary>
    /// Delegating handler for compressing Web API responses.
    /// </summary>
    /// <remarks>
    /// When a <see cref="ContentEncodingHandler"/> is registered, Web API responses will always be compressed 
    /// if they define an <c>Accept-Encoding</c> header in the request with a value of <c>gzip</c> or 
    /// <c>deflate</c>.  This overrides any behaviour defined by IIS' dynamic compression module.
    /// </remarks>
    /// <example>
    /// 
    /// <para>
    /// The following example shows how to register an <see cref="ContentEncodingHandler"/> with Web API:
    /// </para>
    /// 
    /// <code lang="C#">
    /// public static class WebApiConfig {
    ///     
    ///     public static void Register(HttpConfiguration httpConfig) {
    ///         config.MessageHandlers.Add(new ContentEncodingHandler());
    /// 
    ///         // Register media formatters etc. here...
    ///     }
    /// 
    /// }
    /// </code>
    /// 
    /// </example>
    public sealed class ContentEncodingHandler : DelegatingHandler {

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return base.SendAsync(request, cancellationToken)
                       .ContinueWith(t => {
                           var response = t.Result;

                           if (response.Content != null && request.Headers.AcceptEncoding != null && request.Headers.AcceptEncoding.Any()) {
                               foreach (var encoding in request.Headers.AcceptEncoding) {
                                   if (!CompressedHttpContent.IsContentTypeSupported(encoding.Value)) {
                                       continue;
                                   }

                                   response.Content = new CompressedHttpContent(response.Content, encoding.Value);
                                   break;
                               }
                           }

                           return response;
                       }, cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        }

    }

}

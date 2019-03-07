using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using Common.Logging;
using AppStore.Client;

namespace ExampleApp.Controllers {
    /// <summary>
    /// Web API controller that will query Data Core to get the available data sources.
    /// </summary>
    [RoutePrefix("api/data")]
    [Authorize]
    public class DataApiController : ApiController {

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger<DataApiController>();


        /// <summary>
        /// Gets the available data sources from Data Core.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// The content of a successful request will be an object describing the caller and the data sources that the 
        /// user has authorized the application to access.
        /// </returns>
        [HttpGet]
        [Route("datasources")]
        [ResponseType(typeof(ExampleApp.Models.DataSourcesViewModel))]
        public async Task<IHttpActionResult> GetDataSources(CancellationToken cancellationToken) {
            try {
                var owinContext = Request.GetOwinContext();
                var dataCoreConnectionSettings = owinContext.GetDataCoreConnectionSettings();
                var dataCoreClient = new DataCore.Client.DataCoreHttpClient(dataCoreConnectionSettings, new LogManager());

                var dataSources = await dataCoreClient.GetDataSourcesAsync(cancellationToken).ConfigureAwait(false);
                if (cancellationToken.IsCancellationRequested) {
                    return StatusCode(System.Net.HttpStatusCode.NoContent); // 204
                }

                var result = new ExampleApp.Models.DataSourcesViewModel() {
                    DataSources = dataSources.OrderBy(x => x.Name.DisplayName).ToArray()
                };
                return Ok(result); // 200
            }
            catch (Exception e) {
                s_log.Error($"An error occurred in {nameof(DataApiController)}.{nameof(GetDataSources)}.", e);
                throw; // 500
            }
        }


        /// <summary>
        /// Gets snapshot values for the specified tags.
        /// </summary>
        /// <param name="dataSourceName">The data source name.</param>
        /// <param name="tag">The tags.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// A partial view that contains the tag values.
        /// </returns>
        [HttpGet]
        [Route("values/{dataSourceName}/snapshot")]
        [ResponseType(typeof(ExampleApp.Models.TagSnapshotValuesViewModel))]
        public async Task<IHttpActionResult> GetSnapshotTagValues(string dataSourceName, [FromUri] string[] tag, CancellationToken cancellationToken) {
            if (tag == null || !tag.Any()) {
                return BadRequest("You must specify at least one tag to query.");
            }

            try {
                var owinContext = Request.GetOwinContext();
                var dataCoreConnectionSettings = owinContext.GetDataCoreConnectionSettings();
                var dataCoreClient = new DataCore.Client.DataCoreHttpClient(dataCoreConnectionSettings, new LogManager());

                var values = await dataCoreClient.GetSnapshotTagValuesAsync(dataSourceName, tag, cancellationToken).ConfigureAwait(false);
                var result = new ExampleApp.Models.TagSnapshotValuesViewModel() {
                    Values = new Dictionary<string, DataCore.Client.Model.SnapshotTagValueDictionary>() {
                        { dataSourceName, values }
                    }
                };
                return Ok(result); // 200
            }
            catch (Exception e) {
                s_log.Error($"An error occurred in {nameof(DataApiController)}.{nameof(GetSnapshotTagValues)}.", e);
                throw; // 500
            }
        }


        /// <summary>
        /// Performs a test purchase action. Requires the <c>AccountDebit</c> scope to be requested 
        /// when signing the user into the app (see <see cref="Startup.ConfigureAppStoreAuthentication(Owin.IAppBuilder)"/>).
        /// See the remarks about this method for further information.
        /// </summary>
        /// <param name="cancellationToken">
        ///   The cancellation token for the operation.
        /// </param>
        /// <returns>
        ///   Successful responses contain no content.
        /// </returns>
        /// <remarks>
        /// 
        /// IMPORTANT: calling this action will debit the calling user unless the status of the app 
        /// on the App Store is set to "pending". This can be done by visiting 
        /// https://appstore.intelligentplant.com/Developer/Home, and opening the settings for your 
        /// app registration.
        /// 
        /// </remarks>
        [HttpPost]
        [Route("buy")]
        public async Task<IHttpActionResult> Purchase(CancellationToken cancellationToken) {
            try {
                var owinContext = Request.GetOwinContext();
                var appstoreConnectionSettings = owinContext.GetAppStoreConnectionSettings();
                var _appStoreClient = new AppStoreHttpClient(appstoreConnectionSettings);

                var _result = await _appStoreClient.DebitUserAsync(1, cancellationToken).ConfigureAwait(false);

                if (_result.Success) {
                    s_log.Info($"Transaction successful");
                    return StatusCode(System.Net.HttpStatusCode.NoContent); // 204
                }

                s_log.Info($"Transaction unsuccessful: {_result.Message}");

                return BadRequest();
            }
            catch (Exception e) {
                s_log.Error($"An error occurred in {nameof(DataApiController)}.{nameof(Purchase)}.", e);
                throw; // 500
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using Common.Logging;

using Microsoft.AspNet.Identity;

namespace ExampleApp.Controllers {

    /// <summary>
    /// MVC controller that displays the App Store data viewer.
    /// </summary>
    [RoutePrefix("dataviewer")]
    [Authorize]
    public class DataViewerController : Controller {

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger<DataViewerController>();


        /// <summary>
        /// Displays the main data viewer view.
        /// </summary>
        /// <returns>
        /// The main data viewer view.
        /// </returns>
        [HttpGet]
        [Route("")]
        public ActionResult Index() {
            try {
                return View(); // 200
            }
            catch (Exception e) {
                s_log.Error($"An error occurred in {nameof(DataViewerController)}.{nameof(Index)}.", e);
                throw; // 500
            }
        }


        /// <summary>
        /// Gets a page of tags from the specified data source.
        /// </summary>
        /// <param name="dataSourceName">The data source name.</param>
        /// <param name="name">The tag name filter to apply.</param>
        /// <param name="page">The page of tags to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token for the request.</param>
        /// <returns>
        /// A partial view describing the tags.
        /// </returns>
        [HttpPost]
        [Route("tags")]
        public async Task<ActionResult> GetTags(string dataSourceName, string name, int page, CancellationToken cancellationToken) {
            try {
                var owinContext = Request.GetOwinContext();
                var dataCoreConnectionSettings = owinContext.GetDataCoreConnectionSettings();
                var dataCoreClient = new DataCore.Client.DataCoreHttpClient(dataCoreConnectionSettings, new LogManager());

                var nameFilter = String.IsNullOrWhiteSpace(name)
                    ? "*"
                    : name;

                if (!nameFilter.StartsWith("*")) {
                    nameFilter = "*" + nameFilter;
                }

                if (!nameFilter.EndsWith("*")) {
                    nameFilter = nameFilter + "*";
                }

                var filter = new DataCore.Client.Model.TagSearchFilter(nameFilter) {
                    PageSize = 5,
                    Page = page < 1 ? 1 : page
                };
                var tags = await dataCoreClient.GetTagsAsync(dataSourceName, filter, cancellationToken).ConfigureAwait(false);

                return PartialView("_TagSearchResultPartial", new Models.TagSearchResultViewModel() { DataSourceName = dataSourceName, Filter = filter, Tags = tags }); // 200
            }
            catch (Exception e) {
                s_log.Error($"An error occurred in {nameof(DataViewerController)}.{nameof(GetTags)}.", e);
                throw; // 500
            }
        }

    }

}

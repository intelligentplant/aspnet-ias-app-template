using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Common.Logging;

namespace ExampleApp.Filters {
    /// <summary>
    /// MVC error handler that writes errors to the application log.
    /// </summary>
    internal class MvcErrorHandlerAttribute : HandleErrorAttribute {

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger<MvcErrorHandlerAttribute>();


        /// <summary>
        /// Handles MVC exceptions.
        /// </summary>
        /// <param name="filterContext">The exception context.</param>
        public override void OnException(ExceptionContext filterContext) {
            if (filterContext != null && !filterContext.ExceptionHandled) {
                var controllerName = (string) filterContext.RouteData.Values["controller"];
                var actionName = (string) filterContext.RouteData.Values["action"];

                s_log.Error($"An unhandled error occurred while processing action {actionName} on controller {controllerName}.", filterContext.Exception);
            }
            base.OnException(filterContext);
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

using Common.Logging;

namespace ExampleApp {

    /// <summary>
    /// Configures ASP.NET MVC.
    /// </summary>
    public class MvcConfig {

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger<MvcConfig>();


        /// <summary>
        /// Configures ASP.NET MVC.
        /// </summary>
        public static void Configure() {
            s_log.Debug("Registering MVC areas.");
            AreaRegistration.RegisterAllAreas();

            s_log.Debug("Registering global MVC filters.");
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            s_log.Debug("Registering MVC routes.");
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            s_log.Debug("Registering script and CSS bundles.");
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

    }

}

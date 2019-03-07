using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ExampleApp {

    /// <summary>
    /// Configures application routing.
    /// </summary>
    public class RouteConfig {

        /// <summary>
        /// Registers application routes.
        /// </summary>
        /// <param name="routes">The application route collection.</param>
        public static void RegisterRoutes(RouteCollection routes) {
            // Leave AXD routes alone.
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // Initialize MVC attribute-based routing.
            routes.MapMvcAttributeRoutes();

            // Initialize default MVC routing.
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }

}

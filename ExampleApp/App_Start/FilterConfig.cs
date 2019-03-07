using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ExampleApp {

    /// <summary>
    /// Registers MVC filters.
    /// </summary>
    public class FilterConfig {

        /// <summary>
        /// Registers global MVC filters.
        /// </summary>
        /// <param name="filters">The filter collection to add global filters to.</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new ExampleApp.Filters.MvcErrorHandlerAttribute());
        }

    }

}

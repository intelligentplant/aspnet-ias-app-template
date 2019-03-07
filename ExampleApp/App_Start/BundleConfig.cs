using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Optimization;

namespace ExampleApp {

    /// <summary>
    /// Configures script and CSS bundles.
    /// </summary>
    public class BundleConfig {

        /// <summary>
        /// Registers script and CSS bundles.
        /// </summary>
        /// <param name="bundles">The bundle collection to add script and CSS bundles to.</param>
        public static void RegisterBundles(BundleCollection bundles) {
            // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                            "~/Scripts/jquery-{version}.js",
                            "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      // To apply your own Bootstrap theme, remove the "*-ip.css" files from the bundle 
                      // and add your own boostrap.css and bootstrap-theme.css files to the bundle instead.
                      "~/Content/bootstrap-ip.css",
                      "~/Content/bootstrap-theme-ip.css",
                      "~/Content/bootstrap-custom-ip.css",
                      //"~/Content/bootstrap.css",
                      //"~/Content/bootstrap-theme.css",
                      "~/Content/font-awesome.css",
                      "~/Content/Site.css"));
        }

    }
}

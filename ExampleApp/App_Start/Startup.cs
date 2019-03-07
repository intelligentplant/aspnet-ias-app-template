using System;
using System.Configuration;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

using Common.Logging;

using Microsoft.Owin;
using Microsoft.Owin.Logging;

using Owin;

[assembly: OwinStartup(typeof(ExampleApp.Startup))]

namespace ExampleApp {
    /// <summary>
    /// OWIN startup class.
    /// </summary>
    public partial class Startup {

        /// <summary>
        /// Logging.
        /// </summary>
        private static readonly ILog s_log = LogManager.GetLogger<Startup>();


        /// <summary>
        /// Configures the OWIN application.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        public void Configuration(IAppBuilder app) {
            var asm = Assembly.GetExecutingAssembly();
            s_log.Info($"{Environment.NewLine}{Environment.NewLine}[{asm.GetCustomAttribute<AssemblyProductAttribute>().Product} {asm.GetName().Version}]{Environment.NewLine}{asm.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright}{Environment.NewLine}");
            s_log.Info("Starting application.");

            // Configure OWIN to use Common.Logging.
            app.SetLoggerFactory(new ExampleApp.Logging.CommonOwinLoggerFactory());

            s_log.Info("Configuring Data Core connection settings.");

            // Middleware that will add Data Core connection settings to the OWIN environment for each call.
            app.UseDataCoreConnectionSettings();

            app.UseAppStoreConnectionSettings();

            s_log.Info("Configuring authentication.");
            ConfigureAppStoreAuthentication(app);

            s_log.Info("Configuring ASP.NET MVC.");
            MvcConfig.Configure();

            s_log.Info("Configuring Web API.");
            WebApiConfig.Configure(app);

            s_log.Info("Configuring SignalR");
            SignalRConfig.Configure(app);
        }

    }
}

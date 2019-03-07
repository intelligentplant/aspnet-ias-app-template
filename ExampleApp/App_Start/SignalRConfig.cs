using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

using Microsoft.AspNet.SignalR;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Owin;

namespace ExampleApp {
    /// <summary>
    /// Configures SignalR.
    /// </summary>
    public class SignalRConfig {

        /// <summary>
        /// Registers SignalR with the specified OWIN application.
        /// </summary>
        /// <param name="app">The OWIN application.</param>
        public static void Configure(IAppBuilder app) {
            // If we want to apply custom JSON.NET settings, we have to configure them here and then add a dependency 
            // resolver for the JSON.NET serializer type.
            var serializer = new JsonSerializer() {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };
            // Serialize enum values as strings.
            serializer.Converters.Add(new StringEnumConverter());

            // Register the serializer with the SignalR dependency resolver.
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

            // Initialize SignalR.
            app.MapSignalR();
        }

    }
}
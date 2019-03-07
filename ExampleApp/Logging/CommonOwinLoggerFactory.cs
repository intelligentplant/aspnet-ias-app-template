using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Microsoft.Owin.Logging;

namespace ExampleApp.Logging {

    /// <summary>
    /// OWIN <see cref="ILoggerFactory"/> that supplies <see cref="CommonOwinLogger"/> instances to OWIN 
    /// components for logging purposes.
    /// </summary>
    /// <example>
    /// To use, add the following code to your OWIN configuration method:
    /// 
    /// <code language="C#" title="ILoggerFactory Configuration">
    /// public void Configuration(IAppBuilder app) {
    ///     app.SetLoggerFactory(new CommonOwinLoggerFactory());
    ///     
    ///     // Other configuration here...
    /// }
    /// </code>
    /// </example>
    public class CommonOwinLoggerFactory : ILoggerFactory {

        /// <summary>
        /// Creates a new logger.
        /// </summary>
        /// <param name="name">The logger name.</param>
        /// <returns>
        /// A <see cref="CommonOwinLogger"/> instance.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
        public ILogger Create(string name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            return new CommonOwinLogger(name);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

using Common.Logging;

using Microsoft.Owin.Logging;

namespace ExampleApp.Logging {
    /// <summary>
    /// OWIN <see cref="ILogger"/> implementation that uses Common.Logging as the logger.
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
    public class CommonOwinLogger : ILogger {

        /// <summary>
        /// logger.
        /// </summary>
        private readonly ILog _log;

        /// <summary>
        /// Flags if <see cref="_log"/> supports trace messages.
        /// </summary>
        private readonly bool _canLogTrace;

        /// <summary>
        /// Flags if <see cref="_log"/> supports debug messages.
        /// </summary>
        private readonly bool _canLogDebug;

        /// <summary>
        /// Flags if <see cref="_log"/> supports info messages.
        /// </summary>
        private readonly bool _canLogInfo;

        /// <summary>
        /// Flags if <see cref="_log"/> supports warning messages.
        /// </summary>
        private readonly bool _canLogWarn;

        /// <summary>
        /// Flags if <see cref="_log"/> supports error messages.
        /// </summary>
        private readonly bool _canLogError;

        /// <summary>
        /// Flags if <see cref="_log"/> supports fatal messages.
        /// </summary>
        private readonly bool _canLogFatal;


        /// <summary>
        /// Creates a new <see cref="CommonOwinLogger"/> object.
        /// </summary>
        /// <param name="name">The logger name.</param>
        internal CommonOwinLogger(string name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            _log = LogManager.GetLogger(name);
            _canLogTrace = _log.IsTraceEnabled;
            _canLogDebug = _log.IsDebugEnabled;
            _canLogInfo = _log.IsInfoEnabled;
            _canLogWarn = _log.IsWarnEnabled;
            _canLogError = _log.IsErrorEnabled;
            _canLogFatal = _log.IsFatalEnabled;
        }


        /// <summary>
        /// Writes a message to the logger.
        /// </summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="eventId">The event ID.</param>
        /// <param name="state">The state object describing the message.</param>
        /// <param name="exception">The exception associated with the message.</param>
        /// <param name="formatter">A formatter function that can convert the state and exception into a readable message.</param>
        /// <returns>
        /// A flag specifying if the log write was successful.
        /// </returns>
        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter) {
            bool result;

            switch (eventType) {
                case TraceEventType.Critical:
                    if (_canLogFatal && formatter != null) {
                        _log.Fatal(formatter(state, exception), exception);
                    }
                    result = _canLogFatal;
                    break;
                case TraceEventType.Error:
                    if (_canLogError && formatter != null) {
                        _log.Error(formatter(state, exception), exception);
                    }
                    result = _canLogError;
                    break;
                case TraceEventType.Warning:
                    if (_canLogWarn && formatter != null) {
                        _log.Warn(formatter(state, exception), exception);
                    }
                    result = _canLogWarn;
                    break;
                case TraceEventType.Information:
                    if (_canLogInfo && formatter != null) {
                        _log.Info(formatter(state, exception), exception);
                    }
                    result = _canLogInfo;
                    break;
                default:
                    if (_canLogDebug && formatter != null) {
                        _log.Debug(formatter(state, exception), exception);
                    }
                    result = _canLogDebug;
                    break;
            }

            return result;
        }

    }
}
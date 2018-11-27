using System;
using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Provides metadata to attach to a log.
    /// </summary>
    public interface ILogglyMetadataProvider
    {
        /// <summary>
        /// Meta to add to each log.
        /// </summary>
        Dictionary<string, string> Meta { get; }
    }

    /// <summary>
    /// ILogTarget implementation that forwards to Unity.
    /// </summary>
    public class LogglyLogTarget : ILogTarget
    {
        /// <summary>
        /// Keeps log data.
        /// </summary>
        private class LogRecord
        {
            /// <summary>
            /// The log level.
            /// </summary>
            public string Level;

            /// <summary>
            /// Formatted log level.
            /// </summary>
            public string Message;

            /// <summary>
            /// Stack trace.
            /// </summary>
            public string StackTrace;
        }

        /// <summary>
        /// The log formatter.
        /// </summary>
        private readonly ILogFormatter _formatter = new DefaultLogFormatter
        {
            Level = false,
            Timestamp = false,
            TypeName = true
        };

        /// <summary>
        /// Loggly customer token.
        /// </summary>
        private readonly string _customerToken;

        /// <summary>
        /// Tag.
        /// </summary>
        private readonly string _tag;

        /// <summary>
        /// Provides metadata to attach to logs.
        /// </summary>
        private readonly ILogglyMetadataProvider _provider;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;

        /// <summary>
        /// WWW requires main thread, so we queue logs for processing on next frame.
        /// </summary>
        private readonly List<LogRecord> _records = new List<LogRecord>();

        /// <summary>
        /// Log level at which to filter.
        /// </summary>
        public LogLevel Filter;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="customerToken">Loggly customer token.</param>
        /// <param name="tag">Tag each log with source.</param>
        /// <param name="provider">Provides metadata to attach to logs.</param>
        /// <param name="bootstrapper">Bootstraps coroutines.</param>
        public LogglyLogTarget(
            string customerToken,
            string tag,
            ILogglyMetadataProvider provider,
            IBootstrapper bootstrapper)
        {
            _customerToken = customerToken;
            _tag = tag;
            _provider = provider;
            _bootstrapper = bootstrapper;

            bootstrapper.BootstrapCoroutine(Watch());
        }
        
        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            if (level < Filter)
            {
                return;
            }

            // WWW requires main thread, so we queue for next frame.
            lock (_records)
            {
                _records.Add(new LogRecord
                {
                    Level = level.ToString(),
                    Message = _formatter.Format(level, caller, message),
                    StackTrace = Environment.StackTrace
                });
            }
        }

        /// <summary>
        /// Watches message queue on main thread.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Watch()
        {
            while (true)
            {
                LogRecord[] copy = null;

                lock (_records)
                {
                    if (_records.Count > 0)
                    {
                        copy = _records.ToArray();
                        _records.Clear();
                    }
                }

                if (null != copy)
                {
                    foreach (var record in copy)
                    {
                        var loggingForm = new WWWForm();

                        loggingForm.AddField("level", record.Level);
                        loggingForm.AddField("message", record.Message);
                        loggingForm.AddField("stackTrace", record.StackTrace);
                        loggingForm.AddField("deviceModel", SystemInfo.deviceModel);
                        loggingForm.AddField("platform", UnityEngine.Application.platform.ToString());

                        // attach application specific metadata
                        var meta = _provider.Meta;
                        foreach (var pair in meta)
                        {
                            loggingForm.AddField(pair.Key, pair.Value);
                        }

                        _bootstrapper.BootstrapCoroutine(SendData(loggingForm));
                    }
                }

                yield return null;
            }
        }

        /// <summary>
        /// Sends data.
        /// </summary>
        /// <param name="form">The form to send.</param>
        /// <returns></returns>
        private IEnumerator SendData(WWWForm form)
        {
            yield return new WWW(
                string.Format(
                    "https://logs-01.loggly.com/inputs/{0}/tag/{1}",
                    _customerToken,
                    _tag),
                form);
        }
    }
}

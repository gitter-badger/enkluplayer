using System;
using System.Text;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Service with debug utilities.
    /// </summary>
    public class DebugService : ApplicationService, ILogTarget
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IVoiceCommandManager _voice;
        private readonly PerfMetricsCollector _perfMetricsCollector;
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Formats logs.
        /// </summary>
        private readonly ILogFormatter _formatter = new DefaultLogFormatter();

        /// <summary>
        /// The object that keeps the logs.
        /// </summary>
        private StringBuilder _builder;
        
        /// <inheritdoc />
        public LogLevel Filter
        {
            get
            {
                return LogLevel.Debug;
            }
            set
            {
                // do nothing
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DebugService(
            MessageTypeBinder binder,
            IMessageRouter messages,
            IVoiceCommandManager voice,
            PerfMetricsCollector perfMetricsCollector,
            ApiController api,
            ApplicationConfig config)
            : base(binder, messages)
        {
            _voice = voice;
            _perfMetricsCollector = perfMetricsCollector;
            _api = api;
            _config = config;
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            RestartTrace();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            ReleaseTraceResources();
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            _builder.AppendLine(_formatter.Format(level, caller, message));
        }
        
        /// <summary>
        /// Restarts the trace listening.
        /// </summary>
        private void RestartTrace()
        {
            ReleaseTraceResources();
            
            _voice.Register("trace", Voice_OnTrace);
        }

        /// <summary>
        /// Releases any resources associated with tracing.
        /// </summary>
        private void ReleaseTraceResources()
        {
            _voice.Unregister("trace", "start", "stop", "abort");

            Log.RemoveLogTarget(this);

            if (null != _builder)
            {
                _builder.Remove(0, _builder.Length);
                _builder = null;
            }
        }

        /// <summary>
        /// Called when 'trace' is spoken.
        /// </summary>
        /// <param name="cmd">The command.</param>
        private void Voice_OnTrace(string cmd)
        {
            _voice.Unregister("trace");
            _voice.Register("start", Voice_OnStart);
        }

        /// <summary>
        /// Called when 'start' is spoken.
        /// </summary>
        /// <param name="cmd">The command.</param>
        private void Voice_OnStart(string cmd)
        {
            _voice.Unregister("start");
            _voice.Register("stop", Voice_OnStop);
            _voice.Register("abort", Voice_OnAbort);

            // start log collection
            _builder = new StringBuilder("Dump:\n\n");
            Log.AddLogTarget(this);
        }

        /// <summary>
        /// Called when 'stop' is spoken.
        /// </summary>
        /// <param name="cmd">The command.</param>
        private void Voice_OnStop(string cmd)
        {
            // generate log dump
            var dump = _builder.ToString();

            // send it in
            _api
                .Utilities
                .SendEmail(new Request
                {
                    Body = dump,
                    EmailAddress = _config.Debug.DumpEmail,
                    Subject = string.Format("Dump - {0}", DateTime.Now),
                    FirstName = ""
                })
                .OnFailure(ex => Log.Error(this, "Could not send debug dump to {0} : {1}.",
                    _config.Debug.DumpEmail,
                    ex));

            RestartTrace();
        }

        /// <summary>
        /// Called when 'abort' is spoken.
        /// </summary>
        /// <param name="cmd">The command.</param>
        private void Voice_OnAbort(string cmd)
        {
            RestartTrace();
        }
    }
}
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
        private readonly IUIManager _ui;
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
            IUIManager ui,
            PerfMetricsCollector perfMetricsCollector,
            ApiController api,
            ApplicationConfig config)
            : base(binder, messages)
        {
            _voice = voice;
            _ui = ui;
            _perfMetricsCollector = perfMetricsCollector;
            _api = api;
            _config = config;
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            RestartTrace();

            _voice.Register("reset", Voice_OnReset);
            _voice.Register("update", Voice_OnUpdate);
            _voice.Register("experience", Voice_OnExperience);
            _voice.Register("network", Voice_OnNetwork);
            _voice.Register("anchors", Voice_OnAnchors);
            _voice.Register("performance", Voice_OnPerformance);
            _voice.Register("logging", Voice_OnLogging);
            _voice.Register("optimization", Voice_Optimization);
            _voice.RegisterAdmin("crash", _ => Log.Fatal(this, "Test crash."));
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            ReleaseTraceResources();

            _voice.Unregister("reset");
            _voice.Unregister("performance");
            _voice.Unregister("logging");
            _voice.Unregister("experience");
            _voice.Unregister("network");
            _voice.Unregister("anchors");
            _voice.Unregister("performance");
            _voice.Unregister("logging");
            _voice.Unregister("crash");
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

        /// <summary>
        /// Called when the reset command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnReset(string command)
        {
            int id;
            _ui
                .Open<ConfirmationUIView>(new UIReference
                {
                    UIDataId = UIDataIds.CONFIRMATION
                }, out id)
                .OnSuccess(el =>
                {
                    el.Message = "Are you sure you want to exit the application?";
                    el.OnConfirm += UnityEngine.Application.Quit;
                    el.OnCancel += () => _ui.Close(id);
                })
                .OnFailure(ex => Log.Error(this, "Could not open reset confirmation popup : {0}", ex));
        }

        /// <summary>
        /// Called when the update command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnUpdate(string command)
        {
            int id;
            _ui
                .Open<ConfirmationUIView>(new UIReference
                {
                    UIDataId = UIDataIds.CONFIRMATION
                }, out id)
                .OnSuccess(el =>
                {
                    el.Message = "Are you sure you want to update the application?";
                    el.OnConfirm += () =>
                    {
                        // HACK
                        AppDataLoader.ForceUpdate = true;
                    };
                    el.OnCancel += () => _ui.Close(id);
                })
                .OnFailure(ex => Log.Error(this, "Could not open update confirmation popup : {0}", ex));
        }

        /// <summary>
        /// Called when the experience command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnExperience(string command)
        {
            int id;
            _ui
                .OpenOverlay<ExperienceUIView>(new UIReference
                {
                    UIDataId = UIDataIds.EXPERIENCE
                },
                    out id)
                .OnSuccess(el =>
                {
                    el.OnClose += () => _ui.Close(id);
                })
                .OnFailure(e => Log.Error(this, e));
        }

        /// <summary>
        /// Called when the network command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnNetwork(string command)
        {
            int id;
            _ui
                .OpenOverlay<NetworkUIView>(new UIReference
                {
                    UIDataId = UIDataIds.NETWORK
                },
                    out id)
                .OnSuccess(el =>
                {
                    el.OnClose += () => _ui.Close(id);
                })
                .OnFailure(e => Log.Error(this, e));
        }

        /// <summary>
        /// Called when the anchors command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnAnchors(string command)
        {
            int id;
            _ui
                .OpenOverlay<AnchorUIView>(new UIReference
                {
                    UIDataId = UIDataIds.ANCHORS
                },
                    out id)
                .OnSuccess(el =>
                {
                    el.OnClose += () => _ui.Close(id);
                })
                .OnFailure(e => Log.Error(this, e));
        }

        /// <summary>
        /// Called when the performance command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnPerformance(string command)
        {
            // open
            int hudId;
            _ui
                .OpenOverlay<PerfDisplayUIView>(new UIReference
                {
                    UIDataId = UIDataIds.PERF_HUD
                }, out hudId)
                .OnSuccess(el => el.OnClose += () => _ui.Close(hudId));
        }

        /// <summary>
        /// Called when the logging command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnLogging(string command)
        {
            int hudId;
            _ui
                .OpenOverlay<LoggingUIView>(new UIReference
                {
                    UIDataId = UIDataIds.LOGGING_HUD
                }, out hudId)
                .OnSuccess(el => el.OnClose += () => _ui.Close(hudId));
        }

        /// <summary>
        /// Called when the optimization command is recognized.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_Optimization(string command)
        {
            int hudId;
            _ui
                .OpenOverlay<OptimizationUIView>(new UIReference
                {
                    UIDataId = UIDataIds.OPTIMIZATION_HUD
                }, out hudId)
                .OnSuccess(el => el.OnClose += () => _ui.Close(hudId));
        }
    }
}
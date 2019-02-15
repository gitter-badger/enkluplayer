using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;
using UnityEngine;

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
        private readonly IBootstrapper _bootstrapper;
        private readonly IVoiceCommandManager _voice;
        private readonly IDeviceMetaProvider _meta;
        private readonly IPrimaryAnchorManager _anchorManager;
        private readonly PerfMonitor _perfMonitor;
        private readonly ApiController _api;
        private readonly ApplicationConfig _config;
        private readonly RuntimeStats _runtimeStats;

        /// <summary>
        /// Formats logs.
        /// </summary>
        private readonly ILogFormatter _formatter = new DefaultLogFormatter();

        /// <summary>
        /// Cached camera transform.
        /// </summary>
        private readonly Transform _camera;

        /// <summary>
        /// The object that keeps the logs.
        /// </summary>
        private StringBuilder _builder;

        /// <summary>
        /// Whether or not this service has been started. Used to stop bootstrapped coroutines.
        /// </summary>
        private bool _started = false;
        
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
            IBootstrapper bootstrapper,
            IVoiceCommandManager voice,
            IDeviceMetaProvider meta,
            IPrimaryAnchorManager anchorManager,
            PerfMetricsCollector perfMetricsCollector,
            RuntimeStats runtimeStats,
            ApiController api,
            ApplicationConfig config)
            : base(binder, messages)
        {
            _bootstrapper = bootstrapper;
            _voice = voice;
            _meta = meta;
            _anchorManager = anchorManager;
            _perfMonitor = perfMetricsCollector.PerfMonitor;
            _runtimeStats = runtimeStats;
            _api = api;
            _config = config;

            var mainCam = Camera.main;
            if (mainCam != null)
            {
                _camera = mainCam.transform;
            }
            else
            {
                Log.Error(this, "No MainCamera found.");
            }
        }

        /// <inheritdoc />
        public override void Start()
        {
            base.Start();

            _started = true;
            _bootstrapper.BootstrapCoroutine(UpdateMetaStats());

            RestartTrace();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _started = false;

            ReleaseTraceResources();
        }

        /// <inheritdoc />
        public override void Update(float dt)
        {
            base.Update(dt);
            
            // ----- Memory -----
            _runtimeStats.Device.AllocatedMemory = _perfMonitor.Memory.Allocated;
            _runtimeStats.Device.ReservedMemory = _perfMonitor.Memory.Total;
            _runtimeStats.Device.MonoMemory = _perfMonitor.Memory.Mono;
            _runtimeStats.Device.GpuMemory = _perfMonitor.Memory.Gpu;
            _runtimeStats.Device.GraphicsDriverMemory = _perfMonitor.Memory.GraphicsDriver;

            // ----- Camera -----
            var position = _camera.position;
            var rotation = _camera.rotation;
            var anchor = _anchorManager.RelativeTransform(ref position, ref rotation);
            
            // Only update the camera position if we have a relative relationship.
            if (anchor != null)
            {
                _runtimeStats.Camera.Position = _camera.position;
                _runtimeStats.Camera.Rotation = _camera.rotation;
                _runtimeStats.Camera.AnchorRelativeTo = anchor.Id;
            }
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            _builder.AppendLine(_formatter.Format(level, caller, message));
        }

        /// <summary>
        /// Update device meta related information
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateMetaStats()
        {
            while (true)
            {
                _runtimeStats.Device.Battery = _meta.Meta().Battery;
            
                // Since this only tracks battery so far - no need to be spammy!
                yield return new WaitForSeconds(60);

                if (!_started)
                {
                    yield break;
                }
            }
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

        private void Anchor_OnUpdate()
        {
            var len = _anchorManager.Anchors.Count;
            _runtimeStats.Anchors.States = new RuntimeStats.AnchorsInfo.State[len];
            
            // ----- Anchors -----
            for (var i = 0; i < len; i++)
            {
                var anchor = _anchorManager.Anchors[i];
                
                _runtimeStats.Anchors.States[i] = new RuntimeStats.AnchorsInfo.State
                {
                    Status = anchor.Status
                };

                switch (anchor.Status)
                {
                    case WorldAnchorWidget.WorldAnchorStatus.IsReadyLocated:
                        _runtimeStats.Anchors.States[i].TimeUnlocated = 0;
                        break;
                    case WorldAnchorWidget.WorldAnchorStatus.IsReadyNotLocated:
                        _runtimeStats.Anchors.States[i].TimeUnlocated =
                            Time.realtimeSinceStartup - anchor.UnlocatedStartTime;
                        break;
                }
            }
        }
    }
}
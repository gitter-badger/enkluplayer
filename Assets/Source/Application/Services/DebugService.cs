using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.Trellis.Messages;
using CreateAR.Trellis.Messages.SendEmail;
using Newtonsoft.Json;
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
        private readonly IUIManager _ui;
        private readonly IHttpService _http;
        private readonly AnchorManager _anchorManager;
        private readonly StandardScriptLoader _scriptLoader;
        private readonly StandardAssetLoader _assetLoader;
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
        private bool _started;

        /// <summary>
        /// Cached counters to determine if lists need to be re-serialized.
        /// </summary>
        private int _assetFailures;
        private int _scriptFailures;

        /// <summary>
        /// Unsub action for load app message.
        /// </summary>
        private Action _loadAppUnsub;

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
            IAnchorManager anchorManager,
            IUIManager ui,
            IScriptLoader scriptLoader,
            IAssetLoader assetLoader,
            IHttpService http,
            PerfMetricsCollector perfMetricsCollector,
            RuntimeStats runtimeStats,
            ApiController api,
            ApplicationConfig config)
            : base(binder, messages)
        {
            _bootstrapper = bootstrapper;
            _voice = voice;
            _meta = meta;
            _anchorManager = (AnchorManager) anchorManager;
            _perfMonitor = perfMetricsCollector.PerfMonitor;
            _runtimeStats = runtimeStats;
            _ui = ui;
            _scriptLoader = (StandardScriptLoader) scriptLoader;
            _assetLoader = (StandardAssetLoader) assetLoader;
            _http = http;
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
            _loadAppUnsub = _messages.Subscribe(MessageTypes.LOAD_APP, App_OnLoad);
            
            _bootstrapper.BootstrapCoroutine(UpdateMetaStats());
            
            RestartTrace();

            // http logging
            _http.OnRequest += Http_OnRequest;

            // voice commands
            _voice.Register("reset", Voice_OnReset);
            _voice.Register("update", Voice_OnUpdate);
            _voice.Register("experience", Voice_OnExperience);
            _voice.Register("network", Voice_OnNetwork);
            _voice.Register("anchors", Voice_OnAnchors);
            _voice.Register("performance", Voice_OnPerformance);
            _voice.Register("logging", Voice_OnLogging);
            _voice.Register("optimization", Voice_Optimization);
            _voice.RegisterAdmin("crash", _ => Log.Fatal(this, "Test crash."));

#if NETFX_CORE
            _voice.Register("origin", str =>
            {
                UnityEngine.XR.InputTracking.Recenter();
            });
#endif
            _voice.Register("bypass", Voice_OnAnchorBypass);
        }

        /// <inheritdoc />
        public override void Stop()
        {
            base.Stop();

            _started = false;
            _loadAppUnsub();
            
            ReleaseTraceResources();

            _http.OnRequest -= Http_OnRequest;

            // voice commands
            _voice.Unregister("reset");
            _voice.Unregister("performance");
            _voice.Unregister("logging");
            _voice.Unregister("experience");
            _voice.Unregister("network");
            _voice.Unregister("anchors");
            _voice.Unregister("performance");
            _voice.Unregister("logging");
            _voice.Unregister("crash");
            _voice.Unregister("origin");
            _voice.Unregister("bypass");
        }

        /// <inheritdoc />
        public override void Update(float dt)
        {
            base.Update(dt);

            _runtimeStats.Uptime = Time.realtimeSinceStartup;
            
            UpdateMemory();
            UpdateCamera();
            UpdateAssets();
            UpdateScripts();
            UpdateAnchors();
        }

        private void UpdateScripts()
        {
            _runtimeStats.Experience.ScriptState.QueueLength = _scriptLoader.QueueLength;
            if (_scriptFailures != _scriptLoader.LoadFailures.Count)
            {
                _scriptFailures = _scriptLoader.LoadFailures.Count;
                var str = string.Empty;

                for (var i = 0; i < _scriptFailures; i++)
                {
                    var failure = _scriptLoader.LoadFailures[i];
                    str += string.Format("{0} - {1}\n", failure.ScriptData.Id, failure.Exception);
                }

                _runtimeStats.Experience.AssetState.Errors = str;
            }
        }

        private void UpdateAssets()
        {
            _runtimeStats.Experience.AssetState.QueueLength = _assetLoader.Queue.Count;
            _runtimeStats.Experience.AssetState.NextLoad =
                _assetLoader.Queue.Count > 0 ? _assetLoader.Queue[0].ToString() : "None";
            if (_assetFailures != _assetLoader.LoadFailures.Count)
            {
                _assetFailures = _assetLoader.LoadFailures.Count;
                var str = string.Empty;

                for (var i = 0; i < _assetFailures; i++)
                {
                    var failure = _assetLoader.LoadFailures[i];
                    str += string.Format("{0} - {1}\n", failure.AssetData.Guid, failure.Exception);
                }

                _runtimeStats.Experience.AssetState.Errors = str;
            }
        }

        private void UpdateCamera()
        {
            var position = _camera.position;
            var rotation = _camera.rotation;
            var anchor = RelativeTransform(ref position, ref rotation);

            // Only update the camera position if we have a relative relationship.
            if (anchor != null)
            {
                _runtimeStats.Camera.Position = _camera.position;
                _runtimeStats.Camera.Rotation = _camera.rotation;
                _runtimeStats.Camera.AnchorRelativeTo = anchor.Id;
            }
            else
            {
                _runtimeStats.Camera.AnchorRelativeTo = "None.";
            }
        }

        private void UpdateMemory()
        {
            _runtimeStats.Device.AllocatedMemory = _perfMonitor.Memory.Allocated;
            _runtimeStats.Device.ReservedMemory = _perfMonitor.Memory.Total;
            _runtimeStats.Device.MonoMemory = _perfMonitor.Memory.Mono;
            _runtimeStats.Device.GpuMemory = _perfMonitor.Memory.Gpu;
            _runtimeStats.Device.GraphicsDriverMemory = _perfMonitor.Memory.GraphicsDriver;
        }

        /// <inheritdoc />
        public void OnLog(LogLevel level, object caller, string message)
        {
            _builder.AppendLine(_formatter.Format(level, caller, message));
        }

        /// <summary>
        /// Modifies a position/rotation relative to a located anchor. The primary anchor is prioritized.
        /// The anchor used for relative positioning is returned. If all anchors aren't located, null is returned.
        /// </summary>
        private WorldAnchorWidget RelativeTransform(ref Vector3 position, ref Quaternion rotation)
        {
            WorldAnchorWidget anchor = null;

            // use the primary anchor if possible
            if (null != _anchorManager.Primary
                && _anchorManager.Primary.Status == WorldAnchorStatus.IsReadyLocated)
            {
                anchor = _anchorManager.Primary;
            }
            // otherwise use the first located one we can find
            else
            {
                for (int i = 0, len = _anchorManager.Anchors.Count; i < len; i++)
                {
                    if (_anchorManager.Anchors[i].Status == WorldAnchorStatus.IsReadyLocated)
                    {
                        anchor = _anchorManager.Anchors[i];
                        break;
                    }
                }
            }

            if (null != anchor)
            {
                position = anchor.GameObject.transform.InverseTransformPoint(position);
                rotation = Quaternion.Inverse(rotation) * anchor.GameObject.transform.rotation;
            }

            return anchor;
        }
        
        /// <summary>
        /// Called when an anchor is added to the scene.
        /// </summary>
        private void UpdateAnchors()
        {
            var len = _anchorManager.Anchors.Count;
            if (len != _runtimeStats.Anchors.States.Length)
            {
                _runtimeStats.Anchors.States = new RuntimeStats.AnchorsInfo.State[len];
            }

            for (var i = 0; i < len; i++)
            {
                var anchor = _anchorManager.Anchors[i];

                _runtimeStats.Anchors.States[i].Id = anchor.Id;
                _runtimeStats.Anchors.States[i].Status = anchor.Status;
                _runtimeStats.Anchors.States[i].TimeUnlocated = anchor.UnlocatedStartTime < float.Epsilon
                    ? 0
                    : Time.realtimeSinceStartup - anchor.UnlocatedStartTime;
            }
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

        /// <summary>
        /// Called when bypass voice command is used.
        /// </summary>
        /// <param name="command">The command.</param>
        private void Voice_OnAnchorBypass(string command)
        {
            if (!AnchorManager.AreAllAnchorsReady)
            {
                _anchorManager.BypassAnchorRequirement();
            }
        }
        
        /// <summary>
        /// Called when an experience loads.
        /// </summary>
        private void App_OnLoad(object _)
        {
            _runtimeStats.Experience.ExperienceId = _config.Play.AppId;
        }

        /// <summary>
        /// Called when an HTTP request is made.
        /// </summary>
        /// <param name="verb">The HTTP verb.</param>
        /// <param name="uri">The uri.</param>
        /// <param name="headers">The headers.</param>
        /// <param name="payload">The payload.</param>
        private void Http_OnRequest(
            string verb,
            string uri,
            Dictionary<string, string> headers,
            object payload)
        {
            var message = string.Format("curl -X {0} {1}", verb, uri);
            foreach (var pair in headers)
            {
                var key = pair.Key;
                var value = pair.Value;
                if (key == "Authorization")
                {
                    value = "[Hidden]";
                }

                message += string.Format("\\\n\t-H '{0}: {1}'", key, value);
            }

            if (null != payload)
            {
                var stringPayload = payload.ToString();
                try
                {
                    stringPayload = JsonConvert.SerializeObject(payload);
                }
                catch
                {
                    // ignore
                }

                message += string.Format("\\\n\t-d '{0}'", stringPayload);
            }

            Log.Info(this, message);
        }
    }
}

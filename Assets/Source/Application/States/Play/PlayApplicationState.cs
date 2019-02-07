using System;
using System.Collections;
using System.Linq;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.AR;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.Scripting;
using CreateAR.EnkluPlayer.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Manages the play state.
    /// </summary>
    public class PlayApplicationState : IState
    {
        /// <summary>
        /// Name of the playmode scene to load.
        /// </summary>
        private const string SCENE_NAME = "PlayMode";

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly ApplicationConfig _config;
        private readonly IBootstrapper _bootstrapper;
        private readonly IScriptRequireResolver _resolver;
        private readonly IDesignController _design;
        private readonly IAppController _app;
        private readonly IArService _ar;
        private readonly IUIManager _ui;
        private readonly IConnection _connection;
        private readonly IMessageRouter _messages;
        private readonly IVoiceCommandManager _voice;
        private readonly IAssetLoader _assetLoader;
        private readonly IMetricsService _metrics;
        private readonly IAppQualityController _quality;
        private readonly ITweenManager _tweens;
        private readonly ITouchManager _touches;
        private readonly IImageCapture _imageCapture;
        private readonly IVideoCapture _videoCapture;

        /// <summary>
        /// Status.
        /// </summary>
        private int _connectionStatusId = -1;
        
        /// <summary>
        /// Context for designer.
        /// </summary>
        private DesignerContext _context;
        
        /// <summary>
        /// Interrupt view id.
        /// </summary>
        private int _interruptId;

        /// <summary>
        /// Unsubscribe for critical error subscriber.
        /// </summary>
        private Action _criticalErrorUnsub;

        /// <summary>
        /// Id for critical error message.
        /// </summary>
        private int _criticalErrorId;

        /// <summary>
        /// UI id.
        /// </summary>
        private int _loadingScreenId;

        /// <summary>
        /// UI frame.
        /// </summary>
        private UIManagerFrame _frame;

        /// <summary>
        /// Timer for tracking loss.
        /// </summary>
        private int _trackingId;

        /// <summary>
        /// Whether the AR service has reported tracking loss or not.
        /// </summary>
        private bool _trackingLost;

        /// <summary>
        /// Plays an App.
        /// </summary>
        public PlayApplicationState(
            ApplicationConfig config,
            IBootstrapper bootstrapper,
            IScriptRequireResolver resolver,
            IDesignController design,
            IAppController app,
            IArService ar,
            IUIManager ui,
            IConnection connection,
            IMessageRouter messages,
            IVoiceCommandManager voice,
            IAssetLoader assetLoader,
            IMetricsService metrics,
            IAppQualityController quality,
            ITweenManager tweens,
            ITouchManager touches,
            IImageCapture imageCapture,
            IVideoCapture videoCapture)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _resolver = resolver;
            _design = design;
            _app = app;
            _ar = ar;
            _ui = ui;
            _connection = connection;
            _messages = messages;
            _voice = voice;
            _assetLoader = assetLoader;
            _metrics = metrics;
            _quality = quality;
            _tweens = tweens;
            _touches = touches;
            _imageCapture = imageCapture;
            _videoCapture = videoCapture;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "PlayApplicationState::Enter()");
            
            _context = new DesignerContext
            {
                Edit = _config.Play.Edit
            };

            // setup UI
            _frame = _ui.CreateFrame();
            _ui.Open<ICommonLoadingView>(new UIReference
            {
                UIDataId = UIDataIds.LOADING
            }, out _loadingScreenId);

            // allow the cursor to be hidden
            _config.Cursor.ForceShow = false;
            
            // watch tracking
            _ar.OnTrackingOffline += Ar_OnTrackingOffline;
            _ar.OnTrackingOnline += Ar_OnTrackingOnline;
            
            _resolver.Initialize(
#if NETFX_CORE
                // reference by hand
                System.Reflection.Assembly.Load(new System.Reflection.AssemblyName("Assembly-CSharp"))
#else
                AppDomain.CurrentDomain.GetAssemblies()
#endif
            );

            // watch for errors
            _criticalErrorUnsub = _messages.Subscribe(
                MessageTypes.PLAY_CRITICAL_ERROR,
                Messages_OnCriticalError);

            // watch loading
            _app.OnReady += App_OnReady;

            // listen for reset command
            _voice.Register("reset", Voice_OnReset);
            _voice.Register("update", Voice_OnUpdate);
            
            // load playmode scene
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
            _tweens.Update(dt);
            _touches.Update();

#if !UNITY_WEBGL
            if (_config.Play.Edit)
            {
                UpdateConnectionStatus();
            }
#endif
        }

        /// <inheritdoc />
        public void Exit()
        {
            Log.Info(this, "PlayApplicationState::Exit()");

            // kill all tweens
            _tweens.StopAll();

            // shutoff quality
            _quality.Teardown();

            // unwatch tracking
            _ar.OnTrackingOffline -= Ar_OnTrackingOffline;
            _ar.OnTrackingOnline -= Ar_OnTrackingOnline;

            // unsubscribe from critical errors
            _criticalErrorUnsub();
            _criticalErrorId = -1;

            // stop listening for voice commands
            _voice.Unregister("reset");
            _voice.Unregister("performance");
            _voice.Unregister("logging");
            
            // Cleanup image/video capture in case the experience didn't 
            _imageCapture.Abort();
            _videoCapture.Teardown();

            // stop watching loads
            _app.OnReady -= App_OnReady;

            Log.Info(this, "Teardown app.");

            // teardown app
            _app.Unload();

            Log.Info(this, "Teardown designer.");

            // teardown designer
            _design.Teardown();
            
            // unload playmode scene
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(SCENE_NAME));

            // close UI
            _frame.Release();

            // clear everything in the queue
            _assetLoader.ClearDownloadQueue();

            // set the cursor back to always drawing
            _config.Cursor.ForceShow = true;
        }

        /// <summary>
        /// Waits for the scene to load.
        /// </summary>
        /// <param name="op">Load operation.</param>
        /// <returns></returns>
        private IEnumerator WaitForScene(AsyncOperation op)
        {
            yield return op;

            var config = _context.PlayConfig = Object.FindObjectOfType<PlayModeConfig>();
            if (null == config)
            {
                throw new Exception("Could not find PlayModeConfig.");
            }
            
            // start the app controller
            if (_config.Play.Edit)
            {
                _app.Edit();
            }
            else
            {
                _app.Play();
            }

            // start designer
            _design.Setup(_context, _app);

            // perf
            _voice.Register("performance", _ =>
            {
                // open
                int hudId;
                _ui
                    .OpenOverlay<PerfDisplayUIView>(new UIReference
                    {
                        UIDataId = "Perf.Hud"
                    }, out hudId)
                    .OnSuccess(el => el.OnClose += () => _ui.Close(hudId));
            });

            // logging
            _voice.Register("logging", _ =>
            {
                int hudId;
                _ui
                    .OpenOverlay<LoggingUIView>(new UIReference
                    {
                        UIDataId = "Logging.Hud"
                    }, out hudId)
                    .OnSuccess(el => el.OnClose += () => _ui.Close(hudId));
            });
        }

        /// <summary>
        /// Updates connection status UI.
        /// </summary>
        private void UpdateConnectionStatus()
        {
            if (!_connection.IsConnected
                && -1 == _connectionStatusId)
            {
                _ui
                    .Open<IStatusView>(new UIReference
                    {
                        UIDataId = "Status.Connection"
                    }, out _connectionStatusId)
                    .OnFailure(exception => Log.Error(this, "Could not open connection status UI : {0}", exception));
            }
            else if (_connection.IsConnected
                     && -1 != _connectionStatusId)
            {
                _ui.Close(_connectionStatusId);
                _connectionStatusId = -1;
            }
        }

        /// <summary>
        /// Called when there is some critical application error and we need to go back to userprofile.
        /// </summary>
        private void Messages_OnCriticalError(object _)
        {
            if (-1 != _criticalErrorId)
            {
                return;
            }

            _ui
                .Open<ICommonErrorView>(new UIReference
                {
                    UIDataId = UIDataIds.ERROR
                }, out _criticalErrorId)
                .OnSuccess(el =>
                {
                    el.Message = "There was an error making a change to the scene. Please reload.";
                    el.Action = "Reload";
                    el.OnOk += () =>
                    {
                        _ui.Close(_criticalErrorId);

                        // exit
                        _messages.Publish(MessageTypes.USER_PROFILE);
                    };
                })
                .OnFailure(ex =>
                {
                    Log.Error(this, "Could not open error dialog : {0}", ex);

                    _messages.Publish(MessageTypes.USER_PROFILE);
                });
        }

        /// <summary>
        /// Called when we've lost AR tracking.
        /// </summary>
        private void Ar_OnTrackingOffline()
        {
            // Guard against AR service reporting different, but still failing, states
            if (_trackingLost)
            {
                return;
            }
            _trackingLost = true;
            
            Log.Info(this, "Ar tracking lost!");

            _trackingId = _metrics.Timer(MetricsKeys.ANCHOR_TRACKING_LOST).Start();

            _ui.Open<IUIElement>(new UIReference
            {
                UIDataId = "Ar.Interrupted"
            }, out _interruptId);
        }
        
        /// <summary>
        /// Called when AR tracking is back online.
        /// </summary>
        private void Ar_OnTrackingOnline()
        {
            _trackingLost = false;
            
            Log.Info(this, "Ar tracking back online.");

            _metrics.Timer(MetricsKeys.ANCHOR_TRACKING_LOST).Stop(_trackingId);

            _ui.Close(_interruptId);
        }

        /// <summary>
        /// Called when app is ready.
        /// </summary>
        private void App_OnReady()
        {
            _ui.Close(_loadingScreenId);

            // setup quality
            var id = _app.Scenes.All.FirstOrDefault();
            if (!string.IsNullOrEmpty(id))
            {
                _quality.Setup(_app.Scenes.Root(id));
            }
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
    }
}
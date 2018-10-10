using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using CreateAR.EnkluPlayer.AR;
using CreateAR.EnkluPlayer.Assets;
using CreateAR.EnkluPlayer.Scripting;
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
        /// Configuration.
        /// </summary>
        private readonly ApplicationConfig _config;

        /// <summary>
        /// Bootstraps coroutines.
        /// </summary>
        private readonly IBootstrapper _bootstrapper;
        
        /// <summary>
        /// Resolves script requires.
        /// </summary>
        private readonly IScriptRequireResolver _resolver;

        /// <summary>
        /// Controls design mode.
        /// </summary>
        private readonly IDesignController _design;

        /// <summary>
        /// Manages app.
        /// </summary>
        private readonly IAppController _app;

        /// <summary>
        /// AR interface.
        /// </summary>
        private readonly IArService _ar;

        /// <summary>
        /// UI interface.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// Application-wide messages.
        /// </summary>
        private readonly IMessageRouter _messages;

        /// <summary>
        /// Handles voice commands.
        /// </summary>
        private readonly IVoiceCommandManager _voice;

        /// <summary>
        /// Loads assets.
        /// </summary>
        private readonly IAssetLoader _assetLoader;

        /// <summary>
        /// Metrics.
        /// </summary>
        private readonly IMetricsService _metrics;

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
            IMetricsService metrics)
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
            
            // load playmode scene
            _bootstrapper.BootstrapCoroutine(WaitForScene(
                SceneManager.LoadSceneAsync(
                    SCENE_NAME,
                    LoadSceneMode.Additive)));
        }

        /// <inheritdoc />
        public void Update(float dt)
        {
#if !UNITY_WEBGL
            UpdateConnectionStatus();
#endif
        }

        /// <inheritdoc />
        public void Exit()
        {
            Log.Info(this, "PlayApplicationState::Exit()");
            
            // unwatch tracking
            _ar.OnTrackingOffline -= Ar_OnTrackingOffline;
            _ar.OnTrackingOnline -= Ar_OnTrackingOnline;

            // unsubscribe from critical errors
            _criticalErrorUnsub();
            _criticalErrorId = -1;

            // stop listening for voice commands
            _voice.Unregister("reset");

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

            // initialize with app id
            _app.Play();

            // start designer
            _design.Setup(_context, _app);
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
    }
}
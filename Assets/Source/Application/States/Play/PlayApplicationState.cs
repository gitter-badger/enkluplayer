using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.AR;
using Jint.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer
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
            IConnection connection)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _resolver = resolver;
            _design = design;
            _app = app;
            _ar = ar;
            _ui = ui;
            _connection = connection;
        }

        /// <inheritdoc />
        public void Enter(object context)
        {
            Log.Info(this, "PlayApplicationState::Enter()");
            
            _context = new DesignerContext
            {
                Edit = _config.Play.Edit
            };
            
            // watch tracking
            _ar.OnTrackingOffline += Ar_OnTrackingOffline;
            _ar.OnTrackingOnline += Ar_OnTrackingOnline;
            
            _resolver.Initialize(
#if NETFX_CORE
                // reference by hand
#else
                AppDomain.CurrentDomain.GetAssemblies()
#endif
            );

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
            
            // teardown app
            _app.Unload();

            // teardown designer
            _design.Teardown();
            
            // unload playmode scene
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(SCENE_NAME));

            // close connection status
            _ui.Close(_connectionStatusId);
            _connectionStatusId = -1;
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
        /// Called when we've lost AR tracking.
        /// </summary>
        private void Ar_OnTrackingOffline()
        {
            Log.Info(this, "Ar tracking lost!");

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

            _ui.Close(_interruptId);
        }
    }
}
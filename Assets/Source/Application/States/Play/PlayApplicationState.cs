using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
        /// Messages.
        /// </summary>
        private readonly IMessageRouter _messages;

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
        /// Context for designer.
        /// </summary>
        private DesignerContext _context;

        /// <summary>
        /// Time at which state was entered.
        /// </summary>
        private DateTime _enterTime;

        /// <summary>
        /// True iff status has been cleared.
        /// </summary>
        private bool _statusCleared;
        
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
            IMessageRouter messages,
            IScriptRequireResolver resolver,
            IDesignController design,
            IAppController app,
            IArService ar,
            IUIManager ui)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _messages = messages;
            _resolver = resolver;
            _design = design;
            _app = app;
            _ar = ar;
            _ui = ui;
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

            _enterTime = DateTime.Now;
            _statusCleared = false;

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
            if (!_statusCleared
                && DateTime.Now.Subtract(_enterTime).TotalSeconds > 5)
            {
                _messages.Publish(MessageTypes.STATUS, "");
                _statusCleared = true;
            }
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
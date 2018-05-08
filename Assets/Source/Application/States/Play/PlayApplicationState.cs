using System;
using System.Collections;
using CreateAR.Commons.Unity.Http;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
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
        /// Connection.
        /// </summary>
        private readonly IConnection _connection;

        /// <summary>
        /// UI.
        /// </summary>
        private readonly IUIManager _ui;

        /// <summary>
        /// Voice controls.
        /// </summary>
        private readonly IVoiceCommandManager _voice;

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
        /// Plays an App.
        /// </summary>
        public PlayApplicationState(
            ApplicationConfig config,
            IBootstrapper bootstrapper,
            IMessageRouter messages,
            IScriptRequireResolver resolver,
            IDesignController design,
            IAppController app,
            IConnection connection,
            IUIManager ui,
            IVoiceCommandManager voice)
        {
            _config = config;
            _bootstrapper = bootstrapper;
            _messages = messages;
            _resolver = resolver;
            _design = design;
            _app = app;
            _connection = connection;
            _ui = ui;
            _voice = voice;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            Log.Info(this, "PlayApplicationState::Enter()");

            _context = new DesignerContext
            {
                Edit = _config.Play.Edit
            };

            _enterTime = DateTime.Now;
            _statusCleared = false;

#if NETFX_CORE || UNITY_IOS || UNITY_ANDROID
            _messages.Publish(
                MessageTypes.STATUS,
                NetworkUtils.GetNetworkSummary());
#endif
            
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

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            if (!_statusCleared
                && DateTime.Now.Subtract(_enterTime).TotalSeconds > 5)
            {
                _messages.Publish(MessageTypes.STATUS, "");
                _statusCleared = true;
            }
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            Log.Info(this, "PlayApplicationState::Exit()");

            _voice.Unregister("profile");

            // teardown app
            _app.Unload();

            // teardown designer
            if (IsDesignerEnabled())
            {
                _design.Teardown();
            }
            
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

            if (IsDesignerEnabled())
            {
                _design.Setup(_context, _app);

                _voice.Register("profile", Voice_OnProfile);
            }
            else
            {
                int id;
                _ui
                    .Open<ErrorPopupUIView>(
                        new UIReference
                        {
                            UIDataId = UIDataIds.ERROR
                        },
                        out id)
                    .OnSuccess(el =>
                    {
                        el.Message =
                            "You appear to be offline. Because of this, edit mode has been disabled. At any time, say 'profile' to go back to your profile.";
                        el.Action = "Got it";
                        el.OnOk += () =>
                        {
                            _ui.Pop();

                            _voice.Register("profile", Voice_OnProfile);
                        };
                    });
            }
        }

        /// <summary>
        /// True iff designer is enabled.
        /// </summary>
        /// <returns></returns>
        private bool IsDesignerEnabled()
        {
            return _app.CanEdit && _connection.IsConnected;
        }

        /// <summary>
        /// Called when voice commands say go back.
        /// </summary>
        /// <param name="command">The command that was said.</param>
        private void Voice_OnProfile(string command)
        {
            _messages.Publish(MessageTypes.USER_PROFILE);
        }
    }
}
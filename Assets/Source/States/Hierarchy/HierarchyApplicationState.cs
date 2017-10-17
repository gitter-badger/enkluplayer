using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// State for moving about the hierarchy.
    /// </summary>
    public class HierarchyApplicationState : IState
    {
        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        private const string SCENE_NAME = "HierarchyMode";

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IApplicationState _state;
        private readonly IAssetManager _assets;
        private readonly IInputManager _input;
        private readonly IMessageRouter _router;

        /// <summary>
        /// Unsubscribe.
        /// </summary>
        private Action _unsub;
        
        /// <summary>
        /// Input state.
        /// </summary>
        [Inject(NamedInjections.INPUT_STATE_DEFAULT)]
        public IState InputState { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HierarchyApplicationState(
            IApplicationState state,
            IAssetManager assets,
            IInputManager input,
            IMessageRouter router)
        {
            _state = state;
            _assets = assets;
            _input = input;
            _router = router;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            // load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                SCENE_NAME,
                LoadSceneMode.Additive);

            // input
            _input.ChangeState(InputState);

            // select!
            _unsub = _router.Subscribe(
                MessageTypes.SELECT_CONTENT,
                Messages_OnSelectContent);
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _input.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            _unsub();

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }

        /// <summary>
        /// Called when content has been selected.
        /// </summary>
        /// <param name="message">Event.</param>
        private void Messages_OnSelectContent(object message)
        {
            var @event = (SelectContentEvent) message;

            Log.Info(this, "Select content : {0}.", @event.ContentId);
        }
    }
}
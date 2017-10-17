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
            IInputManager input)
        {
            _state = state;
            _assets = assets;
            _input = input;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
        {
            // load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                SCENE_NAME,
                LoadSceneMode.Additive);

            // input
            _input.ChangeState(InputState);
            
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            _input.Update(dt);
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }
    }
}
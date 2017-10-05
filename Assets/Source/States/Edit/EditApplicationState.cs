using UnityEngine.SceneManagement;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state that is used in edit mode.
    /// </summary>
    public class EditApplicationState : IState
    {
        /// <summary>
        /// Name of the scene to load.
        /// </summary>
        private const string SCENE_NAME = "EditMode";

        /// <summary>
        /// The input mechanism.
        /// </summary>
        private readonly IInputManager _input;
        
        /// <summary>
        /// Input state.
        /// </summary>
        [Inject(NamedInjections.INPUT_STATE_DEFAULT)]
        public IState InputState { get; set; }

        /// <summary>
        /// Creates a new EditApplicationState.
        /// </summary>
        /// <param name="input">The input mechanism.</param>
        public EditApplicationState(IInputManager input)
        {
            _input = input;
        }

        /// <summary>
        /// A meaningful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[EditApplicationState]";
        }

        /// <summary>
        /// Called when the state is entered.
        /// </summary>
        public void Enter()
        {
            // load scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(SCENE_NAME, LoadSceneMode.Additive);

            _input.ChangeState(InputState);
        }

        public void Update(float dt)
        {
            _input.Update(dt);
        }

        public void Exit()
        {
            _input.ChangeState(null);

            // unload scene
            UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(
                UnityEngine.SceneManagement.SceneManager.GetSceneByName(SCENE_NAME));
        }
    }
}
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Application state that is used in edit mode.
    /// </summary>
    public class EditApplicationState : IState
    {
        /// <summary>
        /// The input mechanism.
        /// </summary>
        private readonly IInputManager _input;

        /// <summary>
        /// The default state for input in edit mode.
        /// </summary>
        private readonly IState _defaultState;
        
        /// <summary>
        /// Creates a new EditApplicationState.
        /// </summary>
        /// <param name="input">The input mechanism.</param>
        /// <param name="defaultState">The default state for input in edit mode.</param>
        public EditApplicationState(
            IInputManager input,
            IState defaultState)
        {
            _input = input;
            _defaultState = defaultState;
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
            Log.Info(this, "Enter {0}.", _defaultState);

            _input.ChangeState(_defaultState);
        }

        public void Update(float dt)
        {
            _input.Update(dt);
        }

        public void Exit()
        {
            
        }
    }
}

using System;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// A simple finite state machine.
    /// </summary>
    public class FiniteStateMachine
    {
        /// <summary>
        /// Collection of possible states.
        /// </summary>
        private readonly IState[] _states;

        /// <summary>
        /// Current state.
        /// </summary>
        private IState _state;

        /// <summary>
        /// Creates a new FSM that can only transition between these states.
        /// </summary>
        /// <param name="states">The states to transition between.</param>
        public FiniteStateMachine(IState[] states)
        {
            _states = states;
        }

        /// <summary>
        /// Changes to the state of type T.
        /// </summary>
        /// <typeparam name="T">The type of the state to transition to.</typeparam>
        public void Change<T>() where T : IState
        {
            Change(typeof(T));
        }

        /// <summary>
        /// Changes to the state of the given type.
        /// </summary>
        /// <param name="type">The type of the state to transition to.</param>
        public void Change(Type type)
        {
            IState newState = null;

            for (int i = 0, len = _states.Length; i < len; i++)
            {
                var state = _states[i];
                if (state.GetType() == type)
                {
                    if (_state == state)
                    {
                        return;
                    }

                    newState = state;
                    break;
                }
            }

            Log.Info(this, "Change({0} -> {1})",
                null == _state ? "[Null]" : _state.ToString(),
                null == newState ? "[Null]" : newState.ToString());

            if (null != _state)
            {
                _state.Exit();
            }

            _state = newState;

            if (null != _state)
            {
                _state.Enter();
            }
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        /// <param name="dt">The time that has elapsed since the last Update.</param>
        public void Update(float dt)
        {
            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }
}
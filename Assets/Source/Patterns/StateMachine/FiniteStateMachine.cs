using System;
using Void = CreateAR.Commons.Unity.Async.Void;

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
        public IState Current { get; private set; }

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
            Change(typeof(T), Void.Instance);
        }

        /// <summary>
        /// Changes to the state of type T.
        /// </summary>
        /// <typeparam name="T">The type of the state to transition to.</typeparam>
        /// <param name="context">Parameter to pass to enter method.</param>
        public void Change<T>(object context) where T : IState
        {
            Change(typeof(T), context);
        }

        /// <summary>
        /// Changes to the state of the given type.
        /// </summary>
        /// <param name="type">The type of the state to transition to.</param>
        public void Change(Type type)
        {
            Change(type, Void.Instance);
        }

        /// <summary>
        /// Changes to the state of the given type.
        /// </summary>
        /// <param name="type">The type of the state to transition to.</param>
        /// <param name="context">Parameter to pass to enter method.</param>
        public void Change(Type type, object context)
        {
            IState newState = null;

            for (int i = 0, len = _states.Length; i < len; i++)
            {
                var state = _states[i];
                if (state.GetType() == type)
                {
                    newState = state;
                    break;
                }
            }
            
            if (null != Current)
            {
                Current.Exit();
            }

            Current = newState;

            if (null != Current)
            {
                Current.Enter(context);
            }
        }

        /// <summary>
        /// Updates the current state.
        /// </summary>
        /// <param name="dt">The time that has elapsed since the last Update.</param>
        public void Update(float dt)
        {
            if (null != Current)
            {
                Current.Update(dt);
            }
        }
    }
}
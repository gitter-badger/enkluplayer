﻿using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Very basic state machine implementation.
    /// </summary>
    public class StateMachine
    {
        /// <summary>
        /// The current state.
        /// </summary>
        private IState _state;

        /// <summary>
        /// Changes state.
        /// </summary>
        /// <param name="state">The state to transition to.</param>
        public void Change(IState state)
        {
            if (state == _state)
            {
                return;
            }

            Log.Info(this, "Change({0} -> {1})",
                null == _state ? "[Null]" : _state.ToString(),
                null == state ? "[Null]" : state.ToString());

            if (null != _state)
            {
                _state.Exit();
            }

            _state = state;

            if (null != _state)
            {
                _state.Enter();
            }
        }

        /// <summary>
        /// Called every frame.
        /// </summary>
        /// <param name="dt"></param>
        public void Update(float dt)
        {
            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }
}
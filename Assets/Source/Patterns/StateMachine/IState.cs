using System;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class FinineStateMachine
    {
        private readonly IState[] _states;
        private IState _state;

        public FinineStateMachine(IState[] states)
        {
            _states = states;
        }

        public void Change<T>() where T : IState
        {
            Change(typeof(T));
        }

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

        public void Update(float dt)
        {
            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }
    
    public class StateMachine
    {
        private IState _state;

        public void Change(IState state)
        {
            Log.Info(this, "Change([{0}] -> [{1}])",
                _state,
                state);

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

        public void Update(float dt)
        {
            if (null != _state)
            {
                _state.Update(dt);
            }
        }
    }

    public interface IState
    {
        void Enter();
        void Update(float dt);
        void Exit();
    }
}

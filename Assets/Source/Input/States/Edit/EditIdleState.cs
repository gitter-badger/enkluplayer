using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Input state when idle in edit mode.
    /// </summary>
    public class EditIdleState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMultiInput _input;

        /// <summary>
        /// Called when this state requests a transition.
        /// </summary>
        public event Action<Type> OnTransition;

        /// <summary>
        /// Creates a new EditIdleStaet.
        /// </summary>
        public EditIdleState(IMultiInput input)
        {
            _input = input;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            //
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            var points = _input.Points;

            if (1 == points.Count)
            {
                var point = points[0];
                if (point.IsDown)
                {
                    OnTransition(typeof(EditRotateState));
                }
            }

            if (2 == points.Count)
            {
                var point = points[0];

                if (point.IsDown)
                {
                    OnTransition(typeof(EditPanState));
                }
            }
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            //
        }
    }
}
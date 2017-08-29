using System;

namespace CreateAR.SpirePlayer
{
    public class EditIdleState : IState
    {
        private readonly IMultiInput _input;

        public event Action<Type> OnNext;

        public EditIdleState(IMultiInput input)
        {
            _input = input;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
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
                    OnNext(typeof(EditRotateState));
                }
            }

            if (2 == points.Count)
            {
                var point = points[0];

                if (point.IsDown)
                {
                    OnNext(typeof(EditPanState));
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
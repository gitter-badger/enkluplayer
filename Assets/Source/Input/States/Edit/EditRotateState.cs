using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Input state for controlling rotation.
    /// </summary>
    public class EditRotateState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMultiInput _input;
        private readonly MainCamera _camera;
        private readonly InputConfig _config;

        /// <summary>
        /// Called when a state should be transitioned to.
        /// </summary>
        public event Action<Type> OnTransition;

        /// <summary>
        /// Crates a new rotation state.
        /// </summary>
        public EditRotateState(
            IMultiInput input,
            MainCamera camera,
            InputConfig config)
        {
            _input = input;
            _camera = camera;
            _config = config;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter()
        {

        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            Assert.IsTrue(1 == _input.Points.Count, "EditRotateState expects exactly one point.");

            var point = _input.Points[0];
            if (point.IsUp)
            {
                OnTransition(typeof(EditIdleState));
            }
            else
            {
                var screenDelta = point.CurrentPosition - point.PreviousPosition;
                var transform = _camera.transform;
                transform.Rotate(
                    Vector3.right,
                    _config.RotateMultiplier * screenDelta.y);
                transform.Rotate(
                    Vector3.up,
                    -_config.RotateMultiplier * screenDelta.x,
                    Space.World);
            }
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {

        }
    }
}
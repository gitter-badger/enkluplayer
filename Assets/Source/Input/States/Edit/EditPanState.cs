using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Input state that controls panning.
    /// </summary>
    public class EditPanState : IState
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IMultiInput _input;
        private readonly MainCamera _camera;
        private readonly InputConfig _config;

        /// <summary>
        /// Called when this state requests a transition.
        /// </summary>
        public event Action<Type> OnTransition;

        /// <summary>
        /// Creates a new EditPanState.
        /// </summary>
        public EditPanState(
            IMultiInput input,
            MainCamera camera,
            InputConfig config)
        {
            _input = input;
            _camera = camera;
            _config = config;
        }

        /// <inheritdoc cref="IState"/>
        public void Enter(object context)
        {
            
        }

        /// <inheritdoc cref="IState"/>
        public void Update(float dt)
        {
            Assert.IsTrue(2 == _input.Points.Count, "EditPanState expects exactly two points.");

            var a = _input.Points[0];
            var b = _input.Points[1];

            if (a.IsUp || b.IsUp)
            {
                OnTransition(typeof(EditIdleState));
            }
            else
            {
                var screenDelta = a.CurrentPosition - a.PreviousPosition;
                _camera.transform.Translate(
                    _config.TranslateMultiplier * 
                    -(Vector3.up * screenDelta.y + Vector3.right * screenDelta.x));
            }
        }

        /// <inheritdoc cref="IState"/>
        public void Exit()
        {
            
        }
    }
}
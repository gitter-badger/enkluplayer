using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    public class EditRotateState : IState
    {
        private readonly IMultiInput _input;
        private readonly MainCamera _camera;
        private readonly InputConfig _config;
        private Vector3 _startFloorIntersection;

        public event Action<Type> OnNext;

        public EditRotateState(
            IMultiInput input,
            MainCamera camera,
            InputConfig config)
        {
            _input = input;
            _camera = camera;
            _config = config;
        }

        public void Enter()
        {

        }

        public void Update(float dt)
        {
            Assert.IsTrue(1 == _input.Points.Count, "EditRotateState expects exactly one point.");

            var point = _input.Points[0];
            if (point.IsUp)
            {
                OnNext(typeof(EditIdleState));
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

        public void Exit()
        {

        }
    }
}
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    public class EditPanState : IState
    {
        private readonly IMultiInput _input;
        private readonly MainCamera _camera;
        private readonly InputConfig _config;
        private Vector3 _startFloorIntersection;

        public event Action<Type> OnNext;

        public EditPanState(
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
            Assert.IsTrue(2 == _input.Points.Count, "EditPanState expects exactly two points.");

            var a = _input.Points[0];
            var b = _input.Points[1];

            if (a.IsUp || b.IsUp)
            {
                OnNext(typeof(EditIdleState));
            }
            else
            {
                var screenDelta = a.CurrentPosition - a.PreviousPosition;
                _camera.transform.Translate(
                    _config.TranslateMultiplier * 
                    -(Vector3.up * screenDelta.y + Vector3.right * screenDelta.x));
            }
        }

        public void Exit()
        {
            
        }
    }
}
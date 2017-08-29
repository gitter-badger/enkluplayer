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

    public class EditModeInputState : IInputState
    {
        private readonly IMultiInput _input;
        private readonly MainCamera _camera;
        private readonly InputConfig _config;

        private FiniteStateMachine _states;

        public EditModeInputState(
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
            var idle = new EditIdleState(_input);
            idle.OnNext += type => _states.Change(type);

            var pan = new EditPanState(_input, _camera, _config);
            pan.OnNext += type => _states.Change(type);

            var rotate = new EditRotateState(_input, _camera, _config);
            rotate.OnNext += type => _states.Change(type);

            _states = new FiniteStateMachine(new IState[]
            {
                idle, pan, rotate
            });

            _states.Change<EditIdleState>();
        }

        public void Update(float dt)
        {
            _states.Update(dt);
        }

        public void Exit()
        {
            _states.Change<EditIdleState>();
            _states = null;
        }
    }
}
using CreateAR.Commons.Unity.DebugRenderer;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class EditPanState : IState
    {
        private readonly DebugRenderer _renderer;
        private readonly InputPoint _a;
        private readonly InputPoint _b;
        private Vector3 _startFloorIntersection;

        public EditPanState(
            DebugRenderer renderer,
            InputPoint a,
            InputPoint b)
        {
            _renderer = renderer;
            _a = a;
            _b = b;
        }

        public void Enter()
        {
            
        }

        public void Update(float dt)
        {
            var handle2D = _renderer.Handle2D("Input.EditModeInputState");
            if (null != handle2D)
            {
                handle2D.Draw(context =>
                {
                    context.Color(Color.green);
                    context.Line(_a.DownPosition, _a.CurrentPosition);
                    context.Line(_b.DownPosition, _b.CurrentPosition);
                });
            }

            var handle = _renderer.Handle("Input.EditModeInputState");
            if (null != handle)
            {
                handle.Draw(context =>
                {
                    context.Color(Color.yellow);
                    context.Cube(_a.DownWorldSpacePosition, 2);
                    context.Cube(_a.CurrentWorldSpacePosition, 2);

                    context.Cube(_b.DownWorldSpacePosition, 2);
                    context.Cube(_b.CurrentWorldSpacePosition, 2);
                });
            }
        }

        public void Exit()
        {
            
        }
    }

    public class EditRotateState : IState
    {
        private readonly DebugRenderer _renderer;
        private readonly InputPoint _point;
        private Vector3 _startFloorIntersection;

        public EditRotateState(
            DebugRenderer renderer,
            InputPoint point)
        {
            _renderer = renderer;
            _point = point;
        }

        public void Enter()
        {

        }

        public void Update(float dt)
        {
            var handle2D = _renderer.Handle2D("Input.EditModeInputState");
            if (null != handle2D)
            {
                handle2D.Draw(context =>
                {
                    context.Color(Color.green);
                    context.Line(_point.DownPosition, _point.CurrentPosition);
                });
            }

            var handle = _renderer.Handle("Input.EditModeInputState");
            if (null != handle)
            {
                handle.Draw(context =>
                {
                    context.Color(Color.yellow);
                    context.Cube(_point.DownWorldSpacePosition, 2);
                    context.Cube(_point.CurrentWorldSpacePosition, 2);
                });
            }
        }

        public void Exit()
        {

        }
    }

    public class EditIdleState : IState
    {
        private readonly DebugRenderer _renderer;
        private readonly IMultiInput _input;
        private readonly StateMachine _states;

        public EditIdleState(
            DebugRenderer renderer,
            IMultiInput input,
            StateMachine states)
        {
            _renderer = renderer;
            _input = input;
            _states = states;
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
                    _states.Change(new EditRotateState(
                        _renderer,
                        point));
                }
            }

            if (2 == points.Count)
            {
                var point = points[0];

                if (point.IsDown)
                {
                    _states.Change(new EditPanState(
                        _renderer,
                        points[0],
                        points[1]));
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
        private readonly DebugRenderer _renderer;
        private readonly IMultiInput _input;
        private readonly StateMachine _states = new StateMachine();

        public EditModeInputState(
            DebugRenderer renderer,
            IMultiInput input)
        {
            _renderer = renderer;
            _input = input;
        }

        public void Enter()
        {
            _states.Change(new EditIdleState(_renderer, _input, _states));
        }

        public void Update(float dt)
        {
            _states.Update(dt);
        }

        public void Exit()
        {
            _states.Change(null);
        }
    }
}

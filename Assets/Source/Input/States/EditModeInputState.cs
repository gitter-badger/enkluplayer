using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace CreateAR.SpirePlayer
{
    public class EditPanState : IState
    {
        private readonly IMultiInput _input;
        private Vector3 _startFloorIntersection;

        public event Action<Type> OnNext;

        public EditPanState(IMultiInput input)
        {
            _input = input;
        }

        public void Enter()
        {
            
        }

        public void Update(float dt)
        {
            Assert.IsTrue(2 == _input.Points.Count, "EditPanState expects exactly two points.");

            var a = _input.Points[0];
            var b = _input.Points[1];

            DebugDraw(a, b);
        }

        public void Exit()
        {
            
        }

        private void DebugDraw(InputPoint a, InputPoint b)
        {
            var handle2D = Render.Handle2D("Input.Edit.Pan");
            if (null != handle2D)
            {
                handle2D.Draw(context =>
                {
                    context.Color(Color.green);
                    context.Line(a.DownPosition, a.CurrentPosition);
                    context.Line(b.DownPosition, b.CurrentPosition);
                });
            }

            var handle = Render.Handle("Input.Edit.Pan");
            if (null != handle)
            {
                handle.Draw(context =>
                {
                    context.Color(Color.yellow);
                    context.Cube(a.DownWorldSpacePosition, 2);
                    context.Cube(a.CurrentWorldSpacePosition, 2);

                    context.Cube(b.DownWorldSpacePosition, 2);
                    context.Cube(b.CurrentWorldSpacePosition, 2);
                });
            }
        }
    }

    public class EditRotateState : IState
    {
        private readonly IMultiInput _input;
        private Vector3 _startFloorIntersection;

        public event Action<Type> OnNext;

        public EditRotateState(IMultiInput input)
        {
            _input = input;
        }

        public void Enter()
        {

        }

        public void Update(float dt)
        {
            Assert.IsTrue(1 == _input.Points.Count, "EditRotateState expects exactly one point.");

            var point = _input.Points[0];
            
            DebugDraw(point);
        }

        public void Exit()
        {

        }

        private void DebugDraw(InputPoint point)
        {
            var handle2D = Render.Handle2D("Input.Edit.Rotate");
            if (null != handle2D)
            {
                handle2D.Draw(context =>
                {
                    context.Color(Color.green);
                    context.Line(point.DownPosition, point.CurrentPosition);
                });
            }

            var handle = Render.Handle("Input.Edit.Rotate");
            if (null != handle)
            {
                handle.Draw(context =>
                {
                    context.Color(Color.yellow);
                    context.Cube(point.DownWorldSpacePosition, 2);
                    context.Cube(point.CurrentWorldSpacePosition, 2);
                });
            }
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
        private readonly FinineStateMachine _states;

        public EditModeInputState(IMultiInput input)
        {
            _input = input;

            var idle = new EditIdleState(_input);
            idle.OnNext += _states.Change;

            var pan = new EditPanState(_input);
            pan.OnNext += _states.Change;

            var rotate = new EditRotateState(_input);
            rotate.OnNext += _states.Change;

            _states = new FinineStateMachine(new IState[]
            {
                idle, pan, rotate
            });
        }

        public void Enter()
        {
            _states.Change<EditIdleState>();
        }

        public void Update(float dt)
        {
            _states.Update(dt);
        }

        public void Exit()
        {
            _states.Change<EditIdleState>();
        }
    }
}
using CreateAR.Commons.Unity.DebugRenderer;

namespace CreateAR.SpirePlayer
{
    public class InputManager : IInputManager
    {
        private readonly DebugRenderer _renderer;
        private readonly IMultiInput _input;

        private IInputState _state;

        public InputManager(
            DebugRenderer renderer,
            IMultiInput input)
        {
            _renderer = renderer;
            _input = input;
        }

        public void ChangeState(IInputState state)
        {
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
            _input.Update(dt);

            if (null != _state)
            {
                _state.Update(dt);
            }

            DebugDraw();
        }

        private void DebugDraw()
        {
            var handle = _renderer.Handle2D("Input.Touches");
            if (null != handle)
            {
                handle.Draw(context =>
                {
                    var points = _input.Points;
                    for (int i = 0, len = points.Count; i < len; i++)
                    {
                        var point = points[i];

                        context.Square(point.CurrentPosition, 50f);
                    }
                });
            }
        }
    }
}
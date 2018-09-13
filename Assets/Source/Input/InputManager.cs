namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Controls input across the application.
    /// </summary>
    public class InputManager : IInputManager
    {
        /// <summary>
        /// <c>IMultiInput</c> implementation.
        /// </summary>
        private readonly IMultiInput _input;

        /// <summary>
        /// Controls input states.
        /// </summary>
        private readonly StateMachine _states = new StateMachine();

        /// <inheritdoc cref="IInputManager"/>
        public IMultiInput MultiInput
        {
            get
            {
                return _input;
            }
        }

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="input"></param>
        public InputManager(IMultiInput input)
        {
            _input = input;
        }

        /// <inheritdoc cref="IInputManager"/>
        public void ChangeState(IState state)
        {
            _states.Change(state);
        }

        /// <inheritdoc cref="IInputManager"/>
        public void Update(float dt)
        {
            _input.Update(dt);
            _states.Update(dt);

            DebugDraw();
        }

        /// <summary>
        /// Draws each input point.
        /// </summary>
        private void DebugDraw()
        {
            var handle = Render.Handle2D("Input.Points");
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
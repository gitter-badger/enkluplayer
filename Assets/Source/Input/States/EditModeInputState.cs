using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer
{
    public class EditModeInputState : IInputState
    {
        private readonly IMultiInput _input;

        public EditModeInputState(IMultiInput input)
        {
            _input = input;
        }

        public void Enter()
        {
            
        }

        public void Update(float dt)
        {
            var points = _input.Points;

            if (1 == points.Count)
            {
                var point = points[0];
                if (point.IsDown)
                {
                    Log.Info(this, "Start[Rotate]");
                }
                else if (point.IsUp)
                {
                    Log.Info(this, "Stop[Rotate]");
                }
                else
                {
                    //
                }
            }

            if (2 == points.Count)
            {
                var point = points[0];

                if (point.IsDown)
                {
                    Log.Info(this, "Start[Pan]");
                }
                else if (point.IsUp)
                {
                    Log.Info(this, "Stop[Pan]");
                }
                else
                {
                    //
                }
            }
        }

        public void Exit()
        {
            
        }
    }
}

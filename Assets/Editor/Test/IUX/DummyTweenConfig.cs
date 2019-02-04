using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Test.UI
{
    public class DummyTweenConfig : ITweenConfig
    {
        public TweenProfile[] Profiles { get; private set; }
        public float DurationSeconds(TweenType type)
        {
            return 0;
        }
    }
}
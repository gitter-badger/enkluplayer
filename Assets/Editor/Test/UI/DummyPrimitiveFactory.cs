using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyPrimitiveFactory : IPrimitiveFactory
    {
        public IText Text()
        {
            return null;
        }

        public IActivator Activator()
        {
            return null;
        }

        public IReticle Reticle()
        {
            return null;
        }
    }
}
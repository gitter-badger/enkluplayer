using CreateAR.SpirePlayer.UI;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyPrimitiveFactory : IPrimitiveFactory
    {
        public TextPrimitive Text()
        {
            return null;
        }

        public IActivator Activator()
        {
            return null;
        }

        public ReticlePrimitive Reticle()
        {
            return null;
        }
    }
}
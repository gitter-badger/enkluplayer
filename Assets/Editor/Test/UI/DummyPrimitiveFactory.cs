using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyPrimitiveFactory : IPrimitiveFactory
    {
        public TextPrimitive Text(ElementSchema elementSchema)
        {
            return null;
        }

        public ActivatorPrimitive Activator(ElementSchema elementSchema)
        {
            return null;
        }

        public ReticlePrimitive Reticle()
        {
            return null;
        }
    }
}
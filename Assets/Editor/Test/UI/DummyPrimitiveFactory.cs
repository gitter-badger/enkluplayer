using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer.Test.UI
{
    public class DummyPrimitiveFactory : IPrimitiveFactory
    {
        public TextPrimitive Text(ElementSchema elementSchema)
        {
            return null;
        }

        public ActivatorPrimitive Activator(ElementSchema elementSchema, Widget target)
        {
            return null;
        }

        public ReticlePrimitive Reticle()
        {
            return null;
        }

        public Float Float(ElementSchema selementSchema)
        {
            return null;
        }
    }
}
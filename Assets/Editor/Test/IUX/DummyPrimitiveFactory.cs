using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Test.UI
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

        public FloatWidget Float(ElementSchema selementSchema)
        {
            return null;
        }
    }
}
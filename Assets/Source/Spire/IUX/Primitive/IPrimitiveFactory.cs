namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives
    /// </summary>
    public interface IPrimitiveFactory
    {
        TextPrimitive Text();
        ActivatorPrimitive Activator();
        ReticlePrimitive Reticle();
    }
}
namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives
    /// </summary>
    public interface IPrimitiveFactory
    {
        TextPrimitive Text();
        ActivatorMonoBehaviour Activator();
        ReticlePrimitive Reticle();
    }
}
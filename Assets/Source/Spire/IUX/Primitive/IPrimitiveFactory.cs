namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives.
    /// </summary>
    public interface IPrimitiveFactory
    {
        /// <summary>
        /// Creates text.
        /// </summary>
        /// <returns></returns>
        TextPrimitive Text();

        /// <summary>
        /// Creates an Activator.
        /// </summary>
        /// <returns></returns>
        ActivatorPrimitive Activator();

        /// <summary>
        /// Creates a reticle.
        /// </summary>
        /// <returns></returns>
        ReticlePrimitive Reticle();
    }
}
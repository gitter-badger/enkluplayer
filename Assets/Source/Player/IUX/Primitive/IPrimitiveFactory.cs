﻿namespace CreateAR.EnkluPlayer.IUX
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
        TextPrimitive Text(ElementSchema schema);

        /// <summary>
        /// Creates an Activator.
        /// </summary>
        /// <returns></returns>
        ActivatorPrimitive Activator(ElementSchema schema, Widget target);

        /// <summary>
        /// Creates a reticle.
        /// </summary>
        /// <returns></returns>
        ReticlePrimitive Reticle();
    }
}
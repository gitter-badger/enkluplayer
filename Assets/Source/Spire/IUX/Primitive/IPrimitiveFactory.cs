using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives
    /// </summary>
    public interface IPrimitiveFactory
    {
        ITextPrimitive LoadText(IWidget widget);
        IActivatorPrimitive LoadActivator(IWidget widget);
        IReticlePrimitive LoadReticle(IWidget widget);
    }
}

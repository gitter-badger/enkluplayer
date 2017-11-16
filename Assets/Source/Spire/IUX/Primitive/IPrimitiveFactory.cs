using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives
    /// </summary>
    public interface IPrimitiveFactory
    {
        IText LoadText();
        IActivator LoadActivator();
        IReticle LoadReticle();
    }
}

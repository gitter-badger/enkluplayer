using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives
    /// </summary>
    public interface IPrimitiveFactory
    {
        IText Text();
        IActivator Activator();
        IReticle Reticle();
    }
}

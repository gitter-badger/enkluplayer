using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Creates basic UI primitives
    /// </summary>
    public interface IPrimitiveFactory
    {
        ITextPrimitive RequestText(Transform parent);
        IActivatorPrimitive RequestActivator(Transform parent);
        void Release(IPrimitive primitive);
    }
}

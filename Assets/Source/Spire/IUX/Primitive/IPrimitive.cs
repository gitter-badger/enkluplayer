using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Basic interface for all element primitives
    /// </summary>
    public interface IPrimitive
    {
        /// <summary>
        /// Positioning
        /// </summary>
        IWidget Widget { get; }

        /// <summary>
        /// Loads the primitive
        /// </summary>
        /// <param name="widget"></param>
        void Load(IWidget widget);

        /// <summary>
        /// Unloads the primitive
        /// </summary>
        void Unload();
    }
}

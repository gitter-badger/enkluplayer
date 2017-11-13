using UnityEngine;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Basic interface for all element primitives
    /// </summary>
    public interface IPrimitive
    {
        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="schema"></param>
        void Load(ElementSchema schema);

        /// <summary>
        /// Positioning
        /// </summary>
        Transform Transform { get; }
    }
}

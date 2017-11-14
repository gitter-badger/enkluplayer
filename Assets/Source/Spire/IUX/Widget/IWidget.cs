using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public interface IWidget
    {
        /// <summary>
        /// Associated Game Object in the Unity Hierarchy.
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// Render color (composition of local and parent colors).
        /// </summary>
        SpirePlayer.Col4 Color { get; }

        /// <summary>
        /// Visibility Accessor/Mutator
        /// </summary>
        bool LocalVisible { get; set; }
    }
}
